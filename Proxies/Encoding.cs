using System;
using JSIL.Meta;
using JSIL.Proxy;
using System.Text;

namespace JSIL.Proxies
{
    [JSProxy(
        typeof(Encoding),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class EncodingProxy
    {
        [JSReplacement("System.Text.Encoding.get_ASCII()")]
        [JSIsPure]
        public static Encoding ASCII { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.Text.Encoding.get_UTF8()")]
        [JSIsPure]
        public static Encoding UTF8 { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.Text.Encoding.get_UTF7()")]
        [JSIsPure]
        public static Encoding UTF7 { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.Text.Encoding.get_Unicode()")]
        [JSIsPure]
        public static Encoding Unicode { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.Text.Encoding.get_BigEndianUnicode()")]
        [JSIsPure]
        public static Encoding BigEndianUnicode { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.Text.Encoding.GetByteCount($chars)")]
        [JSIsPure]
        public int GetByteCount(Char[] chars)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetByteCount($chars)")]
        [JSIsPure]
        public int GetByteCount(string s)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetByteCount($chars, $a, $b)")]
        [JSIsPure]
        public int GetByteCount(Char[] chars, int a, int b)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetBytes($arg)")]
        [JSIsPure]
        public byte[] GetBytes(Char[] chars)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetBytes($arg, $a, $b)")]
        [JSIsPure]
        public byte[] GetBytes(Char[] chars, int a, int b)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetBytes($arg, $a, $b, $bytes, $c)")]
        [JSIsPure]
        public int GetBytes(Char[] chars, int a, int b, byte[] bytes, int c)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetBytes($arg)")]
        [JSIsPure]
        public byte[] GetBytes(string s)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetBytes($arg, $a, $b, $bytes, $c)")]
        [JSIsPure]
        public int GetBytes(string s, int a, int b, byte[] bytes, int c)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetCharCount($arg)")]
        [JSIsPure]
        public int GetCharCount(byte[] bytes)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetCharCount($arg, $a, $b)")]
        [JSIsPure]
        public int GetCharCount(byte[] bytes, int a ,int b)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetChars($arg)")]
        [JSIsPure]
        public Char[] GetChars(byte[] bytes)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetChars($arg, $a, $b)")]
        [JSIsPure]
        public Char[] GetChars(byte[] bytes, int a, int b)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetString($arg)")]
        [JSIsPure]
        public string GetString(byte[] bytes)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Text.Encoding.GetString($arg, $a, $b)")]
        [JSIsPure]
        public string GetString(byte[] bytes, int a, int b)
        {
            throw new InvalidOperationException();
        }
    }
}
