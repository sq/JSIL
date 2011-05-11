using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL {
    public class CLRSpecialIdentifiers {
        protected readonly TypeSystem TypeSystem;

        public readonly JSIdentifier MemberwiseClone;

        public CLRSpecialIdentifiers (TypeSystem typeSystem) {
            TypeSystem = typeSystem;

            MemberwiseClone = new JSStringIdentifier("MemberwiseClone", TypeSystem.Object); 
        }
    };

    public class JSSpecialIdentifiers {
        protected readonly TypeSystem TypeSystem;

        public readonly JSIdentifier prototype, eval;
        public readonly JSDotExpression floor;

        public JSSpecialIdentifiers (TypeSystem typeSystem) {
            TypeSystem = typeSystem;

            prototype = Object("prototype");
            eval = new JSFakeMethod("eval", TypeSystem.Object, TypeSystem.String);
            floor = new JSDotExpression(Object("Math"), new JSFakeMethod("floor", TypeSystem.Int64));
        }

        public JSIdentifier call (TypeReference returnType) {
            return new JSFakeMethod("call", returnType);
        }

        protected JSIdentifier Object (string name) {
            return new JSStringIdentifier(name, TypeSystem.Object);
        }
    };

    public class JSILIdentifier : JSIdentifier {
        public readonly TypeSystem TypeSystem;

        public readonly JSDotExpression GlobalNamespace,
            CopyMembers;

        public JSILIdentifier (TypeSystem typeSystem) {
            TypeSystem = typeSystem;

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
            return new JSInvocationExpression(
                Dot("CheckType", TypeSystem.Boolean),
                expression, new JSType(targetType)
            );
        }

        public JSInvocationExpression TryCast (JSExpression expression, TypeReference targetType) {
            return new JSInvocationExpression(
                Dot("TryCast", targetType),
                expression, new JSType(targetType)
            );
        }

        public JSInvocationExpression Cast (JSExpression expression, TypeReference targetType) {
            return new JSInvocationExpression(
                Dot("Cast", targetType),
                expression, new JSType(targetType)
            );
        }

        public JSInvocationExpression NewArray (TypeReference elementType, JSExpression sizeOrArrayInitializer) {
            var arrayType = new ArrayType(elementType);

            return new JSInvocationExpression(
                new JSDotExpression(
                    Dot("Array", TypeSystem.Object),
                    new JSFakeMethod("New", arrayType, arrayType)
                ),
                new JSType(elementType),
                sizeOrArrayInitializer
            );
        }

        public JSInvocationExpression NewMultidimensionalArray (TypeReference elementType, JSExpression[] dimensions) {
            var arrayType = new ArrayType(elementType, dimensions.Length);

            return new JSInvocationExpression(
                new JSDotExpression(
                    Dot("MultidimensionalArray", TypeSystem.Object),
                    new JSFakeMethod("New", arrayType, TypeSystem.Object, TypeSystem.Object)
                ),
                (new JSExpression[] { new JSType(elementType) }.Concat(dimensions)).ToArray()
            );
        }

        public JSInvocationExpression NewDelegate (TypeReference delegateType, JSExpression thisReference, JSExpression targetMethod) {
            return new JSInvocationExpression(
                new JSDotExpression(
                    Dot("Delegate", TypeSystem.Object),
                    new JSFakeMethod("New", delegateType, TypeSystem.String, TypeSystem.Object, TypeSystem.Object)
                ),
                JSLiteral.New(delegateType),
                thisReference,
                targetMethod
            );
        }

        public JSNewExpression NewMemberReference (JSExpression target, JSLiteral member) {
            var resultType = new ByReferenceType(target.GetExpectedType(TypeSystem));

            return new JSNewExpression(
                Dot("MemberReference", resultType),
                target, member
            );
        }

        public JSNewExpression NewReference (JSExpression initialValue) {
            var resultType = new ByReferenceType(initialValue.GetExpectedType(TypeSystem));

            return new JSNewExpression(
                Dot("Variable", resultType),
                initialValue
            );
        }

        public JSNewExpression NewCollectionInitializer (JSArrayExpression values) {
            return new JSNewExpression(
                Dot("CollectionInitializer", TypeSystem.Object),
                values.Values.ToArray()
            );
        }

        public JSInvocationExpression Coalesce (JSExpression left, JSExpression right, TypeReference expectedType) {
            return new JSInvocationExpression(
                Dot("Coalesce", expectedType),
                left, right
            );
        }
    }
}
