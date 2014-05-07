using JSIL.Meta;
using JSIL.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace JSIL.Proxies
{
    [JSProxy(typeof(String), JSProxyMemberPolicy.ReplaceDeclared, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_String
    {
        [JSReplacement("System.String.StartsWith($this, $text)")]
        [JSIsPure]
        public bool StartsWith(string text, StringComparison comparisonType)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.indexOf($value)")]
        [JSIsPure]
        public int IndexOf(string value, StringComparison comparisonType)
        {
            throw new InvalidOperationException();
        }

        [JSReplacement("$this.indexOf($value, $startIndex)")]
        [JSIsPure]
        public int IndexOf(string value, int startIndex, StringComparison comparisonType)
        {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(typeof(StringComparison), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_StringComparison
    {
    }

    [JSProxy(typeof(Encoding), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Text_Encoding
    {
    }

    [JSProxy(typeof(ASCIIEncoding), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Text_ASCIIEncoding
    {
    }

    [JSProxy(typeof(UTF8Encoding), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Text_UTF8Encoding
    {
    }

    [JSProxy(typeof(UTF7Encoding), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Text_UTF7Encoding
    {
    }

    [JSProxy(typeof(UnicodeEncoding), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Text_UnicodeEncoding
    {
    }

    [JSProxy(typeof(StringBuilder), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceNone, false)]
    [JSStubOnly]
    public class System_Text_StringBuilder
    {
    }
}
