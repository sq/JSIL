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
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Field |
        AttributeTargets.Parameter
    )]
    public class JSReplacement : Attribute {
        public JSReplacement (string Expression) {
        }
    }

    /// <summary>
    /// Specifies that references to this property or event's accessors should be replaced with specified javascript expressions when translating code to JavaScript.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Event
    )]
    public class JSAccessorReplacement : Attribute {
        public JSAccessorReplacement (
            string Get = null,
            string Set = null,
            string Add = null,
            string Remove = null
        ) {
        }
    }
}
