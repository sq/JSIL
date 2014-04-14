using JSIL.Meta;
using JSIL.Proxy;
using System;
using System.Threading;

namespace JSIL.Proxies
{
    [JSProxy(typeof(Thread), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Threading_Thread
    {
    }

    [JSProxy(typeof(Monitor), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public static class System_Threading_Monitor
    {
    }
}
