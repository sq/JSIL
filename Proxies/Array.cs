using System;
using JSIL.Meta;

namespace JSIL.Proxies {
    [JSProxy(typeof(Array))]
    public abstract class ArrayProxy {
        [JSChangeName("length")]
        abstract public int Length { get; }

        [JSIgnore]
        public void Set (int x, object value) {
            throw new NotImplementedException();
        }

        [JSIgnore]
        public void Set (int x, int y, object value) {
            throw new NotImplementedException();
        }

        [JSIgnore]
        public void Set (int x, int y, int z, object value) {
            throw new NotImplementedException();
        }

        [JSIgnore]
        public object Get (int x) {
            throw new NotImplementedException();
        }

        [JSIgnore]
        public object Get (int x, int y) {
            throw new NotImplementedException();
        }

        [JSIgnore]
        public object Get (int x, int y, int z) {
            throw new NotImplementedException();
        }
    }
}
