using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        new [] { 
            typeof(SByte), typeof(Int16), typeof(Int32), typeof(Int64), 
            typeof(Byte), typeof(UInt16), typeof(UInt32), typeof(UInt64) 
        },
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class IntegerProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        new public static AnyType Parse (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSChangeName("toString")]
        public string ToString (params AnyType[] arguments) {
            return base.ToString();
        }
    }

    [JSProxy(
        new[] { 
            typeof(Single), typeof(Double), typeof(Decimal)
        },
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class NumberProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        new public static AnyType Parse (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSChangeName("toString")]
        public string ToString (params AnyType[] arguments) {
            return base.ToString();
        }
    }

    [JSProxy(
        typeof(Decimal),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class DecimalProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        new public static Decimal op_Explicit (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }
}
