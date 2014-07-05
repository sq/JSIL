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
            floor = new JSDotExpression(Object("Math"), new JSFakeMethod("floor", TypeSystem.Int32, null, methodTypes));
            fromCharCode = new JSDotExpression(Object("String"), new JSFakeMethod("fromCharCode", TypeSystem.Char, new[] { TypeSystem.Int32 }, methodTypes));
            charCodeAt = new JSFakeMethod("charCodeAt", TypeSystem.Int32, new[] { TypeSystem.Char }, methodTypes);
        }

        public JSFakeMethod valueOf (TypeReference returnType) {
            return new JSFakeMethod("valueOf", returnType, null, MethodTypes);
        }

        public JSFakeMethod call (TypeReference returnType) {
            return new JSFakeMethod("call", returnType, null, MethodTypes);
        }

        protected JSIdentifier Object (string name) {
            return new JSStringIdentifier(name, TypeSystem.Object);
        }
    };

    [JSAstIgnoreInheritedMembers]
    public class JSILIdentifier : JSIdentifier {
        public readonly TypeSystem TypeSystem;
        public readonly ITypeInfoSource TypeInfo;
        public readonly MethodTypeFactory MethodTypes;
        public readonly JSSpecialIdentifiers JS;

        [JSAstIgnore]
        public readonly JSDotExpressionBase GlobalNamespace, CopyMembers;

        public JSILIdentifier (
            MethodTypeFactory methodTypes, 
            TypeSystem typeSystem, 
            ITypeInfoSource typeInfo,
            JSSpecialIdentifiers js
        ) {
            TypeSystem = typeSystem;
            TypeInfo = typeInfo;
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

        public JSNewArrayElementReference NewElementReference (JSExpression target, JSExpression index) {
            var arrayType = target.GetActualType(TypeSystem);
            TypeReference resultType;

            if (PackedArrayUtil.IsPackedArrayType(arrayType)) {
                resultType = new ByReferenceType(
                    PackedArrayUtil.GetElementType(arrayType)
                );
            } else if (TypeUtil.IsArray(TypeUtil.DereferenceType(arrayType))) {
                resultType = new ByReferenceType(
                    arrayType.GetElementType()
                );
            } else {
                throw new ArgumentException("Cannot create a reference to an element of a value of type '" + arrayType.FullName + "'", target.ToString());
            }

            if (PackedArrayUtil.IsPackedArrayType(target.GetActualType(TypeSystem)))
                return new JSNewPackedArrayElementReference(resultType, target, index);
            else
                return new JSNewArrayElementReference(resultType, target, index);
        }

        public JSNewExpression NewMemberReference (JSExpression target, JSLiteral member) {
            var resultType = new ByReferenceType(member.GetActualType(TypeSystem));

            return new JSNewExpression(
                Dot("MemberReference", resultType),
                null, null, target, member
            );
        }

        public JSNewBoxedVariable NewReference (JSExpression initialValue) {
            var valueType = initialValue.GetActualType(TypeSystem);

            return new JSNewBoxedVariable(
                initialValue, valueType
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

        public JSInvocationExpression ObjectEquals (JSExpression left, JSExpression right) {
            return JSInvocationExpression.InvokeStatic(
                Dot("ObjectEquals", TypeSystem.Boolean),
                new[] { left, right }, true
            );
        }

        public JSInvocationExpression StructEquals (JSExpression left, JSExpression right) {
            return JSInvocationExpression.InvokeStatic(
                Dot("StructEquals", TypeSystem.Boolean),
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

        public JSExpression NullableHasValue (JSExpression nullableExpression) {
            return new JSBinaryOperatorExpression(
                JSOperator.NotEqual, 
                nullableExpression, new JSNullLiteral(TypeSystem.Object), 
                TypeSystem.Boolean
            );
        }

        public JSExpression ValueOfNullable (JSExpression nullableExpression) {
            if (nullableExpression is JSValueOfNullableExpression)
                return nullableExpression;

            return new JSValueOfNullableExpression(nullableExpression);
        }

        public JSExpression ValueOfNullableOrDefault (JSExpression nullableExpression, JSExpression defaultValue) {
            var valueType = nullableExpression.GetActualType(TypeSystem);
            valueType = TypeUtil.StripNullable(valueType);

            return JSInvocationExpression.InvokeStatic(
                Dot("Nullable_ValueOrDefault", valueType),
                new[] { nullableExpression, defaultValue }, true
            );
        }

        public JSInvocationExpression CreateInstanceOfType (TypeReference type) {
            return JSInvocationExpression.InvokeStatic(
                Dot(new JSFakeMethod("CreateInstanceOfType", type, new[] { TypeSystem.Object }, MethodTypes)),
                new[] { new JSTypeOfExpression(type) }
            );
        }

        public JSInvocationExpression FreezeImmutableObject (JSExpression @object) {
            return JSInvocationExpression.InvokeStatic(
                Dot(new JSFakeMethod("FreezeImmutableObject", TypeSystem.Void, new[] { TypeSystem.Object }, MethodTypes)),
                new[] { @object }
            );
        }

        public JSInvocationExpression StackAlloc (JSExpression sizeInBytes, TypeReference pointerType) {
            if (!pointerType.IsPointer)
                throw new InvalidOperationException("Type being stack-allocated must be a pointer");

            return JSInvocationExpression.InvokeStatic(
                Dot(new JSFakeMethod("StackAlloc", pointerType, new[] { TypeSystem.Int32, TypeSystem.Object }, MethodTypes)),
                new[] { sizeInBytes, new JSType(pointerType.GetElementType()) }
            );
        }

        public JSInvocationExpression CreateNamedFunction (TypeReference resultType, JSExpression name, JSExpression argumentNames, JSExpression body, JSExpression closure = null) {
            var nae = argumentNames as JSNewArrayExpression;
            if (nae != null)
                argumentNames = nae.SizeOrArrayInitializer;

            // FIXME: We should do a cast of the result to ensure it's actually the requested result type instead of just a raw JS function
            return JSInvocationExpression.InvokeStatic(
                Dot(
                    new JSFakeMethod(
                        "CreateNamedFunction", resultType,
                        new[] {
                            TypeSystem.String, new ArrayType(TypeSystem.String), TypeSystem.String, TypeSystem.Object
                        }, MethodTypes
                    )
                ),
                new[] { 
                    name, argumentNames, body, closure
                }
            );
        }

        public JSInvocationExpression ThrowNullReferenceException () {
            return JSInvocationExpression.InvokeStatic(
                Dot(new JSFakeMethod("ThrowNullReferenceException", TypeSystem.Void, new TypeReference[0], MethodTypes)),
                new JSExpression[0]
            );
        }
    }

    public class SpecialIdentifiers {
        public readonly TypeSystem TypeSystem;
        public readonly JSSpecialIdentifiers JS;
        public readonly CLRSpecialIdentifiers CLR;
        public readonly JSILIdentifier JSIL;

        public SpecialIdentifiers (MethodTypeFactory methodTypes, TypeSystem typeSystem, ITypeInfoSource typeInfo) {
            TypeSystem = typeSystem;
            JS = new JSSpecialIdentifiers(methodTypes, typeSystem);
            CLR = new CLRSpecialIdentifiers(typeSystem);
            JSIL = new JSILIdentifier(methodTypes, typeSystem, typeInfo, JS);
        }
    }
}
