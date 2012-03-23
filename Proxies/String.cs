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
        [JSIsPure]
        new public static string Format (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.ConcatString.apply(null, $arguments)")]
        [JSIsPure]
        new public static string Concat (params string[] arguments) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.ConcatString.apply(null, $arguments)")]
        [JSIsPure]
        new public static string Concat (params object[] arguments) {
            throw new InvalidOperationException();
        }

        [JSReplacement("String($value)")]
        [JSIsPure]
        new public static string Concat (object value) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.ConcatString($a, $b)")]
        [JSIsPure]
        new public static string Concat (object a, object b) {
            throw new InvalidOperationException();
        }

        [JSReplacement("($a + $b)")]
        [JSIsPure]
        new public static string Concat (string a, string b) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.ConcatString($a, $b, $c)")]
        [JSIsPure]
        new public static string Concat (object a, object b, object c) {
            throw new InvalidOperationException();
        }

        [JSReplacement("($a + $b + $c)")]
        [JSIsPure]
        new public static string Concat (string a, string b, string c) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.ConcatString($a, $b, $c, $d)")]
        [JSIsPure]
        new public static string Concat (object a, object b, object c, object d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("($a + $b + $c + $d)")]
        [JSIsPure]
        new public static string Concat (string a, string b, string c, string d) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$lhs + $ch")]
        [JSIsPure]
        new public static string Concat (string lhs, char ch) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        [JSIsPure]
        public abstract string[] Split (AnyType[] dividers);

        [JSExternal]
        [JSRuntimeDispatch]
        [JSIsPure]
        public abstract string[] Split (AnyType[] dividers, StringSplitOptions options);

        [JSExternal]
        [JSRuntimeDispatch]
        [JSIsPure]
        public abstract string[] Split (AnyType[] dividers, int maximumCount, StringSplitOptions options);

        [JSChangeName("length")]
        [JSNeverReplace]
        abstract public int Length { get; }

        [JSReplacement("$this[$index]")]
        [JSIsPure]
        abstract public char get_Chars (int index);

        [JSReplacement("$lhs == $rhs")]
        [JSIsPure]
        public static bool operator == (StringProxy lhs, StringProxy rhs) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$lhs != $rhs")]
        [JSIsPure]
        public static bool operator != (StringProxy lhs, StringProxy rhs) {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.String.StartsWith($this, $text)")]
        [JSIsPure]
        new public bool StartsWith (string text) {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.String.EndsWith($this, $text)")]
        [JSIsPure]
        new public bool EndsWith (string text) {
            throw new InvalidOperationException();
        }
    }
}
