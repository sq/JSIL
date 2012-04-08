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
        public static AnyType Parse (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSReplacement("($this).toString()")]
        public string ToString (params AnyType[] arguments) {
            return base.ToString();
        }

        [JSReplacement("JSIL.CompareNumbers($this, $rhs)")]
        public int CompareTo (AnyType rhs) {
            throw new InvalidOperationException();
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
        public static AnyType Parse (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSReplacement("($this).toString()")]
        public string ToString (params AnyType[] arguments) {
            return base.ToString();
        }

        [JSReplacement("JSIL.CompareNumbers($this, $rhs)")]
        public int CompareTo (AnyType rhs) {
            throw new InvalidOperationException();
        }
    }
}
