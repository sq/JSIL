using JSIL.Meta;
using JSIL.Proxy;
using System;

namespace JSIL.Proxies
{
    [JSProxy(typeof(Random), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Random
    {
    }
}
