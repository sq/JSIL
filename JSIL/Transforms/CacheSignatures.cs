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

        public readonly bool LocalCachingEnabled;
        public readonly Dictionary<MemberIdentifier, Dictionary<CachedSignatureRecord, int>> LocalCachedSignatureSets;
        public readonly Dictionary<CachedSignatureRecord, int> CachedSignatures;
        public readonly Dictionary<CachedInterfaceMemberRecord, int> CachedInterfaceMembers;
        private readonly Stack<JSFunctionExpression> FunctionStack = new Stack<JSFunctionExpression>();
        private readonly CachedSignatureRecordComparer Comparer = new CachedSignatureRecordComparer();
        private readonly CachedInterfaceMemberRecordComparer InterfaceMemberComparer = new CachedInterfaceMemberRecordComparer();

        public SignatureCacher (TypeInfoProvider typeInfo, bool localCachingEnabled) {
            LocalCachedSignatureSets = new Dictionary<MemberIdentifier, Dictionary<CachedSignatureRecord, int>>(
                new MemberIdentifier.Comparer(typeInfo)
            );
            CachedSignatures = new Dictionary<CachedSignatureRecord, int>(Comparer);
            CachedInterfaceMembers = new Dictionary<CachedInterfaceMemberRecord, int>(InterfaceMemberComparer);
            VisitNestedFunctions = true;
            LocalCachingEnabled = localCachingEnabled;
        }

        private void CacheSignature (MethodReference method, MethodSignature signature, bool isConstructor) {
            Func<GenericParameter, bool> filter =
                (gp) => {
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

            Dictionary<CachedSignatureRecord, int> signatureSet;
            int index;

            if (cacheLocally && LocalCachingEnabled) {
                var fn = FunctionStack.Peek();
                if ((fn.Method == null) || (fn.Method.Method == null))
                    return;

                var functionIdentifier = fn.Method.Method.Identifier;
                if (!LocalCachedSignatureSets.TryGetValue(functionIdentifier, out signatureSet))
                    signatureSet = LocalCachedSignatureSets[functionIdentifier] = new Dictionary<CachedSignatureRecord, int>(Comparer);
            } else {
                signatureSet = CachedSignatures;
            }

            var record = new CachedSignatureRecord(method, signature, isConstructor);

            if (!signatureSet.ContainsKey(record))
                signatureSet.Add(record, signatureSet.Count);
        }

        private void CacheInterfaceMember (TypeReference declaringType, string memberName) {
            if (TypeUtil.IsOpenType(declaringType))
                return;

            var record = new CachedInterfaceMemberRecord(declaringType, memberName);

            if (!CachedInterfaceMembers.ContainsKey(record))
                CachedInterfaceMembers.Add(record, CachedInterfaceMembers.Count);
        }

        private JSRawOutputIdentifier MakeRawOutputIdentifierForIndex (TypeReference type, int index) {
            return new JSRawOutputIdentifier(
                (f) => f.WriteRaw("$s{0:X2}", index),
                type
            );
        }

        public void VisitNode (JSFunctionExpression fe) {
            FunctionStack.Push(fe);

            try {
                VisitChildren(fe);
            } finally {
                var functionIdentifier = fe.Method.Method.Identifier;
                Dictionary<CachedSignatureRecord, int> signatureSet;
                if (LocalCachedSignatureSets.TryGetValue(functionIdentifier, out signatureSet)) {
                    var trType = new TypeReference("System", "Type", null, null);

                    int i = 0;
                    foreach (var kvp in signatureSet) {
                        var record = kvp.Key;
                        var stmt = new JSVariableDeclarationStatement(new JSBinaryOperatorExpression(
                            JSOperator.Assignment,
                            MakeRawOutputIdentifierForIndex(trType, kvp.Value),
                            new JSLocalCachedSignatureExpression(trType, record.Method, record.Signature, record.IsConstructor),
                            trType
                        ));

                        fe.Body.Statements.Insert(i++, stmt);
                    }
                }

                FunctionStack.Pop();
            }
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

            if ((method != null) && method.DeclaringType.IsInterface)
                CacheInterfaceMember(jsm.Reference.DeclaringType, jsm.Reference.Name);

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

            if ((enclosingFunction.Method != null) || (enclosingFunction.Method.Method != null)) {
                var functionIdentifier = enclosingFunction.Method.Method.Identifier;
                Dictionary<CachedSignatureRecord, int> localSignatureSet;

                if (LocalCachedSignatureSets.TryGetValue(functionIdentifier, out localSignatureSet)) {
                    CachedSignatureRecord localRecord;
                    if (localSignatureSet.TryGetValue(record, out index)) {
                        output.WriteRaw("$s{0:X2}", index);

                        return;
                    }
                }
            }

            if (!CachedSignatures.TryGetValue(record, out index))
                output.Signature(methodReference, methodSignature, referenceContext, forConstructor, true);
            else
                output.WriteRaw("$S{0:X2}()", index);
        }

        /// <summary>
        /// Writes an interface member reference to the output.
        /// </summary>
        public void WriteInterfaceMemberToOutput (
            JavascriptFormatter output, JavascriptAstEmitter emitter, JSFunctionExpression enclosingFunction,
            MethodReference methodReference, JSExpression method,
            TypeReferenceContext referenceContext
        ) {
            int index;
            var record = new CachedInterfaceMemberRecord(methodReference.DeclaringType, methodReference.Name);

            if (!CachedInterfaceMembers.TryGetValue(record, out index)) {
                output.Identifier(methodReference.DeclaringType, referenceContext, false);
                output.Dot();
                emitter.Visit(method);
            } else
                output.WriteRaw("$IM{0:X2}()", index);
        }
    }
}
