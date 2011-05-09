using System;

namespace JSIL.Meta {
    /// <summary>
    /// Specifies that a class, method, property or field should be ignored when translating code to JavaScript.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Method | 
        AttributeTargets.Property | AttributeTargets.Field |
        AttributeTargets.Event | AttributeTargets.Constructor |
        AttributeTargets.Module | AttributeTargets.Struct |
        AttributeTargets.Enum | AttributeTargets.Interface
    )]
    public class JSIgnore : Attribute {
    }

    /// <summary>
    /// Specifies that references to this identifier should be replaced with a specified javascript expression when translating code to JavaScript.
    /// The special token '$this' can be used to refer to the this-reference from within the expression.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Field |
        AttributeTargets.Property | AttributeTargets.Constructor
    )]
    public class JSReplacement : Attribute {
        public JSReplacement (string expression) {
        }
    }

    /// <summary>
    /// Specifies that the name of this member should be changed when translating code to javascript.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Field |
        AttributeTargets.Property
    )]
    public class JSChangeName : Attribute {
        public JSChangeName (string newName) {
        }
    }

    /// <summary>
    /// Specifies that, if overloaded, the correct overload of this method to invoke should be decided at runtime instead of compile time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class JSRuntimeDispatch : Attribute {
    }

    public enum JSProxyMemberPolicy {
        ReplaceDeclared,
        ReplaceNone
    }

    public enum JSProxyAttributePolicy {
        Add,
        Replace
    }

    /// <summary>
    /// Specifies that a type should be treated as a proxy for another type, replacing the target type's members and/or attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class JSProxy : Attribute {
        /// <param name="type">The type to proxy.</param>
        /// <param name="memberPolicy">Determines how members defined in the proxied type should be replaced with members defined by the proxy type.</param>
        /// <param name="attributePolicy">Determines how how attributes defined in the proxied type should be replaced with attributes attached to the proxy type.</param>
        public JSProxy (
            Type type,
            JSProxyMemberPolicy memberPolicy = JSProxyMemberPolicy.ReplaceDeclared,
            JSProxyAttributePolicy attributePolicy = JSProxyAttributePolicy.Add
        ) {
        }

        /// <param name="types">The types to proxy.</param>
        /// <param name="memberPolicy">Determines how members defined in the proxied type should be replaced with members defined by the proxy type.</param>
        /// <param name="attributePolicy">Determines how how attributes defined in the proxied type should be replaced with attributes attached to the proxy type.</param>
        public JSProxy (
            Type[] types,
            JSProxyMemberPolicy memberPolicy = JSProxyMemberPolicy.ReplaceDeclared,
            JSProxyAttributePolicy attributePolicy = JSProxyAttributePolicy.Add
        ) {
        }
    }
}
