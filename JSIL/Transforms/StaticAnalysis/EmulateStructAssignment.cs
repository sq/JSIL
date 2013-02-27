using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class EmulateStructAssignment : StaticAnalysisJSAstVisitor {
        public const bool Tracing = false;

        public readonly TypeInfoProvider TypeInfo;
        public readonly CLRSpecialIdentifiers CLR;
        public readonly TypeSystem TypeSystem;
        public readonly bool OptimizeCopies;

        private FunctionAnalysis2ndPass SecondPass = null;

        protected readonly Dictionary<string, int> ReferenceCounts = new Dictionary<string, int>();

        public EmulateStructAssignment (QualifiedMemberIdentifier member, IFunctionSource functionSource, TypeSystem typeSystem, TypeInfoProvider typeInfo, CLRSpecialIdentifiers clr, bool optimizeCopies)
            : base (member, functionSource) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
            CLR = clr;
            OptimizeCopies = optimizeCopies;
        }

        protected bool IsImmutable (JSExpression target) {
            while (target is JSReferenceExpression)
                target = ((JSReferenceExpression)target).Referent;

            var fieldAccess = target as JSFieldAccess;
            if (fieldAccess != null) {
                return fieldAccess.Field.Field.Metadata.HasAttribute("JSIL.Meta.JSImmutable");
            }

            var dot = target as JSDotExpressionBase;
            if (dot != null) {
                if (IsImmutable(dot.Target))
                    return true;
                else if (IsImmutable(dot.Member))
                    return true;
            }

            var indexer = target as JSIndexerExpression;
            if (indexer != null) {
                if (IsImmutable(indexer.Target))
                    return true;
            }

            return false;
        }

        protected bool IsCopyNeededForAssignmentTarget (JSExpression target) {
            if (!OptimizeCopies)
                return true;

            if (IsImmutable(target))
                return false;

            var variable = target as JSVariable;
            if (variable != null)
                return SecondPass.ModifiedVariables.Contains(variable.Name);

            return true;
        }

        protected bool IsCopyNeeded (JSExpression value, out GenericParameter relevantParameter) {
            relevantParameter = null;

            if ((value == null) || (value.IsNull))
                return false;

            while (value is JSReferenceExpression)
                value = ((JSReferenceExpression)value).Referent;

            var sce = value as JSStructCopyExpression;
            if (sce != null)
                return false;

            var valueType = value.GetActualType(TypeSystem);
            var cte = value as JSChangeTypeExpression;
            var cast = value as JSCastExpression;

            TypeReference originalType;
            int temp;

            if (cte != null) {
                originalType = cte.Expression.GetActualType(TypeSystem);
            } else if (cast != null) {
                originalType = cast.Expression.GetActualType(TypeSystem);
            } else {
                originalType = null;
            }

            if (originalType != null) {
                originalType = TypeUtil.FullyDereferenceType(originalType, out temp);

                if (!IsStructOrGenericParameter(valueType) && !IsStructOrGenericParameter(originalType))
                    return false;

                relevantParameter = (originalType as GenericParameter) ?? (valueType as GenericParameter);
            } else {
                if (!IsStructOrGenericParameter(valueType))
                    return false;

                relevantParameter = (valueType as GenericParameter);
            }

            if (IsTypeExcludedFromCopies(valueType)) 
                return false;

            if (
                (value is JSLiteral) ||
                (value is JSNewExpression) ||
                (value is JSPassByReferenceExpression)
            ) {
                return false;
            }

            if (!OptimizeCopies)
                return true;

            if (IsImmutable(value)) {
                return false;
            }

            var valueTypeInfo = TypeInfo.GetExisting(valueType);
            if ((valueTypeInfo != null) && valueTypeInfo.IsImmutable)
                return false;
            
            // If the expression is a parameter that is only used once and isn't aliased,
            //  we don't need to copy it.
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

            var rightInvocation = value as JSInvocationExpression;
            if (rightInvocation == null)
                return true;

            var invokeMethod = rightInvocation.JSMethod;
            if (invokeMethod == null)
                return true;

            var secondPass = GetSecondPass(invokeMethod);
            if (secondPass == null)
                return true;

            // If this expression is the return value of a function invocation, we can eliminate struct
            //  copies if the return value is a 'new' expression.
            if (secondPass.ResultIsNew)
                return false;

            // We can also eliminate a return value copy if the return value is one of the function's 
            //  arguments, and we are sure that argument does not need a copy either.
            if (secondPass.ResultVariable != null) {
                var parameters = invokeMethod.Method.Parameters;
                int parameterIndex = -1;

                for (var i = 0; i < parameters.Length; i++) {
                    if (parameters[i].Name != secondPass.ResultVariable)
                        continue;

                    parameterIndex = i;
                    break;
                }

                if (parameterIndex < 0)
                    return true;

                return IsCopyNeeded(rightInvocation.Arguments[parameterIndex], out relevantParameter);
            }
 
            return true;
        }

        public static bool IsTypeExcludedFromCopies (TypeReference valueType) {
            if (valueType.FullName.StartsWith("System.Nullable"))
                return true;

            if (valueType.FullName == "System.Decimal")
                return true;

            return false;
        }

        public void VisitNode (JSFunctionExpression fn) {
            var countRefs = new CountVariableReferences(ReferenceCounts);
            countRefs.Visit(fn.Body);

            SecondPass = GetSecondPass(fn.Method);
            if (SecondPass == null)
                throw new InvalidDataException("No second-pass static analysis data for function '" + fn.Method.QualifiedIdentifier + "'");

            VisitChildren(fn);
        }

        public void VisitNode (JSPairExpression pair) {
            GenericParameter relevantParameter;
            if (IsCopyNeeded(pair.Value, out relevantParameter)) {
                if (Tracing)
                    Debug.WriteLine(String.Format("struct copy introduced for object value {0}", pair.Value));

                pair.Value = MakeCopyForExpression(pair.Value, relevantParameter);
            }

            VisitChildren(pair);
        }

        protected bool IsParameterCopyNeeded (FunctionAnalysis2ndPass sa, string parameterName, JSExpression expression, out GenericParameter relevantParameter) {
            if (!IsCopyNeeded(expression, out relevantParameter))
                return false;

            if (sa == null)
                return true;

            if (!OptimizeCopies)
                return true;

            bool modified = true, escapes = true, isResult = false;

            if (parameterName != null) {
                modified = sa.ModifiedVariables.Contains(parameterName);
                escapes = sa.EscapingVariables.Contains(parameterName);
                isResult = sa.ResultVariable == parameterName;
            }

            return modified || (escapes && !isResult);
        }

        public void VisitNode (JSInvocationExpression invocation) {
            FunctionAnalysis2ndPass sa = null;

            if (invocation.JSMethod != null)
                sa = GetSecondPass(invocation.JSMethod);

            var parms = invocation.Parameters.ToArray();

            for (int i = 0, c = parms.Length; i < c; i++) {
                var pd = parms[i].Key;
                var argument = parms[i].Value;

                string parameterName = null;
                if (pd != null)
                    parameterName = pd.Name;

                GenericParameter relevantParameter;
                if (IsParameterCopyNeeded(sa, parameterName, argument, out relevantParameter)) {
                    if (Tracing)
                        Debug.WriteLine(String.Format("struct copy introduced for argument #{0}: {1}", i, argument));

                    invocation.Arguments[i] = MakeCopyForExpression(argument, relevantParameter);
                } else {
                    if (Tracing && TypeUtil.IsStruct(argument.GetActualType(TypeSystem)))
                        Debug.WriteLine(String.Format("struct copy elided for argument #{0}: {1}", i, argument));
                }
            }

            var thisReference = invocation.ThisReference;
            if (
                (thisReference != null) && 
                (sa != null) && 
                sa.ViolatesThisReferenceImmutability && 
                !(ParentNode is JSCommaExpression)
            ) {
                // The method we're calling violates immutability so we need to clone the this-reference
                //  before we call it.
                var thisReferenceType = thisReference.GetActualType(TypeSystem);
                if (TypeUtil.IsStruct(thisReferenceType)) {
                    if (!(thisReference is JSVariable) && !(thisReference is JSFieldAccess))
                        throw new NotImplementedException("Unsupported invocation of method that reassigns this within an immutable struct: " + invocation.ToString());

                    var cloneExpr = new JSBinaryOperatorExpression(
                        JSOperator.Assignment, thisReference, new JSStructCopyExpression(thisReference), thisReferenceType
                    );
                    var commaExpression = new JSCommaExpression(cloneExpr, invocation);

                    ParentNode.ReplaceChild(invocation, commaExpression);
                    VisitReplacement(commaExpression);
                    return;
                }
            }

            VisitChildren(invocation);
        }

        public void VisitNode (JSDelegateInvocationExpression invocation) {
            for (int i = 0, c = invocation.Arguments.Count; i < c; i++) {
                var argument = invocation.Arguments[i];

                GenericParameter relevantParameter;
                if (IsCopyNeeded(argument, out relevantParameter)) {
                    if (Tracing)
                        Debug.WriteLine(String.Format("struct copy introduced for argument argument #{0}: {1}", i, argument));

                    invocation.Arguments[i] = MakeCopyForExpression(argument, relevantParameter);
                }
            }

            VisitChildren(invocation);
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            if (boe.Operator != JSOperator.Assignment) {
                base.VisitNode(boe);
                return;
            }

            GenericParameter relevantParameter;

            if (IsCopyNeeded(boe.Right, out relevantParameter)) {
                var rightVars = boe.Right.SelfAndChildrenRecursive.OfType<JSVariable>().ToArray();

                // Even if the assignment target is never modified, if the assignment *source*
                //  gets modified, we need to make a copy here, because the target is probably
                //  being used as a back-up copy.
                var rightVarsModified = (rightVars.Any((rv) => SecondPass.ModifiedVariables.Contains(rv.Name)));

                if (rightVarsModified || IsCopyNeededForAssignmentTarget(boe.Left)) {
                    if (Tracing)
                        Debug.WriteLine(String.Format("struct copy introduced for assignment rhs {0}", boe.Right));

                    boe.Right = MakeCopyForExpression(boe.Right, relevantParameter);
                } else {
                    if (Tracing)
                        Debug.WriteLine(String.Format("struct copy elided for assignment rhs {0}", boe.Right));
                }
            }

            VisitChildren(boe);
        }

        protected JSStructCopyExpression MakeCopyForExpression (JSExpression expression, GenericParameter relevantParameter) {
            if (relevantParameter != null)
                return new JSConditionalStructCopyExpression(relevantParameter, expression);
            else
                return new JSStructCopyExpression(expression);
        }

        protected static bool IsStructOrGenericParameter (TypeReference tr) {
            int temp;
            var derefed = TypeUtil.FullyDereferenceType(tr, out temp);

            var gp = derefed as GenericParameter;
            if (gp != null) {
                foreach (var constraint in gp.Constraints) {
                    if (constraint.FullName == "System.Object") {
                        // Class constraint. Excludes structs.
                        return false;
                    }
                }
                
                return true;
            } else
                return TypeUtil.IsStruct(tr);
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
