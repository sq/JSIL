using JSIL.Meta;
using JSIL.Proxy;
using System;
using System.Globalization;
using System.Reflection;

namespace JSIL.Proxies
{
    [JSProxy(
        typeof(Activator),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class ActivatorProxy
    {
        [JSReplacement("System.Activator.CreateInstance($type)")]
        [JSIsPure]
        public static Object CreateInstance(Type type)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Activator.CreateInstance($type, $array)")]
        [JSIsPure]
        public static Object CreateInstance(Type type, Object[] array)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Activator.CreateInstance()")]
        [JSIsPure]
        public static T CreateInstance<T>()
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Activator.CreateInstance($type, $bindingAttr, $binder, $args, $culture)")]
        [JSIsPure]
        public static Object CreateInstance(Type type, BindingFlags bindingAttr, 
            Binder binder, Object[] args, CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }
}
