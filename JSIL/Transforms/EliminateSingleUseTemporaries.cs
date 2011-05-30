using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class EliminateSingleUseTemporaries : JSAstVisitor {
        public const int TraceLevel = 0;

        public readonly TypeSystem TypeSystem;
        public readonly Dictionary<string, JSVariable> Variables;
        public readonly HashSet<JSVariable> EliminatedVariables = new HashSet<JSVariable>();

        protected FunctionStaticData Data = null;

        public EliminateSingleUseTemporaries (TypeSystem typeSystem, Dictionary<string, JSVariable> variables) {
            TypeSystem = typeSystem;
            Variables = variables;
        }

        protected void EliminateVariable (JSNode context, JSVariable variable, JSExpression replaceWith) {
            {
                var replacer = new VariableEliminator(
                    variable,
                    JSChangeTypeExpression.New(replaceWith, TypeSystem, variable.GetExpectedType(TypeSystem))
                );
                replacer.Visit(context);
            }

            {
                var replacer = new VariableEliminator(variable, replaceWith);
                var assignments = (from a in Data.Data.Assignments where 
                                       a.NewValue.AllChildrenRecursive.Any((_n) => variable.Equals(_n))
                                       select a).ToArray();

                foreach (var a in assignments) {
                    if (variable.Equals(a.NewValue)) {
                        Data.Data.Assignments.Remove(a);
                        Data.Data.Assignments.Add(
                            new FunctionAnalysis.Assignment(
                                a.StatementIndex, a.NodeIndex,
                                a.Target, replaceWith, a.Operator,
                                a.TargetType, a.SourceType
                            )
                        );
                    } else {
                        replacer.Visit(a.NewValue);
                    }
                }
            }

            Variables.Remove(variable.Identifier);
        }

        protected bool IsEffectivelyConstant (JSVariable target, JSExpression source) {
            if (source == null)
                return true;

            // Handle special cases where our interpretation of 'constant' needs to be more flexible
            {
                var ie = source as JSIndexerExpression;
                if (ie != null) {
                    if (
                        IsEffectivelyConstant(target, ie.Target) &&
                        IsEffectivelyConstant(target, ie.Index)
                    )
                        return true;
                }
            }

            {
                var ae = source as JSArrayExpression;
                if (
                    (ae != null) &&
                    (from av in ae.Values select IsEffectivelyConstant(target, av)).All((b) => b)
                )
                    return true;
            }

            {
                var de = source as JSDotExpression;
                if (
                    (de != null) &&
                    IsEffectivelyConstant(target, de.Target) &&
                    IsEffectivelyConstant(target, de.Member)
                )
                    return true;
            }

            {
                var ie = source as JSInvocationExpression;
                if (
                    (ie != null) && ie.ConstantIfArgumentsAre &&
                    IsEffectivelyConstant(target, ie.ThisReference) &&
                    ie.Arguments.All((a) => IsEffectivelyConstant(target, a))
                )
                    return true;
            }

            if ((source is JSUnaryOperatorExpression) || (source is JSBinaryOperatorExpression)) {
                if (source.Children.OfType<JSExpression>().All((_v) => IsEffectivelyConstant(target, _v)))
                    return true;
            }

            if (source.IsConstant)
                return true;

            var d = Data.Data;

            // Try to find a spot between the source variable's assignments where all of our
            //  copies and accesses can fit. If we find one, our variable is effectively constant.
            var v = source as JSVariable;
            if (v != null) {
                var assignments = (from a in d.Assignments where v.Equals(a.Target) select a).ToArray();
                if (assignments.Length < 1)
                    return v.IsParameter;

                var targetAssignments = (from a in d.Assignments where v.Equals(a.Target) select a).ToArray();
                if (targetAssignments.Length < 1)
                    return false;

                var targetAccesses = (from a in d.Accesses where target.Equals(a.Source) select a).ToArray();
                if (targetAccesses.Length < 1)
                    return false;

                var targetFirstAssigned = targetAssignments.FirstOrDefault();
                var targetLastAssigned = targetAssignments.LastOrDefault();

                var targetFirstAccessed = targetAccesses.FirstOrDefault();
                var targetLastAccessed = targetAccesses.LastOrDefault();

                bool foundAssignmentSlot = false;

                for (int i = 0, c = assignments.Length; i < c; i++) {
                    int assignment = assignments[i].StatementIndex, nextAssignment = int.MaxValue;
                    if (i < c - 1)
                        nextAssignment = assignments[i + 1].StatementIndex;

                    if (
                        (targetFirstAssigned.StatementIndex >= assignment) &&
                        (targetFirstAssigned.StatementIndex < nextAssignment) &&
                        (targetFirstAccessed.StatementIndex >= assignment) &&
                        (targetLastAccessed.StatementIndex <= nextAssignment)
                    ) {
                        foundAssignmentSlot = true;
                        break;
                    }
                }

                if (!foundAssignmentSlot)
                    return false;

                return true;
            }

            // TODO
            /*
            var accesses = (from a in d.Accesses where v.Equals(a.Source) select a).ToArray();
            if (accesses.Length < 1)
                return false;

            var firstAccess = accesses.FirstOrDefault();
            var lastAccess = accesses.LastOrDefault();

            if (firstAccess == firstAssignment + 1)
                return true;
             */

            return false;
        }

        public void VisitNode (JSFunctionExpression fn) {
            // Create a new visitor for nested function expressions
            if (Stack.OfType<JSFunctionExpression>().Skip(1).FirstOrDefault() != null) {
                bool eliminated = false;

                do {
                    var nested = new EliminateSingleUseTemporaries(TypeSystem, fn.AllVariables);
                    nested.Visit(fn);
                    eliminated = nested.EliminatedVariables.Count > 0;
                } while (eliminated);

                return;
            }

            var nullList = new List<int>();
            Data = new StaticAnalyzer(TypeSystem).Analyze(fn);

            VisitChildren(fn);

            var d = Data.Data;

            foreach (var v in Data.AllVariables.ToArray()) {
                if (v.IsReference || v.IsThis || v.IsParameter)
                    continue;

                var valueType = v.GetExpectedType(TypeSystem);
                if (valueType.IsByReference || ILBlockTranslator.IsIgnoredType(valueType))
                    continue;

                var assignments = (from a in d.Assignments where v.Equals(a.Target) select a).ToArray();
                var accesses = (from a in d.Accesses where v.Equals(a.Source) select a).ToArray();

                if (assignments.FirstOrDefault() == null) {
                    if (accesses.Length == 0) {
                        if (TraceLevel >= 1)
                            Debug.WriteLine(String.Format("Eliminating {0} because it is never used.", v));

                        EliminatedVariables.Add(v);
                        EliminateVariable(fn, v, new JSNullExpression());
                    } else {
                        if (TraceLevel >= 2)
                            Debug.WriteLine(String.Format("Never found an initial assignment for {0}.", v));
                    }

                    continue;
                }

                if ((from a in accesses where a.IsControlFlow select a).FirstOrDefault() != null) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it participates in control flow.", v));

                    continue;
                }

                /*
                if ((from a in d.Assignments where v.Equals(a.Target) && a.IsConversion select a).FirstOrDefault() != null) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it undergoes type conversion.", v));

                    continue;
                }
                 */

                if (assignments.Length > 1) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it is reassigned.", v));

                    continue;
                }

                var copies = (from a in d.Assignments where v.Equals(a.SourceVariable) select a).ToArray();
                if ((copies.Length + accesses.Length) > 1) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it is used multiple times.", v));

                    continue;
                }

                var replacement = assignments.First().NewValue;
                if (replacement.AllChildrenRecursive.Contains(v)) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it contains a self-reference.", v));

                    continue;
                }

                if (!IsEffectivelyConstant(v, replacement)) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it is not a constant expression.", v));

                    continue;
                }

                if (TraceLevel >= 1)
                    Debug.WriteLine(String.Format("Eliminating {0} <- {1}", v, replacement));

                var transferDataTo = replacement as JSVariable;
                if (transferDataTo != null) {
                    foreach (var access in accesses) {
                        d.Accesses.Remove(access);
                        d.Accesses.Add(new FunctionAnalysis.Access(
                            access.StatementIndex, access.NodeIndex,
                            transferDataTo, access.IsControlFlow
                        ));
                    }

                    foreach (var assignment in assignments) {
                        d.Assignments.Remove(assignment);
                        d.Assignments.Add(new FunctionAnalysis.Assignment(
                            assignment.StatementIndex, assignment.NodeIndex,
                            transferDataTo, assignment.NewValue, assignment.Operator,
                            assignment.TargetType, assignment.SourceType
                            
                       ));
                    }
                }

                Data.Data.Assignments.RemoveAll((a) => v.Equals(a.Target));
                Data.Data.Accesses.RemoveAll((a) => v.Equals(a.Source));

                EliminatedVariables.Add(v);

                EliminateVariable(fn, v, replacement);
            }
        }
    }

    public class VariableEliminator : JSAstVisitor {
        public readonly JSVariable Variable;
        public readonly JSExpression Replacement;

        public VariableEliminator (JSVariable variable, JSExpression replacement) {
            Variable = variable;
            Replacement = replacement;
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            if ((boe.Operator == JSOperator.Assignment) && (boe.Left.Equals(Variable))) {
                ParentNode.ReplaceChild(boe, new JSNullExpression());
            } else {
                VisitChildren(boe);
            }
        }

        public void VisitNode (JSVariable variable) {
            if (ParentNode is JSFunctionExpression) {
                // In argument list
                return;
            }

            if (Variable.Equals(variable)) {
                ParentNode.ReplaceChild(variable, Replacement);
            } else {
                VisitChildren(variable);
            }
        }
    }
}
