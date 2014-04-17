using System;
using JSIL.Meta;
using JSIL.Proxy;
using System.Reflection;

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

    [JSProxy(typeof(MemberInfo), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Reflection_MemberInfo
    {
    }

    [JSProxy(typeof(MethodBase), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Reflection_MethodBase
    {
    }

    [JSProxy(typeof(MethodInfo), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Reflection_MethodInfo
    {
    }

    [JSProxy(typeof(ConstructorInfo), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Reflection_ConstructorInfo
    {
    }

    [JSProxy(typeof(FieldInfo), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Reflection_FieldInfo
    {
    }

    [JSProxy(typeof(EventInfo), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Reflection_EventInfo
    {
    }

    [JSProxy(typeof(PropertyInfo), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Reflection_PropertyInfo
    {
    }

    [JSProxy(typeof(Assembly), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Reflection_Assembly
    {
    }

    [JSProxy("System.Reflection.RuntimeAssembly", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Reflection_RuntimeAssembly
    {
    }

    [JSProxy(typeof(ParameterInfo), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Reflection_ParameterInfo
    {
    }
}
