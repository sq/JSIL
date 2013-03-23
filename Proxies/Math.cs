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
        [JSIsPure]
        public static AnyType Abs (AnyType value) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.sqrt($d)")]
        [JSIsPure]
        public static double Sqrt (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.cos($d)")]
        [JSIsPure]
        public static double Cos (double d) {
            throw new InvalidOperationException();
        }
        
        [JSReplacement("Math.sin($d)")]
        [JSIsPure]
        public static double Sin (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.acos($d)")]
        [JSIsPure]
        public static double Acos (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.asin($d)")]
        [JSIsPure]
        public static double Asin (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.tan($d)")]
        [JSIsPure]
        public static double Tan (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.atan($d)")]
        [JSIsPure]
        public static double Atan (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.atan2($y, $x)")]
        [JSIsPure]
        public static double Atan2 (double y, double x) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.log($d)")]
        [JSIsPure]
        public static double Log (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("(Math.log($a) / Math.log($newBase))")]
        [JSIsPure]
        public static double Log (double a, double newBase) {
            throw new InvalidOperationException();
        }

        [JSReplacement("(Math.log($d) / Math.LN10)")]
        [JSIsPure]
        public static double Log10 (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.round($d)")]
        [JSIsPure]
        public static double Round (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.floor($d)")]
        [JSIsPure]
        public static double Floor (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("($d | 0)")]
        [JSIsPure]
        public static double Truncate (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.ceil($d)")]
        [JSIsPure]
        public static double Ceiling (double d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Math.pow($base, $exponent)")]
        [JSIsPure]
        public static AnyType Pow (AnyType @base, AnyType exponent) {
            throw new InvalidOperationException();
        }
    }
}
