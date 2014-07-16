using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class SignatureCacher : JSAstVisitor {
        public struct CachedSignatureRecord {
            public readonly MethodReference Method;
            public readonly MethodSignature Signature;
            public readonly bool IsConstructor;

            public CachedSignatureRecord (MethodReference method, MethodSignature signature, bool isConstructor) {
                Method = method;
                Signature = signature;
                IsConstructor = isConstructor;
            }

            public override int GetHashCode () {
                return Method.Name.GetHashCode() ^ Signature.GetHashCode() ^ IsConstructor.GetHashCode();
            }

            public bool Equals (ref CachedSignatureRecord rhs) {
                var result =
                    (
                        (Method == rhs.Method) ||
                        (Method.FullName == rhs.Method.FullName)
                    ) &&
                    Signature.Equals(rhs.Signature) &&
                    (IsConstructor == rhs.IsConstructor);

                if (!result)
                    return false;
                else
                    return result;
            }

            public override bool Equals (object obj) {
                if (obj is CachedSignatureRecord) {
                    var rhs = (CachedSignatureRecord)obj;
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

        public struct CachedInterfaceMemberRecord {
            public readonly TypeReference InterfaceType;
            public readonly string InterfaceMember;

            public CachedInterfaceMemberRecord (TypeReference declaringType, string memberName) {
                InterfaceType = declaringType;
                InterfaceMember = memberName;
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
                    var rhs = (CachedInterfaceMemberRecord)obj;
                    return Equals(ref rhs);
                } else
                    return base.Equals(obj);
            }

            public override int GetHashCode () {
                return InterfaceType.Name.GetHashCode() ^ InterfaceMember.GetHashCode();
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
            private static readonly CachedSignatureRecordComparer Comparer = new CachedSignatureRecordComparer();
            private static readonly CachedInterfaceMemberRecordComparer InterfaceMemberComparer = new CachedInterfaceMemberRecordComparer();

            public readonly Dictionary<CachedSignatureRecord, int> Signatures;
            public readonly Dictionary<CachedInterfaceMemberRecord, int> InterfaceMembers;

            public CacheSet () {
                Signatures = new Dictionary<CachedSignatureRecord, int>(Comparer);
                InterfaceMembers = new Dictionary<CachedInterfaceMemberRecord, int>(InterfaceMemberComparer);
            }
        }

        public readonly bool LocalCachingEnabled;
        public readonly Dictionary<MemberIdentifier, CacheSet> LocalCachedSets;
        public readonly CacheSet Global = new CacheSet();
        private readonly Stack<JSFunctionExpression> FunctionStack = new Stack<JSFunctionExpression>();

        public SignatureCacher (TypeInfoProvider typeInfo, bool localCachingEnabled) {
            LocalCachedSets = new Dictionary<MemberIdentifier, CacheSet>(
                new MemberIdentifier.Comparer(typeInfo)
            );
            VisitNestedFunctions = true;
            LocalCachingEnabled = localCachingEnabled;
        }

        private CacheSet GetCacheSet (bool cacheLocally) {
            CacheSet result = Global;

            if (cacheLocally && LocalCachingEnabled) {
                var fn = FunctionStack.Peek();
                if ((fn.Method == null) || (fn.Method.Method == null))
                    return Global;

                var functionIdentifier = fn.Method.Method.Identifier;
                if (!LocalCachedSets.TryGetValue(functionIdentifier, out result))
                    result = LocalCachedSets[functionIdentifier] = new CacheSet();
            }

            return result;
        }

        private void CacheSignature (MethodReference method, MethodSignature signature, bool isConstructor) {
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

            var cacheLocally = false;

            if (TypeUtil.IsOpenType(signature.ReturnType, filter))
                cacheLocally = true;
            else if (signature.ParameterTypes.Any((gp) => TypeUtil.IsOpenType(gp, filter)))
                cacheLocally = true;
            else if (TypeUtil.IsOpenType(method.DeclaringType, filter))
                cacheLocally = true;

            var set = GetCacheSet(cacheLocally);
            var record = new CachedSignatureRecord(method, signature, isConstructor);

            if (!set.Signatures.ContainsKey(record))
                set.Signatures.Add(record, set.Signatures.Count);
        }

        private void CacheInterfaceMember (TypeReference declaringType, string memberName) {
            var cacheLocally = false;

            if (TypeUtil.IsOpenType(declaringType))
                cacheLocally = true;

            var unexpandedType = declaringType;
            if (!TypeUtil.ExpandPositionalGenericParameters(unexpandedType, out declaringType))
                declaringType = unexpandedType;

            var set = GetCacheSet(cacheLocally);
            var record = new CachedInterfaceMemberRecord(declaringType, memberName);

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
                    var trType = new TypeReference("System", "Type", null, null);

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

        public void VisitNode(JSMethodOfExpression methodOf)
        {
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
            var record = new CachedSignatureRecord(methodReference, methodSignature, forConstructor);

            if ((enclosingFunction.Method != null) && (enclosingFunction.Method.Method != null)) {
                var functionIdentifier = enclosingFunction.Method.Method.Identifier;
                CacheSet localSignatureSet;

                if (LocalCachedSets.TryGetValue(functionIdentifier, out localSignatureSet)) {
                    if (localSignatureSet.Signatures.TryGetValue(record, out index)) {
                        output.WriteRaw("$s{0:X2}", index);

                        return;
                    }
                }
            }

            if (!Global.Signatures.TryGetValue(record, out index))
                output.Signature(methodReference, methodSignature, referenceContext, forConstructor, true);
            else
                output.WriteRaw("$S{0:X2}()", index);
        }

        /// <summary>
        /// Writes an interface member reference to the output.
        /// </summary>
        public void WriteInterfaceMemberToOutput (
            JavascriptFormatter output, JavascriptAstEmitter emitter, JSFunctionExpression enclosingFunction,
            JSMethod jsMethod, JSExpression method,
            TypeReferenceContext referenceContext
        ) {
            int index;
            var record = new CachedInterfaceMemberRecord(jsMethod.Reference.DeclaringType, jsMethod.Identifier);

            if ((enclosingFunction.Method != null) || (enclosingFunction.Method.Method != null)) {
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
                emitter.Visit(method);
            } else
                output.WriteRaw("$IM{0:X2}()", index);
        }
    }
}
