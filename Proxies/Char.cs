#pragma warning disable 0660
#pragma warning disable 0661

using System;
using System.Collections.Generic;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(char),
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class CharProxy {
        [JSReplacement("$c.toLowerCase()")]
        [JSIsPure]
        public static char ToLower (char c) {
            throw new InvalidOperationException();
        }

        // FIXME: Are ECMAScript strings always normalized?
        [JSReplacement("$c.toLowerCase()")]
        [JSIsPure]
        public static char ToLowerInvariant (char c) {
            throw new InvalidOperationException();
        }

        [JSReplacement("$c.toUpperCase()")]
        [JSIsPure]
        public static char ToUpper (char c) {
            throw new InvalidOperationException();
        }

        // FIXME: Are ECMAScript strings always normalized?
        [JSReplacement("$c.toUpperCase()")]
        [JSIsPure]
        public static char ToUpperInvariant (char c) {
            throw new InvalidOperationException();
        }
    }
}
