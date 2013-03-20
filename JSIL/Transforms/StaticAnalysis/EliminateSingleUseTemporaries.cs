using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class EliminateSingleUseTemporaries : StaticAnalysisJSAstVisitor {
        public static bool DryRun = false;
        public static int TraceLevel = 0;

        public readonly TypeSystem TypeSystem;
        public readonly Dictionary<string, JSVariable> Variables;
        public readonly HashSet<JSVariable> EliminatedVariables = new HashSet<JSVariable>();

        protected FunctionAnalysis1stPass FirstPass = null;

        public EliminateSingleUseTemporaries (QualifiedMemberIdentifier member, IFunctionSource functionSource, TypeSystem typeSystem, Dictionary<string, JSVariable> variables)
            : base (member, functionSource) {
            TypeSystem = typeSystem;
            Variables = variables;
        }

        protected void EliminateVariable (JSNode context, JSVariable variable, JSExpression replaceWith, QualifiedMemberIdentifier method) {
            {
                var replacer = new VariableEliminator(
                    variable,
                    JSChangeTypeExpression.New(replaceWith, variable.GetActualType(TypeSystem), TypeSystem)
                );
                replacer.Visit(context);
            }

            {
                var replacer = new VariableEliminator(variable, replaceWith);
                var assignments = (from a in FirstPass.Assignments where 
                                       variable.Equals(a.NewValue) ||
                                       a.NewValue.SelfAndChildrenRecursive.Any((_n) => variable.Equals(_n))
                                       select a).ToArray();

                foreach (var a in assignments) {
                    if (!variable.Equals(a.NewValue))
                        replacer.Visit(a.NewValue);
                }
            }

            Variables.Remove(variable.Identifier);
            FunctionSource.InvalidateFirstPass(method);
        }

        protected bool IsEffectivelyConstant (JSVariable target, JSExpression source) {
            if ((source == null) || (source.IsNull))
                return false;

            // Can't eliminate struct temporaries, since that might eliminate some implied copies.
            if (TypeUtil.IsStruct(target.IdentifierType))
                return false;

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
                var de = source as JSDotExpressionBase;
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

                if ((ie != null) && (ie.JSMethod != null)) {
                    var sa = GetSecondPass(ie.JSMethod);
                    if (sa != null) {
                        if (sa.IsPure) {
                            if (ie.Arguments.All((a) => IsEffectivelyConstant(target, a)))
                                return true;
                            else
                                return false;
                        }
                    }
                }
            }

            if ((source is JSUnaryOperatorExpression) || (source is JSBinaryOperatorExpression)) {
                if (source.Children.OfType<JSExpression>().All((_v) => IsEffectivelyConstant(target, _v)))
                    return true;
            }

            if (source.IsConstant)
                return true;

            // Try to find a spot between the source variable's assignments where all of our
            //  copies and accesses can fit. If we find one, our variable is effectively constant.
            var v = source as JSVariable;
            if (v != null) {
                var assignments = (from a in FirstPass.Assignments where v.Equals(a.Target) select a).ToArray();
                if (assignments.Length < 1)
                    return v.IsParameter;

                var targetAssignments = (from a in FirstPass.Assignments where v.Equals(a.Target) select a).ToArray();
                if (targetAssignments.Length < 1)
                    return false;

                var targetAccesses = (from a in FirstPass.Accesses where target.Equals(a.Source) select a).ToArray();
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

                // If we didn't find a slot, check to see if all the assignments come before all the accesses.
                if (!foundAssignmentSlot) {
                    var minAccessIndex = targetAccesses.Min((a) => a.StatementIndex);

                    if (assignments.All((a) => a.StatementIndex < minAccessIndex))
                        foundAssignmentSlot = true;
                }

                if (!foundAssignmentSlot)
                    return false;

                return true;
            }

            return false;
        }

        public void VisitNode (JSFunctionExpression fn) {
            var nullList = new List<int>();
            FirstPass = GetFirstPass(fn.Method.QualifiedIdentifier);
            if (FirstPass == null)
                throw new InvalidOperationException(String.Format(
                    "No first pass static analysis data for method '{0}'",
                    fn.Method.QualifiedIdentifier
                ));

            VisitChildren(fn);

            foreach (var v in fn.AllVariables.Values.ToArray()) {
                if (v.IsThis || v.IsParameter)
                    continue;

                var valueType = v.GetActualType(TypeSystem);
                if (TypeUtil.IsIgnoredType(valueType))
                    continue;

                var assignments = (from a in FirstPass.Assignments where v.Equals(a.Target) select a).ToArray();
                var reassignments = (from a in FirstPass.Assignments where v.Equals(a.SourceVariable) select a).ToArray();
                var accesses = (from a in FirstPass.Accesses where v.Equals(a.Source) select a).ToArray();
                var invocations = (from i in FirstPass.Invocations where v.Name == i.ThisVariable select i).ToArray();

                if (FirstPass.VariablesPassedByRef.Contains(v.Name)) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it is passed by reference.", v));

                    continue;
                }

                if (assignments.FirstOrDefault() == null) {
                    if ((accesses.Length == 0) && (invocations.Length == 0) && (reassignments.Length == 0)) {
                        if (TraceLevel >= 1)
                            Debug.WriteLine(String.Format("Eliminating {0} because it is never used.", v));

                        if (!DryRun) {
                            EliminatedVariables.Add(v);
                            EliminateVariable(fn, v, new JSEliminatedVariable(v), fn.Method.QualifiedIdentifier);

                            // We've invalidated the static analysis data so the best choice is to abort.
                            break;
                        }
                    } else {
                        if (TraceLevel >= 2)
                            Debug.WriteLine(String.Format("Never found an initial assignment for {0}.", v));
                    }

                    continue;
                }

                if (invocations.Length > 1) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; methods are invoked on it multiple times.", v));

                    continue;
                }

                if ((from a in accesses where a.IsControlFlow select a).FirstOrDefault() != null) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it participates in control flow.", v));

                    continue;
                }

                if (assignments.Length > 1) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it is reassigned.", v));

                    continue;
                }

                var copies = (from a in FirstPass.Assignments where v.Equals(a.SourceVariable) select a).ToArray();
                if ((copies.Length + accesses.Length) > 1) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it is used multiple times.", v));

                    continue;
                }

                var replacementAssignment = assignments.First();
                var replacement = replacementAssignment.NewValue;
                if (replacement.SelfAndChildrenRecursive.Contains(v)) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it contains a self-reference.", v));

                    continue;
                }

                if (!IsEffectivelyConstant(v, replacement)) {
                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; {1} is not a constant expression.", v, replacement));

                    continue;
                }

                var replacementField = replacement as JSFieldAccess;
                if (replacementField == null) {
                    var replacementRef = replacement as JSReferenceExpression;
                    if (replacementRef != null)
                        replacementField = replacementRef.Referent as JSFieldAccess;
                }

                if (replacementField != null) {
                    var lastAccess = accesses.LastOrDefault();

                    var affectedFields = replacement.SelfAndChildrenRecursive.OfType<JSField>().ToArray();
                    bool invalidatedByLaterFieldAccess = false;
                    foreach (var field in affectedFields) {
                        foreach (var fieldAccess in FirstPass.FieldAccesses) {
                            // Different field. Note that we only compare the FieldInfo, not the this-reference.
                            // Otherwise, aliasing (accessing the same field through two this references) would cause us
                            //  to incorrectly eliminate a local.
                            if (fieldAccess.Field.Field != replacementField.Field.Field)
                                continue;

                            // Ignore field accesses before the replacement was initialized
                            if (fieldAccess.NodeIndex <= replacementAssignment.NodeIndex)
                                continue;

                            // If the field access comes after the last use of the temporary, we don't care
                            if ((lastAccess != null) && (fieldAccess.StatementIndex > lastAccess.StatementIndex))
                                continue;

                            // It's a read, so no impact on whether this optimization is valid
                            if (fieldAccess.IsRead)
                                continue;

                            if (TraceLevel >= 2)
                                Debug.WriteLine(String.Format("Cannot eliminate {0}; {1} is potentially mutated later", v, replacementField.Field));

                            invalidatedByLaterFieldAccess = true;
                            break;
                        }

                        if (invalidatedByLaterFieldAccess)
                            break;
                    }

                    if (invalidatedByLaterFieldAccess)
                        continue;
                }

                if (TraceLevel >= 1)
                    Debug.WriteLine(String.Format("Eliminating {0} <- {1}", v, replacement));

                if (!DryRun) {
                    EliminatedVariables.Add(v);
                    EliminateVariable(fn, v, replacement, fn.Method.QualifiedIdentifier);

                    // We've invalidated the static analysis data so the best choice is to abort.
                    break;
                }
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
            if (CurrentName == "FunctionSignature") {
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
