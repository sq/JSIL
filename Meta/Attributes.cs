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
    /// Specifies that this type is implemented externally and only stub should  be generated when translating code to JavaScript
    ///  (but does not prevent use of the type like <see cref="JSIgnore"/> and <see cref="JSExternal"/> does.)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class JSStubOnly : Attribute
    {
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
    /// To insert a dollar sign into the replacement expression, write '$$'.
    /// When replacing a constructor, '$this' can be used to refer to the this-reference if the constructor is being called in-place on a struct instance.
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
    /// Specifies that uses of this constructor should be replaced with invocations of a static method.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Constructor
    )]
    public class JSChangeToStaticMethod : Attribute {
        public JSChangeToStaticMethod (string staticMethodName) {
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
    /// If applied to a field, specifies that you wish for JSIL to treat the specified field as if it is immutable.
    /// Struct copies will not be generated for the annotated field or any of its members.
    /// If applied to a class/struct, the class/struct and all its fields are treated as if they are immutable.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct
    )]
    public class JSImmutable : Attribute {
    }

    /// <summary>
    /// Specifies that it is invalid to access this property by invoking its getter/setter
    ///  methods directly in JavaScript.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property
    )]
    public class JSAlwaysAccessAsProperty : Attribute {
    }

    /// <summary>
    /// Specifies that the target should be represented as a packed struct array in JavaScript
    ///  instead of as a normal JavaScript array containing object instances.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [JSIL.Runtime.LinkedType(typeof(JSIL.Runtime.IPackedArray<>))]
    public class JSPackedArray : Attribute {
    }

    /// <summary>
    /// Specifies that JSIL should represent the named argument(s) as packed arrays.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [JSIL.Runtime.LinkedType(typeof(JSIL.Runtime.IPackedArray<>))]
    public class JSPackedArrayArgumentsAttribute : Attribute {
        public readonly string[] ArgumentNames;

        public JSPackedArrayArgumentsAttribute (params string[] argumentNames) {
            ArgumentNames = argumentNames;
        }
    }

    /// <summary>
    /// Specifies that JSIL should represent the function's return value as a packed array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [JSIL.Runtime.LinkedType(typeof(JSIL.Runtime.IPackedArray<>))]
    public class JSPackedArrayReturnValueAttribute : Attribute {
    }

    /// <summary>
    /// Specifies that the function can accept arguments that are packed arrays but does not require them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class JSAllowPackedArrayArgumentsAttribute : Attribute {
    }
}
