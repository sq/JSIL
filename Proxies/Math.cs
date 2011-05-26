using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Math),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class MathProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public static AnyType Min (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        public static AnyType Max (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.abs($value)")]
        public static AnyType Abs (AnyType value) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.sqrt($d)")]
        public static double Sqrt (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.cos($d)")]
        public static double Cos (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.sin($d)")]
        public static double Sin (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.pow($base, $exponent)")]
        public static AnyType Pow (AnyType @base, AnyType exponent) {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        typeof(Random),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class RandomProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public static AnyType Next (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }
}
