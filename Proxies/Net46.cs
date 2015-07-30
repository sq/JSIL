using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {

    // HACK to fix issue #768
    [JSProxy("System.Resources.FastResourceComparer")]
    [JSStubOnly]
    public class System_Resources_FastResourceComparer
    {
    }
}
