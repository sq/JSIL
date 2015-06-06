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

            return result;
        }

        private FunctionAnalysis1stPass.NodeIndices GetParentNodeIndices () {
            var count = NodeIndexStack.Count;
            var buffer = ImmutableArrayPool<int>.Allocate(count);
            NodeIndexStack.CopyTo(buffer.Array, buffer.Offset);
            return new FunctionAnalysis1stPass.NodeIndices(buffer);
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

                    if (ReturnsSeen == 1) {
                        // Don't flag as escaping since this is handled by having a result variable.
                        State.ResultVariable = retVar.Identifier;
                    } else if (State.ResultVariable != retVar.Identifier) {
                        // We have more than one possible result variable so flag both as escaping.
                        State.EscapingVariables.Add(State.ResultVariable);
                        State.ResultVariable = null;
                        State.EscapingVariables.Add(retVar.Identifier);
                    }
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
            // Generate synthetic assignments for each parameter so that static analysis doesn't think
            //  that they are written to zero times (the correct answer is one)
            foreach (var parameter in fn.Parameters) {
                State.Assignments.Add(new FunctionAnalysis1stPass.Assignment(
                    GetParentNodeIndices(),
                    StatementIndex, NodeIndex, 
                    parameter.Name,
                    // FIXME: Should this be a null expression or something instead?
                    parameter,
                    JSOperator.Assignment,
                    parameter.GetActualType(TypeSystem),
                    parameter.GetActualType(TypeSystem)
                ));
            }

            VisitChildren(fn);
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
            expression = JSReferenceExpression.Strip(expression);

            var variable = expression as JSVariable;
            var dot = expression as JSDotExpressionBase;            

            if (dot != null)
                variable = ExtractAffectedVariable(dot.Target);

            return variable;
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
                            variable.Identifier, uoe, uoe.Operator,
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
            left = JSReferenceExpression.Strip(left);

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
                                leftVar.Identifier, boe.Right, boe.Operator,
                                leftType, rightType
                            )
                        );
                    }

                    ModifiedVariable(leftVar);
                }

                if (
                    boe.Left.SelfAndChildrenRecursive.FirstOrDefault((n) =>
                        (n is JSField) || (n is JSProperty) || (n is JSWriteThroughReferenceExpression)
                    ) != null
                ) {
                    var rightVars = new HashSet<JSVariable>(ExtractExposedVariables(boe.Right));

                    foreach (var variable in rightVars)
                        State.EscapingVariables.Add(variable.Identifier);
                }
            }
        }

        public static HashSet<JSVariable> ExtractExposedVariables (JSNode containingNode, Predicate<JSNode> haltPredicate = null) {
            var extractor = new VariableExtractor(VariableExtractor.Modes.ExposedVariables, haltPredicate);
            extractor.Visit(containingNode);
            return extractor.Variables;
        }

        public static HashSet<JSVariable> ExtractInvolvedVariables (JSNode containingNode, Predicate<JSNode> haltPredicate = null) {
            var extractor = new VariableExtractor(VariableExtractor.Modes.InvolvedVariables, haltPredicate);
            extractor.Visit(containingNode);
            return extractor.Variables;
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
                    parentNodeIndices, StatementIndex, NodeIndex, v.Identifier, "element modified"
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
                if (fa.IsWrite)
                    State.SideEffects.Add(new FunctionAnalysis1stPass.SideEffect(
                        parentNodeIndices, StatementIndex, NodeIndex, v.Identifier, "field modified"
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
                        GetParentNodeIndices(), StatementIndex, NodeIndex, v.Identifier, "property set"
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

                    foreach (var v in ExtractInvolvedVariables(kvp.Value)) {
                        if (!variables.ContainsKey(v.Name))
                            variables[v.Name] = v;
                    }
                }

                var pni = GetParentNodeIndices();

                foreach (var variable in variables.Values) {
                    ModifiedVariable(variable);
                    State.EscapingVariables.Add(variable.Name);

                    State.Accesses.Add(new FunctionAnalysis1stPass.Access(
                        pni, StatementIndex, NodeIndex, variable.Name, false
                    ));
                }
            }

            VisitChildren(verbatim);
        }

        private Dictionary<string, ArraySegment<string>> ExtractAffectedVariables (JSExpression method, IEnumerable<KeyValuePair<ParameterDefinition, JSExpression>> parameters) {
            var variables = new Dictionary<string, ArraySegment<string>>();
            var paramsArray = parameters.ToArray();

            int i = 0;
            foreach (var kvp in paramsArray) {
                var value = new ArraySegment<string>((from v in ExtractInvolvedVariables(kvp.Value) select v.Name).ToArray());

                if ((kvp.Key == null) || String.IsNullOrWhiteSpace(kvp.Key.Name)) {
                    variables.Add(String.Format("#{0}", i++), value);
                } else {
                    if (
                        variables.ContainsKey(kvp.Key.Name)
                    ) {
                        if (kvp.Key.CustomAttributes.Any((ca) => ca.AttributeType.Name == "ParamArrayAttribute")) {
                            var left = variables[kvp.Key.Name];
                            var right = value;
                            variables[kvp.Key.Name] = left.ToEnumerable().Concat(right.ToEnumerable()).ToImmutableArray(left.Count + right.Count);
                        } else
                            throw new InvalidDataException(String.Format(
                                "Multiple parameters named '{0}' for invocation of '{1}'. Parameter list follows: '{2}'",
                                kvp.Key.Name, method,
                                String.Join(", ", paramsArray)
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
                var pni = GetParentNodeIndices();
                State.Invocations.Add(new FunctionAnalysis1stPass.Invocation(
                    pni, StatementIndex, NodeIndex, thisVar, method, ie.Method, variables
                ));

                // HACK: Synthesize an assignment record for direct invocations of constructors on struct locals
                if ((method != null) && (method.Method.Name == ".ctor")) {
                    var t = thisVar.GetActualType(TypeSystem);
                    var synthesizedAssignment = new FunctionAnalysis1stPass.Assignment(
                        pni, StatementIndex, NodeIndex, thisVar.Name,
                        new JSNewExpression(t, method.Reference, method.Method, ie.Arguments.ToArray()),
                        JSOperator.Assignment,
                        t, t
                    );
                    State.Assignments.Add(synthesizedAssignment);
                }
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
                        tcb.CatchVariable.Identifier, new JSNullExpression(), JSOperator.Assignment,
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
                        variable.Identifier, isControlFlow
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
        public struct NodeIndices {
            public readonly ArraySegment<int> Indices;

            public NodeIndices (ArraySegment<int> indices) {
                Indices = indices;
            }

            public int this[int index] {
                get {
                    return Indices.Array[Indices.Offset + index];
                }
            }

            internal bool Contains (int scopeNodeIndex) {
                return Array.IndexOf(Indices.Array, scopeNodeIndex, Indices.Offset, Indices.Count) >= 0;
            }
        }

        public class Item {
            public readonly NodeIndices ParentNodeIndices;
            public readonly int StatementIndex;
            public readonly int NodeIndex;

            public Item (NodeIndices parentNodeIndices, int statementIndex, int nodeIndex) {
                ParentNodeIndices = parentNodeIndices;
                StatementIndex = statementIndex;
                NodeIndex = nodeIndex;
            }
        }

        public class Access : Item {
            public readonly string Source;
            public readonly bool IsControlFlow;

            public Access (
                NodeIndices parentNodeIndices, int statementIndex, int nodeIndex,
                string source, bool isControlFlow
            ) : base (parentNodeIndices, statementIndex, nodeIndex) { 
                Source = source;
                IsControlFlow = isControlFlow;
            }

            public override string ToString () {
                if (IsControlFlow)
                    return String.Format("ControlFlow {0})", Source);
                else
                    return Source;
            }
        }

        public class Assignment : Item {
            public readonly string Target;
            public readonly JSExpression NewValue;
            public readonly string SourceVariable;
            public readonly JSOperator Operator;
            public readonly TypeReference SourceType, TargetType;
            public readonly bool IsConversion;

            public Assignment (
                NodeIndices parentNodeIndices, int statementIndex, int nodeIndex, 
                string target, JSExpression newValue,
                JSOperator @operator,
                TypeReference targetType, TypeReference sourceType
            ) : base (parentNodeIndices, statementIndex, nodeIndex) {
                Target = target;
                NewValue = newValue;

                var newVariable = newValue as JSVariable;
                if (newVariable != null)
                    SourceVariable = newVariable.Identifier;
                else
                    SourceVariable = null;

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
                NodeIndices parentNodeIndices, int statementIndex, int nodeIndex, 
                TypeInfo type
            ) : base(parentNodeIndices, statementIndex, nodeIndex) {
                Type = type;
            }
        }

        public class SideEffect : Item {
            public readonly string Variable;
            public readonly string Type;

            public SideEffect (
                NodeIndices parentNodeIndices, int statementIndex, int nodeIndex, 
                string variable, string type
            ) : base (parentNodeIndices, statementIndex, nodeIndex) {
                Variable = variable;
                Type = type;
            }
        }

        public class FieldAccess : Item {
            public readonly JSField Field;
            public readonly bool IsRead;

            public FieldAccess (
                NodeIndices parentNodeIndices, int statementIndex, int nodeIndex,
                JSField field, bool isRead
            ) : base(parentNodeIndices, statementIndex, nodeIndex) {
                Field = field;
                IsRead = isRead;
            }
        }

        public class Invocation : Item {
            public readonly JSType ThisType;
            private readonly string[] _ThisVariable;

            public readonly JSMethod Method;
            public readonly object NonJSMethod;
            public readonly Dictionary<string, ArraySegment<string>> Variables;

            public Invocation (
                NodeIndices parentNodeIndices, int statementIndex, int nodeIndex, 
                JSType type, JSMethod method, object nonJSMethod,
                Dictionary<string, ArraySegment<string>> variables
            )
                : base(parentNodeIndices, statementIndex, nodeIndex) {
                ThisType = type;
                _ThisVariable = null;
                Method = method;
                if (method == null)
                    NonJSMethod = nonJSMethod;
                else
                    NonJSMethod = null;
                Variables = variables;
            }

            public Invocation (
                NodeIndices parentNodeIndices, int statementIndex, int nodeIndex,
                JSVariable thisVariable, JSMethod method, object nonJSMethod,
                Dictionary<string, ArraySegment<string>> variables
            ) : base(parentNodeIndices, statementIndex, nodeIndex) {
                if (thisVariable != null)
                    _ThisVariable = new[] { thisVariable.Identifier };
                else
                    _ThisVariable = null;

                ThisType = null;
                Method = method;
                if (method == null)
                    NonJSMethod = nonJSMethod;
                else
                    NonJSMethod = null;
                Variables = variables;
            }

            public string ThisVariable {
                get {
                    if (_ThisVariable != null)
                        return _ThisVariable[0];
                    else
                        return null;
                }
            }

            public IEnumerable<KeyValuePair<string, ArraySegment<string>>> ThisAndVariables {
                get {
                    if (_ThisVariable != null)
                        yield return new KeyValuePair<string, ArraySegment<string>>(
                            "this", new ArraySegment<string>(_ThisVariable)
                        );

                    foreach (var v in Variables)
                        yield return v;
                }
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
        public const bool TraceEscapes       = false;
        public const bool Tracing            = false;

        protected readonly bool _IsPure;
        protected bool? _CachedIsPure;
        protected bool _ComputingPurity = false;

        public readonly Dictionary<string, HashSet<string>> VariableAliases;
        public readonly HashSet<FieldInfo> MutatedFields;
        public readonly HashSet<HashSet<FieldInfo>> RecursivelyMutatedFields;
        private readonly HashSet<string> ModifiedVariables;
        private readonly HashSet<string> EscapingVariables;
        public readonly HashSet<string> IndirectSideEffectVariables;
        public readonly string ResultVariable;
        public readonly bool ResultIsNew;
        public readonly bool ViolatesThisReferenceImmutability;
        public readonly bool IsSealed;

        public readonly FunctionCache FunctionCache;
        public readonly FunctionAnalysis1stPass Data;

        public FunctionAnalysis2ndPass (
            FunctionCache functionCache, 
            FunctionAnalysis1stPass data, 
            bool isSealed
        ) {
            FunctionAnalysis2ndPass invocationSecondPass;

            FunctionCache = functionCache;
            Data = data;
            IsSealed = isSealed;

            if (data == null)
                throw new ArgumentNullException("data");
            else if (data.Function == null)
                throw new ArgumentNullException("data.Function");
            else if (data.Function.Method == null)
                throw new ArgumentNullException("data.Function.Method");
            else if (data.Function.Method.Method == null)
                throw new ArgumentNullException("data.Function.Method.Method");

            if (data.Function.Method.Method.Metadata.HasAttribute("JSIsPure"))
                _IsPure = true;
            else
                _IsPure = (data.StaticReferences.Count == 0) &&
                    (data.SideEffects.Count == 0);

            VariableAliases = new Dictionary<string, HashSet<string>>();
            foreach (var assignment in data.Assignments) {
                if (assignment.SourceVariable != null) {
                    HashSet<string> aliases;
                    if (!VariableAliases.TryGetValue(assignment.SourceVariable, out aliases))
                        VariableAliases[assignment.SourceVariable] = aliases = new HashSet<string>();

                    aliases.Add(assignment.Target);
                }
            }

            var parameterNames = new HashSet<string>(
                from p in data.Function.Parameters select p.Name
            );

            parameterNames.Add("this");

            var parms = data.Function.Method.Method.Metadata.GetAttributeParameters("JSIL.Meta.JSMutatedArguments");
            if (parms != null) {
                ModifiedVariables = new HashSet<string>();
                foreach (var p in parms) {
                    var s = p.Value as string;
                    if (s != null)
                        ModifiedVariables.Add(s);
                }
            } else {
                ModifiedVariables = new HashSet<string>();

                foreach (var kvp in data.ModificationCount) {
                    var isParameter = parameterNames.Contains(kvp.Key);

                    if (
                        kvp.Value >= 
                        (isParameter 
                            ? 1 
                            : 2
                        )
                    ) {
                        ModifiedVariables.Add(kvp.Key);
                    }
                }

                if (TraceModifications && (ModifiedVariables.Count > 0))
                    Console.WriteLine("Tagged variables as modified due to modification count: {0}", String.Join(", ", ModifiedVariables));

                foreach (var v in Data.VariablesPassedByRef) {
                    if (TraceModifications)
                        Console.WriteLine("Tagging variable '{0}' as modified because it is passed byref", v);

                    ModifiedVariables.Add(v);
                }

                foreach (var invocation in Data.Invocations) {
                    if (invocation.Method != null)
                        invocationSecondPass = functionCache.GetSecondPass(invocation.Method, Data.Identifier);
                    else
                        invocationSecondPass = null;

                    foreach (var invocationKvp in invocation.ThisAndVariables) {
                        if (invocationKvp.Value.Count == 0)
                            continue;

                        bool modified;

                        if (invocationSecondPass != null) {
                            modified = invocationSecondPass.IsVariableModified(invocationKvp.Key);
                        } else {
                            modified = true;
                        }

                        if (
                            (invocationKvp.Value.Count == 1) 
                            && modified
                        ) {
                            var relevantVariable = invocationKvp.Value.Array[invocationKvp.Value.Offset];

                            if (TraceModifications)
                                Console.WriteLine("Parameter '{0}::{1}' modified; flagging variable '{2}'", GetMethodName(invocation.Method), invocationKvp.Key, relevantVariable);

                            ModifiedVariables.Add(relevantVariable);
                        }
                    }
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
                EscapingVariables = new HashSet<string>(Data.EscapingVariables);

                // Scan over all the invocations performed by this function and see if any of them cause
                //  a variable to escape
                foreach (var invocation in Data.Invocations) {
                    if (invocation.Method != null)
                        invocationSecondPass = functionCache.GetSecondPass(invocation.Method, Data.Identifier);
                    else
                        invocationSecondPass = null;

                    foreach (var invocationKvp in invocation.ThisAndVariables) {
                        if (invocationKvp.Value.Count == 0)
                            continue;

                        bool escapes;

                        if (invocationSecondPass != null) {
                            // FIXME: Ignore return?
                            escapes = invocationSecondPass.DoesVariableEscape(invocationKvp.Key, true);
                        } else {
                            escapes = true;
                        }

                        if (invocationKvp.Value.Count > 1) {
                            // FIXME: Is this right?
                            // Multiple variables -> a binary operator expression or an invocation.
                            // In either case, it should be impossible for any of them to escape without being flagged otherwise.

                            if (escapes && TraceEscapes)
                                Console.WriteLine(
                                    "Parameter '{0}::{1}' escapes but it is a composite so we are not flagging variables {2}",
                                    GetMethodName(invocation.Method), invocationKvp.Key, String.Join(", ", invocationKvp.Value)
                                );

                            continue;
                        } else {
                            var relevantVariable = invocationKvp.Value.Array[invocationKvp.Value.Offset];

                            if (escapes) {
                                if (TraceEscapes)
                                    Console.WriteLine("Parameter '{0}::{1}' escapes; flagging variable '{2}'", GetMethodName(invocation.Method), invocationKvp.Key, relevantVariable);

                                EscapingVariables.Add(relevantVariable);
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
                (
                    !data.Function.Method.Method.IsStatic && 
                    data.Function.Method.Method.DeclaringType.IsImmutable &&
                    data.ReassignsThisReference
                ) ||
                data.Function.Method.Method.Name == ".ctor"
            ) {
                ViolatesThisReferenceImmutability = true;
                ModifiedVariables.Add("this");
            }

            IndirectSideEffectVariables = new HashSet<string>();

            // Invocations that reassign or mutate this need to have a synthesized side effect
            foreach (var invocation in data.Invocations) {
                if (invocation.ThisVariable == null)
                    continue;
                else if (IndirectSideEffectVariables.Contains(invocation.ThisVariable))
                    continue;

                bool shouldSynthesizeSideEffect = false;
                do {
                    var jsm = invocation.Method;
                    if (jsm == null) {
                        shouldSynthesizeSideEffect = true;
                        break;
                    }

                    var targetSecondPass = functionCache.GetSecondPass(invocation.Method, data.Identifier);
                    if (targetSecondPass == null) {
                        shouldSynthesizeSideEffect = true;
                        break;
                    }

                    if (targetSecondPass.ViolatesThisReferenceImmutability) {
                        shouldSynthesizeSideEffect = true;
                        break;
                    } else if (
                        (targetSecondPass.Data != null) && 
                        targetSecondPass.Data.SideEffects.Any(se => se.Variable == "this")
                    ) {
                        // FIXME: Is this necessary or is it overly conservative?
                        shouldSynthesizeSideEffect = true;
                        break;
                    }

                    if (!shouldSynthesizeSideEffect)
                        ;
                } while (false);

                if (shouldSynthesizeSideEffect)
                    IndirectSideEffectVariables.Add(invocation.ThisVariable);
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

            RecursivelyMutatedFields = recursivelyMutatedFields;

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
                if (!method.IsStatic)
                    ModifiedVariables.Add("this");
            } else {
                ModifiedVariables = new HashSet<string>();
            }

            parms = method.Metadata.GetAttributeParameters("JSIL.Meta.JSEscapingArguments");
            if (parms != null) {
                EscapingVariables = new HashSet<string>(GetAttributeArguments<string>(parms));
            } else if (!_IsPure) {
                EscapingVariables = new HashSet<string>(from p in method.Parameters select p.Name);
                if (!method.IsStatic)
                    EscapingVariables.Add("this");
            } else {
                EscapingVariables = new HashSet<string>();
            }

            VariableAliases = new Dictionary<string, HashSet<string>>();
            IndirectSideEffectVariables = new HashSet<string>();

            ResultVariable = null;
            ResultIsNew = method.Metadata.HasAttribute("JSIL.Meta.JSResultIsNew");

            MutatedFields = null;

            ViolatesThisReferenceImmutability = (method.Name == ".ctor");

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

        public bool IsVariableModified (string variableName) {
            return ModifiedVariables.Contains(variableName);
        }

        public bool DoesVariableEscape (string variableName, bool includeReturn) {
            if (includeReturn && (variableName == ResultVariable))
                return true;

            return EscapingVariables.Contains(variableName);
        }

        public int GetNumberOfEscapingVariables (bool includeReturn) {
            var result = EscapingVariables.Count;

            if (includeReturn && (ResultVariable != null))
                result += 1;

            return result;
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

        public override int GetHashCode () {
            return EscapingVariables.Count.GetHashCode() ^
                ModifiedVariables.Count.GetHashCode() ^
                ResultIsNew.GetHashCode() ^
                ResultVariable.GetHashCode() ^
                VariableAliases.Count.GetHashCode() ^
                ViolatesThisReferenceImmutability.GetHashCode() ^
                IsPure.GetHashCode();
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

    internal class VariableExtractor : JSAstVisitor {
        public enum Modes {
            ExposedVariables,
            InvolvedVariables
        }

        public readonly Modes Mode;
        public readonly HashSet<JSVariable> Variables = new HashSet<JSVariable>();
        public readonly Predicate<JSNode> HaltPredicate;

        public VariableExtractor (Modes mode, Predicate<JSNode> haltPredicate) {
            Mode = mode;
            HaltPredicate = haltPredicate;
            DefaultVisitPredicate = (node, name) => {
                if (haltPredicate != null)
                    return !haltPredicate(node);
                else
                    return true;
            };
        }

        public void VisitNode (JSVariable variable) {
            bool doAdd = true;
            if (
                (ParentNode is JSFieldAccess) &&
                (Mode == Modes.ExposedVariables)
            )
                doAdd = false;

            if (doAdd)
                Variables.Add(variable);

            VisitChildren(variable);
        }
    }
}
