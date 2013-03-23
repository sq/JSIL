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
        [JSResultIsNew]
        T Get (int index);
        void* GetReference (int index);
        [JSEscapingArguments("value")]
        [JSMutatedArguments()]
        void Set (int index, T value);
        int Length { get; }
    }

    public static class TypedArrayExtensionMethods {
        /// <summary>
        /// If the specified array is backed by a typed array, returns its backing array buffer.
        /// </summary>
        [JSReplacement("JSIL.GetArrayBuffer($array)")]
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
        public static byte[] GetBackingTypedArray<T> (this T[] array)
            where T : struct {

            throw new NotImplementedException("Not supported when running as C#");
        }
    }
}
