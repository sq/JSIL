using System;

namespace JSIL.Meta {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    public class JSIgnore : Attribute {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class JSReplacement : Attribute {
        public readonly string Identifier;
        public readonly bool Qualified;

        public JSReplacement (string identifier, bool qualified) {
            Identifier = identifier;
            Qualified = qualified;
        }
    }
}
