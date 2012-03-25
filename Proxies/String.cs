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

        [JSReplacement("$this.toLowerCase()")]
        [JSIsPure]
        new public string ToLower () {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.toUpperCase()")]
        [JSIsPure]
        new public string ToUpper () {
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

        [JSReplacement("$this.trim()")]
        [JSIsPure]
        new public string Trim () {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.String.Compare($this, $rhs)")]
        [JSIsPure]
        new public int CompareTo (string rhs) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.indexOf($value)")]
        [JSIsPure]
        new public int IndexOf (char value) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.indexOf($value, $startIndex)")]
        [JSIsPure]
        new public int IndexOf (char value, int startIndex) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.indexOf($value)")]
        [JSIsPure]
        new public int IndexOf (string value) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.indexOf($value, $startIndex)")]
        [JSIsPure]
        new public int IndexOf (string value, int startIndex) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.lastIndexOf($value)")]
        [JSIsPure]
        new public int LastIndexOf (char value) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.lastIndexOf($value)")]
        [JSIsPure]
        new public int LastIndexOf (string value) {
            throw new InvalidOperationException();
        }

        // FIXME
        /*
        [JSReplacement("$this.lastIndexOf($value, $startIndex)")]
        [JSIsPure]
        new public int LastIndexOf (char value, int startIndex) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.lastIndexOf($value, $startIndex)")]
        [JSIsPure]
        new public int LastIndexOf (string value, int startIndex) {
            throw new InvalidOperationException();
        }
         */

        [JSReplacement("System.String.Replace($this, $oldText, $newText)")]
        [JSIsPure]
        new public string Replace (string oldText, string newText) {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.String.Replace($this, $oldChar, $newChar)")]
        [JSIsPure]
        new public string Replace (char oldChar, char newChar) {
            throw new InvalidOperationException();
        }
    }
}
