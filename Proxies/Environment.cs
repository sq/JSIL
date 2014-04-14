using JSIL.Meta;
using JSIL.Proxy;
using System;

namespace JSIL.Proxies
{
    [JSProxy(
        typeof(Environment),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public class EnvironmentProxy
    {
        [JSReplacement("System.Environment.GetFolderPath($folder)")]
        [JSIsPure]
        public static string GetFolderPath(System.Environment.SpecialFolder folder)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("System.Environment.get_NewLine()")]
        [JSIsPure]
        public static string NewLine { get { throw new InvalidOperationException(); } }

        [JSReplacement("System.Environment.get_TickCount()")]
        [JSIsPure]
        public static int TickCount { get { throw new InvalidOperationException(); } }
    }
}
