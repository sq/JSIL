using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Console),
        memberPolicy: JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class ConsoleProxy {
        [JSReplacement("$typeof(this).__PublicInterface__.WriteLine(String.fromCharCode($arg))")]
        public static void WriteLine(char arg)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine()
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(bool value)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(char[] value)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(char[] buffer, int index, int count)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(decimal value)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(double value)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(float value)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(int value)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(uint value)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(long value)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(ulong value)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(object value)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(string value)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(string format, object arg0)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(string format, object arg0, object arg1)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(string format, object arg0, object arg1, object arg2, object arg3, __arglist)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void WriteLine(string format, params object[] arg)
        {
            throw new InvalidOperationException();
        }

        [JSRuntimeDispatch]
        [JSExternal]
        [JSMutatedArguments()]
        [JSEscapingArguments()]
        public static void Write (params AnyType[] arguments) {
            throw new InvalidOperationException();
        }
    }
}
