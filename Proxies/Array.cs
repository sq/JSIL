using System;
using JSIL.Meta;

namespace JSIL.Proxies {
    [JSProxy(typeof(Array))]
    public abstract class ArrayProxy {
        [JSChangeName("length")]
        abstract public int Length { get; }
    }
}
