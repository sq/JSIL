using System;
using System.IO;
using JSIL.Meta;
using JSIL.Proxy;
using Microsoft.Win32.SafeHandles;

namespace JSIL.Proxies {
    [JSProxy(
        "System.IO.FileStream",
        memberPolicy: JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class FileStreamProxy {
        [JSIgnore]
        public FileStreamProxy (IntPtr handle, AnyType access) {
            throw new NotImplementedException();
        }
        [JSIgnore]
		public FileStreamProxy(IntPtr handle, AnyType access, bool ownsHandle) {
            throw new NotImplementedException();
        }
        [JSIgnore]
		public FileStreamProxy(IntPtr handle, AnyType access, bool ownsHandle, int bufferSize) {
            throw new NotImplementedException();
        }
        [JSIgnore]
		public FileStreamProxy(IntPtr handle, AnyType access, bool ownsHandle, int bufferSize, bool isAsync) {
            throw new NotImplementedException();
        }
        [JSIgnore]
        public FileStreamProxy (SafeFileHandle handle, AnyType access) {
            throw new NotImplementedException();
        }
        [JSIgnore]
		public FileStreamProxy(SafeFileHandle handle, AnyType access, int bufferSize) {
            throw new NotImplementedException();
        }
        [JSIgnore]
        public FileStreamProxy (SafeFileHandle handle, AnyType access, int bufferSize, bool isAsync) {
            throw new NotImplementedException();
        }
    }

    [JSProxy(typeof(FileStream), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_FileStream
    {
    }

    [JSProxy(
        typeof(TextReader),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class TextReaderProxy
    {
        [JSReplacement("System.IO.TextReader.Dispose()")]
        [JSIsPure]
        public void Dispose()
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.IO.TextReader.Dispose($b)")]
        [JSIsPure]
        public void Dispose(bool b)
        {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(typeof(MarshalByRefObject), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_MarshalByRefObject
    {
    }

    [JSProxy(typeof(File), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_File
    {
    }

    [JSProxy(typeof(Path), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_Path
    {
    }

    [JSProxy(typeof(Stream), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_Stream
    {
    }

    [JSProxy(typeof(MemoryStream), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_MemoryStream
    {
    }

    [JSProxy(typeof(SeekOrigin), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_IO_SeekOrigin
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
