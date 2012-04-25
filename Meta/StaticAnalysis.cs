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
}
