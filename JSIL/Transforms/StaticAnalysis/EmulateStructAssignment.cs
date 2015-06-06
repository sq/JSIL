#pragma warning disable 0162
#pragma warning restore 0429

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
        public const bool TraceElidedCopies          = false;
        public const bool TraceInsertedCopies        = false;
        public const bool TracePostOptimizedCopies   = false;
        public const bool TracePostOptimizeDecisions = false;

        public readonly TypeInfoProvider TypeInfo;
        public readonly CLRSpecialIdentifiers CLR;
        public readonly TypeSystem TypeSystem;
        public readonly MethodTypeFactory MethodTypes;
        public readonly bool OptimizeCopies;

        private FunctionAnalysis2ndPass SecondPass = null;
        private JSExpression ResultReferenceReplacement = null;

        protected readonly Dictionary<string, int> ReferenceCounts = new Dictionary<string, int>();

        public EmulateStructAssignment (
            QualifiedMemberIdentifier member, 
            IFunctionSource functionSource, 
            TypeSystem typeSystem, 
            TypeInfoProvider typeInfo, 
            CLRSpecialIdentifiers clr, 
            MethodTypeFactory methodTypes,
            bool optimizeCopies
        )
            : base (member, functionSource) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
            CLR = clr;
            MethodTypes = methodTypes;
            OptimizeCopies = optimizeCopies;
        }

        protected bool IsImmutable (JSExpression target) {
            target = JSReferenceExpression.Strip(target);

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
            target = JSReferenceExpression.Strip(target);

            if (!OptimizeCopies)
                return true;

            if (IsImmutable(target))
                return false;

            var variable = target as JSVariable;
            if (variable != null)
                return SecondPass.IsVariableModified(variable.Name);

            return true;
        }

        protected bool IsCopyAlwaysUnnecessaryForAssignmentTarget (JSExpression target) {
            target = JSReferenceExpression.Strip(target);

            var targetDot = target as JSDotExpressionBase;

            // The assignment is performing a write into an element proxy, so a copy is unnecessary
            //  because the element proxy immediately unpacks the value into the array.
            if (
                (targetDot != null) && 
                PackedArrayUtil.IsElementProxy(targetDot.Target)
            )
                return true;

            return false;
        }

        protected bool IsCopyNeeded (
            JSExpression value, 
            out GenericParameter relevantParameter,
            bool allowImmutabilityOptimizations = true
        ) {
            relevantParameter = null;

            if ((value == null) || (value.IsNull))
                return false;

            value = JSReferenceExpression.Strip(value);

            var sce = value as JSStructCopyExpression;
            if (sce != null)
                return false;

            var valueType = value.GetActualType(TypeSystem);
            var valueTypeDerefed = TypeUtil.DereferenceType(valueType) ?? valueType;

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

                if (!IsStructOrGenericParameter(valueTypeDerefed) && !IsStructOrGenericParameter(originalType))
                    return false;

                relevantParameter = (originalType as GenericParameter) ?? (valueTypeDerefed as GenericParameter);
            } else {
                if (!IsStructOrGenericParameter(valueTypeDerefed))
                    return false;

                relevantParameter = (valueTypeDerefed as GenericParameter);
            }

            if (value is JSValueOfNullableExpression)
                return true;

            if (IsTypeExcludedFromCopies(valueType)) 
                return false;

            if (
                (value is JSLiteral) ||
                (value is JSNewExpression) ||
                (value is JSPassByReferenceExpression) ||
                (value is JSNewBoxedVariable) ||
                (value is JSDefaultValueLiteral) ||
                (value is JSFieldOfExpression)
            ) {
                return false;
            }

            if (!OptimizeCopies)
                return true;

            if (IsImmutable(value) && allowImmutabilityOptimizations)
                return false;

            var valueDot = value as JSDotExpressionBase;

            // The value is being read out of an element proxy
            if (
                (valueDot != null) &&
                PackedArrayUtil.IsElementProxy(valueDot.Target)
            ) {
                // BROKEN: no copy is necessary - the read unpacks the value on demand from the packed array.
                // return false;

                // HACK: Currently struct fields are proxies so that writes to them work correctly.
                // FIXME: Maybe return true here always?
                // return true;
            }

            var valueTypeInfo = TypeInfo.GetExisting(valueType);
            if ((valueTypeInfo != null) && valueTypeInfo.IsImmutable && allowImmutabilityOptimizations)
                return false;
            
            // If the expression is a parameter that is only used once and isn't aliased,
            //  we don't need to copy it.
            var rightVar = value as JSVariable;
            if (rightVar != null) {
                int referenceCount;
                if (
                    ReferenceCounts.TryGetValue(rightVar.Identifier, out referenceCount) &&
                    (referenceCount == 1) && 
                    !rightVar.IsReference && 
                    rightVar.IsParameter &&
                    !SecondPass.VariableAliases.ContainsKey(rightVar.Identifier)
                ) {
                    if (TraceElidedCopies)
                        Console.WriteLine(String.Format("Returning false from IsCopyNeeded for parameter {0} because reference count is 1 and it has no aliases", value));

                    return false;
                }
            }
 
            return true;
        }

        public static bool IsTypeExcludedFromCopies (TypeReference valueType) {
            if (valueType.FullName.StartsWith("System.Nullable"))
                return true;

            if (valueType.FullName == "System.Decimal")
                return true;

            // HACK: Never copy references to structs
            if (valueType.IsByReference)
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
                if (TraceInsertedCopies)
                    Console.WriteLine("struct copy introduced for object value {0}", pair.Value);

                pair.Value = MakeCopyForExpression(pair.Value, relevantParameter);
            }

            VisitChildren(pair);
        }

        protected bool IsArgumentCopyNeeded (FunctionAnalysis2ndPass sa, string parameterName, JSExpression expression, out GenericParameter relevantParameter) {
            if (!IsCopyNeeded(expression, out relevantParameter))
                return false;

            if (sa == null)
                return true;

            if (!OptimizeCopies)
                return true;

            bool modified = true, escapes = true, isResult = false;

            if (parameterName != null) {
                modified = sa.IsVariableModified(parameterName);
                escapes = sa.DoesVariableEscape(parameterName, true);
                isResult = sa.ResultVariable == parameterName;
            }

            var result = modified || (escapes && !isResult);

            if (!result) {
                if (TraceElidedCopies)
                    Console.WriteLine("argument {0} ('{1}') needs no copy because it isn't modified and doesn't escape", expression, parameterName);
            }

            return result;
        }

        public void VisitNode (JSNewExpression newexp) {
            FunctionAnalysis2ndPass sa = null;

            if (newexp.Constructor != null)
                // HACK
                sa = GetSecondPass(new JSMethod(newexp.ConstructorReference, newexp.Constructor, MethodTypes));

            CloneArgumentsIfNecessary(newexp.Parameters, newexp.Arguments, sa);

            VisitChildren(newexp);
        }

        public void VisitNode (JSInvocationExpression invocation) {
            FunctionAnalysis2ndPass sa = null;

            if (invocation.JSMethod != null)
                sa = GetSecondPass(invocation.JSMethod);

            CloneArgumentsIfNecessary(invocation.Parameters, invocation.Arguments, sa);

            var thisReference = invocation.ThisReference;
            var thisReferenceType = thisReference.GetActualType(TypeSystem);
            var thisReferenceIsStruct = TypeUtil.IsStruct(thisReferenceType) && 
                !TypeUtil.IsNullable(thisReferenceType);

            bool thisReferenceNeedsCopy = false;
            bool thisReferenceNeedsCopyAndReassignment = false;

            if (thisReferenceIsStruct && !invocation.SuppressThisClone) {
                var isMethodInvocation = (thisReference != null) && 
                    (sa != null) && 
                    !(ParentNode is JSCommaExpression) &&
                    !(thisReference is JSStructCopyExpression) &&
                    !(
                        (ParentNode is JSResultReferenceExpression) &&
                        Stack.OfType<JSCommaExpression>().Any()
                    );


                // If a struct is immutable, a method may reassign the this-reference,
                //  i.e. 'this = otherstruct' successfully.
                // In this scenario we have to clone the old this-reference, invoke
                //  the method on the clone, and replace the old this-reference with
                //  the new, modified clone.

                thisReferenceNeedsCopyAndReassignment = isMethodInvocation && sa.ViolatesThisReferenceImmutability;


                // When invoking a method that mutates a struct's members or lets them escape,
                //  we need to copy the this-reference if it isn't writable.

                // FIXME: We're white-listing writable targets, but we probably want to blacklist
                //  non-writable targets instead, so that if a fn's result is new we don't clone it
                //  to use it as a this-reference.

                // FIXME: Handle pointers, replace x.get().foo() with some sort of comma expr,
                //  like ($x = x.get(), $x.foo(), x.set($x)) ?
                var isWritableInstance = 
                    (thisReference is JSFieldAccess) ||
                        (thisReference is JSVariable) ||
                        (thisReference is JSReadThroughReferenceExpression);

                thisReferenceNeedsCopy = isMethodInvocation &&
                    (
                        sa.IsVariableModified("this") || 
                        // Maybe don't include return here?
                        sa.DoesVariableEscape("this", true)
                    ) &&
                    (!isWritableInstance);
            }

            GenericParameter relevantParameter;
            var isCopyNeeded = IsCopyNeeded(thisReference, out relevantParameter, false);
            if (
                thisReferenceNeedsCopyAndReassignment && isCopyNeeded
            ) {
                if (TraceInsertedCopies)
                    Console.WriteLine("Cloning this-reference because method reassigns this: {0}", invocation);

                if (
                    (thisReference is JSFieldAccess) ||
                    (thisReference is JSVariable)
                ) {
                    var rre = ParentNode as JSResultReferenceExpression;
                    var cloneExpr = new JSBinaryOperatorExpression(
                        JSOperator.Assignment, thisReference, MakeCopyForExpression(thisReference, relevantParameter), thisReferenceType
                    );
                    var commaExpression = new JSCommaExpression(
                        cloneExpr,
                        (rre != null)
                            ? (JSExpression)new JSResultReferenceExpression(invocation)
                            : (JSExpression)invocation
                    );

                    if (rre != null) {
                        ResultReferenceReplacement = commaExpression;
                        return;
                    } else {
                        ParentNode.ReplaceChild(invocation, commaExpression);
                        VisitReplacement(commaExpression);
                        return;
                    }
                } else {
                    // Huh?
                    invocation.ReplaceChild(thisReference, MakeCopyForExpression(thisReference, relevantParameter));
                    VisitChildren(invocation);
                    return;
                }
            } else if (thisReferenceNeedsCopy && isCopyNeeded) {
                if (TraceInsertedCopies)
                    Console.WriteLine("Cloning this-reference because method mutates this and this-reference is not field/local: {0}", invocation);

                invocation.ReplaceChild(thisReference, MakeCopyForExpression(thisReference, relevantParameter));
                VisitChildren(invocation);
                return;
            }

            VisitChildren(invocation);            
        }

        public void VisitNode (JSResultReferenceExpression rre) {
            VisitChildren(rre);

            if (ResultReferenceReplacement != null) {
                var newRre = ResultReferenceReplacement;
                ResultReferenceReplacement = null;
                ParentNode.ReplaceChild(rre, newRre);
                VisitReplacement(newRre);
            }
        }

        private void CloneArgumentsIfNecessary(
            IEnumerable<KeyValuePair<ParameterDefinition, JSExpression>> parameters,
            IList<JSExpression> argumentValues,
            FunctionAnalysis2ndPass sa
        ) {
            var parms = parameters.ToArray();

            for (int i = 0, c = parms.Length; i < c; i++)
            {
                var pd = parms[i].Key;
                var argument = parms[i].Value;

                string parameterName = null;
                if (pd != null)
                    parameterName = pd.Name;

                GenericParameter relevantParameter;
                if (IsArgumentCopyNeeded(sa, parameterName, argument, out relevantParameter))
                {
                    if (TraceInsertedCopies)
                        Console.WriteLine(String.Format("struct copy introduced for argument #{0}: {1}", i, argument));

                    argumentValues[i] = MakeCopyForExpression(argument, relevantParameter);
                }
                else
                {
                    if (TraceElidedCopies && TypeUtil.IsStruct(argument.GetActualType(TypeSystem)))
                        Console.WriteLine(String.Format("struct copy elided for argument #{0}: {1}", i, argument));
                }
            }
        }

        public void VisitNode (JSDelegateInvocationExpression invocation) {
            for (int i = 0, c = invocation.Arguments.Count; i < c; i++) {
                var argument = invocation.Arguments[i];

                GenericParameter relevantParameter;
                if (IsCopyNeeded(argument, out relevantParameter)) {
                    if (TraceInsertedCopies)
                        Console.WriteLine(String.Format("struct copy introduced for argument #{0}: {1}", i, argument));

                    invocation.Arguments[i] = MakeCopyForExpression(argument, relevantParameter);
                }
            }

            VisitChildren(invocation);
        }

        private bool IsVarEffectivelyConstantHere (JSVariable variable) {
            if (variable == null)
                return false;
            if (variable.IsParameter)
                return false;

            // If we're making a copy of a local variable, and this is the last reference to 
            //  the variable, we can eliminate the copy.
            if (
                !SecondPass.Data.VariablesPassedByRef.Contains(variable.Identifier) &&

                SecondPass.Data.Accesses
                    .Where(a => a.Source == variable.Identifier)
                    // FIXME: This should probably be NodeIndex but that gets out of sync somehow? Outdated analysis data?
                    .All(a => a.StatementIndex <= StatementIndex) &&

                SecondPass.Data.Assignments
                    .Where(a => a.Target == variable.Identifier)
                    // FIXME: This should probably be NodeIndex but that gets out of sync somehow? Outdated analysis data?
                    .All(a => a.StatementIndex < StatementIndex)
            ) {
                return true;
            }

            return false;
        }

        private bool IsStructVariableNeverMutated (JSExpression expr) {
            var variable = expr as JSVariable;

            if (variable == null)
                return false;

            // FIXME: ref/out don't generate a SideEffect. Should they?
            // I think reassignment through ref/out is safe but mutation through
            //  them is not safe. On the other hand, ref/out vars should always
            //  generate a copy on read... right?

            if (SecondPass == null)
                return false;
            else if (SecondPass.IndirectSideEffectVariables.Contains(variable.Name))
                return false;
            else if (SecondPass.Data == null)
                return false;

            foreach (var se in SecondPass.Data.SideEffects)
                if (se.Variable == variable.Name)
                    return false;

            return true;
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            if (boe.Operator != JSOperator.Assignment) {
                base.VisitNode(boe);
                return;
            }

            bool doPostOptimization = false;

            GenericParameter relevantParameter;

            if (IsCopyNeeded(boe.Right, out relevantParameter)) {
                var rightVars = new HashSet<JSVariable>(StaticAnalyzer.ExtractInvolvedVariables(boe.Right));

                // Even if the assignment target is never modified, if the assignment *source*
                //  gets modified, we need to make a copy here, because the target is probably
                //  being used as a back-up copy.
                var rightVarsMutated = rightVars.Any((rv) => !IsStructVariableNeverMutated(rv));
                var rightVarsAreReferences = rightVars.Any((rv) => rv.IsReference);

                // We need to ensure that the lhs is never mutated. Reassignment is fine.
                bool leftVarMutated = !IsStructVariableNeverMutated(boe.Left);

                bool rightVarIsEffectivelyConstantHere = 
                    IsVarEffectivelyConstantHere(boe.Right as JSVariable) &&
                    !leftVarMutated;

                if (
                    (
                        rightVarsMutated || 
                        leftVarMutated ||
                        IsCopyNeededForAssignmentTarget(boe.Left) || 
                        rightVarsAreReferences
                    ) &&
                    !IsCopyAlwaysUnnecessaryForAssignmentTarget(boe.Left) &&
                    !rightVarIsEffectivelyConstantHere
                ) {
                    if (TraceInsertedCopies)
                        Console.WriteLine("struct copy introduced for assignment {0} = {1}", boe.Left, boe.Right);

                    doPostOptimization = true;
                    boe.Right = MakeCopyForExpression(boe.Right, relevantParameter);
                } else {
                    if (TraceElidedCopies) {
                        Console.WriteLine(
                            "struct copy elided for assignment {0} = {1}{2}", boe.Left, boe.Right,
                            rightVarIsEffectivelyConstantHere 
                                ? " (rhs is effectively constant due to following all other accesses)" 
                                : ""
                        );
                    }
                }
            } else {
                if (TraceElidedCopies && TypeUtil.IsStruct(boe.Right.GetActualType(TypeSystem)))
                    Console.WriteLine(String.Format("no copy needed for assignment {0} = {1}", boe.Left, boe.Right));
            }

            VisitChildren(boe);

            if (doPostOptimization)
                PostOptimizeAssignment(boe, boe.Right);
        }

        void PostOptimizeAssignment (JSBinaryOperatorExpression boe, JSExpression rhs) {
            var rhsCopy = rhs as JSStructCopyExpression;
            if (rhsCopy == null)
                return;

            var expr = rhsCopy.Struct;
            var rre = expr as JSResultReferenceExpression;
            if (rre != null)
                expr = rre.Referent;

            var invocation = expr as JSInvocationExpression;
            if (invocation == null)
                return;

            var invokeMethod = invocation.JSMethod;
            if (invokeMethod == null)
                return;

            var invocationSecondPass = GetSecondPass(invokeMethod);
            if (invocationSecondPass == null)
                return;

            bool eliminateCopy = false;
            string eliminateReason = null;

            if (invocationSecondPass.ResultIsNew) {
                // If this expression is the return value of a function invocation, we can eliminate struct
                //  copies if the return value is a 'new' expression.
                eliminateCopy = true;
                eliminateReason = "Result is new";
            } else if (invocationSecondPass.ResultVariable != null) {
                // We can also eliminate a return value copy if the return value is one of the function's 
                //  arguments, and we are sure that argument does not escape (other than through the return
                //  statement, that is).
                
                var parameters = invokeMethod.Method.Parameters;
                int parameterIndex = -1;

                for (var i = 0; i < parameters.Length; i++) {
                    if (parameters[i].Name != invocationSecondPass.ResultVariable)
                        continue;

                    parameterIndex = i;
                    break;
                }

                // We found a single parameter that acts as the result of this function call.
                if (parameterIndex >= 0) {
                    GenericParameter relevantParameter;
                    var innerValue = invocation.Arguments[parameterIndex];

                    // Identify any local variables that are a dependency of the result parameter.
                    var localVariableDependencies = StaticAnalyzer.ExtractInvolvedVariables(
                        innerValue,
                        (n) =>
                            // If a variable is inside a copy expression we can ignore it as a dependency.
                            // The copy ensures that the dependency is resolved at the time of invocation.
                            (n is JSStructCopyExpression) ||
                            // If a variable is inside an invocation expression we can ignore it as a dependency.
                            // Dependency resolution would have occurred for the invocation already.
                            (n is JSInvocationExpression)
                    );

                    // Was the result parameter already copied when invoking the function?
                    // If so, we can eliminate the copy of the result because all dependencies have been resolved.
                    // Note that this is separate from the variable dependency extraction above -
                    //  if an invocation has multiple variable dependencies, the above merely narrows them down
                    //  while this detects cases where there are effectively *no* dependencies of any kind.
                    // This also detects cases where the argument is the result of an invocation or property access
                    //  and the result was copied.
                    var isAlreadyCopied = innerValue is JSStructCopyExpression;

                    var dependenciesModified = localVariableDependencies.Any(iv => SecondPass.IsVariableModified(iv.Name));

                    var copyNeededForTarget = IsCopyNeededForAssignmentTarget(boe.Left);

                    var escapes = invocationSecondPass.DoesVariableEscape(invocationSecondPass.ResultVariable, false);
                    var modified = invocationSecondPass.IsVariableModified(invocationSecondPass.ResultVariable);

                    var traceMessage = (Action)(() => {
                        if (TracePostOptimizeDecisions)
                            Console.WriteLine(
                                "< {0}: {1} > escapes:{2} modified:{3} copyNeededForTarget({4}):{5} inputsModified:{6}", 
                                parameters[parameterIndex].Name, innerValue, escapes, modified, boe.Left, copyNeededForTarget, dependenciesModified
                            );
                    });

                    if (isAlreadyCopied) {
                        eliminateCopy = true;
                        eliminateReason = "Result was already copied as part of argument list";
                    } else if (!escapes && !dependenciesModified) {
                        if (!copyNeededForTarget) {
                            eliminateCopy = true;
                            eliminateReason = "Target does not require a copy and other criteria are met";
                        } else if (!modified) {
                            eliminateCopy = true;
                            eliminateReason = "Input not modified, doesn't escape, no dependency changes";
                        }

                        traceMessage();
                    } else {
                        traceMessage();
                    }
                }
            }

            if (eliminateCopy) {
                if (TracePostOptimizedCopies)
                    Console.WriteLine("Post-optimized assignment {0} because {1}", boe, eliminateReason);

                boe.ReplaceChild(rhsCopy, rhsCopy.Struct);
            }
        }

        public void VisitNode (JSNewBoxedVariable nbv) {
            GenericParameter relevantParameter;

            var initialValue = nbv.InitialValue;
            var initialValueDerefed = initialValue;
            initialValueDerefed = JSReferenceExpression.Strip(initialValueDerefed);

            var initialValueType = initialValueDerefed.GetActualType(TypeSystem);

            if (!nbv.SuppressClone &&
                IsCopyNeeded(nbv.InitialValue, out relevantParameter) &&
                // We don't need to make a copy if the source value is a reference (like T& this)
                !((initialValueType) != null && initialValueType.IsByReference)
            ) {
                nbv.ReplaceChild(nbv.InitialValue, new JSStructCopyExpression(nbv.InitialValue));
            }

            VisitChildren(nbv);
        }

        public void VisitNode (JSWriteThroughReferenceExpression wtre) {
            var rightType = wtre.GetActualType(TypeSystem);
            GenericParameter relevantParameter;

            if (
                IsStructOrGenericParameter(rightType) &&
                IsCopyNeeded(wtre.Right, out relevantParameter)
            ) {
                bool rightVarIsEffectivelyConstantHere =
                    IsVarEffectivelyConstantHere(wtre.Right as JSVariable);

                if (rightVarIsEffectivelyConstantHere) {
                    if (TraceElidedCopies)
                        Console.WriteLine("struct copy elided for write-through-reference rhs {0} (rhs is effectively constant)", wtre);
                } else {
                    if (TraceInsertedCopies)
                        Console.WriteLine("struct copy introduced for write-through-reference rhs {0}", wtre);

                    var replacement = new JSWriteThroughReferenceExpression(
                        (JSVariable)wtre.Left, MakeCopyForExpression(wtre.Right, relevantParameter)
                    );
                    ParentNode.ReplaceChild(wtre, replacement);
                    VisitReplacement(replacement);
                }
            } else {
                VisitChildren(wtre);
            }
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
