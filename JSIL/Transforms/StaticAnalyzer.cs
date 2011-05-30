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
                (variable != null) && (uoe.Operator is JSUnaryMutationOperator)
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

            if (
                (boe.Left.AllChildrenRecursive.OfType<JSField>().FirstOrDefault() != null) ||
                (boe.Left.AllChildrenRecursive.OfType<JSProperty>().FirstOrDefault() != null)
            ) {
                foreach (var variable in boe.Right.AllChildrenRecursive.OfType<JSVariable>())
                    State.EscapingVariables.Add(variable);
            }
        }

        public void VisitNode (JSField field) {
            State.StaticReferences.Add(new FunctionAnalysis.StaticReference(
                StatementIndex, NodeIndex, field.Field.DeclaringType
            ));

            VisitChildren(field);
        }

        public void VisitNode (JSProperty property) {
            State.StaticReferences.Add(new FunctionAnalysis.StaticReference(
                StatementIndex, NodeIndex, property.Property.DeclaringType
            ));

            VisitChildren(property);
        }

        public void VisitNode (JSInvocationExpression ie) {
            foreach (var argument in ie.Arguments) {
                foreach (var variable in argument.AllChildrenRecursive.OfType<JSVariable>())
                    State.EscapingVariables.Add(variable);
            }

            var target = ie.Target as JSDotExpression;
            JSType type = null;
            JSMethod method = null;
            if (target != null) {
                type = target.Target as JSType;
                method = target.Member as JSMethod;
            }

            State.Invocations.Add(new FunctionAnalysis.Invocation(
                StatementIndex, NodeIndex, type, method
            ));

            VisitChildren(ie);
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
                bool isControlFlow = (enclosingStatement is JSIfStatement) || (enclosingStatement is JSWhileLoop) || 
                    (enclosingStatement is JSSwitchStatement) || (enclosingStatement is JSForLoop);

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
            public readonly JSVariable Target;

            public SideEffect (int statementIndex, int nodeIndex, JSVariable target = null)
                : base (statementIndex, nodeIndex) {
                Target = target;
            }
        }

        public class StaticReference : Item {
            public readonly TypeInfo Type;

            public StaticReference (int statementIndex, int nodeIndex, TypeInfo type)
                : base(statementIndex, nodeIndex) {
                Type = type;
            }
        }

        public class Invocation : Item {
            public readonly JSType Type;
            public readonly JSMethod Method;

            public Invocation (int statementIndex, int nodeIndex, JSType type, JSMethod method) 
                : base (statementIndex, nodeIndex) {
                Type = type;
                Method = method;
            }
        }

        public readonly JSFunctionExpression Function;
        public readonly List<Access> Accesses = new List<Access>();
        public readonly List<Assignment> Assignments = new List<Assignment>();
        public readonly List<SideEffect> SideEffects = new List<SideEffect>();
        public readonly HashSet<JSVariable> EscapingVariables = new HashSet<JSVariable>();
        public readonly List<StaticReference> StaticReferences = new List<StaticReference>();
        public readonly List<Invocation> Invocations = new List<Invocation>();

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
        public readonly bool IsPure;
        public readonly FunctionAnalysis Data;

        public FunctionStaticData (FunctionAnalysis data) {
            Data = data;
            IsPure = (data.SideEffects.Count == 0) &&
                (data.StaticReferences.Count == 0) &&
                (data.Invocations.Count == 0);

            Console.WriteLine("{0}: '{1}'", IsPure ? "Pure" : "Impure", data.Function.OriginalMethodReference.FullName);
            if (data.EscapingVariables.Count > 0)
                Console.WriteLine("  Escaping variables: {0}", String.Join(", ", (from v in data.EscapingVariables select v.Name).ToArray()));
        }

        public IEnumerable<JSVariable> AllVariables {
            get {
                return Data.Function.AllVariables.Values;
            }
        }
    }
}
