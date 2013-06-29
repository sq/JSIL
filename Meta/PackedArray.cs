using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Meta;

namespace JSIL.Runtime {
    internal class LinkedTypeAttribute : Attribute {
        public readonly Type Type;

        public LinkedTypeAttribute (Type type) {
            Type = type;
        }
    }

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

        [JSResultIsNew]
        void* GetReference (int index);

        [JSEscapingArguments()]
        [JSMutatedArguments("result")]
        void GetItemInto (int index, out T result);

        [JSIsPure]
        int Length { get; }
    }

    public static class PackedArray {
        [JSReplacement("JSIL.PackedArray.New($T, $size)")]
        [JSPackedArrayReturnValue]
        public static T[] New<T> (int size) 
            where T : struct
        {
            return new T[size];
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
