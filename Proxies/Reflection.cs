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

        // HACK to compensate for the fact that GetType relies on stack crawling to identify the currently executing assembly.

        [JSReplacement("JSIL.ReflectionGetTypeInternal($assemblyof(executing), $typeName, false, false)")]
        public static Type GetType (string typeName) {
            throw new NotImplementedException();
        }

        [JSReplacement("JSIL.ReflectionGetTypeInternal($assemblyof(executing), $typeName, $throwOnFail, false)")]
        public static Type GetType (string typeName, bool throwOnFail) {
            throw new NotImplementedException();
        }

        [JSReplacement("JSIL.ReflectionGetTypeInternal($assemblyof(executing), $typeName, $throwOnFail, $ignoreCase)")]
        public static Type GetType (string typeName, bool throwOnFail, bool ignoreCase) {
            throw new NotImplementedException();
        }
    }

    [JSProxy(
        "System.RuntimeType",
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class RuntimeTypeProxy {
    }
}
