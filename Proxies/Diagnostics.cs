using JSIL.Meta;
using JSIL.Proxy;
using System;
using System.Diagnostics;

namespace JSIL.Proxies
{
    [JSProxy(typeof(Trace), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    class System_Diagnostics_Trace
    {
    }
}
