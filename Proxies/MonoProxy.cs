using JSIL.Meta;
using JSIL.Proxy;
using System;

namespace JSIL.Proxies
{
    [JSProxy(typeof(System.Runtime.CompilerServices.RuntimeHelpers))]
    [JSIgnore]
    public abstract class RuntimeHelpers { }
}
