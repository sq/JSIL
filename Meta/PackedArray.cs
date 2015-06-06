using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JSIL.Meta;

namespace JSIL.Runtime {
    internal class LinkedTypeAttribute : Attribute {
        public readonly Type Type;

        public LinkedTypeAttribute (Type type) {
            Type = type;
        }
    }

    /// <summary>
    /// A packed array that is allocated in the managed heap.
    /// Packed arrays always have a dense in-memory representation, even in JS. 
    /// This allows passing them to APIs like WebGL along with pinning and fast marshalling/unmarshalling.
    /// </summary>
    public unsafe interface IPackedArray<T> {
        T this[int index] {
            [JSRuntimeDispatch]
            [JSResultIsNew]
            [JSIsPure]
            get;
            // HACK: value technically escapes, but since the packed array stores its raw values instead of its reference, we don't want it to be copied.
            [JSRuntimeDispatch]
            [JSEscapingArguments()]
            [JSMutatedArguments()]
            set;
        }

        /// <summary>
        /// Returns a reference to an element of the packed array.
        /// </summary>
        [JSRuntimeDispatch]
        [JSResultIsNew]
        [JSIsPure]
        void* GetReference (int index);

        /// <summary>
        /// Returns a proxy for the particular element of the packed array. You can use the proxy's members to manipulate the packed element directly.
        /// </summary>
        [JSRuntimeDispatch]
        [JSResultIsNew]
        [JSIsPure]
        T GetItemProxy (int index);

        /// <summary>
        /// Reads an element out of the packed array, into result.
        /// </summary>
        [JSEscapingArguments()]
        [JSMutatedArguments("result")]
        [JSRuntimeDispatch]
        void GetItemInto (int index, out T result);

        [JSIsPure]
        [JSRuntimeDispatch]
        int Length { get; }
    }

    /// <summary>
    /// A packed array that is allocated in the native (emscripten in JS) heap.
    /// Because this array is allocated in the native heap, you must explicitly free it.
    /// </summary>
    public class NativePackedArray<T> : IDisposable
        where T : struct
    {
        public readonly int Length;

        private readonly T[] _Array;
        private bool IsNotDisposed;

        public NativePackedArray (int length) {
            _Array = new T[length];
            Length = length;
            IsNotDisposed = true;
        }

        /// <summary>
        /// If you load multiple emscripten modules, it's necessary to specify which one's heap you wish to allocate into.
        /// </summary>
        /// <param name="dllName">The name of the module (the name you pass to DllImport)</param>
        public NativePackedArray (string dllName, int length)
            : this (length) {
        }

        public T[] Array {
            [JSIsPure]
            get {
                if (!IsNotDisposed)
                    throw new ObjectDisposedException("this");

                return _Array;
            }
        }

        public GCHandle AllocHandle () {
            return GCHandle.Alloc(_Array);
        }

        public GCHandle AllocHandle (GCHandleType type) {
            return GCHandle.Alloc(_Array, type);
        }

        [JSIsPure]
        public static implicit operator T[] (NativePackedArray<T> nativeArray) {
            if (!nativeArray.IsNotDisposed)
                throw new ObjectDisposedException("nativeArray");

            return nativeArray.Array;
        }

        [JSReplacement("JSIL.PackedArray.Dispose($this)")]
        public void Dispose () {
            if (!IsNotDisposed)
                throw new ObjectDisposedException("this");

            IsNotDisposed = false;
        }
    }

    public static class PackedArray {
        [JSReplacement("JSIL.PackedArray.New($T, $length)")]
        [JSPackedArrayReturnValue]
        public static T[] New<T> (int length) 
            where T : struct
        {
            return new T[length];
        }
    }

    public static class TypedArrayExtensionMethods {
        /// <summary>
        /// If the specified array is backed by a typed array, returns its backing array buffer.
        /// </summary>
        [JSReplacement("JSIL.GetArrayBuffer($array)")]
        [JSAllowPackedArrayArguments]
        [JSIsPure]
        public static dynamic GetArrayBuffer<T> (this T[] array)
            where T : struct {

            throw new NotImplementedException("Not supported when running as C#");
        }
    }

    public static class PackedArrayExtensionMethods {
        /// <summary>
        /// If the specified array is a packed array, returns its backing typed array.
        /// </summary>
        [JSReplacement("JSIL.GetBackingTypedArray($array)")]
        [JSAllowPackedArrayArguments]
        [JSIsPure]
        public static dynamic GetBackingTypedArray<T> (this T[] array)
            where T : struct {

            throw new NotImplementedException("Not supported when running as C#");
        }
    }
}
