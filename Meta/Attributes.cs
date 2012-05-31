using System;

namespace JSIL.Meta {
    public enum JSReadPolicy {
        Unmodified,
        LogWarning,
        ThrowError,
        ReturnDefaultValue
    }

    public enum JSInvokePolicy {
        Unmodified,
        LogWarning,
        ThrowError,
        ReturnDefaultValue
    }

    public enum JSWritePolicy {
        Unmodified,
        LogWarning,
        ThrowError,
        DiscardValue
    }

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
    /// Specifies a policy to apply to reads, writes, or invocations of a member when translating code to JavaScript.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Property | 
        AttributeTargets.Field | AttributeTargets.Event | 
        AttributeTargets.Constructor
    )]
    public class JSPolicy : Attribute {
        public JSPolicy (
            JSReadPolicy read = JSReadPolicy.Unmodified,
            JSWritePolicy write = JSWritePolicy.Unmodified,
            JSInvokePolicy invoke = JSInvokePolicy.Unmodified
        ) {
        }
    }

    /// <summary>
    /// Specifies that references to this identifier should be replaced with a specified javascript expression when translating code to JavaScript.
    /// To refer to a parameter within the replacement expression, prefix the parameter name with a dollar sign - the this-reference becomes $this, for example.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Constructor |
        AttributeTargets.Property | AttributeTargets.Class | 
        AttributeTargets.Struct
    )]
    public class JSReplacement : Attribute {
        public JSReplacement (string expression) {
        }
    }

    /// <summary>
    /// Specifies that the name of this member or type should be changed when translating code to javascript.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Field |
        AttributeTargets.Property | AttributeTargets.Class | 
        AttributeTargets.Struct | AttributeTargets.Enum
    )]
    public class JSChangeName : Attribute {
        public JSChangeName (string newName) {
        }
    }

    /// <summary>
    /// Specifies that, if overloaded, the correct overload of this method to invoke should be decided at runtime instead of compile time.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Constructor |
        AttributeTargets.Property                
    )]
    public class JSRuntimeDispatch : Attribute {
    }

    /// <summary>
    /// Specifies that this member or type is implemented externally and should not be generated when translating code to JavaScript
    ///  (but does not prevent use of the member/type like <see cref="JSIgnore"/> does.)
    /// Note that while external methods will generate a clear warning at runtime if used without being defined, the same is not true
    ///  for fields or classes - fields will simply be undefined, and classes may produce a JavaScript TypeError or ReferenceError.
    /// The behavior of external properties depends on where you apply the attribute: Marking the property itself as external means
    ///  that the property definition will be omitted and the getter and setter will not be translated.
    /// Marking a property's getter or setter as external behaves the same as marking a method as external - the property definition
    ///  will still be translated, so once the externals are implemented the property will work as expected.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Constructor |
        AttributeTargets.Property | AttributeTargets.Class |
        AttributeTargets.Field
    )]
    public class JSExternal : Attribute {
    }

    /// <summary>
    /// Specifies that this method should be renamed to .cctor2 so that it runs as a second static constructor for the containing
    ///  type in JS. If the method is part of a proxy, it will run as the second static constructor for the proxied type(s).
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Constructor
    )]
    public class JSExtraStaticConstructor : Attribute {
    }

    /// <summary>
    /// Specifies that you wish to replace an existing constructor with one from your proxy. This is necessary because
    ///  the compiler automatically generates hidden constructors for your proxy classes.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Constructor
    )]
    public class JSReplaceConstructor : Attribute {
    }

    /// <summary>
    /// Specifies that you wish for JSIL to treat the specified field as if it is immutable.
    /// Struct copies will not be generated for the annotated field or any of its members.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Field
    )]
    public class JSImmutable : Attribute {
    }
}
