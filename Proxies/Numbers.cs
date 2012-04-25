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
        [JSReplacement("($this).toString()")]
        public string ToString (params AnyType[] arguments) {
            return base.ToString();
        }

        [JSReplacement("JSIL.CompareValues($this, $rhs)")]
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
        [JSReplacement("($this).toString()")]
        public string ToString (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSReplacement("isNaN($value)")]
        public static bool IsNaN (NumberProxy value) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.CompareValues($this, $rhs)")]
        public int CompareTo (AnyType rhs) {
            throw new InvalidOperationException();
        }
    }
}
