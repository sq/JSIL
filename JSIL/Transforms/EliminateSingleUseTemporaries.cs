using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Transforms {
    // This only works correctly for cases where a variable is assigned once and used once.
    // With a better algorithm it could detect and handle more sophisticated cases, but it's probably not worth it.
    public class EliminateSingleUseTemporaries : JSAstVisitor {
        public const int TraceLevel = 0;

        public readonly TypeSystem TypeSystem;
        public readonly Dictionary<string, JSVariable> Variables;
        public readonly Dictionary<JSVariable, JSChangeTypeExpression> FirstValues = new Dictionary<JSVariable, JSChangeTypeExpression>();
        public readonly Dictionary<JSVariable, List<int>> Assignments = new Dictionary<JSVariable, List<int>>();
        public readonly Dictionary<JSVariable, List<int>> Copies = new Dictionary<JSVariable, List<int>>();
        public readonly Dictionary<JSVariable, List<int>> Accesses = new Dictionary<JSVariable, List<int>>();
        public readonly Dictionary<JSVariable, List<int>> ControlFlowAccesses = new Dictionary<JSVariable, List<int>>();

        public EliminateSingleUseTemporaries (TypeSystem typeSystem, Dictionary<string, JSVariable> variables) {
            TypeSystem = typeSystem;
            Variables = variables;
        }

        protected void EliminateVariable (JSNode context, JSVariable variable, JSExpression replaceWith) {
            var replacer = new VariableEliminator(variable, replaceWith);

            replacer.Visit(context);

            foreach (var fv in FirstValues.Values)
                replacer.Visit(fv);

            Variables.Remove(variable.Identifier);
        }

        public void VisitNode (JSFunctionExpression fn) {
            var nullList = new List<int>();

            VisitChildren(fn);

            foreach (var v in FirstValues.Keys.ToArray()) {
                if (v.IsReference || v.IsThis || v.IsParameter)
                    continue;

                List<int> assignments, accesses, copies, controlFlowAccesses;

                if (!Assignments.TryGetValue(v, out assignments)) {
                    if (TraceLevel >= 1)
                        Debug.WriteLine(String.Format("Never found an initial assignment for {0}.", v));

                    continue;
                }

                if (ControlFlowAccesses.TryGetValue(v, out controlFlowAccesses) && (controlFlowAccesses.Count > 0)) {
                    if (TraceLevel >= 1)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it participates in control flow.", v));

                    continue;
                }

                if (assignments.Count > 1) {
                    if (TraceLevel >= 1)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it is reassigned.", v));

                    continue;
                }

                if (!Accesses.TryGetValue(v, out accesses))
                    accesses = nullList;

                if (!Copies.TryGetValue(v, out copies))
                    copies = nullList;

                if ((copies.Count + accesses.Count) > 1) {
                    if (TraceLevel >= 1)
                        Debug.WriteLine(String.Format("Cannot eliminate {0}; it is used multiple times.", v));

                    continue;
                }

                if (TraceLevel >= 1)
                    Debug.WriteLine(String.Format("Eliminating {0}", v));

                var replacement = FirstValues[v];
                FirstValues.Remove(v);
                EliminateVariable(fn, v, replacement);
            }
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

        protected void AddToList (Dictionary<JSVariable, List<int>> dict, JSVariable variable, int index) {
            List<int> list;

            if (!dict.TryGetValue(variable, out list))
                dict[variable] = list = new List<int>();

            list.Add(index);
        }

        public void VisitNode (JSUnaryOperatorExpression uoe) {
            var variable = uoe.Expression as JSVariable;

            if ((uoe.Operator == JSOperator.PostDecrement) ||
                (uoe.Operator == JSOperator.PostIncrement) ||
                (uoe.Operator == JSOperator.PreDecrement) ||
                (uoe.Operator == JSOperator.PreIncrement)) {

                if (TraceLevel >= 2)
                    Debug.WriteLine(String.Format(
                        "{0:0000} Reassigns: {1}\r\n{2}",
                        NodeIndex, variable, uoe
                    ));

                AddToList(Assignments, variable, NodeIndex);
            }

            VisitChildren(uoe);
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var isAssignment = boe.Operator == JSOperator.Assignment;
            var leftVar = boe.Left as JSVariable;
            var rightVar = boe.Right as JSVariable;

            if ((leftVar != null) && isAssignment) {
                bool isFirst = false;

                if (!FirstValues.ContainsKey(leftVar)) {
                    isFirst = true;
                    FirstValues.Add(leftVar, new JSChangeTypeExpression(boe.Right, leftVar.GetExpectedType(TypeSystem)));
                }

                AddToList(Assignments, leftVar, NodeIndex);

                if (TraceLevel >= 2)
                    Debug.WriteLine(String.Format(
                        "{0:0000} {2}: {1}\r\n{3}", 
                        NodeIndex, leftVar, isFirst ? "Assigns" : "Reassigns", boe
                    ));

                if (rightVar != null) {
                    AddToList(Copies, rightVar, NodeIndex);

                    if (TraceLevel >= 2)
                        Debug.WriteLine(String.Format(
                            "{0:0000} Copies: {1} into {2}\r\n{3}",
                            NodeIndex, rightVar, leftVar, boe
                        ));
                }
            }

            VisitChildren(boe);
        }

        public void VisitNode (JSVariable variable) {
            if (
                GetEnclosingNodes<JSBinaryOperatorExpression>(
                    (boe) => {
                        var isAssignment = boe.Operator == JSOperator.Assignment;
                        return isAssignment && (boe.Left.Equals(variable) || boe.Right.Equals(variable));
                    },
                    (n) => (n is JSStatement) || (n is JSBinaryOperatorExpression)
                ).Count() == 0
            ) {
                var enclosingStatement = GetEnclosingNodes<JSStatement>().FirstOrDefault();

                if (TraceLevel >= 2)
                    Debug.WriteLine(String.Format(
                        "{0:0000} Accesses: {1}\r\n{2}",
                        NodeIndex, variable, enclosingStatement
                    ));

                if (enclosingStatement is JSExpressionStatement)
                    AddToList(Accesses, variable, NodeIndex);
                else
                    AddToList(ControlFlowAccesses, variable, NodeIndex);
            } else {
                if (TraceLevel >= 3)
                    Debug.WriteLine(String.Format(
                        "{0:0000} Ignoring Access: {1}\r\n{2}",
                        NodeIndex, variable, GetEnclosingNodes<JSStatement>().FirstOrDefault()
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
            if (Variable.Equals(variable)) {
                ParentNode.ReplaceChild(variable, Replacement);
            } else {
                VisitChildren(variable);
            }
        }
    }
}
