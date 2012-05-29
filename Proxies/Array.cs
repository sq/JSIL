using System;
using JSIL.Meta;
using JSIL.Proxy;

namespace JSIL.Proxies {
    [JSProxy(
        typeof(Array),
        memberPolicy: JSProxyMemberPolicy.ReplaceDeclared
    )]
    public abstract class ArrayProxy {
        [JSChangeName("length")]
        [JSNeverReplace]
        abstract public int Length { get; }

        [JSChangeName("length")]
        [JSNeverReplace]
        abstract public long LongLength { get; } 


        [JSReplacement("JSIL.Array.New($elementType, $size)")]
        public static System.Array CreateInstance (Type elementType, Int32 size) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.MultidimensionalArray.New.apply(null, [$elementType].concat($sizes))")]
        public static System.Array CreateInstance (Type elementType, AnyType[] sizes) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.MultidimensionalArray.New($elementType, $sizeX, $sizeY)")]
        public static System.Array CreateInstance (Type elementType, AnyType sizeX, AnyType sizeY) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.MultidimensionalArray.New($elementType, $sizeX, $sizeY, $sizeZ)")]
        public static System.Array CreateInstance (Type elementType, AnyType sizeX, AnyType sizeY, AnyType sizeZ) {
            throw new InvalidOperationException();
        }

        [JSExternal]
        [JSRuntimeDispatch]
        public abstract void Set (params AnyType[] values);

        [JSExternal]
        [JSRuntimeDispatch]
        public abstract AnyType Get (params AnyType[] values);

        [JSReplacement("$this.Get.apply($this, $indices)")]
        public abstract AnyType GetValue (AnyType[] indices);

        [JSReplacement("$this.Set.apply($this, $indices.concat([$value]))")]
        public abstract void SetValue (AnyType value, AnyType[] indices);

        [JSReplacement("Array.prototype.indexOf.call($array, $value)")]
        public static int IndexOf (AnyType[] array, AnyType value) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Array.prototype.indexOf.call($array, $value, $startIndex)")]
        public static int IndexOf (AnyType[] array, AnyType value, int startIndex) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Array.prototype.indexOf.call($array, $value)")]
        public static int IndexOf<T> (T[] array, T value) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Array.prototype.indexOf.call($array, $value, $startIndex)")]
        public static int IndexOf<T> (T[] array, T value, int startIndex) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.Array.Clone($this)")]
        public object Clone () {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.Array.CopyTo($this, $array, $destinationIndex)")]
        public void CopyTo (Array array, int destinationIndex) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Array.prototype.sort.call($array)")]
        public static void Sort (AnyType[] array) {
            throw new InvalidOperationException();
        }

        [JSReplacement("Array.prototype.sort.call($array)")]
        public static void Sort<T> (T[] array) {
            throw new InvalidOperationException();
        }

        [JSReplacement("JSIL.GetEnumerator($this)")]
        [JSIsPure]
        [JSResultIsNew]
        public System.Collections.IEnumerator GetEnumerator () {
            throw new InvalidOperationException();
        }

    }
}
