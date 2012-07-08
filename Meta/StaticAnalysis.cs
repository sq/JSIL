using System;

namespace JSIL.Meta {
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
}
