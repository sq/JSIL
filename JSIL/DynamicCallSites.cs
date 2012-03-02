using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using Microsoft.CSharp.RuntimeBinder;
using Mono.Cecil;

namespace JSIL {
    internal class DynamicCallSiteInfoCollection {
        protected readonly Dictionary<string, DynamicCallSiteInfo> CallSites = new Dictionary<string, DynamicCallSiteInfo>();
        protected readonly Dictionary<string, FieldReference> Aliases = new Dictionary<string, FieldReference>();

        public bool Get (ILVariable localVariable, out DynamicCallSiteInfo info) {
            FieldReference storageField;

            if (Aliases.TryGetValue(localVariable.Name, out storageField))
                return Get(storageField, out info);

            info = null;
            return false;
        }

        public bool Get (FieldReference storageField, out DynamicCallSiteInfo info) {
            return CallSites.TryGetValue(storageField.FullName, out info);
        }

        public DynamicCallSiteInfo InitializeCallSite (FieldReference storageField, string bindingType, TypeReference targetType, JSExpression[] arguments) {
            DynamicCallSiteInfo callSiteInfo;

            switch (bindingType) {
                case "GetIndex":
                    callSiteInfo = new DynamicCallSiteInfo.GetIndex(targetType, arguments);
                break;
                case "SetIndex":
                    callSiteInfo = new DynamicCallSiteInfo.SetIndex(targetType, arguments);
                break;
                case "GetMember":
                    callSiteInfo = new DynamicCallSiteInfo.GetMember(targetType, arguments);
                break;
                case "SetMember":
                    callSiteInfo = new DynamicCallSiteInfo.SetMember(targetType, arguments);
                break;
                case "Invoke":
                    callSiteInfo = new DynamicCallSiteInfo.Invoke(targetType, arguments);
                break;
                case "InvokeMember":
                    callSiteInfo = new DynamicCallSiteInfo.InvokeMember(targetType, arguments);
                break;
                case "UnaryOperation":
                    callSiteInfo = new DynamicCallSiteInfo.UnaryOperation(targetType, arguments);
                break;
                case "BinaryOperation":
                    callSiteInfo = new DynamicCallSiteInfo.BinaryOperation(targetType, arguments);
                break;
                case "Convert":
                    callSiteInfo = new DynamicCallSiteInfo.Convert(targetType, arguments);
                break;
                default:
                    throw new NotImplementedException(String.Format("Call sites of type '{0}' not implemented.", bindingType));
            }

            return CallSites[storageField.FullName] = callSiteInfo;
        }

        public void SetAlias (ILVariable variable, FieldReference fieldReference) {
            Aliases[variable.Name] = fieldReference;
        }
    }

    internal abstract class DynamicCallSiteInfo {
        public readonly TypeReference ReturnType;
        protected readonly JSExpression[] Arguments;

        protected DynamicCallSiteInfo (TypeReference targetType, JSExpression[] arguments) {            
            Arguments = arguments;

            var targetTypeName = targetType.FullName;
            var git = targetType as GenericInstanceType;

            if (targetTypeName.StartsWith("System.Action`")) {
                ReturnType = null;
            } else if (targetTypeName.StartsWith("System.Func`")) {
                ReturnType = git.GenericArguments[git.GenericArguments.Count - 1];
            } else
                throw new NotImplementedException("This type of call site target is not implemented");

            if ((BinderFlags & CSharpBinderFlags.ResultDiscarded) == CSharpBinderFlags.ResultDiscarded)
                ReturnType = null;
        }

        protected JSExpression FixupThisArgument (JSExpression thisArgument, TypeSystem typeSystem) {
            var toe = thisArgument as JSTypeOfExpression;
            if (toe != null)
                return toe.Type;

            var expectedType = thisArgument.GetExpectedType(typeSystem);
            if (expectedType.FullName == "System.Type")
                return JSDotExpression.New(thisArgument, new JSStringIdentifier("__PublicInterface__"));

            return thisArgument;
        }

        protected TypeReference UnwrapType (JSExpression expression) {
            var type = expression as JSType;
            var toe = expression as JSTypeOfExpression;

            var invocation = expression as JSInvocationExpression;
            if (invocation != null) {
                var firstArg = invocation.Arguments.FirstOrDefault();
                type = type ?? firstArg as JSType;
                toe = toe ?? firstArg as JSTypeOfExpression;
            }

            if (toe != null)
                type = toe.Type;

            if (type != null)
                return type.Type;

            throw new NotImplementedException("Unrecognized type expression");
        }

        public abstract JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments);

        public CSharpBinderFlags BinderFlags {
            get {
                return (CSharpBinderFlags)((JSEnumLiteral)Arguments[0]).Value;
            }
        }

        public class InvokeMember : DynamicCallSiteInfo {
            public InvokeMember (TypeReference targetType, JSExpression[] arguments)
                : base (targetType, arguments) {
            }

            public string MemberName {
                get {
                    return ((JSStringLiteral)Arguments[1]).Value;
                }
            }

            public JSExpression TypeArguments {
                get {
                    return Arguments[2];
                }
            }

