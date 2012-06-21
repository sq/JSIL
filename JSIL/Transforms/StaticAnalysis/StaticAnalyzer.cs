using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class StaticAnalyzer : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly IFunctionSource FunctionSource;

        protected int ReturnsSeen = 0;

        protected FunctionAnalysis1stPass State;

        public StaticAnalyzer (TypeSystem typeSystem, IFunctionSource functionSource) {
            TypeSystem = typeSystem;
            FunctionSource = functionSource;
        }

        public FunctionAnalysis1stPass FirstPass (JSFunctionExpression function) {
            State = new FunctionAnalysis1stPass(function);

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
                            StatementIndex, NodeIndex,
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

            var leftDot = left as JSDotExpressionBase;
            // If the LHS is one or more nested dot expressions, unnest them (leftward) to find the root variable.
            while (leftDot != null) {
                if (leftDot.Target is JSDotExpressionBase)
                    leftDot = (JSDotExpressionBase)leftDot.Target;
                else
                    break;
            }

            var leftVar = ExtractAffectedVariable(left);

            VisitChildren(boe);

            if (isAssignment) {
                if (leftVar != null) {
                    var leftType = left.GetActualType(TypeSystem);
                    var rightType = boe.Right.GetActualType(TypeSystem);

                    State.Assignments.Add(
                        new FunctionAnalysis1stPass.Assignment(
                            StatementIndex, NodeIndex,
                            leftVar, boe.Right, boe.Operator,
                            leftType, rightType
                        )
                    );

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

        public void VisitNode (JSFieldAccess fa) {
            var field = fa.Field;
            var v = ExtractAffectedVariable(fa.ThisReference);

            if (fa.HasGlobalStateDependency) {
                State.StaticReferences.Add(new FunctionAnalysis1stPass.StaticReference(
                    StatementIndex, NodeIndex, field.Field.DeclaringType
                ));
            } else if (v != null) {
                State.SideEffects.Add(new FunctionAnalysis1stPass.SideEffect(
                    StatementIndex, NodeIndex, v
                ));
            }

            VisitChildren(fa);
        }

        public void VisitNode (JSPropertyAccess prop) {
            var parentBoe = ParentNode as JSBinaryOperatorExpression;
            var v = ExtractAffectedVariable(prop.Target);
            var p = prop.Property.Property;

            if (prop.HasGlobalStateDependency) {
                State.StaticReferences.Add(new FunctionAnalysis1stPass.StaticReference(
                    StatementIndex, NodeIndex, p.DeclaringType
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
                        StatementIndex, NodeIndex, v
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

        public void VisitNode (JSInvocationExpression ie) {
            var variables = new Dictionary<string, string[]>();

            int i = 0;
            foreach (var kvp in ie.Parameters) {
                var value = (from v in kvp.Value.SelfAndChildrenRecursive.OfType<JSVariable>() select v.Name).ToArray();

                if (kvp.Key == null) {
                    variables.Add(String.Format("#{0}", i++), value);
                } else {
                    if (
                        variables.ContainsKey(kvp.Key.Name) &&
                        kvp.Key.CustomAttributes.Any((ca) => ca.AttributeType.Name == "ParamArrayAttribute")
                    ) {
                        variables[kvp.Key.Name] = variables[kvp.Key.Name].Concat(value).ToArray();
                    } else {
                        variables.Add(kvp.Key.Name, value);
                    }
                }
            }

            var type = ie.JSType;
            var thisVar = ExtractAffectedVariable(ie.ThisReference);
            var method = ie.JSMethod;

            if (thisVar != null) {
                ModifiedVariable(thisVar);

                State.Invocations.Add(new FunctionAnalysis1stPass.Invocation(
                    StatementIndex, NodeIndex, thisVar, method, variables
                ));
            } else {
                State.Invocations.Add(new FunctionAnalysis1stPass.Invocation(
                    StatementIndex, NodeIndex, type, method, variables
                ));
            }

            VisitChildren(ie);
        }

        public void VisitNode (JSTryCatchBlock tcb) {
            if (tcb.CatchVariable != null) {
                State.Assignments.Add(
                    new FunctionAnalysis1stPass.Assignment(
                        StatementIndex, NodeIndex,
                        tcb.CatchVariable, new JSNullExpression(), JSOperator.Assignment,
                        tcb.CatchVariable.Type, tcb.CatchVariable.Type
                    )
                );

                ModifiedVariable(tcb.CatchVariable);
            }

            VisitChildren(tcb);
        }

        public void VisitNode (JSVariable variable) {
            if (CurrentName == "FunctionSignature") {
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
                    isControlFlow |= enclosingBlock is JSLoopStatement;

                State.Accesses.Add(
                    new FunctionAnalysis1stPass.Access(
                        StatementIndex, NodeIndex,
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
                IsConversion = !TypeUtil.TypesAreEqual(targetType, sourceType);
            }

            public override string ToString () {
                return String.Format("{0} {1} {2}", Target, Operator, NewValue);
            }
        }

        public class StaticReference : Item {
            public readonly TypeInfo Type;

            public StaticReference (int statementIndex, int nodeIndex, TypeInfo type)
                : base(statementIndex, nodeIndex) {
                Type = type;
            }
        }

        public class SideEffect : Item {
            public readonly JSVariable Variable;

            public SideEffect (int statementIndex, int nodeIndex, JSVariable variable)
                : base (statementIndex, nodeIndex) {
                Variable = variable;
            }
        }

        public class Invocation : Item {
            public readonly JSType ThisType;
            public readonly string ThisVariable;
            public readonly JSMethod Method;
            public readonly IDictionary<string, string[]> Variables;

            public Invocation (int statementIndex, int nodeIndex, JSType type, JSMethod method, IDictionary<string, string[]> variables) 
                : base (statementIndex, nodeIndex) {
                ThisType = type;
                ThisVariable = null;
                Method = method;
                Variables = variables;
            }

            public Invocation (int statementIndex, int nodeIndex, JSVariable thisVariable, JSMethod method, IDictionary<string, string[]> variables)
                : base(statementIndex, nodeIndex) {
                ThisVariable = thisVariable.Identifier;
                ThisType = null;
                Method = method;
                Variables = variables;
            }
        }

        public readonly JSFunctionExpression Function;
        public readonly List<Access> Accesses = new List<Access>();
        public readonly List<Assignment> Assignments = new List<Assignment>();
        public readonly HashSet<string> VariablesPassedByRef = new HashSet<string>();
        public readonly Dictionary<string, int> ModificationCount = new Dictionary<string, int>();
        public readonly HashSet<string> EscapingVariables = new HashSet<string>();
        public readonly List<SideEffect> SideEffects = new List<SideEffect>();
        public readonly List<StaticReference> StaticReferences = new List<StaticReference>();
        public readonly List<Invocation> Invocations = new List<Invocation>();

        // If not null, this method's return value is always the result of a call to a particular method.
        public JSMethod ResultMethod = null;
        // If not null, this method's return value is always a particular variable.
        public string ResultVariable = null;
        // If true, this method's return value is always a 'new' expression.
        public bool ResultIsNew = false;

        public FunctionAnalysis1stPass (JSFunctionExpression function) {
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
        public const bool Tracing = false;

        protected readonly bool _IsPure;
        protected bool? _CachedIsPure = false;
        protected bool _ComputingPurity = false;

        public readonly Dictionary<string, HashSet<string>> VariableAliases;
        public readonly HashSet<string> ModifiedVariables;
        public readonly HashSet<string> EscapingVariables;
        public readonly string ResultVariable;
        public readonly bool ResultIsNew;

        public readonly IFunctionSource FunctionSource;
        public readonly FunctionAnalysis1stPass Data;

        public FunctionAnalysis2ndPass (IFunctionSource functionSource, FunctionAnalysis1stPass data) {
            FunctionSource = functionSource;
            Data = data;
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

            ModifiedVariables = new HashSet<string>(
                data.ModificationCount.Where((kvp) => {
                    var isParameter = parameterNames.Contains(kvp.Key);
                    return kvp.Value >= (isParameter ? 1 : 2);
                }).Select((kvp) => kvp.Key)
            );

            foreach (var v in Data.VariablesPassedByRef)
                ModifiedVariables.Add(v);

            EscapingVariables = Data.EscapingVariables;
            ResultVariable = Data.ResultVariable;
            ResultIsNew = Data.ResultIsNew;

            var seenMethods = new HashSet<string>();
            var rm = Data.ResultMethod;
            while (rm != null) {
                var currentMethod = rm.QualifiedIdentifier.ToString();
                if (seenMethods.Contains(currentMethod))
                    break;

                seenMethods.Add(currentMethod);

                var rmfp = functionSource.GetFirstPass(rm.QualifiedIdentifier);
                if (rmfp == null) {
                    ResultIsNew = rm.Method.Metadata.HasAttribute("JSIL.Meta.JSResultIsNew");
                    break;
                }

                rm = rmfp.ResultMethod;
                ResultIsNew = rmfp.ResultIsNew;
            }

            Trace(data.Function.Method.Reference.FullName);
        }

        public FunctionAnalysis2ndPass (IFunctionSource functionSource, MethodInfo method) {
            FunctionSource = functionSource;
            Data = null;
            _IsPure = method.Metadata.HasAttribute("JSIL.Meta.JSIsPure");

            var parms = method.Metadata.GetAttributeParameters("JSIL.Meta.JSMutatedArguments");
            if (parms != null) {
                ModifiedVariables = new HashSet<string>();
                foreach (var p in parms) {
                    var s = p.Value as string;
                    if (s != null)
                        ModifiedVariables.Add(s);
                }
            } else if (!_IsPure) {
                ModifiedVariables = new HashSet<string>(from p in method.Parameters select p.Name);
            } else {
                ModifiedVariables = new HashSet<string>();
            }

            parms = method.Metadata.GetAttributeParameters("JSIL.Meta.JSEscapingArguments");
            if (parms != null) {
                EscapingVariables = new HashSet<string>();
                foreach (var p in parms) {
                    var s = p.Value as string;
                    if (s != null)
                        EscapingVariables.Add(s);
                }
            } else if (!_IsPure) {
                EscapingVariables = new HashSet<string>(from p in method.Parameters select p.Name);
            } else {
                EscapingVariables = new HashSet<string>();
            }

            VariableAliases = new Dictionary<string, HashSet<string>>();

            ResultVariable = null;
            ResultIsNew = method.Metadata.HasAttribute("JSIL.Meta.JSResultIsNew");

            Trace(method.Member.FullName);
        }

        protected bool DetermineIfPure () {
            foreach (var i in Data.Invocations) {
                if (i.Method == null)
                    return false;

                var secondPass = FunctionSource.GetSecondPass(i.Method);
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
    }
}
