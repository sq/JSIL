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
}
