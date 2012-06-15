using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
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
        protected readonly MethodTypeFactory MethodTypes;

        public readonly JSIdentifier prototype, eval;
        public readonly JSFakeMethod toString, charCodeAt;
        public readonly JSDotExpressionBase floor, fromCharCode;

        public JSSpecialIdentifiers (MethodTypeFactory methodTypes, TypeSystem typeSystem) {
            TypeSystem = typeSystem;
            MethodTypes = methodTypes;

            prototype = Object("prototype");
            eval = new JSFakeMethod("eval", TypeSystem.Object, new[] { TypeSystem.String }, methodTypes);
            toString = new JSFakeMethod("toString", TypeSystem.String, null, methodTypes);
            floor = new JSDotExpression(Object("Math"), new JSFakeMethod("floor", TypeSystem.Int64, null, methodTypes));
            fromCharCode = new JSDotExpression(Object("String"), new JSFakeMethod("fromCharCode", TypeSystem.Char, new[] { TypeSystem.Int32 }, methodTypes));
            charCodeAt = new JSFakeMethod("charCodeAt", TypeSystem.Int32, new[] { TypeSystem.Char }, methodTypes);
        }

        public JSFakeMethod Number (TypeReference returnType) {
            return new JSFakeMethod("Number", returnType, null, MethodTypes);
        }

        public JSFakeMethod call (TypeReference returnType) {
            return new JSFakeMethod("call", returnType, null, MethodTypes);
        }

        protected JSIdentifier Object (string name) {
            return new JSStringIdentifier(name, TypeSystem.Object);
        }
    };

    public class JSILIdentifier : JSIdentifier {
        public readonly TypeSystem TypeSystem;
        public readonly MethodTypeFactory MethodTypes;
        public readonly JSSpecialIdentifiers JS;

        public readonly JSDotExpressionBase GlobalNamespace, CopyMembers;

        public JSILIdentifier (MethodTypeFactory methodTypes, TypeSystem typeSystem, JSSpecialIdentifiers js) {
            TypeSystem = typeSystem;
            MethodTypes = methodTypes;
            JS = js;

            GlobalNamespace = Dot("GlobalNamespace", TypeSystem.Object);
            CopyMembers = Dot("CopyMembers", TypeSystem.Void);
        }

        public override string Identifier {
            get { return "JSIL"; }
        }

        protected JSDotExpressionBase Dot (JSIdentifier rhs) {
            return new JSDotExpression(this, rhs);
        }

        protected JSDotExpressionBase Dot (string rhs, TypeReference rhsType = null) {
            return Dot(new JSFakeMethod(rhs, rhsType, null, MethodTypes));
        }

        public JSInvocationExpression GetTypeFromAssembly (JSExpression assembly, JSExpression typeName, JSExpression throwOnFail) {
            return JSInvocationExpression.InvokeStatic(
                Dot("GetTypeFromAssembly", new TypeReference("System", "Type", TypeSystem.Object.Module, TypeSystem.Object.Scope)),
                new[] { assembly, typeName, new JSNullLiteral(TypeSystem.Object), throwOnFail }, true
            );
        }

        public JSInvocationExpression GetTypeOf (JSExpression expression) {
            return JSInvocationExpression.InvokeStatic(
                Dot("GetType", new TypeReference("System", "Type", TypeSystem.Object.Module, TypeSystem.Object.Scope)),
                new[] { expression }, true
            );
        }

        public JSNewArrayExpression NewArray (TypeReference elementType, JSExpression sizeOrArrayInitializer) {
            return new JSNewArrayExpression(elementType, sizeOrArrayInitializer);
        }

        public JSNewArrayExpression NewMultidimensionalArray (TypeReference elementType, JSExpression[] dimensions, JSExpression initializer = null) {
            return new JSNewArrayExpression(elementType, dimensions, initializer);
        }

        public JSInvocationExpression NewDelegate (TypeReference delegateType, JSExpression thisReference, JSExpression targetMethod) {
            return JSInvocationExpression.InvokeStatic(
                new JSDotExpression(
                    new JSType(delegateType),
                    new JSFakeMethod("New", delegateType, new[] { TypeSystem.Object, TypeSystem.Object }, MethodTypes)
                ), new [] { thisReference, targetMethod },
                true
            );
        }

        public JSNewExpression NewMemberReference (JSExpression target, JSLiteral member) {
            var resultType = new ByReferenceType(member.GetActualType(TypeSystem));

            return new JSNewExpression(
                Dot("MemberReference", resultType),
                null, null, target, member
            );
        }

        public JSNewExpression NewReference (JSExpression initialValue) {
            var resultType = new ByReferenceType(initialValue.GetActualType(TypeSystem));

            return new JSNewExpression(
                Dot("Variable", resultType),
                null, null, initialValue
            );
        }

        public JSNewExpression NewCollectionInitializer (IEnumerable<JSArrayExpression> values) {
            return new JSNewExpression(
                Dot("CollectionInitializer", TypeSystem.Object),
                null, null, values.ToArray()
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
                    new JSFakeMethod("ShallowCopy", TypeSystem.Void, new[] { arrayType, arrayType }, MethodTypes)
                ), new[] { array, initializer }
            );
        }
    }

    public class SpecialIdentifiers {
        public readonly TypeSystem TypeSystem;
        public readonly JSSpecialIdentifiers JS;
        public readonly CLRSpecialIdentifiers CLR;
        public readonly JSILIdentifier JSIL;

        public SpecialIdentifiers (MethodTypeFactory methodTypes, TypeSystem typeSystem) {
            TypeSystem = typeSystem;
            JS = new JSSpecialIdentifiers(methodTypes, typeSystem);
            CLR = new CLRSpecialIdentifiers(typeSystem);
            JSIL = new JSILIdentifier(methodTypes, typeSystem, JS);
        }
    }
}
