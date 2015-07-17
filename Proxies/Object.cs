#pragma warning disable 0108

using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Object),
        memberPolicy: JSProxyMemberPolicy.ReplaceNone,
        attributePolicy: JSProxyAttributePolicy.ReplaceDeclared
        )]
    public abstract class ObjectProxy
    {
        [JSIsPure]
        [JSExternal]
        new abstract public Type GetType ();

        [JSExternal]
        [JSNeverReplace]
        new abstract public AnyType MemberwiseClone ();

        [JSChangeName("toString")]
        [JSNeverReplace]
        [JSRuntimeDispatch]
        new abstract public string ToString ();

        [JSIsPure]
        [JSChangeName("Object.Equals")]
        [JSNeverReplace]
        [JSRuntimeDispatch]
        new public abstract bool Equals (object obj);

        [JSIsPure]
        [JSReplacement("JSIL.ObjectEquals($objA, $objB)")]
        public static bool Equals (object objA, object objB) {
            throw new InvalidOperationException();
        }

        [JSIsPure]
        [JSNeverReplace]
        public static bool ReferenceEquals (object objA, object objB) {
            throw new InvalidOperationException();
        }
    }

    [JSProxy(
        typeof (Object),
        memberPolicy: JSProxyMemberPolicy.ReplaceNone,
        attributePolicy: JSProxyAttributePolicy.ReplaceDeclared,
        inheritable: false)]
    public abstract class ObjectGetHashCodeProxy
    {
        [JSIsPure]
        [JSReplacement("JSIL.ObjectHashCode($this, !$explicitThis(method), $declaringType(method))")]
        public new abstract int GetHashCode();

        [JSReplacement("JSIL.ObjectEqualsInstance($this, $obj, !$explicitThis(method), $declaringType(method))")]
        new public abstract bool Equals(object obj);
    }

    [JSProxy(
        new[]
        {
            typeof (SByte), typeof (Int16), typeof (Int32),
            typeof (Byte), typeof (UInt16), typeof (UInt32),
            typeof (String)
        },
        memberPolicy: JSProxyMemberPolicy.ReplaceNone,
        attributePolicy: JSProxyAttributePolicy.ReplaceDeclared)]
    public abstract class JSTypesGetHashCodeProxy
    {
        [JSIsPure]
        [JSReplacement("JSIL.ObjectHashCode($this, false)")]
        public new abstract int GetHashCode();

        [JSReplacement("JSIL.ObjectEquals($this, $obj)")]
        new public abstract bool Equals(object obj);
    }
}
