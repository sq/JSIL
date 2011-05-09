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
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class JSRuntimeDispatch : Attribute {
    }

    /// <summary>
    /// Specifies that this method is implemented externally and should not be generated when translating code to JavaScript
    ///  (but does not prevent use of the method like <see cref="JSIgnore"/> does.)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class JSExternal : Attribute {
    }
}
