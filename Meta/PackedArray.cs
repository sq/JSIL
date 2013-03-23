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
            [JSResultIsNew]
            [JSIsPure]
            get;
            [JSEscapingArguments("value")]
            [JSMutatedArguments()]
            set;
        }
        [JSResultIsNew]
        void* GetReference (int index);
        [JSIsPure]
        int Length { get; }
    }

    public static class TypedArrayExtensionMethods {
        /// <summary>
        /// If the specified array is backed by a typed array, returns its backing array buffer.
        /// </summary>
        [JSReplacement("JSIL.GetArrayBuffer($array)")]
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
        [JSIsPure]
        public static byte[] GetBackingTypedArray<T> (this T[] array)
            where T : struct {

            throw new NotImplementedException("Not supported when running as C#");
        }
    }
}
