using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Meta;

namespace JSIL.Proxies {
    [JSProxy(typeof(Object))]
    public abstract class ObjectProxy {
        [JSReplacement("JSIL.GetType($this)")]
        new abstract public Type GetType ();

        [JSChangeName("toString")]
        new abstract public string ToString ();
    }

    [JSProxy(typeof(Array))]
    public abstract class ArrayProxy {
        [JSChangeName("length")]
        abstract public int Length { get; }
    }
}
