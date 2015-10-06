#pragma warning disable 0114

using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        new [] { 
            typeof(SByte), typeof(Int16), typeof(Int32), 
            typeof(Byte), typeof(UInt16), typeof(UInt32)
        },
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class IntegerProxy {
        [JSReplacement("($this).toString()")]
        public string ToString () {
            return base.ToString();
        }

        [JSReplacement("$typeof(this).__PublicInterface__.$$ToString($this, $format, null)")]
        public string ToString (string format) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$typeof(this).__PublicInterface__.$$ToString($this, $format, $formatProvider)")]
        public string ToString (string format, IFormatProvider formatProvider) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$typeof(this).__PublicInterface__.$$ToString($this, null, $formatProvider)")]
        public string ToString(IFormatProvider formatProvider)
        {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSReplacement("($this === $rhs)")]
        public bool Equals (IntegerProxy rhs) {
            throw new InvalidOperationException();
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
        public string ToString () {
            throw new InvalidOperationException();
        }

        [JSReplacement("$typeof(this).__PublicInterface__.$$ToString($this, $format, null)")]
        public string ToString (string format) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$typeof(this).__PublicInterface__.$$ToString($this, $format, $formatProvider)")]
        public string ToString (string format, IFormatProvider formatProvider) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$typeof(this).__PublicInterface__.$$ToString($this, null, $formatProvider)")]
        public string ToString(IFormatProvider formatProvider)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("isNaN($value)")]
        public static bool IsNaN (NumberProxy value) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSReplacement("($this === $rhs)")]
        public bool Equals (NumberProxy rhs) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.CompareValues($this, $rhs)")]
        public int CompareTo (AnyType rhs) {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        new[] { 
            typeof(UInt64), typeof(Int64)
        },
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class LargeIntegerProxy {
        [JSReplacement("($this).toString()")]
        public string ToString () {
            return base.ToString();
        }

        [JSReplacement("$typeof(this).__PublicInterface__.$$ToString($this, $format, null)")]
        public string ToString(string format)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("$typeof(this).__PublicInterface__.$$ToString($this, $format, $formatProvider)")]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("$typeof(this).__PublicInterface__.$$ToString($this, null, $formatProvider)")]
        public string ToString(IFormatProvider formatProvider)
        {
            throw new InvalidOperationException();
        }
    }
}
