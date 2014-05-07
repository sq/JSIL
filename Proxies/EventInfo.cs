using JSIL.Meta;
using JSIL.Proxy;
using System;
using System.Reflection;

namespace JSIL.Proxies
{
    [JSProxy(
        typeof(EventInfo),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class EventInfoProxy
    {
        [JSReplacement("System.Reflection.EventInfo.AddEventHandler($object, $delegate)")]
        [JSIsPure]
        public virtual void AddEventHandler(Object obj, Delegate del)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Reflection.EventInfo.RemoveEventHandler($object, $delegate)")]
        [JSIsPure]
        public virtual void RemoveEventHandler(Object obj, Delegate del)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Reflection.EventInfo.GetAddMethod()")]
        [JSIsPure]
        public MethodInfo GetAddMethod()
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Reflection.EventInfo.GetAddMethod($b)")]
        [JSIsPure]
        public MethodInfo GetAddMethod(bool b)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Reflection.EventInfo.GetRemoveMethod()")]
        [JSIsPure]
        public MethodInfo GetRemoveMethod()
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Reflection.EventInfo.GetRemoveMethod($b)")]
        [JSIsPure]
        public MethodInfo GetRemoveMethod(bool b)
        {
            throw new InvalidOperationException();
        }




    }
}
