using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        "System.Windows.Forms.AccessibleObject",
        JSProxyMemberPolicy.ReplaceDeclared,
        JSProxyAttributePolicy.ReplaceDeclared,
        JSProxyInterfacePolicy.ReplaceAll,
        false
    )]
    public abstract class AccessibleObjectProxy {
    }

    [JSProxy(
        "System.Windows.Forms.Control",
        JSProxyMemberPolicy.ReplaceDeclared,
        inheritable: true
    )]
    public abstract class ControlProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public ControlProxy (params AnyType[] values) {
        }
    }

    [JSProxy(
        new[] { 
            "System.Windows.Forms.StatusBar/StatusBarPanelCollection",
            "System.Windows.Forms.TabControl/TabPageCollection"
        },
        JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class ControlCollectionProxy {
        [JSRuntimeDispatch]
        [JSExternal]
        public abstract AnyType Add (params AnyType[] values);

        [JSRuntimeDispatch]
        [JSExternal]
        public abstract AnyType this[AnyType key] {
            get;
        }
    }
}
