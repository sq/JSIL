using System;
using System.Runtime.InteropServices;
using JSIL.Meta;
using JSIL.Proxy;
using System.IO;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(GCHandle),
        memberPolicy: JSProxyMemberPolicy.ReplaceNone,
        attributePolicy: JSProxyAttributePolicy.ReplaceDeclared
    )]
    public abstract class GCHandleProxy {
        [JSAllowPackedArrayArguments]
        public static GCHandle Alloc (object value) {
            throw new NotImplementedException();
        }

        [JSAllowPackedArrayArguments]
        public static GCHandle Alloc (object value, GCHandleType type) {
            throw new NotImplementedException();
        }
    }

    [JSProxy(typeof(IntPtr), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IntPtr
    {
    }

    [JSProxy(typeof(UIntPtr), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_UIntPtr
    {
    }

    [JSProxy("System.Void", JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Void
    {
    }

    [JSProxy(typeof(Marshal), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Runtime_InteropServices_Marshal
    {
    }

    [JSProxy(typeof(GCHandle), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Runtime_InteropServices_GCHandle
    {
    }

    [JSProxy(typeof(BinaryWriter), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_BinaryWriter
    {
    }

    [JSProxy(typeof(BinaryReader), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_BinaryReader
    {
    }

    [JSProxy(typeof(TextReader), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_TextReader
    {
    }

    [JSProxy(typeof(StreamReader), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_StreamReader
    {
    }

    [JSProxy(typeof(TextWriter), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_TextWriter
    {
    }
}
