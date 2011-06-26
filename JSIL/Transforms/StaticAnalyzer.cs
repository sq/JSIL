using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public interface IFunctionSource {
        JSFunctionExpression GetExpression (JSMethod method);
        FunctionStaticData GetStaticData (JSFunctionExpression function);
        FunctionStaticData GetStaticData (JSMethod method);
    }

    public class StaticAnalyzer : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly IFunctionSource FunctionSource;

        protected FunctionAnalysis State;

        public StaticAnalyzer (TypeSystem typeSystem, IFunctionSource functionSource) {
            TypeSystem = typeSystem;
            FunctionSource = functionSource;
        }

        public FunctionStaticData Analyze (JSFunctionExpression function) {
            State = new FunctionAnalysis(function);

            Visit(function);

            State.Accesses.Sort(FunctionAnalysis.ItemComparer);
            State.Assignments.Sort(FunctionAnalysis.ItemComparer);
            State.SideEffects.Sort(FunctionAnalysis.ItemComparer);

            return new FunctionStaticData(FunctionSource, State);
        }

        public void VisitNode (JSFunctionExpression fn) {
            /*
            // Do not analyze nested function expressions
            if (Stack.OfType<JSFunctionExpression>().Skip(1).FirstOrDefault() != null)
                return;
             */

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
                var leftVars = new HashSet<JSVariable>(boe.Left.AllChildrenRecursive.OfType<JSVariable>());
                var rightVars = new HashSet<JSVariable>(boe.Right.AllChildrenRecursive.OfType<JSVariable>());

                foreach (var variable in rightVars.Except(leftVars))
                    State.EscapingVariables.Add(variable.Identifier);
            }
        }

        public void VisitNode (JSDotExpression dot) {
            var field = dot.Member as JSField;
            var property = dot.Member as JSProperty;

            if (dot.IsStatic) {
                if (field != null) {
                    State.StaticReferences.Add(new FunctionAnalysis.StaticReference(
                        StatementIndex, NodeIndex, field.Field.DeclaringType
                    ));
                } else if (property != null) {
                    State.StaticReferences.Add(new FunctionAnalysis.StaticReference(
                        StatementIndex, NodeIndex, property.Property.DeclaringType
                    ));
                }
            }

            VisitChildren(dot);
        }

        public void VisitNode (JSInvocationExpression ie) {
            var variables = new Dictionary<string, string[]>();

            int i = 0;
            foreach (var kvp in ie.Parameters) {
                var value = (from v in kvp.Value.AllChildrenRecursive.OfType<JSVariable>() select v.Name).ToArray();
                if (kvp.Key == null)
                    variables.Add(String.Format("#{0}", i++), value);
                else
                    variables.Add(kvp.Key.Name, value);
            }

            var type = ie.JSType;
            var method = ie.JSMethod;

            State.Invocations.Add(new FunctionAnalysis.Invocation(
                StatementIndex, NodeIndex, type, method, variables
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
                bool isControlFlow = (enclosingStatement is JSIfStatement) || (enclosingStatement is JSSwitchStatement);

                var enclosingBlock = enclosingStatement as JSBlockStatement;
                if (enclosingBlock != null)
                    isControlFlow |= enclosingBlock.IsLoop;

                State.Accesses.Add(
                    new FunctionAnalysis.Access(
                        StatementIndex, NodeIndex,
                        variable, isControlFlow
                    )
                );
            } else {
                // Ignored because it is not an actual access
            }

            if ((ParentNode is JSPassByReferenceExpression) || (ParentNode is JSReferenceExpression))
                State.VariablesPassedByRef.Add(variable.Name);

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
            public readonly IDictionary<string, string[]> Variables;

            public Invocation (int statementIndex, int nodeIndex, JSType type, JSMethod method, IDictionary<string, string[]> variables) 
                : base (statementIndex, nodeIndex) {
                Type = type;
                Method = method;
                Variables = variables;
            }
        }

        public readonly JSFunctionExpression Function;
        public readonly List<Access> Accesses = new List<Access>();
        public readonly List<Assignment> Assignments = new List<Assignment>();
        public readonly List<SideEffect> SideEffects = new List<SideEffect>();
        public readonly HashSet<string> VariablesPassedByRef = new HashSet<string>();
        public readonly HashSet<string> ModifiedVariables = new HashSet<string>();
        public readonly HashSet<string> EscapingVariables = new HashSet<string>();
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
        public const bool Tracing = false;

        protected readonly bool _IsPure;

        public readonly HashSet<string> ModifiedVariables = new HashSet<string>();
        public readonly HashSet<string> EscapingVariables = new HashSet<string>();

        public readonly IFunctionSource FunctionSource;
        public readonly FunctionAnalysis Data;

        public FunctionStaticData (IFunctionSource functionSource, FunctionAnalysis data) {
            FunctionSource = functionSource;
            Data = data;
            _IsPure = (data.SideEffects.Count == 0) &&
                (data.StaticReferences.Count == 0);

            Trace(data.Function.OriginalMethodReference.FullName);
        }

        public FunctionStaticData (IFunctionSource functionSource, MethodInfo method) {
            if (!method.IsExternal)
                throw new InvalidOperationException();

            FunctionSource = functionSource;
            Data = null;
            _IsPure = method.Metadata.HasAttribute("JSIL.Meta.JSIsPure");
            
            var parms = method.Metadata.GetAttributeParameters("JSIL.Meta.JSMutatedArguments");
            foreach (var p in parms) {
                var s = p.Value as string;
                if (s != null)
                    ModifiedVariables.Add(s);
            }

            parms = method.Metadata.GetAttributeParameters("JSIL.Meta.JSEscapingArguments");
            foreach (var p in parms) {
                var s = p.Value as string;
                if (s != null)
                    EscapingVariables.Add(s);
            }

            Trace(method.Member.FullName);
        }

        public bool IsPure {
            get {
                if (Data == null)
                    return _IsPure;

                foreach (var i in Data.Invocations) {
                    var sd = FunctionSource.GetStaticData(i.Method);
                    if (sd == null)
                        return false;

                    if (!sd.IsPure)
                        return false;
                }

                return _IsPure;
            }
        }

        protected void Trace (string name) {
            if (Tracing) {
                Console.WriteLine("{0}", name);
                if (ModifiedVariables.Count > 0)
                    Console.WriteLine("  Modified variables: {0}", String.Join(", ", ModifiedVariables.ToArray()));
                if (EscapingVariables.Count > 0)
                    Console.WriteLine("  Escaping variables: {0}", String.Join(", ", EscapingVariables.ToArray()));
            }
        }
    }
}
