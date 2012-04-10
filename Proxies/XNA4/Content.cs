using System;
using JSIL.Meta;
using JSIL.Proxy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace JSIL.Proxies {
    [JSProxy(
        new[] {
            typeof(Microsoft.Xna.Framework.Content.ContentTypeReader),
            typeof(Microsoft.Xna.Framework.Content.ContentTypeReader<>),
        },
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared,
        inheritable: false
    )]
    public abstract class ContentTypeReaderProxy {
        [JSIgnore]
        public AnyType Read (ContentReader input, AnyType existingInstance) {
            throw new NotImplementedException();
        }
    }
}
