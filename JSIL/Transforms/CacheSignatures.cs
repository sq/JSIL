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
            public readonly int Index;
            public readonly bool IsConstructor;

            public CachedSignatureRecord (MethodReference method, MethodSignature signature, int index, bool isConstructor) {
                Method = method;
                Signature = signature;
                Index = index;
                IsConstructor = isConstructor;
            }

            public override int GetHashCode () {
                return Method.GetHashCode() ^ Signature.GetHashCode() ^ IsConstructor.GetHashCode();
            }

            public bool Equals (CachedSignatureRecord rhs) {
                return
                    (Method == rhs.Method) &&
                    Signature.Equals(rhs.Signature) &&
                    (IsConstructor == rhs.IsConstructor);
            }

            public override bool Equals (object obj) {
                if (obj is CachedSignatureRecord)
                    return Equals((CachedSignatureRecord)obj);
                else
                    return base.Equals(obj);
            }
        }

        public readonly bool LocalCachingEnabled;
        public readonly Dictionary<MemberIdentifier, Dictionary<CachedSignatureRecord, CachedSignatureRecord>> LocalCachedSignatureSets;
        public readonly Dictionary<CachedSignatureRecord, CachedSignatureRecord> CachedSignatures;
        private readonly Stack<JSFunctionExpression> FunctionStack = new Stack<JSFunctionExpression>();
        private int NextID = 0;

        public SignatureCacher (TypeInfoProvider typeInfo, bool localCachingEnabled) {
            LocalCachedSignatureSets = new Dictionary<MemberIdentifier, Dictionary<CachedSignatureRecord, CachedSignatureRecord>>(
                new MemberIdentifier.Comparer(typeInfo)
            );
            CachedSignatures = new Dictionary<CachedSignatureRecord, CachedSignatureRecord>();
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

            Dictionary<CachedSignatureRecord, CachedSignatureRecord> signatureSet;

            if (cacheLocally && LocalCachingEnabled) {
                var fn = FunctionStack.Peek();
                if ((fn.Method == null) || (fn.Method.Method == null))
                    return;

                var functionIdentifier = fn.Method.Method.Identifier;
                if (!LocalCachedSignatureSets.TryGetValue(functionIdentifier, out signatureSet))
                    signatureSet = LocalCachedSignatureSets[functionIdentifier] = new Dictionary<CachedSignatureRecord, CachedSignatureRecord>();
            } else {
                signatureSet = CachedSignatures;
            }

            var record = new CachedSignatureRecord(method, signature, signatureSet.Count, isConstructor);

            if (!signatureSet.ContainsKey(record))
                signatureSet.Add(record, record);
        }

        public void VisitNode (JSFunctionExpression fe) {
            FunctionStack.Push(fe);

            try {
                VisitChildren(fe);
            } finally {
                var functionIdentifier = fe.Method.Method.Identifier;
                Dictionary<CachedSignatureRecord, CachedSignatureRecord> signatureSet;
                if (LocalCachedSignatureSets.TryGetValue(functionIdentifier, out signatureSet)) {
                    var trType = new TypeReference("System", "Type", null, null);

                    int i = 0;
                    foreach (var record in signatureSet.Values) {
                        var stmt = new JSVariableDeclarationStatement(new JSBinaryOperatorExpression(
                            JSOperator.Assignment,
                            new JSRawOutputIdentifier(
                                (f) => f.WriteRaw("$s{0:X2}", record.Index),
                                trType
                            ),
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

        public void WriteToOutput (
            JavascriptFormatter output, JSFunctionExpression enclosingFunction,
            MethodReference methodReference, MethodSignature methodSignature, 
            TypeReferenceContext referenceContext, 
            bool forConstructor
        ) {
            var record = new CachedSignatureRecord(methodReference, methodSignature, -1, forConstructor);

            if ((enclosingFunction.Method != null) || (enclosingFunction.Method.Method != null)) {
                var functionIdentifier = enclosingFunction.Method.Method.Identifier;
                Dictionary<CachedSignatureRecord, CachedSignatureRecord> localSignatureSet;

                if (LocalCachedSignatureSets.TryGetValue(functionIdentifier, out localSignatureSet)) {
                    CachedSignatureRecord localRecord;
                    if (localSignatureSet.TryGetValue(record, out localRecord)) {
                        output.WriteRaw("$s{0:X2}", localRecord.Index);

                        return;
                    }
                }
            }

            if (!CachedSignatures.TryGetValue(record, out record))
                output.Signature(methodReference, methodSignature, referenceContext, forConstructor, true);
            else
                output.WriteRaw("$S{0:X2}()", record.Index);
        }
    }
}
