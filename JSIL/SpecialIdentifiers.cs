using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL {
    public class CLRSpecialIdentifiers {
        protected readonly TypeSystem TypeSystem;

        public readonly JSIdentifier Length,
            MemberwiseClone;

        public CLRSpecialIdentifiers (TypeSystem typeSystem) {
            TypeSystem = typeSystem;

            Length = new JSIdentifier("Length", TypeSystem.Int32);
            MemberwiseClone = new JSIdentifier("MemberwiseClone", TypeSystem.Object); 
        }
    };

    public class JSSpecialIdentifiers {
        protected readonly TypeSystem TypeSystem;

        public readonly JSIdentifier prototype,
            call;

        public JSSpecialIdentifiers (TypeSystem typeSystem) {
            TypeSystem = typeSystem;

            prototype = Object("prototype");
            call = Object("call");
        }

        protected JSIdentifier Object (string name) {
            return new JSIdentifier(name, TypeSystem.Object);
        }
    };

    public class JSILIdentifier : JSIdentifier {
        protected readonly TypeSystem TypeSystem;

        public readonly JSDotExpression UntranslatableNode,
            UntranslatableInstruction;

        public JSILIdentifier (TypeSystem typeSystem)
            : base("JSIL", typeSystem.Object) {

            TypeSystem = typeSystem;

            UntranslatableNode = Dot("UntranslatableNode", TypeSystem.Void);
            UntranslatableInstruction = Dot("UntranslatableInstruction", TypeSystem.Void);
        }

        protected JSDotExpression Dot (JSIdentifier rhs) {
            return new JSDotExpression(this, rhs);
        }

        protected JSDotExpression Dot (string rhs, TypeReference rhsType = null) {
            return Dot(new JSIdentifier(rhs, rhsType));
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
                    new JSIdentifier("New", arrayType)
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
                    new JSIdentifier("New", arrayType)
                ),
                (new JSExpression[] { new JSType(elementType) }.Concat(dimensions)).ToArray()
            );
        }

        public JSInvocationExpression NewDelegate (TypeReference delegateType, JSExpression thisReference, JSExpression targetMethod) {
            return new JSInvocationExpression(
                new JSDotExpression(
                    Dot("Delegate", TypeSystem.Object),
                    new JSIdentifier("New", delegateType)
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

        public JSNewExpression NewCollectionInitializer (JSArrayExpression values) {
            return new JSNewExpression(
                Dot("CollectionInitializer", TypeSystem.Object),
                values.Values.ToArray()
            );
        }

        public JSInvocationExpression Coalesce (JSExpression left, JSExpression right) {
            var tLeft = left.GetExpectedType(TypeSystem);
            var tRight = right.GetExpectedType(TypeSystem);

            if (tLeft.FullName != tRight.FullName)
                throw new NotImplementedException(String.Format(
                    "Coalescing two different types is not implemented: {0}, {1}",
                    tLeft, tRight
                ));

            return new JSInvocationExpression(
                Dot("Coalesce", tLeft),
                left, right
            );
        }
    }
}
