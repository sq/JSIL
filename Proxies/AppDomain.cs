using JSIL.Meta;
using JSIL.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace JSIL.Proxies
{
    [JSProxy(
        typeof(AppDomain),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public sealed class AppDomainProxy
    {
        [JSReplacement("get_CurrentDomain()")]
        [JSIsPure]
        public static AppDomain CurrentDomain { get { throw new InvalidOperationException(); } }

        [JSReplacement("GetAssemblies()")]
        [JSIsPure]
        public Assembly[] GetAssemblies()
        {
            throw new InvalidOperationException();
        }
    }
}
