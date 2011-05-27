using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(String),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class StringProxy {
        [JSExternal]
        [JSRuntimeDispatch]
        new public static string Format (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        new public static string Concat (params string[] arguments) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        new public static string Concat (params object[] arguments) {
            throw new InvalidOperationException();
        }

        [JSReplacement("String($value)")]
        [JSRuntimeDispatch]
        new public static string Concat (object value) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        new public static string Concat (object a, object b) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        new public static string Concat (string a, string b) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        new public static string Concat (object a, object b, object c) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        new public static string Concat (string a, string b, string c) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        new public static string Concat (object a, object b, object c, object d) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        new public static string Concat (string a, string b, string c, string d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$lhs + String.fromCharCode($ch)")]
        [JSRuntimeDispatch]
        new public static string Concat (string lhs, char ch) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        public abstract string[] Split (AnyType[] dividers);

        [JSExternal]
        [JSRuntimeDispatch]
        public abstract string[] Split (AnyType[] dividers, StringSplitOptions options);

        [JSExternal]
        [JSRuntimeDispatch]
        public abstract string[] Split (AnyType[] dividers, int maximumCount, StringSplitOptions options);

        [JSChangeName("length")]
        [JSNeverReplace]
        abstract public int Length { get; }

        [JSReplacement("$this[$index]")]
        abstract public char get_Chars (int index);

        [JSReplacement("$lhs == $rhs")]
        public static bool operator == (StringProxy lhs, StringProxy rhs) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$lhs != $rhs")]
        public static bool operator != (StringProxy lhs, StringProxy rhs) {
            throw new InvalidOperationException();
        }
    }
}
