using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    class ConvertPropertyAccessesToInvocations : JSAstVisitor {
        public readonly TypeSystem TypeSystem;
        public readonly ITypeInfoSource TypeInfo;

        public ConvertPropertyAccessesToInvocations (TypeSystem typeSystem, ITypeInfoSource typeInfo) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
        }

        public static bool CanConvertToInvocation (JSPropertyAccess pa) {
            return (!pa.Property.Property.Metadata.HasAttribute("JSIL.Meta.JSAlwaysAccessAsProperty"));
        }

        private JSExpression ConstructInvocation (
            JSPropertyAccess pa, JSExpression argument = null
        ) {
            JSExpression[] arguments;

            if (argument == null)
                arguments = new JSExpression[0];
            else
                arguments = new JSExpression[] { argument };

            var originalMethod = pa.OriginalMethod;
            var declaringType = originalMethod.Reference.DeclaringType;
            var declaringTypeDef = TypeUtil.GetTypeDefinition(originalMethod.Reference.DeclaringType);
            var thisReferenceType = pa.ThisReference.GetActualType(TypeSystem);
            var isSelf = TypeUtil.TypesAreAssignable(
                TypeInfo, thisReferenceType, declaringType
            );

            bool needsExplicitThis = !pa.IsVirtualCall && ILBlockTranslator.NeedsExplicitThis(
                declaringType, declaringTypeDef,
                originalMethod.Method.DeclaringType,
                isSelf, thisReferenceType,
                originalMethod.Method
            );
                
            JSInvocationExpressionBase invocation;
            if (needsExplicitThis)
                invocation = JSInvocationExpression.InvokeBaseMethod(
                    pa.OriginalMethod.Reference.DeclaringType,
                    pa.OriginalMethod, pa.ThisReference,
                    arguments
                );
            else
                invocation = JSInvocationExpression.InvokeMethod(
                    pa.OriginalMethod, pa.ThisReference, arguments
                );

            JSExpression replacement;
            if (TypeUtil.IsStruct(pa.Property.Property.ReturnType))
                replacement = new JSResultReferenceExpression(invocation);
            else
                replacement = invocation;

            return replacement;
        }

        public void VisitNode (JSPropertyAccess pa) {
            var parentBoe = ParentNode as JSBinaryOperatorExpression;
            var parentUoe = ParentNode as JSUnaryOperatorExpression;

            bool isMutation = ((parentUoe != null) && 
                (parentUoe.Operator is JSUnaryMutationOperator)) ||
                ((parentBoe != null) && (parentBoe.Operator is JSAssignmentOperator));

            if (
                !pa.IsWrite &&
                !isMutation &&
                CanConvertToInvocation(pa)
            ) {
                // getter
                var replacement = ConstructInvocation(pa);

                ParentNode.ReplaceChild(pa, replacement);
                VisitReplacement(replacement);
            } else {
                VisitChildren(pa);
            }
        }

        public void VisitNode (JSBinaryOperatorExpression boe) {
            var pa = boe.Left as JSPropertyAccess;

            if (
                (pa != null) &&
                pa.IsWrite &&
                (boe.Operator == JSOperator.Assignment) &&
                CanConvertToInvocation(pa)
            ) {
                // setter
                var invocation = ConstructInvocation(pa, boe.Right);

                ParentNode.ReplaceChild(boe, invocation);
                VisitReplacement(invocation);
            } else {
                VisitChildren(boe);
            }
        }
    }
}
