using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Ast {
    // Represents a copy of another JSFunctionExpression with the this-reference replaced
    public class JSLambda : JSLiteralBase<JSFunctionExpression> {
        public readonly JSExpression This;
        public readonly bool UseBind;

        public JSLambda (JSFunctionExpression function, JSExpression @this, bool useBind)
            : base(function) {
            if (@this == null)
                throw new ArgumentNullException("this");

            This = @this;
            UseBind = useBind;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Value.GetActualType(typeSystem);
        }

        public override bool IsConstant {
            get {
                return Value.IsConstant;
            }
        }

        public override bool IsNull {
            get {
                return Value.IsNull;
            }
        }

        public override bool HasGlobalStateDependency {
            get {
                return Value.HasGlobalStateDependency;
            }
        }

        public override IEnumerable<JSNode> Children {
            get {
                if (This != null)
                    yield return This;

                // We never want to recurse into the function pointed to by a lambda when doing tree traversal.
                // yield return Value;
            }
        }
    }

    public class JSDefaultValueLiteral : JSLiteralBase<TypeReference> {
        public JSDefaultValueLiteral (TypeReference type)
            : base(type) {
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return Value;
        }

        public override bool Equals (object obj) {
            var rhs = obj as JSDefaultValueLiteral;

            if (rhs != null) {
                return TypeUtil.TypesAreEqual(Value, rhs.Value);
            } else {
                return base.Equals(obj);
            }
        }

        public override string ToString () {
            return String.Format("default({0})", Value.FullName);
        }
    }

    public class JSNullLiteral : JSLiteralBase<object> {
        public readonly TypeReference Type;

        public JSNullLiteral (TypeReference type)
            : base(null) {

            Type = type;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            if (Type != null)
                return Type;
            else
                return typeSystem.Object;
        }

        public override string ToString () {
            return "null";
        }
    }

    public class JSBooleanLiteral : JSLiteralBase<bool> {
        public JSBooleanLiteral (bool value)
            : base(value) {
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return typeSystem.Boolean;
        }
    }

    public class JSCharLiteral : JSLiteralBase<char> {
        public JSCharLiteral (char value)
            : base(value) {
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return typeSystem.Char;
        }

        public override string ToString () {
            return Util.EscapeCharacter(Value);
        }
    }

    public class JSStringLiteral : JSLiteralBase<string> {
        public JSStringLiteral (string value)
            : base(value) {
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return typeSystem.String;
        }

        public override string ToString () {
            return Util.EscapeString(Value, '"');
        }
    }

    public class JSIntegerLiteral : JSLiteralBase<long> {
        public readonly Type OriginalType;

        public JSIntegerLiteral (long value, Type originalType)
            : base(value) {

            OriginalType = originalType;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            if (OriginalType != null) {
                switch (OriginalType.FullName) {
                    case "System.Byte":
                        return typeSystem.Byte;
                    case "System.SByte":
                        return typeSystem.SByte;
                    case "System.UInt16":
                        return typeSystem.UInt16;
                    case "System.Int16":
                        return typeSystem.Int16;
                    case "System.UInt32":
                        return typeSystem.UInt32;
                    case "System.Int32":
                        return typeSystem.Int32;
                    case "System.UInt64":
                        return typeSystem.UInt64;
                    case "System.Int64":
                        return typeSystem.Int64;
                    default:
                        throw new NotImplementedException(String.Format(
                            "Unsupported integer literal type: {0}",
                            OriginalType.FullName
                        ));
                }
            } else
                return typeSystem.Int64;
        }

        public override string ToString () {
            return String.Format("{0}", Value);
        }
    }

    public class JSEnumLiteral : JSLiteralBase<long> {
        public readonly TypeReference EnumType;
        public readonly string[] Names;

        public JSEnumLiteral (long rawValue, params EnumMemberInfo[] members)
            : base(rawValue) {

            EnumType = members.First().DeclaringType;
            Names = (from m in members select m.Name).OrderBy((s) => s).ToArray();
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return EnumType;
        }

        public override string ToString () {
            return String.Format("<{0}>", String.Join(
                " | ", (from n in Names select String.Format("{0}.{1}", EnumType.Name, n)).ToArray()
            ));
        }
    }

    public class JSNumberLiteral : JSLiteralBase<double> {
        public readonly Type OriginalType;

        public JSNumberLiteral (double value, Type originalType)
            : base(value) {

            OriginalType = originalType;
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            if (OriginalType != null) {
                switch (OriginalType.FullName) {
                    case "System.Single":
                        return typeSystem.Single;
                    case "System.Double":
                        return typeSystem.Double;
                    case "System.Decimal":
                        return new TypeReference(typeSystem.Double.Namespace, "Decimal", typeSystem.Double.Module, typeSystem.Double.Scope, true);
                    default:
                        throw new NotImplementedException(String.Format(
                            "Unsupported number literal type: {0}",
                            OriginalType.FullName
                        ));
                }
            } else
                return typeSystem.Double;
        }
    }

    public class JSTypeNameLiteral : JSLiteralBase<TypeReference> {
        public JSTypeNameLiteral (TypeReference value)
            : base(value) {
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return typeSystem.String;
        }
    }

    public class JSAssemblyNameLiteral : JSLiteralBase<AssemblyDefinition> {
        public JSAssemblyNameLiteral (AssemblyDefinition value)
            : base(value) {
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            return typeSystem.String;
        }
    }

    public class JSVerbatimLiteral : JSLiteral {
        public readonly JSMethod OriginalMethod;
        public readonly TypeReference Type;
        public readonly string Expression;
        public readonly IDictionary<string, JSExpression> Variables;

        public JSVerbatimLiteral (JSMethod originalMethod, string expression, IDictionary<string, JSExpression> variables, TypeReference type = null)
            : base(GetValues(variables)) {

            OriginalMethod = originalMethod;
            Type = type;
            Expression = expression;
            Variables = variables;
        }

        protected static JSExpression[] GetValues (IDictionary<string, JSExpression> variables) {
            if (variables != null)
                return variables.Values.ToArray();
            else
                return new JSExpression[0];
        }

        public override object Literal {
            get { return Expression; }
        }

        public override TypeReference GetActualType (TypeSystem typeSystem) {
            if (Type != null)
                return Type;
            else
                return typeSystem.Object;
        }

        public override void ReplaceChild (JSNode oldChild, JSNode newChild) {
            foreach (var key in Variables.Keys.ToArray()) {
                if (Variables[key] == oldChild)
                    Variables[key] = (JSExpression)newChild;
            }

            base.ReplaceChild(oldChild, newChild);
        }

        public override string ToString () {
            string variablesText = "";

            if (Variables != null)
                variablesText = String.Join(", ", (from kvp in Variables select String.Format("{0}={1}", kvp.Key, kvp.Value)).ToArray());

            return String.Format(
                "Verbatim {0} ({1})", OriginalMethod,
                variablesText
            );
        }
    }
}
