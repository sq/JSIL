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
        }

        public readonly Dictionary<MethodSignature, CachedSignatureRecord> CachedSignatures;
        private int NextID = 0;

        public SignatureCacher () {
            CachedSignatures = new Dictionary<MethodSignature, CachedSignatureRecord>();
        }

        private void CacheSignature (MethodReference method, MethodSignature signature, bool isConstructor) {
            if (TypeUtil.IsOpenType(signature.ReturnType))
                return;
            else if (signature.ParameterTypes.Any(TypeUtil.IsOpenType))
                return;

            CachedSignatureRecord record;
            if (!CachedSignatures.TryGetValue(signature, out record))
                CachedSignatures.Add(signature, record = new CachedSignatureRecord(method, signature, NextID++, isConstructor));
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

            if (isOverloaded && JavascriptAstEmitter.CanUseFastOverloadDispatch(ctor))
                isOverloaded = false;

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
            CachedSignatureRecord record;
            if (
                !CachedSignatures.TryGetValue(methodSignature, out record) ||
                (record.Method != methodReference) ||
                (record.IsConstructor != forConstructor) 
            )
                output.Signature(methodReference, methodSignature, referenceContext, forConstructor, true);
            else
                output.WriteRaw("$S{0:X2}()", record.Index);
        }
    }
}
