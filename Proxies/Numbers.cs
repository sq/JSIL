using System;
using JSIL.Meta;
using JSIL.Proxy;

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
        [JSExternal]
        new public static AnyType Parse (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSChangeName("toString")]
        [JSRuntimeDispatch]
        [JSExternal]
        new public abstract AnyType ToString ();
    }

    [JSProxy(
        new[] { 
            typeof(Single), typeof(Double), typeof(Decimal)
        },
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class NumberProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        new public static AnyType Parse (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSChangeName("toString")]
        [JSRuntimeDispatch]
        [JSExternal]
        new public abstract string ToString ();
    }

    [JSProxy(
        typeof(Decimal),
        JSProxyMemberPolicy.ReplaceNone
    )]
    public abstract class DecimalProxy {
        [JSRuntimeDispatch]
        new public static Decimal op_Explicit (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }
}
