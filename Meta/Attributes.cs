using System;

namespace JSIL.Meta {
    /// <summary>
    /// Specifies that a class, method, property or field should be ignored when translating code to JavaScript.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    public class JSIgnore : Attribute {
    }

    /// <summary>
    /// Specifies that references to this identifier should be replaced with a specified javascript expression when translating code to JavaScript.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public class JSReplacement : Attribute {
        public JSReplacement (string expression) {
        }
    }
}
