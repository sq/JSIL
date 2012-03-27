using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class EmulateStructAssignment : JSAstVisitor {
        public const bool Tracing = false;

        public readonly CLRSpecialIdentifiers CLR;
        public readonly IFunctionSource FunctionSource;
        public readonly TypeSystem TypeSystem;
        public readonly bool OptimizeCopies;

        private FunctionAnalysis2ndPass SecondPass = null;

        protected readonly Dictionary<string, int> ReferenceCounts = new Dictionary<string, int>();

        public EmulateStructAssignment (TypeSystem typeSystem, IFunctionSource functionSource, CLRSpecialIdentifiers clr, bool optimizeCopies) {
            TypeSystem = typeSystem;
            FunctionSource = functionSource;
            CLR = clr;
            OptimizeCopies = optimizeCopies;
        }

        public static bool IsStruct (TypeReference type) {
            if (type == null)
                return false;

            type = ILBlockTranslator.DereferenceType(type);
            MetadataType etype = type.MetadataType;

            if (ILBlockTranslator.IsEnum(type))
                return false;

            var git = type as GenericInstanceType;
            if (git != null)
                return git.IsValueType;

            return (etype == MetadataType.ValueType);
        }

        protected bool IsCopyNeeded (JSExpression value) {
            if ((value == null) || (value.IsNull))
                return false;

            while (value is JSReferenceExpression)
                value = ((JSReferenceExpression)value).Referent;

            var valueType = value.GetExpectedType(TypeSystem);

            if (!IsStruct(valueType))
                return false;

            if (valueType.FullName.StartsWith("System.Nullable"))
                return false;

            if (
                (value is JSLiteral) ||
                (value is JSNewExpression) ||
                (value is JSPassByReferenceExpression)
            ) {
                return false;
            }

            var rightVar = value as JSVariable;
            if (rightVar != null) {
                int referenceCount;
                if (
                    ReferenceCounts.TryGetValue(rightVar.Identifier, out referenceCount) &&
                    (referenceCount == 1) && !rightVar.IsReference && rightVar.IsParameter &&
                    !SecondPass.VariableAliases.ContainsKey(rightVar.Identifier)
                ) {
                    if (Tracing)
                        Debug.WriteLine(String.Format("Returning false from IsCopyNeeded for parameter {0} because reference count is 1 and it has no aliases", value));

                    return false;
                }
            }
 
            return true;
        }

        public void VisitNode (JSFunctionExpression fn) {
            // Create a new visitor for nested function expressions
            if (Stack.OfType<JSFunctionExpression>().Skip(1).FirstOrDefault() != null) {
                var nested = new EmulateStructAssignment(TypeSystem, FunctionSource, CLR, OptimizeCopies);
                nested.Visit(fn);
                return;
            }

            var countRefs = new CountVariableReferences(ReferenceCounts);
            countRefs.Visit(fn.Body);

            SecondPass = FunctionSource.GetSecondPass(fn.Method);

            VisitChildren(fn);
        }

        public void VisitNode (JSPairExpression pair) {
            if (IsCopyNeeded(pair.Value)) {
                if (Tracing)
                    Debug.WriteLine(String.Format("struct copy introduced for object value {0}", pair.Value));
                pair.Value = new JSStructCopyExpression(pair.Value);
            }

            VisitChildren(pair);
        }

        public void VisitNode (JSInvocationExpression invocation) {
            FunctionAnalysis2ndPass sa = null;

            if (invocation.JSMethod != null)
                sa = FunctionSource.GetSecondPass(invocation.JSMethod);

            var parms = invocation.Parameters.ToArray();

            for (int i = 0, c = parms.Length; i < c; i++) {
                var pd = parms[i].Key;
                var argument = parms[i].Value;

                if (IsCopyNeeded(argument)) {
                    bool modified = true, escapes = true, isResult = false;

                    if ((sa != null) && (pd != null)) {
                        var varName = pd.Name;
                        modified = sa.ModifiedVariables.Contains(varName);
                        escapes = sa.EscapingVariables.Contains(varName);
                        isResult = sa.ResultVariable == varName;
                    }

                    if (modified || (escapes && !isResult)) {
                        if (Tracing)
                            Debug.WriteLine(String.Format("struct copy introduced for argument {0}", argument));

                        invocation.Arguments[i] = new JSStructCopyExpression(argument);
                    } else {
                        if (Tracing)
                            Debug.WriteLine(String.Format("struct copy elided for argument {0}", argument));
                    }
                }
            }

            VisitChildren(invocation);
        }

        public void VisitNode (JSDelegateInvocationExpression invocation) {
            for (int i = 0, c = invocation.Arguments.Count; i < c; i++) {
                var argument = invocation.Arguments[i];

                if (IsCopyNeeded(argument)) {
                    if (Tracing)
                        Debug.WriteLine(String.Format("struct copy introduced for argument {0}", argument));
                    invocation.Arguments[i] = new JSStructCopyExpression(argument);
                }
            }

            VisitChildren(invocation);
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            if (boe.Operator != JSOperator.Assignment) {
                base.VisitNode(boe);
                return;
            }

            if (IsCopyNeeded(boe.Right)) {
                if (Tracing)
                    Debug.WriteLine(String.Format("struct copy introduced for assignment rhs {0}", boe.Right));

                boe.Right = new JSStructCopyExpression(boe.Right);
            }

            VisitChildren(boe);
        }
    }

    public class CountVariableReferences : JSAstVisitor {
        public readonly Dictionary<string, int> ReferenceCounts;

        public CountVariableReferences (Dictionary<string, int> referenceCounts) {
            ReferenceCounts = referenceCounts;
        }

        public void VisitNode (JSVariable variable) {
            int count;
            if (ReferenceCounts.TryGetValue(variable.Identifier, out count))
                ReferenceCounts[variable.Identifier] = count + 1;
            else
                ReferenceCounts[variable.Identifier] = 1;

            VisitChildren(variable);
        }
    }
}