            public TypeReference UsageContext {
                get {
                    return UnwrapType(Arguments[3]);
                }
            }

            public JSExpression ArgumentInfo {
                get {
                    return Arguments[4];
                }
            }

            public override JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments) {
                var thisArgument = FixupThisArgument(arguments[1], translator.TypeSystem);

                var returnType = ReturnType;
                if (returnType == null)
                    returnType = translator.TypeSystem.Void;

                return JSInvocationExpression.InvokeMethod(
                    new JSStringIdentifier(MemberName, returnType), thisArgument,
                    arguments.Skip(2).ToArray()
                );
            }
        }

        public class Invoke : DynamicCallSiteInfo {
            public Invoke (TypeReference targetType, JSExpression[] arguments)
                : base(targetType, arguments) {
            }

            public TypeReference UsageContext {
                get {
                    return UnwrapType(Arguments[1]);
                }
            }

            public JSExpression ArgumentInfo {
                get {
                    return Arguments[2];
                }
            }

            public override JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments) {
                var thisArgument = arguments[1];

                var returnType = ReturnType;
                if (returnType == null)
                    returnType = translator.TypeSystem.Void;

                return new JSDelegateInvocationExpression(
                    JSChangeTypeExpression.New(thisArgument, translator.TypeSystem, returnType), 
                    returnType, arguments.Skip(2).ToArray()
                );
            }
        }

        public class UnaryOperation : DynamicCallSiteInfo {
            public UnaryOperation (TypeReference targetType, JSExpression[] arguments)
                : base(targetType, arguments) {
            }

            public ExpressionType Operation {
                get {
                    return (ExpressionType)((JSEnumLiteral)Arguments[1]).Value;
                }
            }

            public TypeReference UsageContext {
                get {
                    return UnwrapType(Arguments[2]);
                }
            }

            public JSExpression ArgumentInfo {
                get {
                    return Arguments[3];
                }
            }

            public static JSUnaryOperator GetOperator (ExpressionType et) {
                switch (et) {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        return JSOperator.Negation;
                    case ExpressionType.Not:
                        return JSOperator.BitwiseNot;
                    case ExpressionType.IsTrue:
                        return JSOperator.IsTrue;
                    default:
                        throw new NotImplementedException(String.Format("The unary operator '{0}' is not implemented.", et));
                }
            }

            public override JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments) {
                var returnType = ReturnType;
                if (returnType == null)
                    returnType = translator.TypeSystem.Void;

                switch (Operation) {
                    case ExpressionType.IsTrue:
                        returnType = translator.TypeSystem.Boolean;
                        break;
                }

                return new JSUnaryOperatorExpression(
                    GetOperator(Operation),
                    arguments[1],
                    returnType
                );
            }
        }

        public class BinaryOperation : DynamicCallSiteInfo {
            public BinaryOperation (TypeReference targetType, JSExpression[] arguments)
                : base(targetType, arguments) {
            }

            public ExpressionType Operation {
                get {
                    return (ExpressionType)((JSEnumLiteral)Arguments[1]).Value;
                }
            }

            public TypeReference UsageContext {
                get {
                    return UnwrapType(Arguments[2]);
                }
            }

            public JSExpression ArgumentInfo {
                get {
                    return Arguments[3];
                }
            }

            public static JSBinaryOperator GetOperator (ExpressionType et) {
                switch (et) {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                        return JSOperator.Add;
                    case ExpressionType.And:
                        return JSOperator.BitwiseAnd;
                    case ExpressionType.AndAlso:
                        return JSOperator.LogicalAnd;
                    case ExpressionType.Divide:
                        return JSOperator.Divide;
                    case ExpressionType.Equal:
                        return JSOperator.Equal;
                    case ExpressionType.ExclusiveOr:
                        return JSOperator.BitwiseXor;
                    case ExpressionType.GreaterThan:
                        return JSOperator.GreaterThan;
                    case ExpressionType.GreaterThanOrEqual:
                        return JSOperator.GreaterThanOrEqual;
                    case ExpressionType.LeftShift:
                        return JSOperator.ShiftLeft;
                    case ExpressionType.LessThan:
                        return JSOperator.LessThan;
                    case ExpressionType.LessThanOrEqual:
                        return JSOperator.LessThanOrEqual;
                    case ExpressionType.Modulo:
                        return JSOperator.Remainder;
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                        return JSOperator.Multiply;
                    case ExpressionType.NotEqual:
                        return JSOperator.NotEqual;
                    case ExpressionType.Or:
                        return JSOperator.BitwiseOr;
                    case ExpressionType.OrElse:
                        return JSOperator.LogicalOr;
                    case ExpressionType.RightShift:
                        return JSOperator.ShiftRight;
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        return JSOperator.Subtract;
                    case ExpressionType.AddAssign:
                    case ExpressionType.AddAssignChecked:
                        return JSOperator.AddAssignment;
                    case ExpressionType.AndAssign:
                        return JSOperator.BitwiseAndAssignment;
                    case ExpressionType.DivideAssign:
                        return JSOperator.DivideAssignment;
                    case ExpressionType.ExclusiveOrAssign:
                        return JSOperator.BitwiseXorAssignment;
                    case ExpressionType.LeftShiftAssign:
                        return JSOperator.ShiftLeftAssignment;
                    case ExpressionType.ModuloAssign:
                        return JSOperator.RemainderAssignment;
                    case ExpressionType.MultiplyAssign:
                        return JSOperator.MultiplyAssignment;
                    case ExpressionType.MultiplyAssignChecked:
                    case ExpressionType.OrAssign:
                        return JSOperator.BitwiseOrAssignment;
                    case ExpressionType.RightShiftAssign:
                        return JSOperator.ShiftRightAssignment;
                    case ExpressionType.SubtractAssign:
                    case ExpressionType.SubtractAssignChecked:
                        return JSOperator.SubtractAssignment;
                    default:
                        throw new NotImplementedException(String.Format("The binary operator '{0}' is not implemented.", et));
                }
            }

            public override JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments) {
                var returnType = ReturnType;
                if (returnType == null)
                    returnType = translator.TypeSystem.Void;

                switch (Operation) {
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThanOrEqual:
                        returnType = translator.TypeSystem.Boolean;
                    break;
                }

                return new JSBinaryOperatorExpression(
                    GetOperator(Operation), 
                    arguments[1], arguments[2], 
                    returnType
                );
            }
        }

        public class Convert : DynamicCallSiteInfo {
            public Convert (TypeReference targetType, JSExpression[] arguments)
                : base(targetType, arguments) {
            }

            public TypeReference TargetType {
                get {
                    return UnwrapType(Arguments[1]);
                }
            }

            public TypeReference UsageContext {
                get {
                    return UnwrapType(Arguments[2]);
                }
            }

            public override JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments) {
                return JSCastExpression.New(
                    arguments[1],
                    TargetType
                );
            }
        }

        public class GetMember : DynamicCallSiteInfo {
            public GetMember (TypeReference targetType, JSExpression[] arguments)
                : base(targetType, arguments) {
            }

            public string MemberName {
                get {
                    return ((JSStringLiteral)Arguments[1]).Value;
                }
            }

            public TypeReference UsageContext {
                get {
                    return UnwrapType(Arguments[2]);
                }
            }

            public JSExpression ArgumentInfo {
                get {
                    return Arguments[3];
                }
            }

            public override JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments) {
                var thisArgument = FixupThisArgument(arguments[1], translator.TypeSystem);

                var returnType = ReturnType;
                if (returnType == null)
                    returnType = translator.TypeSystem.Void;

                return JSDotExpression.New(
                    thisArgument,
                    new JSStringIdentifier(MemberName, returnType)
                );
            }
        }

        public class SetMember : DynamicCallSiteInfo {
            public SetMember (TypeReference targetType, JSExpression[] arguments)
                : base(targetType, arguments) {
            }

            public string MemberName {
                get {
                    return ((JSStringLiteral)Arguments[1]).Value;
                }
            }

            public TypeReference UsageContext {
                get {
                    return UnwrapType(Arguments[2]);
                }
            }

            public JSExpression ArgumentInfo {
                get {
                    return Arguments[3];
                }
            }

            public override JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments) {
                var thisArgument = FixupThisArgument(arguments[1], translator.TypeSystem);

                var returnType = ReturnType;
                if (returnType == null)
                    returnType = translator.TypeSystem.Void;

                return new JSBinaryOperatorExpression(
                    JSBinaryOperator.Assignment,
                    JSDotExpression.New(
                        thisArgument,
                        new JSStringIdentifier(MemberName, returnType)
                    ),
                    arguments[2], returnType
                );
            }
        }

        public class GetIndex : DynamicCallSiteInfo {
            public GetIndex (TypeReference targetType, JSExpression[] arguments)
                : base(targetType, arguments) {
            }

            public TypeReference UsageContext {
                get {
                    return UnwrapType(Arguments[1]);
                }
            }

            public JSExpression ArgumentInfo {
                get {
                    return Arguments[2];
                }
            }

            public override JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments) {
                var thisArgument = arguments[1];

                var returnType = ReturnType;
                if (returnType == null)
                    returnType = translator.TypeSystem.Void;

                return new JSIndexerExpression(
                    thisArgument,
                    arguments[2],
                    returnType
                );
            }
        }

        public class SetIndex : DynamicCallSiteInfo {
            public SetIndex (TypeReference targetType, JSExpression[] arguments)
                : base(targetType, arguments) {
            }

            public TypeReference UsageContext {
                get {
                    return UnwrapType(Arguments[1]);
                }
            }

            public JSExpression ArgumentInfo {
                get {
                    return Arguments[2];
                }
            }

            public override JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments) {
                var thisArgument = arguments[1];

                var returnType = ReturnType;
                if (returnType == null)
                    returnType = translator.TypeSystem.Void;

                return new JSBinaryOperatorExpression(
                    JSBinaryOperator.Assignment,
                    new JSIndexerExpression(
                        thisArgument,
                        arguments[2],
                        returnType
                    ),
                    arguments[3], returnType
                );
            }
        }
    }
}
