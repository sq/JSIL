using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class SignatureCacher : JSAstVisitor {
        private const string GenericParameterHostNamespace = "$JSIL";
        private const string GenericParameterHostName = "ParameterHost";

        private const string GenericParameterHostFullName =
            GenericParameterHostNamespace + "." + GenericParameterHostName;

        public static class GenericTypesRewriter {
            private static readonly TypeReference GerericArgumentHost =
                new TypeReference(GenericParameterHostNamespace,
                    GenericParameterHostName, null,
                    new AssemblyNameReference(GenericParameterHostName, new Version()));

            public static RewritedCacheRecord<MethodSignature> Normalized (MethodReference method, MethodSignature signature, bool isConstructor) {
                var targetMappings = new Dictionary<GenericParameter, int>(new GenericParameterComparer());
                var resultSignature = new MethodSignature(signature.TypeInfo,
                    isConstructor
                        ? ResolveTypeReference(method, method.DeclaringType, targetMappings)
                        : ResolveTypeReference(method, signature.ReturnType, targetMappings),
                    signature.ParameterTypes.Select(item => ResolveTypeReference(method, item, targetMappings))
                        .ToArray(),
                    signature.GenericParameterNames);

                var mappings = targetMappings.OrderBy(item => item.Value).Select(item => item.Key).ToArray();

                return new RewritedCacheRecord<MethodSignature>(
                    resultSignature,
                    mappings);
            }

            public static MethodSignature NormalizedConstructorSignature (MethodReference method, MethodSignature signature, bool isConstructor) {
                return new MethodSignature(signature.TypeInfo,
                    isConstructor ? method.DeclaringType : signature.ReturnType,
                    signature.ParameterTypes,
                    signature.GenericParameterNames);
            }

            public static RewritedCacheRecord<TypeReference> Normalized (TypeReference declaringType) {
                var targetMappings = new Dictionary<GenericParameter, int>(new GenericParameterComparer());
                var resolvedType = GenericTypesRewriter.ResolveTypeReference(
                    declaringType,
                    (tr, resolutionStack) => {
                        if (tr is GenericParameter) {
                            var gp = (GenericParameter) tr;
                            return GenericTypesRewriter.ReplaceWithGenericArgument(gp, targetMappings);
                        }
                        return tr;
                    },
                    null);

                var mappings = targetMappings.OrderBy(item => item.Value).Select(item => item.Key).ToArray();

                return new RewritedCacheRecord<TypeReference>(resolvedType, mappings);
            }

            private static TypeReference ResolveTypeReference (MethodReference method, TypeReference typeReference, Dictionary<GenericParameter, int> mappings) {
                var resolvedMethod = method.Resolve();

                return GenericTypesRewriter.ResolveTypeReference(
                    typeReference,
                    (tr, resolutionStack) => {
                        if (tr is GenericParameter) {
                            var gp = (GenericParameter) tr;

                            if (resolutionStack.Count(typeInStack => TypeUtil.TypesAreEqual(typeInStack, gp)) > 1) {
                                return GenericTypesRewriter.ReplaceWithGenericArgument(gp, mappings);
                            }

                            var result = MethodSignature.ResolveGenericParameter(gp, method.DeclaringType);

                            if (result is GenericParameter) {
                                gp = (GenericParameter) result;
                                result = null;
                            }
                            if (result == null) {
                                if (gp.Owner is MethodReference) {
                                    var mr = (MethodReference) gp.Owner;
                                    if (mr.Resolve() == resolvedMethod)
                                        return gp;
                                }

                                return GenericTypesRewriter.ReplaceWithGenericArgument(gp, mappings);
                            }

                            return result;
                        }
                        return tr;
                    }, null);
            }

            private static TypeReference ResolveTypeReference (TypeReference typeReference, Func<TypeReference, Stack<GenericParameter>, TypeReference> resolver, Stack<GenericParameter> resolutionStack) {
                if (resolutionStack == null) {
                    resolutionStack = new Stack<GenericParameter>();
                }

                bool stackPushed = false;
                if (typeReference is GenericParameter) {
                    resolutionStack.Push((GenericParameter)typeReference);
                    stackPushed = true;
                }

                typeReference = resolver(typeReference, resolutionStack);
                if (typeReference is GenericInstanceType) {
                    var git = (GenericInstanceType) typeReference;
                    var newType = new GenericInstanceType(ResolveTypeReference(git.ElementType, resolver, resolutionStack));

                    foreach (var ga in git.GenericArguments) {
                        newType.GenericArguments.Add(ResolveTypeReference(ga, resolver, resolutionStack));
                    }
                    return newType;
                }
                if (typeReference is ArrayType) {
                    var at = (ArrayType) typeReference;
                    var newType = new ArrayType(ResolveTypeReference(at.ElementType, resolver, resolutionStack), at.Rank);
                    return newType;
                }
                if (typeReference is ByReferenceType) {
                    var brt = (ByReferenceType) typeReference;
                    var newType = new ByReferenceType(ResolveTypeReference(brt.ElementType, resolver, resolutionStack));
                    return newType;
                }

                if (stackPushed) {
                    resolutionStack.Pop();
                }

                return typeReference;
            }

            private static GenericParameter ReplaceWithGenericArgument (GenericParameter input, Dictionary<GenericParameter, int> mappings) {
                int index;
                if (!mappings.TryGetValue(input, out index)) {
                    index = mappings.Count;
                    mappings.Add(input, index);
                }
                return new GenericParameter("arg" + (index + 1), GerericArgumentHost);
            }
        }

        public struct RewritedCacheRecord<T> {
            public readonly GenericParameter[] RewritedGenericParameters;
            public readonly T CacheRecord;

            public RewritedCacheRecord (T cacheRecord, GenericParameter[] rewritedGenericParameters) {
                RewritedGenericParameters = rewritedGenericParameters;
                CacheRecord = cacheRecord;
            }
        }

        public struct CachedSignatureRecord {
            public readonly MethodReference Method;
            public readonly MethodSignature Signature;
            public readonly bool IsConstructor;
            public readonly int RewritenGenericParametersCount;

            public CachedSignatureRecord (MethodReference method, MethodSignature signature, bool isConstructor,
                int rewritenGenericParametersCount = 0) {
                Method = method;
                Signature = signature;
                IsConstructor = isConstructor;

                RewritenGenericParametersCount = rewritenGenericParametersCount;
            }

            public override int GetHashCode () {
                return (Method != null ? Method.Name.GetHashCode() : 0) ^ Signature.GetHashCode() ^ IsConstructor.GetHashCode() ^ RewritenGenericParametersCount;
            }

            public bool Equals (ref CachedSignatureRecord rhs) {
                var areMethodEquals = (Method == rhs.Method) ||
                    (Method != null && rhs.Method != null && Method.FullName == rhs.Method.FullName);

                var result =
                    areMethodEquals &&
                        Signature.Equals(rhs.Signature) &&
                        (IsConstructor == rhs.IsConstructor);

                if (!result)
                    return false;
                else
                    return result;
            }

            public override bool Equals (object obj) {
                if (obj is CachedSignatureRecord) {
                    var rhs = (CachedSignatureRecord) obj;
                    return Equals(ref rhs);
                } else
                    return base.Equals(obj);
            }
        }

        public class CachedSignatureRecordComparer : IEqualityComparer<CachedSignatureRecord> {
            public bool Equals (CachedSignatureRecord x, CachedSignatureRecord y) {
                return x.Equals(ref y);
            }

            public int GetHashCode (CachedSignatureRecord obj) {
                return obj.GetHashCode();
            }
        }

        public class IgnoreMethodCachedSignatureRecordComparer : IEqualityComparer<CachedSignatureRecord> {
            public int GetHashCode (CachedSignatureRecord record) {
                return record.Signature.GetHashCode() ^ record.IsConstructor.GetHashCode() ^
                    record.RewritenGenericParametersCount;
            }

            public bool Equals (CachedSignatureRecord lhs, CachedSignatureRecord rhs) {
                var result =
                    lhs.Signature.Equals(rhs.Signature) &&
                        (lhs.IsConstructor == rhs.IsConstructor);

                if (!result)
                    return false;
                else
                    return result;
            }
        }

        public struct CachedInterfaceMemberRecord {
            public readonly TypeReference InterfaceType;
            public readonly string InterfaceMember;
            public readonly int RewritenGenericParametersCount;

            public CachedInterfaceMemberRecord (TypeReference declaringType, string memberName,
                int rewritenGenericParametersCount = 0) {
                InterfaceType = declaringType;
                InterfaceMember = memberName;
                RewritenGenericParametersCount = rewritenGenericParametersCount;
            }


            public bool Equals (ref CachedInterfaceMemberRecord rhs) {
                var result =
                    TypeUtil.TypesAreEqual(InterfaceType, rhs.InterfaceType, true) &&
                        InterfaceMember.Equals(rhs.InterfaceMember);

                if (!result)
                    return false;
                else
                    return result;
            }

            public override bool Equals (object obj) {
                if (obj is CachedInterfaceMemberRecord) {
                    var rhs = (CachedInterfaceMemberRecord) obj;
                    return Equals(ref rhs);
                } else
                    return base.Equals(obj);
            }

            public override int GetHashCode () {
                return InterfaceType.Name.GetHashCode() ^ InterfaceMember.GetHashCode() ^ RewritenGenericParametersCount;
            }
        }

        public class CachedInterfaceMemberRecordComparer : IEqualityComparer<CachedInterfaceMemberRecord> {
            public bool Equals (CachedInterfaceMemberRecord x, CachedInterfaceMemberRecord y) {
                return x.Equals(ref y);
            }

            public int GetHashCode (CachedInterfaceMemberRecord obj) {
                return obj.GetHashCode();
            }
        }

        public class CacheSet {
            private static readonly IgnoreMethodCachedSignatureRecordComparer IgnoreMethodComparer =
                new IgnoreMethodCachedSignatureRecordComparer();

            private static readonly CachedSignatureRecordComparer Comparer = new CachedSignatureRecordComparer();

            private static readonly CachedInterfaceMemberRecordComparer InterfaceMemberComparer =
                new CachedInterfaceMemberRecordComparer();

            public readonly Dictionary<CachedSignatureRecord, int> Signatures;
            public readonly Dictionary<CachedInterfaceMemberRecord, int> InterfaceMembers;

            public CacheSet (bool useMethodSignaturePerMethod) {
                Signatures =
                    new Dictionary<CachedSignatureRecord, int>(useMethodSignaturePerMethod
                        ? (IEqualityComparer<CachedSignatureRecord>) Comparer
                        : IgnoreMethodComparer);
                InterfaceMembers = new Dictionary<CachedInterfaceMemberRecord, int>(InterfaceMemberComparer);
            }
        }

        public readonly bool LocalCachingEnabled;
        public readonly bool PreferLocalCacheForGenericMethodSignatures;
        public readonly bool PreferLocalCacheForGenericInterfaceMethodSignatures;
        public readonly bool UseMethodSignaturePerMethod;

        public readonly Dictionary<MemberIdentifier, CacheSet> LocalCachedSets;
        public readonly CacheSet Global;
        private readonly Stack<JSFunctionExpression> FunctionStack = new Stack<JSFunctionExpression>();

        public SignatureCacher (TypeInfoProvider typeInfo, bool localCachingEnabled,
            bool preferLocalCacheForGenericMethodSignatures, bool preferLocalCacheForGenericInterfaceMethodSignatures,
            bool useMethodSignaturePerMethod) {
            Global = new CacheSet(useMethodSignaturePerMethod);
            LocalCachedSets = new Dictionary<MemberIdentifier, CacheSet>(
                new MemberIdentifier.Comparer(typeInfo)
                );
            VisitNestedFunctions = true;
            LocalCachingEnabled = localCachingEnabled;
            PreferLocalCacheForGenericMethodSignatures = preferLocalCacheForGenericMethodSignatures;
            PreferLocalCacheForGenericInterfaceMethodSignatures = preferLocalCacheForGenericInterfaceMethodSignatures;
            UseMethodSignaturePerMethod = useMethodSignaturePerMethod;
        }

        private CacheSet GetCacheSet (bool cacheLocally) {
            CacheSet result = Global;

            if (cacheLocally && LocalCachingEnabled) {
                var fn = FunctionStack.Peek();
                if ((fn.Method == null) || (fn.Method.Method == null))
                    return Global;

                var functionIdentifier = fn.Method.Method.Identifier;
                if (!LocalCachedSets.TryGetValue(functionIdentifier, out result))
                    result = LocalCachedSets[functionIdentifier] = new CacheSet(UseMethodSignaturePerMethod);
            }

            return result;
        }

        private void CacheSignature (MethodReference method, MethodSignature signature, bool isConstructor) {
            bool cacheLocally;
            CachedSignatureRecord record;
            if (LocalCachingEnabled && PreferLocalCacheForGenericMethodSignatures) {
                record = new CachedSignatureRecord(method,
                    GenericTypesRewriter.NormalizedConstructorSignature(method, signature, isConstructor), isConstructor);
                signature = record.Signature;

                Func<GenericParameter, bool> filter =
                    (gp) => {
                        // If the generic parameter can be expanded given the type that declared the method, don't cache locally.
                        var resolved = MethodSignature.ResolveGenericParameter(gp, method.DeclaringType);
                        // Note that we have to ensure the resolved type is not generic either. A generic parameter can resolve to a
                        //  *different* generic parameter, and that is still correct - i.e. SomeMethod<A> calls SomeMethod<B>,
                        //  in that case resolving B will yield A.
                        if ((resolved != gp) && (resolved != null) && !TypeUtil.IsOpenType(resolved))
                            return false;

                        var ownerMethod = gp.Owner as MethodReference;
                        if (ownerMethod == null)
                            return true;

                        if (ownerMethod == method)
                            return false;
                        // FIXME: Nulls?
                        else if (ownerMethod.Resolve() == method.Resolve())
                            return false;
                        else
                            return true;
                    };

                cacheLocally = false;

                if (TypeUtil.IsOpenType(signature.ReturnType, filter))
                    cacheLocally = true;
                else if (signature.ParameterTypes.Any((gp) => TypeUtil.IsOpenType(gp, filter)))
                    cacheLocally = true;
                else if (isConstructor && TypeUtil.IsOpenType(method.DeclaringType, filter))
                    cacheLocally = true;
            } else {
                cacheLocally = false;
                var rewritenInfo = GenericTypesRewriter.Normalized(method, signature, isConstructor);
                record = new CachedSignatureRecord(
                    method,
                    rewritenInfo.CacheRecord,
                    isConstructor,
                    rewritenInfo.RewritedGenericParameters.Length);
            }

            var set = GetCacheSet(cacheLocally);

            if (!set.Signatures.ContainsKey(record))
                set.Signatures.Add(record, set.Signatures.Count);
        }

        private void CacheInterfaceMember (TypeReference declaringType, string memberName) {
            var cacheLocally = false;
            var originalType = declaringType;

            var unexpandedType = declaringType;
            if (!TypeUtil.ExpandPositionalGenericParameters(unexpandedType, out declaringType))
                declaringType = unexpandedType;

            CachedInterfaceMemberRecord record;

            if (LocalCachingEnabled && PreferLocalCacheForGenericInterfaceMethodSignatures) {
                if (TypeUtil.IsOpenType(declaringType))
                    cacheLocally = true;
                record = new CachedInterfaceMemberRecord(declaringType, memberName);
            } else {
                var rewritten = GenericTypesRewriter.Normalized(declaringType);
                record = new CachedInterfaceMemberRecord(rewritten.CacheRecord, memberName,
                    rewritten.RewritedGenericParameters.Length);
            }

            var set = GetCacheSet(cacheLocally);

            if (!set.InterfaceMembers.ContainsKey(record))
                set.InterfaceMembers.Add(record, set.InterfaceMembers.Count);
        }

        private JSRawOutputIdentifier MakeRawOutputIdentifierForIndex (TypeReference type, int index, bool isSignature) {
            if (isSignature)
                return new JSRawOutputIdentifier(
                    type,
                    "$s{0:X2}", index
                    );
            else
                return new JSRawOutputIdentifier(
                    type,
                    "$im{0:X2}", index
                    );
        }

        public void VisitNode (JSFunctionExpression fe) {
            FunctionStack.Push(fe);

            try {
                VisitChildren(fe);
            } finally {
                var functionIdentifier = fe.Method.Method.Identifier;
                CacheSet localSet;
                if (LocalCachedSets.TryGetValue(functionIdentifier, out localSet)) {
                    var trType = fe.Method.Reference.Module.TypeSystem.SystemType();

                    int i = 0;
                    foreach (var kvp in localSet.Signatures) {
                        var record = kvp.Key;
                        var stmt = new JSVariableDeclarationStatement(new JSBinaryOperatorExpression(
                            JSOperator.Assignment,
                            MakeRawOutputIdentifierForIndex(trType, kvp.Value, true),
                            new JSLocalCachedSignatureExpression(trType, record.Method, record.Signature, record.IsConstructor),
                            trType
                        ));

                        fe.Body.Statements.Insert(i++, stmt);
                    }

                    foreach (var kvp in localSet.InterfaceMembers) {
                        var record = kvp.Key;
                        var stmt = new JSVariableDeclarationStatement(new JSBinaryOperatorExpression(
                            JSOperator.Assignment,
                            MakeRawOutputIdentifierForIndex(trType, kvp.Value, false),
                            new JSLocalCachedInterfaceMemberExpression(trType, record.InterfaceType, record.InterfaceMember),
                            trType
                        ));

                        fe.Body.Statements.Insert(i++, stmt);
                    }
                }

                FunctionStack.Pop();
            }
        }

        public void VisitNode (JSMethodOfExpression methodOf) {
            CacheSignature(methodOf.Reference, methodOf.Method.Signature, false);
        }

        public void VisitNode (JSMethodPointerInfoExpression methodOf) {
            CacheSignature(methodOf.Reference, methodOf.Method.Signature, false);
        }

        public void VisitNode (JSInvocationExpression invocation) {
            var jsm = invocation.JSMethod;
            MethodInfo method = null;
            if (jsm != null)
                method = jsm.Method;

            bool isOverloaded = (method != null) &&
                method.IsOverloadedRecursive &&
                !method.Metadata.HasAttribute("JSIL.Meta.JSRuntimeDispatch");

            if (isOverloaded && JavascriptAstEmitter.CanUseFastOverloadDispatch(method))
                isOverloaded = false;

            if ((method != null) && method.DeclaringType.IsInterface) {
                // HACK
                if (!PackedArrayUtil.IsPackedArrayType(jsm.Reference.DeclaringType))
                    CacheInterfaceMember(jsm.Reference.DeclaringType, jsm.Identifier);
            }

            if ((jsm != null) && (method != null) && isOverloaded)
                CacheSignature(jsm.Reference, method.Signature, false);

            VisitChildren(invocation);
        }

        public void VisitNode (JSNewExpression newexp) {
            var ctor = newexp.Constructor;
            var isOverloaded = (ctor != null) &&
                ctor.IsOverloadedRecursive &&
                !ctor.Metadata.HasAttribute("JSIL.Meta.JSRuntimeDispatch");

            // New improved ConstructorSignature.Construct is faster than fast overload dispatch! :)
            /*
            if (isOverloaded && JavascriptAstEmitter.CanUseFastOverloadDispatch(ctor))
                isOverloaded = false;
             */

            if (isOverloaded)
                CacheSignature(newexp.ConstructorReference, ctor.Signature, true);

            VisitChildren(newexp);
        }

        public void CacheSignaturesForFunction (JSFunctionExpression function) {
            Visit(function);
        }

        /// <summary>
        /// Writes a method signature to the output.
        /// </summary>
        public void WriteSignatureToOutput (
            JavascriptFormatter output, JSFunctionExpression enclosingFunction,
            MethodReference methodReference, MethodSignature methodSignature,
            TypeReferenceContext referenceContext,
            bool forConstructor
            ) {
            int index;
            CachedSignatureRecord cacheRecord;
            GenericParameter[] rewrittenGenericParameters = null;
            if (LocalCachingEnabled && PreferLocalCacheForGenericMethodSignatures) {
                cacheRecord = new CachedSignatureRecord(methodReference,
                    GenericTypesRewriter.NormalizedConstructorSignature(methodReference, methodSignature, forConstructor),
                    forConstructor);
            } else {
                RewritedCacheRecord<MethodSignature> rewritten = GenericTypesRewriter.Normalized(methodReference,
                    methodSignature, forConstructor);
                cacheRecord = new CachedSignatureRecord(methodReference, rewritten.CacheRecord, forConstructor,
                    rewritten.RewritedGenericParameters.Length);
                rewrittenGenericParameters = rewritten.RewritedGenericParameters;
            }

            if ((enclosingFunction.Method != null) && (enclosingFunction.Method.Method != null)) {
                var functionIdentifier = enclosingFunction.Method.Method.Identifier;
                CacheSet localSignatureSet;

                if (LocalCachedSets.TryGetValue(functionIdentifier, out localSignatureSet)) {
                    if (localSignatureSet.Signatures.TryGetValue(cacheRecord, out index)) {
                        output.WriteRaw("$s{0:X2}", index);

                        return;
                    }
                }
            }

            if (!Global.Signatures.TryGetValue(cacheRecord, out index))
                output.Signature(methodReference, methodSignature, referenceContext, forConstructor, true);
            else {
                output.WriteRaw("$S{0:X2}", index);
                output.LPar();
                if (rewrittenGenericParameters != null) {
                    output.CommaSeparatedList(rewrittenGenericParameters, referenceContext);
                }
                output.RPar();
            }
        }

        /// <summary>
        /// Writes an interface member reference to the output.
        /// </summary>
        public void WriteInterfaceMemberToOutput (
            JavascriptFormatter output, Compiler.Extensibility.IAstEmitter astEmitter,
            JSFunctionExpression enclosingFunction,
            JSMethod jsMethod, JSExpression method,
            TypeReferenceContext referenceContext
            ) {
            int index;

            CachedInterfaceMemberRecord record;
            GenericParameter[] rewrittenGenericParameters = null;
            if (LocalCachingEnabled && PreferLocalCacheForGenericInterfaceMethodSignatures) {
                record = new CachedInterfaceMemberRecord(jsMethod.Reference.DeclaringType, jsMethod.Identifier);
            } else {
                var rewritten = GenericTypesRewriter.Normalized(jsMethod.Reference.DeclaringType);
                record = new CachedInterfaceMemberRecord(rewritten.CacheRecord, jsMethod.Identifier,
                    rewritten.RewritedGenericParameters.Length);
                rewrittenGenericParameters = rewritten.RewritedGenericParameters;
            }

            if (enclosingFunction.Method != null && enclosingFunction.Method.Method != null) {
                var functionIdentifier = enclosingFunction.Method.Method.Identifier;
                CacheSet localSignatureSet;

                if (LocalCachedSets.TryGetValue(functionIdentifier, out localSignatureSet)) {
                    if (localSignatureSet.InterfaceMembers.TryGetValue(record, out index)) {
                        output.WriteRaw("$im{0:X2}", index);

                        return;
                    }
                }
            }

            if (!Global.InterfaceMembers.TryGetValue(record, out index)) {
                output.Identifier(jsMethod.Reference.DeclaringType, referenceContext, false);
                output.Dot();
                astEmitter.Emit(method);
            } else {
                output.WriteRaw("$IM{0:X2}", index);
                output.LPar();
                if (rewrittenGenericParameters != null) {
                    output.CommaSeparatedList(rewrittenGenericParameters, referenceContext);
                }
                output.RPar();
            }
        }

        public static bool IsTypeArgument (TypeReference typeReference) {
            if (typeReference is GenericParameter) {
                var gp = (GenericParameter) typeReference;
                if (gp.Owner is TypeReference) {
                    var owner = (TypeReference) gp.Owner;
                    if (owner.Scope.Name == GenericParameterHostName && owner.FullName == GenericParameterHostFullName) {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public class GenericParameterComparer : IEqualityComparer<GenericParameter> {
        public bool Equals (GenericParameter x, GenericParameter y) {
            return TypeUtil.TypesAreEqual(x, y);
        }

        public int GetHashCode (GenericParameter obj) {
            return obj.FullName.GetHashCode();
        }
    }
}