using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL {
    public class CLRSpecialIdentifiers {
        protected readonly TypeSystem TypeSystem;

        new public readonly JSIdentifier MemberwiseClone;

        public CLRSpecialIdentifiers (TypeSystem typeSystem) {
            TypeSystem = typeSystem;

            MemberwiseClone = new JSStringIdentifier("MemberwiseClone", TypeSystem.Object); 
        }
    };

    public class JSSpecialIdentifiers {
        protected readonly TypeSystem TypeSystem;

        public readonly JSIdentifier prototype, eval;
        public readonly JSFakeMethod toString, charCodeAt;
        public readonly JSDotExpression floor, fromCharCode;

        public JSSpecialIdentifiers (TypeSystem typeSystem) {
            TypeSystem = typeSystem;

            prototype = Object("prototype");
            eval = new JSFakeMethod("eval", TypeSystem.Object, TypeSystem.String);
            toString = new JSFakeMethod("toString", TypeSystem.String);
            floor = new JSDotExpression(Object("Math"), new JSFakeMethod("floor", TypeSystem.Int64));
            fromCharCode = new JSDotExpression(Object("String"), new JSFakeMethod("fromCharCode", TypeSystem.Char, TypeSystem.Int32));
            charCodeAt = new JSFakeMethod("charCodeAt", TypeSystem.Int32, TypeSystem.Char);
        }

        public JSFakeMethod call (TypeReference returnType) {
            return new JSFakeMethod("call", returnType);
        }

        protected JSIdentifier Object (string name) {
            return new JSStringIdentifier(name, TypeSystem.Object);
        }
    };

    public class JSILIdentifier : JSIdentifier {
        public readonly TypeSystem TypeSystem;
        public readonly JSSpecialIdentifiers JS;

        public readonly JSDotExpression GlobalNamespace,
            CopyMembers;

        public JSILIdentifier (TypeSystem typeSystem, JSSpecialIdentifiers js) {
            TypeSystem = typeSystem;
            JS = js;

            GlobalNamespace = Dot("GlobalNamespace", TypeSystem.Object);
            CopyMembers = Dot("CopyMembers", TypeSystem.Void);
        }

        public override string Identifier {
            get { return "JSIL"; }
        }

        protected JSDotExpression Dot (JSIdentifier rhs) {
            return new JSDotExpression(this, rhs);
        }

        protected JSDotExpression Dot (string rhs, TypeReference rhsType = null) {
            return Dot(new JSFakeMethod(rhs, rhsType));
        }

        public JSInvocationExpression CheckType (JSExpression expression, TypeReference targetType) {
            return JSInvocationExpression.InvokeStatic(
                Dot("CheckType", TypeSystem.Boolean),
                new[] { expression, new JSType(targetType) }, true
            );
        }

        public JSInvocationExpression TryCast (JSExpression expression, TypeReference targetType) {
            return JSInvocationExpression.InvokeStatic(
                Dot("TryCast", targetType),
                new[] { expression, new JSType(targetType) }, true
            );
        }

        public JSExpression Cast (JSExpression expression, TypeReference targetType) {
            var currentType = ILBlockTranslator.DereferenceType(expression.GetExpectedType(TypeSystem));
            targetType = ILBlockTranslator.DereferenceType(targetType);

            if (targetType.MetadataType == MetadataType.Char) {
                return JSInvocationExpression.InvokeStatic(JS.fromCharCode, new[] { expression }, true);
            } else if (
                (currentType.MetadataType == MetadataType.Char) &&
                ILBlockTranslator.IsIntegral(targetType)
            ) {
                return JSInvocationExpression.InvokeMethod(JS.charCodeAt, expression, new[] { JSLiteral.New(0) }, true);
            } else if (
                ILBlockTranslator.IsEnum(currentType) &&
                ILBlockTranslator.IsIntegral(targetType)
            ) {
                return new JSDotExpression(
                    expression, new JSStringIdentifier("value", targetType)
                );
            } else {
                return JSInvocationExpression.InvokeStatic(
                    Dot("Cast", targetType),
                    new [] { expression, new JSType(targetType) }, true
                );
            }
        }

        public JSInvocationExpression NewArray (TypeReference elementType, JSExpression sizeOrArrayInitializer) {
            var arrayType = new ArrayType(elementType);

            return JSInvocationExpression.InvokeStatic(
                new JSDotExpression(
                    Dot("Array", TypeSystem.Object), 
                    new JSFakeMethod("New", arrayType, arrayType)
                ), new [] { new JSType(elementType), sizeOrArrayInitializer }
            );
        }

        public JSInvocationExpression NewMultidimensionalArray (TypeReference elementType, JSExpression[] dimensions) {
            var arrayType = new ArrayType(elementType, dimensions.Length);

            return JSInvocationExpression.InvokeStatic(
                new JSDotExpression(
                    Dot("MultidimensionalArray", TypeSystem.Object), 
                    new JSFakeMethod("New", arrayType, TypeSystem.Object, TypeSystem.Object)
                ), new JSExpression[] { new JSType(elementType) }.Concat(dimensions).ToArray()
            );
        }

        public JSInvocationExpression NewDelegate (TypeReference delegateType, JSExpression thisReference, JSExpression targetMethod) {
            return JSInvocationExpression.InvokeStatic(
                new JSDotExpression(
                    Dot("Delegate", TypeSystem.Object),
                    new JSFakeMethod("New", delegateType, TypeSystem.String, TypeSystem.Object, TypeSystem.Object)
                ), new [] { JSLiteral.New(delegateType), thisReference, targetMethod }, true
            );
        }

        public JSNewExpression NewMemberReference (JSExpression target, JSLiteral member) {
            var resultType = new ByReferenceType(member.GetExpectedType(TypeSystem));

            return new JSNewExpression(
                Dot("MemberReference", resultType),
                null, target, member
            );
        }

        public JSNewExpression NewReference (JSExpression initialValue) {
            var resultType = new ByReferenceType(initialValue.GetExpectedType(TypeSystem));

            return new JSNewExpression(
                Dot("Variable", resultType),
                null, initialValue
            );
        }

        public JSNewExpression NewCollectionInitializer (JSArrayExpression values) {
            return new JSNewExpression(
                Dot("CollectionInitializer", TypeSystem.Object),
                null, values.Values.ToArray()
            );
        }

        public JSInvocationExpression Coalesce (JSExpression left, JSExpression right, TypeReference expectedType) {
            return JSInvocationExpression.InvokeStatic(
                Dot("Coalesce", expectedType),
                new[] { left, right }, true
            );
        }

        public JSInvocationExpression ShallowCopy (JSExpression array, JSExpression initializer, TypeReference arrayType) {
            return JSInvocationExpression.InvokeStatic(
                new JSDotExpression(
                    Dot("Array", TypeSystem.Object),
                    new JSFakeMethod("ShallowCopy", TypeSystem.Void, arrayType, arrayType)
                ), new[] { array, initializer }
            );
        }
    }
}
