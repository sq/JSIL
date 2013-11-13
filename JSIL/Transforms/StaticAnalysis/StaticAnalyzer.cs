using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class StaticAnalyzer : JSAstVisitor {
        public struct EnclosingNode<T> {
            public T Node;
            public string Name;
            public JSNode Child;
            public string ChildName;
        }

        public readonly TypeSystem TypeSystem;
        public readonly FunctionCache FunctionSource;

        protected int ReturnsSeen = 0;

        protected FunctionAnalysis1stPass State;

        public StaticAnalyzer (TypeSystem typeSystem, FunctionCache functionSource) {
            TypeSystem = typeSystem;
            FunctionSource = functionSource;
        }

        public FunctionAnalysis1stPass FirstPass (QualifiedMemberIdentifier identifier, JSFunctionExpression function) {
            State = new FunctionAnalysis1stPass(identifier, function);

            Visit(function);

            State.Accesses.Sort(FunctionAnalysis1stPass.ItemComparer);
            State.Assignments.Sort(FunctionAnalysis1stPass.ItemComparer);

            var result = State;
            State = null;

            if (false) {
                var bg = new StaticAnalysis.BarrierGenerator(TypeSystem, function);
                bg.Generate();

                var targetFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Barriers"
                );
                Directory.CreateDirectory(targetFolder);

                var typeName = function.Method.QualifiedIdentifier.Type.ToString();
                var methodName = function.Method.Method.Name;

                if (typeName.Length >= 96)
                    typeName = typeName.Substring(0, 93) + "…";

                if (methodName.Length >= 32)
                    methodName = methodName.Substring(0, 29) + "…";

                var filename = String.Format("{0}.{1}", typeName, methodName);

                filename = filename.Replace("<", "").Replace(">", "").Replace("/", "");

                var targetFile = Path.Combine(
                    targetFolder,
                    String.Format("{0}.xml", filename)
                );

                bg.SaveXML(targetFile);
            }

            return result;
        }

        private int[] GetParentNodeIndices () {
            return NodeIndexStack.ToArray();
        }

        public void VisitNode (JSReturnExpression ret) {
            ReturnsSeen++;

            var returnValue = ret.Value;
            JSResultReferenceExpression rre;

            while ((rre = returnValue as JSResultReferenceExpression) != null) {
                returnValue = rre.Referent;
            }

            var retInvocation = returnValue as JSInvocationExpression;
            if (retInvocation != null) {
                State.ResultVariable = null;
                State.ResultIsNew = false;

                if (retInvocation.JSMethod != null) {
                    if (ReturnsSeen == 1)
                        State.ResultMethod = retInvocation.JSMethod;
                    else if (retInvocation.JSMethod != State.ResultMethod)
                        State.ResultMethod = null;
                } else {
                    State.ResultMethod = null;
                }

            } else {
                var retVar = returnValue as JSVariable;
                if (retVar != null) {
                    State.EscapingVariables.Add(retVar.Identifier);

                    if (ReturnsSeen == 1)
                        State.ResultVariable = retVar.Identifier;
                    else if (State.ResultVariable != retVar.Identifier)
                        State.ResultVariable = null;
                } else {
                    State.ResultVariable = null;
                }

                var retNew = returnValue as JSNewExpression;
                if (ReturnsSeen == 1)
                    State.ResultIsNew = (retNew != null);
                else
                    State.ResultIsNew &= (retNew != null);
            }

            VisitChildren(ret);
        }

        public void VisitNode (JSFunctionExpression fn) {
            VisitChildren(fn);
        }

        protected IEnumerable<EnclosingNode<T>> GetEnclosingNodes<T> (Func<T, bool> selector = null, Func<JSNode, bool> halter = null)
            where T : JSNode {

            JSNode previous = null;
            string previousName = null;

            // Fuck the C# compiler and its busted enumerator transform
            // https://connect.microsoft.com/VisualStudio/feedback/details/781746/c-compiler-produces-incorrect-code-for-use-of-enumerator-structs-inside-enumerator-functions
            using (var eNodes = (IEnumerator<JSNode>)Stack.GetEnumerator())
            using (var eNames = (IEnumerator<string>)NameStack.GetEnumerator())
            while (eNodes.MoveNext() && eNames.MoveNext()) {
                var value = eNodes.Current as T;
                var name = eNames.Current;

                if (value == null) {
                    previous = eNodes.Current;
                    previousName = name;
                    continue;
                }

                if ((selector == null) || selector(value)) {
                    yield return new EnclosingNode<T> {
                        Node = value,
                        Child = previous,
                        ChildName = previousName
                    };
                }

                if ((halter != null) && halter(value))
                    yield break;

                previous = eNodes.Current;
                previousName = name;
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

        protected void ModifiedVariable (JSVariable variable) {
            if (!State.ModificationCount.ContainsKey(variable.Name))
                State.ModificationCount[variable.Name] = 1;
            else
                State.ModificationCount[variable.Name] += 1;
        }

        protected JSVariable ExtractAffectedVariable (JSExpression expression) {
            var variable = expression as JSVariable;

            if (variable != null)
                return variable;

            JSDotExpressionBase dot = expression as JSDotExpressionBase;
            while (dot != null) {
                variable = dot.Target as JSVariable;
                if (variable != null)
                    return variable;

                dot = dot.Target as JSDotExpressionBase;
            }

            return null;
        }

        public void VisitNode (JSUnaryOperatorExpression uoe) {
            var variable = ExtractAffectedVariable(uoe.Expression);
            var isMutator = uoe.Operator is JSUnaryMutationOperator;

            VisitChildren(uoe);

            if (isMutator) {
                if (variable != null) {
                    State.Assignments.Add(
                        new FunctionAnalysis1stPass.Assignment(
                            GetParentNodeIndices(), StatementIndex, NodeIndex,
                            variable, uoe, uoe.Operator,
                            variable.GetActualType(TypeSystem), uoe.GetActualType(TypeSystem)
                        )
                    );

                    ModifiedVariable(variable);
                }
            }
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var isAssignment = boe.Operator is JSAssignmentOperator;

            var left = boe.Left;
            // If the LHS is a reference expression, climb through the reference(s) to find the actual target.
            while (left is JSReferenceExpression) {
                left = ((JSReferenceExpression)left).Referent;
            }

            var leftIsNested = false;

            var leftDot = left as JSDotExpressionBase;
            // If the LHS is one or more nested dot expressions, unnest them (leftward) to find the root variable.
            while (leftDot != null) {
                leftIsNested = true;

                if (leftDot.Target is JSDotExpressionBase)
                    leftDot = (JSDotExpressionBase)leftDot.Target;
                else
                    break;
            }

            var leftVar = ExtractAffectedVariable(left);

            VisitChildren(boe);

            if (isAssignment) {
                if (leftVar != null) {
                    // If we found the variable by un-nesting dot expressions, then it's affected, but not assigned.
                    if (!leftIsNested) {
                        if ((left == leftVar) && leftVar.IsThis)
                            State.ReassignsThisReference = true;

                        var leftType = left.GetActualType(TypeSystem);
                        var rightType = boe.Right.GetActualType(TypeSystem);

                        State.Assignments.Add(
                            new FunctionAnalysis1stPass.Assignment(
                                GetParentNodeIndices(), StatementIndex, NodeIndex,
                                leftVar, boe.Right, boe.Operator,
                                leftType, rightType
                            )
                        );
                    }

                    ModifiedVariable(leftVar);
                }

                if (
                    (boe.Left.SelfAndChildrenRecursive.OfType<JSField>().FirstOrDefault() != null) ||
                    (boe.Left.SelfAndChildrenRecursive.OfType<JSProperty>().FirstOrDefault() != null)
                ) {
                    var rightVars = new HashSet<JSVariable>(boe.Right.SelfAndChildrenRecursive.OfType<JSVariable>());

                    foreach (var variable in rightVars)
                        State.EscapingVariables.Add(variable.Identifier);
                }
            }
        }

        public void VisitNode (JSIndexerExpression ie) {
            var v = ExtractAffectedVariable(ie.Target);
            var enclosingBoe = GetEnclosingNodes<JSBinaryOperatorExpression>((boe) => boe.Operator is JSAssignmentOperator).FirstOrDefault();

            if (
                (v != null) &&
                (enclosingBoe.Node != null) &&
                (enclosingBoe.ChildName == "Left")
            ) {
                var parentNodeIndices = GetParentNodeIndices();

                State.SideEffects.Add(new FunctionAnalysis1stPass.SideEffect(
                    parentNodeIndices, StatementIndex, NodeIndex, v, "element modified"
                ));
            } else {
                ;
            }

            VisitChildren(ie);
        }

        public void VisitNode (JSFieldAccess fa) {
            var field = fa.Field;
            var v = ExtractAffectedVariable(fa.ThisReference);

            var parentNodeIndices = GetParentNodeIndices();

            if (fa.HasGlobalStateDependency) {
                State.StaticReferences.Add(new FunctionAnalysis1stPass.StaticReference(
                    parentNodeIndices, StatementIndex, NodeIndex, field.Field.DeclaringType
                ));
            } else if (v != null) {
                State.SideEffects.Add(new FunctionAnalysis1stPass.SideEffect(
                    parentNodeIndices, StatementIndex, NodeIndex, v, "field modified"
                ));
            }

            bool isRead = true;

            var enclosingBoe = GetEnclosingNodes<JSBinaryOperatorExpression>((boe) => boe.Operator is JSAssignmentOperator).FirstOrDefault();
            var enclosingByRef = GetEnclosingNodes<JSPassByReferenceExpression>().FirstOrDefault();
            var enclosingInvocation = GetEnclosingNodes<JSInvocationExpressionBase>().FirstOrDefault();
            if (enclosingBoe.Node != null) {
                if (enclosingBoe.ChildName == "Left")
                    isRead = false;
            } else if (enclosingByRef.Node != null) {
                isRead = false;
            } else if (enclosingInvocation.Node != null) {
                if (enclosingInvocation.ChildName == "ThisReference")
                    isRead = false;
            }

            State.FieldAccesses.Add(new FunctionAnalysis1stPass.FieldAccess(
                parentNodeIndices, StatementIndex, NodeIndex,
                field, isRead
            ));

            VisitChildren(fa);
        }

        public void VisitNode (JSPropertyAccess prop) {
            var parentBoe = ParentNode as JSBinaryOperatorExpression;
            var v = ExtractAffectedVariable(prop.Target);
            var p = prop.Property.Property;

            if (prop.HasGlobalStateDependency) {
                State.StaticReferences.Add(new FunctionAnalysis1stPass.StaticReference(
                    GetParentNodeIndices(), StatementIndex, NodeIndex, p.DeclaringType
                ));
            }

            if (
                (parentBoe != null) && 
                (parentBoe.Operator is JSAssignmentOperator) && 
                (parentBoe.Left == prop)
            ) {
                // Setter
                if (v != null) {
                    State.SideEffects.Add(new FunctionAnalysis1stPass.SideEffect(
                        GetParentNodeIndices(), StatementIndex, NodeIndex, v, "property set"
                    ));
                }
                /*
                State.Invocations.Add(new FunctionAnalysis1stPass.Invocation(
                    StatementIndex, NodeIndex, 
                 */
            } else {
                // Getter
            }

            VisitChildren(prop);
        }

        public void VisitNode (JSVerbatimLiteral verbatim) {
            if (verbatim.Variables != null) {
                var variables = new Dictionary<string, JSVariable>();

                foreach (var kvp in verbatim.Variables) {
                    if (kvp.Value == null)
                        continue;

                    foreach (var v in kvp.Value.SelfAndChildrenRecursive.OfType<JSVariable>()) {
                        if (!variables.ContainsKey(v.Name))
                            variables[v.Name] = v;
                    }
                }

                foreach (var variable in variables.Values) {
                    ModifiedVariable(variable);
                    State.EscapingVariables.Add(variable.Name);
                }
            }

            VisitChildren(verbatim);
        }

        private Dictionary<string, string[]> ExtractAffectedVariables (JSExpression method, IEnumerable<KeyValuePair<ParameterDefinition, JSExpression>> parameters) {
            var variables = new Dictionary<string, string[]>();

            int i = 0;
            foreach (var kvp in parameters) {
                var value = (from v in kvp.Value.SelfAndChildrenRecursive.OfType<JSVariable>() select v.Name).ToArray();

                if ((kvp.Key == null) || String.IsNullOrWhiteSpace(kvp.Key.Name)) {
                    variables.Add(String.Format("#{0}", i++), value);
                } else {
                    if (
                        variables.ContainsKey(kvp.Key.Name)
                    ) {
                        if (kvp.Key.CustomAttributes.Any((ca) => ca.AttributeType.Name == "ParamArrayAttribute"))
                            variables[kvp.Key.Name] = variables[kvp.Key.Name].Concat(value).ToArray();
                        else
                            throw new InvalidDataException(String.Format(
                                "Multiple parameters named '{0}' for invocation of '{1}'. Parameter list follows: '{2}'",
                                kvp.Key.Name, method,
                                String.Join(", ", parameters)
                            ));
                    } else {
                        variables.Add(kvp.Key.Name, value);
                    }
                }
            }

            return variables;
        }

        public void VisitNode (JSInvocationExpression ie) {
            var variables = ExtractAffectedVariables(ie.Method, ie.Parameters);

            var type = ie.JSType;
            var thisVar = ExtractAffectedVariable(ie.ThisReference);
            var method = ie.JSMethod;

            if (thisVar != null) {
                ModifiedVariable(thisVar);

                State.Invocations.Add(new FunctionAnalysis1stPass.Invocation(
                    GetParentNodeIndices(), StatementIndex, NodeIndex, thisVar, method, ie.Method, variables
                ));
            } else {
                State.Invocations.Add(new FunctionAnalysis1stPass.Invocation(
                    GetParentNodeIndices(), StatementIndex, NodeIndex, type, method, ie.Method, variables
                ));
            }

            VisitChildren(ie);
        }

        public void VisitNode (JSNewExpression newexp) {
            if ((newexp.ConstructorReference != null) && (newexp.Constructor != null)) {
                var jsm = new JSMethod(newexp.ConstructorReference, newexp.Constructor, FunctionSource.MethodTypes);
                var variables = ExtractAffectedVariables(jsm, newexp.Parameters);

                State.Invocations.Add(new FunctionAnalysis1stPass.Invocation(
                    GetParentNodeIndices(), StatementIndex, NodeIndex, (JSVariable)null, jsm, newexp.ConstructorReference, variables
                ));
            }

            VisitChildren(newexp);
        }

        public void VisitNode (JSTryCatchBlock tcb) {
            if (tcb.CatchVariable != null) {
                State.Assignments.Add(
                    new FunctionAnalysis1stPass.Assignment(
                        GetParentNodeIndices(), StatementIndex, NodeIndex,
                        tcb.CatchVariable, new JSNullExpression(), JSOperator.Assignment,
                        tcb.CatchVariable.IdentifierType, tcb.CatchVariable.IdentifierType
                    )
                );

                ModifiedVariable(tcb.CatchVariable);
            }

            VisitChildren(tcb);
        }

        public void VisitNode (JSVariable variable) {
            if (CurrentName == "FunctionSignature") {
                // In argument list
                VisitChildren(variable);
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
                !(enclosingStatement.Node is JSVariableDeclarationStatement)
            )) {
                bool isControlFlow =
                    (
                        (enclosingStatement.Node is JSIfStatement) &&
                        (enclosingStatement.ChildName != "Condition")
                    ) || (
                        (enclosingStatement.Node is JSSwitchStatement) &&
                        (enclosingStatement.ChildName != "Condition")
                    );

                // Don't do the condition check here since a loop's condition can be evaluated multiple times
                var enclosingBlock = enclosingStatement.Node as JSBlockStatement;
                if (enclosingBlock != null)
                    isControlFlow |= enclosingBlock is JSLoopStatement;

                State.Accesses.Add(
                    new FunctionAnalysis1stPass.Access(
                        GetParentNodeIndices(), StatementIndex, NodeIndex,
                        variable, isControlFlow
                    )
                );
            } else {
                // Ignored because it is not an actual access
            }

            if (
                (ParentNode is JSPassByReferenceExpression) ||
                (
                    (ParentNode is JSReferenceExpression) &&
                    (Stack.Skip(2).FirstOrDefault() is JSPassByReferenceExpression)
                )
            ) {
                State.VariablesPassedByRef.Add(variable.Name);
            }

            VisitChildren(variable);
        }
    }

    public class FunctionAnalysis1stPass {
        public class Item {
            public readonly int[] ParentNodeIndices;
            public readonly int StatementIndex;
            public readonly int NodeIndex;

            public Item (int[] parentNodeIndices, int statementIndex, int nodeIndex) {
                ParentNodeIndices = parentNodeIndices;
                StatementIndex = statementIndex;
                NodeIndex = nodeIndex;
            }
        }

        public class Access : Item {
            public readonly JSVariable Source;
            public readonly bool IsControlFlow;

            public Access (
                int[] parentNodeIndices, int statementIndex, int nodeIndex,
                JSVariable source, bool isControlFlow
            ) : base (parentNodeIndices, statementIndex, nodeIndex) { 
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
                int[] parentNodeIndices, int statementIndex, int nodeIndex, 
                JSVariable target, JSExpression newValue,
                JSOperator @operator,
                TypeReference targetType, TypeReference sourceType
            ) : base (parentNodeIndices, statementIndex, nodeIndex) {
                Target = target;
                NewValue = newValue;
                SourceVariable = newValue as JSVariable;
                SourceType = sourceType;
                TargetType = targetType;
                Operator = @operator;
                IsConversion = !TypeUtil.TypesAreEqual(targetType, sourceType);
            }

            public override string ToString () {
                return String.Format("{0} {1} {2}", Target, Operator, NewValue);
            }
        }

        public class StaticReference : Item {
            public readonly TypeInfo Type;

            public StaticReference (
                int[] parentNodeIndices, int statementIndex, int nodeIndex, 
                TypeInfo type
            ) : base(parentNodeIndices, statementIndex, nodeIndex) {
                Type = type;
            }
        }

        public class SideEffect : Item {
            public readonly JSVariable Variable;
            public readonly string Type;

            public SideEffect (
                int[] parentNodeIndices, int statementIndex, int nodeIndex, 
                JSVariable variable, string type
            ) : base (parentNodeIndices, statementIndex, nodeIndex) {
                Variable = variable;
                Type = type;
            }
        }

        public class FieldAccess : Item {
            public readonly JSField Field;
            public readonly bool IsRead;

            public FieldAccess (
                int[] parentNodeIndices, int statementIndex, int nodeIndex,
                JSField field, bool isRead
            ) : base(parentNodeIndices, statementIndex, nodeIndex) {
                Field = field;
                IsRead = isRead;
            }
        }

        public class Invocation : Item {
            public readonly JSType ThisType;
            public readonly string ThisVariable;
            public readonly JSMethod Method;
            public readonly object NonJSMethod;
            public readonly IDictionary<string, string[]> Variables;

            public Invocation (
                int[] parentNodeIndices, int statementIndex, int nodeIndex, 
                JSType type, JSMethod method, object nonJSMethod,
                IDictionary<string, string[]> variables
            )
                : base(parentNodeIndices, statementIndex, nodeIndex) {
                ThisType = type;
                ThisVariable = null;
                Method = method;
                if (method == null)
                    NonJSMethod = nonJSMethod;
                else
                    NonJSMethod = null;
                Variables = variables;
            }

            public Invocation (
                int[] parentNodeIndices, int statementIndex, int nodeIndex,
                JSVariable thisVariable, JSMethod method, object nonJSMethod,
                IDictionary<string, string[]> variables
            ) : base(parentNodeIndices, statementIndex, nodeIndex) {
                if (thisVariable != null)
                    ThisVariable = thisVariable.Identifier;
                else
                    ThisVariable = null;

                ThisType = null;
                Method = method;
                if (method == null)
                    NonJSMethod = nonJSMethod;
                else
                    NonJSMethod = null;
                Variables = variables;
            }
        }

        public readonly QualifiedMemberIdentifier Identifier;
        public readonly JSFunctionExpression Function;
        public readonly List<Access> Accesses = new List<Access>();
        public readonly List<Assignment> Assignments = new List<Assignment>();
        public readonly HashSet<string> VariablesPassedByRef = new HashSet<string>();
        public readonly Dictionary<string, int> ModificationCount = new Dictionary<string, int>();
        public readonly HashSet<string> EscapingVariables = new HashSet<string>();
        public readonly List<SideEffect> SideEffects = new List<SideEffect>();
        public readonly List<StaticReference> StaticReferences = new List<StaticReference>();
        public readonly List<Invocation> Invocations = new List<Invocation>();
        public readonly List<FieldAccess> FieldAccesses = new List<FieldAccess>();

        // If not null, this method's return value is always the result of a call to a particular method.
        public JSMethod ResultMethod = null;
        // If not null, this method's return value is always a particular variable.
        public string ResultVariable = null;
        // If true, this method's return value is always a 'new' expression.
        public bool ResultIsNew = false;
        // If true, somewhere within the body of the method, the this-reference is reassigned (only valid for structs).
        public bool ReassignsThisReference = false;

        public FunctionAnalysis1stPass (QualifiedMemberIdentifier identifier, JSFunctionExpression function) {
            Identifier = identifier;
            Function = function;
        }

        public static int ItemComparer (Item lhs, Item rhs) {
            var result = lhs.StatementIndex.CompareTo(rhs.StatementIndex);
            if (result == 0)
                result = lhs.NodeIndex.CompareTo(rhs.NodeIndex);

            return result;
        }
    }

    public class FunctionAnalysis2ndPass {
        public const bool TraceModifications = false;
        public const bool TraceEscapes = false;
        public const bool Tracing = false;

        protected readonly bool _IsPure;
        protected bool? _CachedIsPure;
        protected bool _ComputingPurity = false;

        public readonly Dictionary<string, HashSet<string>> VariableAliases;
        public readonly HashSet<FieldInfo> MutatedFields;
        public readonly HashSet<FieldInfo>[] RecursivelyMutatedFields;
        public readonly HashSet<string> ModifiedVariables;
        public readonly HashSet<string> EscapingVariables;
        public readonly string ResultVariable;
        public readonly bool ResultIsNew;
        public readonly bool ViolatesThisReferenceImmutability;

        public readonly FunctionCache FunctionCache;
        public readonly FunctionAnalysis1stPass Data;

        public FunctionAnalysis2ndPass (FunctionCache functionCache, FunctionAnalysis1stPass data) {
            FunctionAnalysis2ndPass invocationSecondPass;

            FunctionCache = functionCache;
            Data = data;

            if (data.Function.Method.Method.Metadata.HasAttribute("JSIsPure"))
                _IsPure = true;
            else
                _IsPure = (data.StaticReferences.Count == 0) &&
                    (data.SideEffects.Count == 0);

            VariableAliases = new Dictionary<string, HashSet<string>>();
            foreach (var assignment in data.Assignments) {
                if (assignment.SourceVariable != null) {
                    HashSet<string> aliases;
                    if (!VariableAliases.TryGetValue(assignment.SourceVariable.Identifier, out aliases))
                        VariableAliases[assignment.SourceVariable.Identifier] = aliases = new HashSet<string>();

                    aliases.Add(assignment.Target.Identifier);
                }
            }

            var parameterNames = new HashSet<string>(
                from p in data.Function.Parameters select p.Name
            );

            var parms = data.Function.Method.Method.Metadata.GetAttributeParameters("JSIL.Meta.JSMutatedArguments");
            if (parms != null) {
                ModifiedVariables = new HashSet<string>();
                foreach (var p in parms) {
                    var s = p.Value as string;
                    if (s != null)
                        ModifiedVariables.Add(s);
                }
            } else {
                ModifiedVariables = new HashSet<string>(
                    data.ModificationCount.Where((kvp) => {
                        var isParameter = parameterNames.Contains(kvp.Key);
                        return kvp.Value >= (isParameter ? 1 : 2);
                    }).Select((kvp) => kvp.Key)
                );

                if (TraceModifications && (ModifiedVariables.Count > 0))
                    Console.WriteLine("Tagged variables as modified due to modification count: {0}", String.Join(", ", ModifiedVariables));

                foreach (var v in Data.VariablesPassedByRef) {
                    if (TraceModifications)
                        Console.WriteLine("Tagging variable '{0}' as modified because it is passed byref", v);

                    ModifiedVariables.Add(v);
                }
            }

            parms = data.Function.Method.Method.Metadata.GetAttributeParameters("JSIL.Meta.JSEscapingArguments");
            if (parms != null) {
                EscapingVariables = new HashSet<string>();
                foreach (var p in parms) {
                    var s = p.Value as string;
                    if (s != null)
                        EscapingVariables.Add(s);
                }
            } else {
                EscapingVariables = Data.EscapingVariables;

                // Scan over all the invocations performed by this function and see if any of them cause
                //  a variable to escape
                foreach (var invocation in Data.Invocations) {
                    if (invocation.Method != null)
                        invocationSecondPass = functionCache.GetSecondPass(invocation.Method, Data.Identifier);
                    else
                        invocationSecondPass = null;

                    foreach (var invocationKvp in invocation.Variables) {
                        if (invocationKvp.Value.Length == 0)
                            continue;

                        bool escapes;

                        if (invocationSecondPass != null)
                            escapes = invocationSecondPass.EscapingVariables.Contains(invocationKvp.Key);
                        else
                            escapes = true;

                        if (escapes) {
                            if (invocationKvp.Value.Length > 1) {
                                // FIXME: Is this right?
                                // Multiple variables -> a binary operator expression or an invocation.
                                // In either case, it should be impossible for any of them to escape without being flagged otherwise.

                                if (TraceEscapes)
                                    Console.WriteLine(
                                        "Parameter '{0}::{1}' escapes but it is a composite so we are not flagging variables {2}",
                                        GetMethodName(invocation.Method), invocationKvp.Key, String.Join(", ", invocationKvp.Value)
                                    );

                                continue;
                            } else {
                                var escapingVariable = invocationKvp.Value[0];

                                if (TraceEscapes)
                                    Console.WriteLine("Parameter '{0}::{1}' escapes; flagging variable '{2}'", GetMethodName(invocation.Method), invocationKvp.Key, escapingVariable);

                                Data.EscapingVariables.Add(escapingVariable);
                            }
                        }
                    }
                }
            }

            ResultVariable = Data.ResultVariable;
            ResultIsNew = Data.ResultIsNew;

            var seenMethods = new HashSet<string>();
            var rm = Data.ResultMethod;
            while (rm != null) {
                var currentMethod = rm.QualifiedIdentifier.ToString();
                if (seenMethods.Contains(currentMethod))
                    break;

                seenMethods.Add(currentMethod);

                var rmfp = functionCache.GetFirstPass(rm.QualifiedIdentifier, data.Identifier);
                if (rmfp == null) {
                    ResultIsNew = rm.Method.Metadata.HasAttribute("JSIL.Meta.JSResultIsNew");
                    break;
                }

                rm = rmfp.ResultMethod;
                ResultIsNew = rmfp.ResultIsNew;
            }

            if (
                !data.Function.Method.Method.IsStatic && 
                data.Function.Method.Method.DeclaringType.IsImmutable &&
                data.ReassignsThisReference
            ) {
                ViolatesThisReferenceImmutability = true;
            }

            MutatedFields = new HashSet<FieldInfo>(
                from fa in data.FieldAccesses where !fa.IsRead select fa.Field.Field
            );

            var recursivelyMutatedFields = new HashSet<HashSet<FieldInfo>>();

            foreach (var invocation in Data.Invocations) {
                if (invocation.Method != null) {
                    invocationSecondPass = functionCache.GetSecondPass(invocation.Method, Data.Identifier);
                } else {
                    invocationSecondPass = null;
                }

                if ((invocationSecondPass == null) || (invocationSecondPass.MutatedFields == null)) {
                    // Can't know for sure.
                    MutatedFields = null;
                    break;
                }

                recursivelyMutatedFields.Add(invocationSecondPass.MutatedFields);

                foreach (var rrmf in invocationSecondPass.RecursivelyMutatedFields)
                    recursivelyMutatedFields.Add(rrmf);
            }

            RecursivelyMutatedFields = recursivelyMutatedFields.ToArray();

            Trace(data.Function.Method.Reference.FullName);
        }

        public FunctionAnalysis2ndPass (FunctionCache functionCache, MethodInfo method) {
            FunctionCache = functionCache;
            Data = null;
            _IsPure = method.Metadata.HasAttribute("JSIL.Meta.JSIsPure");

            var parms = method.Metadata.GetAttributeParameters("JSIL.Meta.JSMutatedArguments");
            if (parms != null) {
                ModifiedVariables = new HashSet<string>(GetAttributeArguments<string>(parms));
            } else if (!_IsPure) {
                ModifiedVariables = new HashSet<string>(from p in method.Parameters select p.Name);
            } else {
                ModifiedVariables = new HashSet<string>();
            }

            parms = method.Metadata.GetAttributeParameters("JSIL.Meta.JSEscapingArguments");
            if (parms != null) {
                EscapingVariables = new HashSet<string>(GetAttributeArguments<string>(parms));
            } else if (!_IsPure) {
                EscapingVariables = new HashSet<string>(from p in method.Parameters select p.Name);
            } else {
                EscapingVariables = new HashSet<string>();
            }

            VariableAliases = new Dictionary<string, HashSet<string>>();

            ResultVariable = null;
            ResultIsNew = method.Metadata.HasAttribute("JSIL.Meta.JSResultIsNew");

            MutatedFields = null;

            Trace(method.Member.FullName);
        }

        private static IEnumerable<T> GetAttributeArguments<T> (IEnumerable<CustomAttributeArgument> arguments) 
            where T : class
        {
            foreach (var arg in arguments) {
                var value = arg.Value as T;
                var enumerable = arg.Value as IEnumerable<CustomAttributeArgument>;

                if (value != null) {
                    yield return value;
                } else if (enumerable != null) {
                    foreach (var subValue in GetAttributeArguments<T>(enumerable))
                        yield return subValue;
                } else {
                    throw new Exception("Found an attribute argument I couldn't handle, of type " + arg.Type.Name + ": " + arg.Value);
                }
            }
        }

        private static string GetMethodName (JSMethod method) {
            if (method == null)
                return "?";
            else
                return method.Reference.Name;
        }

        protected bool DetermineIfPure () {
            foreach (var i in Data.Invocations) {
                if (i.Method == null)
                    return false;

                var secondPass = FunctionCache.GetSecondPass(i.Method, Data.Identifier);
                if (secondPass == null)
                    return false;

                if (!secondPass.IsPure)
                    return false;
            }

            return _IsPure;
        }

        public bool IsPure {
            get {
                if (Data == null)
                    return _IsPure;

                // If accessed recursively, always return false
                if (_ComputingPurity)
                    return false;

                try {
                    _ComputingPurity = true;

                    if (_CachedIsPure.HasValue)
                        return _CachedIsPure.Value;

                    _CachedIsPure = DetermineIfPure();
                    return _CachedIsPure.Value;
                } finally {
                    _ComputingPurity = false;
                }

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

        public bool Equals (FunctionAnalysis2ndPass rhs, out string[] _differences) {
            var differences = new List<string>();

            if (rhs == this) {
                _differences = null;
                return true;
            }

            if (!EscapingVariables.SequenceEqual(rhs.EscapingVariables))
                differences.Add("EscapingVariables");

            if (!ModifiedVariables.SequenceEqual(rhs.ModifiedVariables))
                differences.Add("ModifiedVariables");

            if (ResultIsNew != rhs.ResultIsNew)
                differences.Add("ResultIsNew");

            if (ResultVariable != rhs.ResultVariable)
                differences.Add("ResultVariable");

            if (!VariableAliases.SequenceEqual(rhs.VariableAliases))
                differences.Add("VariableAliases");

            if (ViolatesThisReferenceImmutability != rhs.ViolatesThisReferenceImmutability)
                differences.Add("ViolatesThisReferenceImmutability");

            if (IsPure != rhs.IsPure)
                differences.Add("IsPure");

            if (differences.Count > 0) {
                _differences = differences.ToArray();
                return false;
            } else {
                _differences = null;
                return true;
            }
        }

        public bool FieldIsMutatedRecursively (FieldInfo field) {
            if (MutatedFields == null)
                // Can't know for sure
                return true;
            else if (MutatedFields.Contains(field))
                return true;

            foreach (var rrmf in RecursivelyMutatedFields)
                if (rrmf.Contains(field))
                    return true;

            return false;
        }

        public override bool Equals (object obj) {
            var rhs = obj as FunctionAnalysis2ndPass;
            if (rhs != null)
                return Equals(rhs);

            return base.Equals(obj);
        }
    }
}
