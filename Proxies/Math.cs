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

        [JSReplacement("Math.tan($d)")]
        public static double Tan (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.round($d)")]
        public static double Round (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.floor($d)")]
        public static double Floor (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("($d | 0)")]
        public static double Truncate (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.ceil($d)")]
        public static double Ceiling (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.pow($base, $exponent)")]
        public static AnyType Pow (AnyType @base, AnyType exponent) {
            throw new InvalidOperationException();
        }
    }
}
