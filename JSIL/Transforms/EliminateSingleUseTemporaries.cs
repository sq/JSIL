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
        public readonly Dictionary<JSVariable, JSExpressionStatement> FirstValues = new Dictionary<JSVariable, JSExpressionStatement>();
        public readonly Dictionary<JSVariable, List<int>> Assignments = new Dictionary<JSVariable, List<int>>();
        public readonly Dictionary<JSVariable, List<int>> Copies = new Dictionary<JSVariable, List<int>>();
        public readonly Dictionary<JSVariable, List<int>> Accesses = new Dictionary<JSVariable, List<int>>();
        public readonly Dictionary<JSVariable, List<int>> Conversions = new Dictionary<JSVariable, List<int>>();
        public readonly Dictionary<JSVariable, List<int>> ControlFlowAccesses = new Dictionary<JSVariable, List<int>>();

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
                foreach (var fv in FirstValues.Values)
                    replacer.Visit(fv);
            }

            Variables.Remove(variable.Identifier);
        }

        protected bool IsEffectivelyConstant (JSVariable target, JSExpression source) {
            // Handle special cases where our interpretation of 'constant' needs to be more flexible
            var ie = source as JSIndexerExpression;
            if (
                (ie != null) && 
                IsEffectivelyConstant(target, ie.Target) && 
                IsEffectivelyConstant(target, ie.Index)
            )
                return true;

            var ae = source as JSArrayExpression;
            if (
                (ae != null) &&
                (from av in ae.Values select IsEffectivelyConstant(target, av)).All((b) => b)
            )
                return true;

            var de = source as JSDotExpression;
            if (
                (de != null) &&
                IsEffectivelyConstant(target, de.Target) &&
                IsEffectivelyConstant(target, de.Member)
            )
                return true;

            if ((source is JSUnaryOperatorExpression) || (source is JSBinaryOperatorExpression)) {
                if (source.Children.OfType<JSExpression>().All((_v) => IsEffectivelyConstant(target, _v)))
                    return true;
            }

            if (source.IsConstant)
                return true;

            List<int> assignments;

            // Try to find a spot between the source variable's assignments where all of our
            //  copies and accesses can fit. If we find one, our variable is effectively constant.
            var v = source as JSVariable;
            if (v != null) {
                if (!Assignments.TryGetValue(v, out assignments))
                    return v.IsParameter;

                List<int> targetAssignments;
                if (!Assignments.TryGetValue(target, out targetAssignments))
                    return false;

                List<int> targetAccesses;
                if (!Accesses.TryGetValue(target, out targetAccesses))
                    return false;

                int targetFirstAssigned, targetLastAssigned;
                if (!GetMinMax(targetAssignments, out targetFirstAssigned, out targetLastAssigned))
                    return false;

                int targetFirstAccessed, targetLastAccessed;
                if (!GetMinMax(targetAccesses, out targetFirstAccessed, out targetLastAccessed))
                    return false;

                bool foundAssignmentSlot = false;

                for (int i = 0, c = assignments.Count; i < c; i++) {
                    int assignment = assignments[i], nextAssignment = int.MaxValue;
                    if (i < c - 1)
                        nextAssignment = assignments[i + 1];

                    if (
                        (targetFirstAssigned >= assignment) &&
                        (targetFirstAssigned < nextAssignment) &&
                        (targetFirstAccessed >= assignment) &&
                        (targetLastAccessed <= nextAssignment)
                    ) {
                        foundAssignmentSlot = true;
                        break;
                    }
                }

                if (!foundAssignmentSlot)
                    return false;

                return true;
            }

            // Attempt to handle the simple case where a variable is assigned a non-constant expression once and then used immediately
            if (!Assignments.TryGetValue(target, out assignments))
                return false;

            if (assignments.Count != 1)
                return false;

            List<int> accesses;
            if (!Accesses.TryGetValue(target, out accesses))
                return false;

            int firstAccess, lastAccess;
            if (!GetMinMax(accesses, out firstAccess, out lastAccess))
                return false;

            if (firstAccess == assignments.First() + 1)
                return true;

            return false;
        }

        public void VisitNode (JSFunctionExpression fn) {
            // Create a new visitor for nested function expressions
            if (Stack.OfType<JSFunctionExpression>().Skip(1).FirstOrDefault() != null) {
                var nested = new EliminateSingleUseTemporaries(TypeSystem, fn.AllVariables);
                nested.Visit(fn);
                return;
            }

            var nullList = new List<int>();

            VisitChildren(fn);

            bool mutated;
            do {
                mutated = false;

                foreach (var v in FirstValues.Keys.ToArray()) {
                    if (v.IsReference || v.IsThis || v.IsParameter)
                        continue;

                    var valueType = v.GetExpectedType(TypeSystem);
                    if (valueType.IsByReference || ILBlockTranslator.IsIgnoredType(valueType))
                        continue;

                    List<int> assignments;
                    List<int> accesses, copies, controlFlowAccesses, conversions;

                    if (!Assignments.TryGetValue(v, out assignments)) {
                        if (TraceLevel >= 2)
                            Debug.WriteLine(String.Format("Never found an initial assignment for {0}.", v));

                        continue;
                    }

                    if (ControlFlowAccesses.TryGetValue(v, out controlFlowAccesses) && (controlFlowAccesses.Count > 0)) {
                        if (TraceLevel >= 2)
                            Debug.WriteLine(String.Format("Cannot eliminate {0}; it participates in control flow.", v));

                        continue;
                    }

                    if (Conversions.TryGetValue(v, out conversions) && (conversions.Count > 0)) {
                        if (TraceLevel >= 2)
                            Debug.WriteLine(String.Format("Cannot eliminate {0}; it undergoes type conversion.", v));

                        continue;
                    }

                    if (assignments.Count > 1) {
                        if (TraceLevel >= 2)
                            Debug.WriteLine(String.Format("Cannot eliminate {0}; it is reassigned.", v));

                        continue;
                    }

                    if (!Accesses.TryGetValue(v, out accesses))
                        accesses = nullList;

                    if (!Copies.TryGetValue(v, out copies))
                        copies = nullList;

                    if ((copies.Count + accesses.Count) > 1) {
                        if (TraceLevel >= 2)
                            Debug.WriteLine(String.Format("Cannot eliminate {0}; it is used multiple times.", v));

                        continue;
                    }

                    var replacement = FirstValues[v].Expression;
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

                    FirstValues.Remove(v);

                    if (TraceLevel >= 1)
                        Debug.WriteLine(String.Format("Eliminating {0} <- {1}", v, replacement));

                    mutated = true;

                    var transferDataTo = replacement as JSVariable;
                    if (transferDataTo != null) {
                        foreach (var access in accesses)
                            AddToList(Accesses, transferDataTo, access);

                        foreach (var copy in copies)
                            AddToList(Copies, transferDataTo, copy);
                    }

                    EliminateVariable(fn, v, replacement);
                }
            } while (mutated);
        }

        protected bool GetMinMax (List<int> indices, out int min, out int max) {
            min = int.MaxValue;
            max = int.MinValue;

            if (indices.Count == 0)
                return false;

            min = indices.Min();
            max = indices.Max();

            return true;
        }

        protected bool GetMinMax (List<Tuple<int, int>> intervals, out int min, out int max) {
            min = int.MaxValue;
            max = int.MinValue;

            if (intervals.Count == 0)
                return false;

            min = intervals.Min((t) => Math.Min(t.Item1, t.Item2));
            max = intervals.Max((t) => Math.Max(t.Item1, t.Item2));

            return true;
        }

        protected IEnumerable<T> GetEnclosingNodes<T> (Func<T, bool> selector = null, Func<JSNode, bool> halter = null)
            where T : JSNode {

            foreach (var n in Stack) {
                var value = n as T;

                if (value != null) {
                    if ((selector == null) || selector(value))
                        yield return value;
                }

                if ((halter != null) && halter(value))
                    break;
            }
        }

        protected IEnumerable<T> GetChildNodes<T> (JSNode root, Func<T, bool> predicate = null)
            where T : JSNode {

            foreach (var n in root.AllChildrenRecursive) {
                var value = n as T;

                if (value != null) {
                    if ((predicate == null) || predicate(value))
                        yield return value;
                }
            }
        }

        protected void AddToList<T> (Dictionary<JSVariable, List<T>> dict, JSVariable variable, T index) {
            List<T> list;

            if (!dict.TryGetValue(variable, out list))
                dict[variable] = list = new List<T>();

            list.Add(index);
        }

        public void VisitNode (JSUnaryOperatorExpression uoe) {
            var variable = uoe.Expression as JSVariable;

            VisitChildren(uoe);

            if (
                (variable != null) && (
                    (uoe.Operator == JSOperator.PostDecrement) ||
                    (uoe.Operator == JSOperator.PostIncrement) ||
                    (uoe.Operator == JSOperator.PreDecrement) ||
                    (uoe.Operator == JSOperator.PreIncrement)
                )
            ) {

                if (TraceLevel >= 3)
                    Debug.WriteLine(String.Format(
                        "{0:0000} Reassigns: {1}\r\n{2}",
                        StatementIndex, variable, uoe
                    ));

                AddToList(Assignments, variable, StatementIndex);
            }
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var isAssignment = boe.Operator == JSOperator.Assignment;
            var leftVar = boe.Left as JSVariable;
            var rightVar = boe.Right as JSVariable;

            VisitChildren(boe);

            if ((leftVar != null) && isAssignment) {
                bool isFirst = false;

                if (!FirstValues.ContainsKey(leftVar)) {
                    isFirst = true;
                    FirstValues.Add(leftVar, new JSExpressionStatement(boe.Right));
                }

                AddToList(Assignments, leftVar, StatementIndex);

                if (TraceLevel >= 3)
                    Debug.WriteLine(String.Format(
                        "{0:0000} {2}: {1}\r\n{3}",
                        StatementIndex, leftVar, isFirst ? "Assigns" : "Reassigns", boe
                    ));

                if (rightVar != null) {
                    AddToList(Copies, rightVar, StatementIndex);

                    if (
                        !ILBlockTranslator.TypesAreEqual(
                            leftVar.GetExpectedType(TypeSystem),
                            rightVar.GetExpectedType(TypeSystem)
                        )
                    ) {
                        AddToList(Conversions, rightVar, StatementIndex);

                        if (TraceLevel >= 3)
                            Debug.WriteLine(String.Format(
                                "{0:0000} Converts: {1} into {2}\r\n{3}",
                                StatementIndex, rightVar, leftVar, boe
                            ));
                    } else {
                        if (TraceLevel >= 3)
                            Debug.WriteLine(String.Format(
                                "{0:0000} Copies: {1} into {2}\r\n{3}",
                                StatementIndex, rightVar, leftVar, boe
                            ));
                    }
                }
            }
        }

        public void VisitNode (JSVariable variable) {
            if (ParentNode is JSFunctionExpression) {
                // In argument list
                return;
            }

            var enclosingStatement = GetEnclosingNodes<JSStatement>().FirstOrDefault();
            var enclosingAssignmentStatements = GetEnclosingNodes<JSExpressionStatement>(
                (es) => {
                    var boe = es.Expression as JSBinaryOperatorExpression;
                    if (boe == null)
                        return false;

                    var isAssignment = boe.Operator == JSOperator.Assignment;
                    var leftIsVariable = boe.Left is JSVariable;
                    return isAssignment && leftIsVariable && 
                        (boe.Left.Equals(variable) || boe.Right.Equals(variable));
                }
            ).ToArray();

            if (enclosingAssignmentStatements.Length == 0) {
                if (TraceLevel >= 3)
                    Debug.WriteLine(String.Format(
                        "{0:0000} Accesses: {1}\r\n{2}",
                        StatementIndex, variable, enclosingStatement
                    ));

                if (enclosingStatement is JSExpressionStatement)
                    AddToList(Accesses, variable, StatementIndex);
                else
                    AddToList(ControlFlowAccesses, variable, StatementIndex);
            } else {
                if (TraceLevel >= 4)
                    Debug.WriteLine(String.Format(
                        "{0:0000} Ignoring Access: {1}\r\n{2}",
                        StatementIndex, variable, GetEnclosingNodes<JSStatement>().FirstOrDefault()
                    ));
            }

            VisitChildren(variable);
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
