using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Type),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class TypeProxy {
        [JSPolicy(
            read: JSReadPolicy.ReturnDefaultValue,
            write: JSWritePolicy.DiscardValue
        )]
        public static System.Reflection.MemberFilter FilterAttribute;
        [JSPolicy(
            read: JSReadPolicy.ReturnDefaultValue,
            write: JSWritePolicy.DiscardValue
        )]
        public static System.Reflection.MemberFilter FilterName;
        [JSPolicy(
            read: JSReadPolicy.ReturnDefaultValue,
            write: JSWritePolicy.DiscardValue
        )]
        public static System.Reflection.MemberFilter FilterNameIgnoreCase;
    }
}
