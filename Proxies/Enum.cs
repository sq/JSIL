using JSIL.Proxy;
using System;
using JSIL.Meta;

namespace JSIL.Proxies
{
    [JSProxy(
        typeof(Enum),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class EnumProxy
    {
        [JSReplacement("System.Enum.Parse($type, $value)")]
        [JSIsPure]
        public static Object Parse(Type type, string value)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Enum.Parse($type, $value, $ignoreCase)")]
        [JSIsPure]
        public static Object Parse(Type type, string value, bool ignoreCase)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Enum.TryParse($value, $result)")]
        [JSIsPure]
        public static bool TryParse<TEnum>(string value, out TEnum result)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Enum.TryParse($value, $ignoreCase, $result)")]
        [JSIsPure]
        public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Enum.GetNames($type)")]
        [JSIsPure]
        public static string[] GetNames(Type enumType)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Enum.GetValues($type)")]
        [JSIsPure]
        public static Array GetValues(Type enumType)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Enum.ToObject($type, $value)")]
        [JSIsPure]
        public static Object ToObject(Type enumType, int value)
        {
            throw new InvalidOperationException();
        }


    }
}
