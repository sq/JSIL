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

        public readonly Dictionary<CachedSignatureRecord, CachedSignatureRecord> CachedSignatures;
        private int NextID = 0;

        public SignatureCacher () {
            CachedSignatures = new Dictionary<CachedSignatureRecord, CachedSignatureRecord>();
            VisitNestedFunctions = true;
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

            if (TypeUtil.IsOpenType(signature.ReturnType, filter))
                return;
            else if (signature.ParameterTypes.Any((gp) => TypeUtil.IsOpenType(gp, filter)))
                return;
            else if (TypeUtil.IsOpenType(method.DeclaringType, filter))
                return;

            var record = new CachedSignatureRecord(method, signature, NextID, isConstructor);
            if (!CachedSignatures.ContainsKey(record)) {
                CachedSignatures.Add(record, record);
                NextID++;
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
            JavascriptFormatter output, 
            MethodReference methodReference, MethodSignature methodSignature, 
            TypeReferenceContext referenceContext, 
            bool forConstructor
        ) {
            var record = new CachedSignatureRecord(methodReference, methodSignature, -1, forConstructor);

            if (
                !CachedSignatures.TryGetValue(record, out record) ||
                (record.Method != methodReference) ||
                (record.IsConstructor != forConstructor) 
            )
                output.Signature(methodReference, methodSignature, referenceContext, forConstructor, true);
            else
                output.WriteRaw("$S{0:X2}()", record.Index);
        }
    }
}
