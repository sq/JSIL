using System.IO;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies.Bcl
{
    [JSProxy(typeof(File), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_File
    {
    }

    [JSProxy(typeof (Path), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared,JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_Path
    {
    }

    [JSProxy(typeof(Stream), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_Stream
    {
    }

    [JSProxy(typeof(FileStream), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_FileStream
    {
    }

    [JSProxy(typeof(MemoryStream), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_MemoryStream
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

    [JSProxy(typeof(StreamReader), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_StreamReader
    {
    }

    [JSProxy(typeof(TextReader), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_TextReader
    {
    }

    [JSProxy(typeof(FileSystemInfo), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_FileSystemInfo
    {
    }

    [JSProxy(typeof(DirectoryInfo), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_DirectoryInfo
    {
    }

    [JSProxy(typeof(FileInfo), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_FileInfo
    {
    }

    [JSProxy(typeof(Directory), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_Directory
    {
    }
}
