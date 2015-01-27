using System;
using System.Runtime.InteropServices;
using JSIL.Meta;

namespace Asm {
    public static class Js {
        [JSReplacement("JSIL.Malloc($size)")]
        public static IntPtr malloc (int size) {
            // FIXME: Import malloc from msvcrt?
            return Marshal.AllocHGlobal(size);
        }

        [JSReplacement("JSIL.Free($ptr)")]
        public static void free (IntPtr ptr) {
            // FIXME: Import free from msvcrt?
            Marshal.FreeHGlobal(ptr);
        }
    }
}
