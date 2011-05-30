using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class StaticAnalyzer : JSAstVisitor {
        public readonly TypeSystem TypeSystem;

        protected FunctionAnalysis State;

        public StaticAnalyzer (TypeSystem typeSystem) {
            TypeSystem = typeSystem;
        }

        public FunctionStaticData Analyze (JSFunctionExpression function) {
            State = new FunctionAnalysis(function);

            Visit(function);

            State.Accesses.Sort(FunctionAnalysis.ItemComparer);
            State.Assignments.Sort(FunctionAnalysis.ItemComparer);
            State.SideEffects.Sort(FunctionAnalysis.ItemComparer);

            return new FunctionStaticData(State);
        }

        public void VisitNode (JSFunctionExpression fn) {
            // Do not analyze nested function expressions
            if (Stack.OfType<JSFunctionExpression>().Skip(1).FirstOrDefault() != null)
                return;

            VisitChildren(fn);
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

        public static IEnumerable<T> GetChildNodes<T> (JSNode root, Func<T, bool> predicate = null)
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
                State.Assignments.Add(
                    new FunctionAnalysis.Assignment(
                        StatementIndex, NodeIndex, 
                        variable, uoe, uoe.Operator,
                        variable.GetExpectedType(TypeSystem), uoe.GetExpectedType(TypeSystem)
                    )
                );
            }
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var isAssignment = boe.Operator is JSAssignmentOperator;
            var leftVar = boe.Left as JSVariable;

            VisitChildren(boe);

            if ((leftVar != null) && isAssignment) {
                bool isFirst = false;
                var leftType = boe.Left.GetExpectedType(TypeSystem);
                var rightType = boe.Right.GetExpectedType(TypeSystem);

                State.Assignments.Add(
                    new FunctionAnalysis.Assignment(
                        StatementIndex, NodeIndex,
                        leftVar, boe.Right, boe.Operator, 
                        leftType, rightType
                    )                    
                );
            }
        }

        public void VisitNode (JSTryCatchBlock tcb) {
            if (tcb.CatchVariable != null) {
                State.Assignments.Add(
                    new FunctionAnalysis.Assignment(
                        StatementIndex, NodeIndex,
                        tcb.CatchVariable, new JSNullExpression(), JSOperator.Assignment,
                        tcb.CatchVariable.Type, tcb.CatchVariable.Type
                    )
                );
            }

            VisitChildren(tcb);
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

            if ((enclosingAssignmentStatements.Length == 0) && (
                !(enclosingStatement is JSTryCatchBlock) &&
                !(enclosingStatement is JSVariableDeclarationStatement)
            )) {
                bool isControlFlow = (enclosingStatement is JSIfStatement) || (enclosingStatement is JSWhileLoop) || (enclosingStatement is JSSwitchStatement);

                State.Accesses.Add(
                    new FunctionAnalysis.Access(
                        StatementIndex, NodeIndex,
                        variable, isControlFlow
                    )
                );
            } else {
                // Ignored because it is not an actual access
            }

            VisitChildren(variable);
        }
    }

    public class FunctionAnalysis {
        public class Item {
            public readonly int StatementIndex;
            public readonly int NodeIndex;

            public Item (int statementIndex, int nodeIndex) {
                StatementIndex = statementIndex;
                NodeIndex = nodeIndex;
            }
        }

        public class Access : Item {
            public readonly JSVariable Source;
            public readonly bool IsControlFlow;

            public Access (int statementIndex, int nodeIndex, JSVariable source, bool isControlFlow)
                : base (statementIndex, nodeIndex) {
                Source = source;
                IsControlFlow = isControlFlow;
            }

            public override string ToString () {
                if (IsControlFlow)
                    return String.Format("ControlFlow {0}", Source);
                else
                    return Source.ToString();
            }
        }

        public class Assignment : Item {
            public readonly int StatementIndex;
            public readonly int NodeIndex;
            public readonly JSVariable Target;
            public readonly JSExpression NewValue;
            public readonly JSVariable SourceVariable;
            public readonly JSOperator Operator;
            public readonly TypeReference SourceType, TargetType;
            public readonly bool IsConversion;

            public Assignment (
                int statementIndex, int nodeIndex, 
                JSVariable target, JSExpression newValue,
                JSOperator @operator,
                TypeReference targetType, TypeReference sourceType
            ) : base (statementIndex, nodeIndex) {
                Target = target;
                NewValue = newValue;
                SourceVariable = newValue as JSVariable;
                SourceType = sourceType;
                TargetType = targetType;
                Operator = @operator;
                IsConversion = !ILBlockTranslator.TypesAreEqual(targetType, sourceType);
            }

            public override string ToString () {
                return String.Format("{0} {1} {2}", Target, Operator, NewValue);
            }
        }

        public class SideEffect : Item {
            public readonly int StatementIndex;
            public readonly int NodeIndex;
            public readonly JSVariable Target;

            public SideEffect (int statementIndex, int nodeIndex, JSVariable target = null)
                : base (statementIndex, nodeIndex) {
                Target = target;
            }
        }

        public readonly JSFunctionExpression Function;
        public readonly List<Access> Accesses = new List<Access>();
        public readonly List<Assignment> Assignments = new List<Assignment>();
        public readonly List<SideEffect> SideEffects = new List<SideEffect>();

        public FunctionAnalysis (JSFunctionExpression function) {
            Function = function;
        }

        public static int ItemComparer (Item lhs, Item rhs) {
            var result = lhs.StatementIndex.CompareTo(rhs.StatementIndex);
            if (result == 0)
                result = lhs.NodeIndex.CompareTo(rhs.NodeIndex);

            return result;
        }
    }

    public class FunctionStaticData {
        public readonly bool HasSideEffects;
        public readonly FunctionAnalysis Data;
        // TODO: public readonly HashSet<JSVariable> EscapingVariables = new HashSet<JSVariable>();

        public FunctionStaticData (FunctionAnalysis data) {
            Data = data;
            HasSideEffects = data.SideEffects.Count > 0;
        }

        public IEnumerable<JSVariable> AllVariables {
            get {
                return Data.Function.AllVariables.Values;
            }
        }
    }
}
