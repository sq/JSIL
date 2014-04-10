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
}
