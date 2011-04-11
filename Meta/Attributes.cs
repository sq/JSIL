using System;

namespace JSIL.Meta {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    public class JSIgnore : Attribute {
    }
}
