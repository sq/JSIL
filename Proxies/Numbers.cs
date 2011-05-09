using System;
using JSIL.Meta;

namespace JSIL.Proxies {
    [JSProxy(
        new [] { 
            typeof(SByte), typeof(Int16), typeof(Int32), typeof(Int64), 
            typeof(Byte), typeof(UInt16), typeof(UInt32), typeof(UInt64) 
        },
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class IntegerProxy {
        [JSRuntimeDispatch]
        new public static object Parse () {
            throw new InvalidOperationException();
        }
    }
}
