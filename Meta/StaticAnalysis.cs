using System;

namespace JSIL.Meta {
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
    /// Specifies that this method should be treated as if it is pure when optimizing javascript.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property
    )]
    public class JSIsPure : Attribute {
    }

    /// <summary>
    /// Specifies that this method's return value does not need to be copied if it is a struct.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property
    )]
    public class JSResultIsNew : Attribute {
    }

    /// <summary>
    /// Provides a list of the names of the arguments mutated by this method for the purposes of javascript optimization.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property
    )]
    public class JSMutatedArguments : Attribute {
        public JSMutatedArguments (params string[] argumentNames) {
        }
    }

    /// <summary>
    /// Provides a list of the names of the arguments that escape from this method for the purposes of javascript optimization.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property
    )]
    public class JSEscapingArguments : Attribute {
        public JSEscapingArguments (params string[] argumentNames) {
        }
    }

    /// <summary>
    /// Tells the static analyzer to treat calls to the Dispose method on this type as pure and subject to optimization.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct
    )]
    public class JSPureDispose : Attribute {
    }

    /// <summary>
    /// Tells the static analyzer that this class represents an array enumerator, and provides the names of the array,
    ///  index and length members so that uses of the enumerator can be replaced with a for-loop or while-loop.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct
    )]
    public class JSIsArrayEnumerator : Attribute {
        public JSIsArrayEnumerator (string arrayMember, string indexMember, string lengthMember) {
        }
    }

    /// <summary>
    /// Tells the static analyzer that the enumerator returned by this method represents an underlying array
    ///  of elements stored in class pointed to by the method's this-reference.
    /// This enables the static analyzer to entirely remove the call to this method and replace it with
    ///  direct access to the array.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method
    )]
    public class JSUnderlyingArray : Attribute {
        public JSUnderlyingArray (string arrayMember, string countMember) {
        }
    }
}
