using System;
using System.Runtime.InteropServices;

public static class Program {
    public struct SDL_TouchFingerEvent {
        public UInt32 type;
        public UInt32 timestamp;
        public Int64 touchId; // SDL_TouchID
        public Int64 fingerId; // SDL_GestureID
        public float x;
        public float y;
        public float dx;
        public float dy;
        public float pressure;
    }

    public static unsafe void Main (string[] args) {
        var t = typeof(SDL_TouchFingerEvent);

        Console.WriteLine(Marshal.SizeOf(t));
        Console.WriteLine(Marshal.OffsetOf(t, "type").ToInt32());
        Console.WriteLine(Marshal.OffsetOf(t, "timestamp").ToInt32());
        Console.WriteLine(Marshal.OffsetOf(t, "touchId").ToInt32());
        Console.WriteLine(Marshal.OffsetOf(t, "fingerId").ToInt32());
        Console.WriteLine(Marshal.OffsetOf(t, "x").ToInt32());
        Console.WriteLine(Marshal.OffsetOf(t, "y").ToInt32());
        Console.WriteLine(Marshal.OffsetOf(t, "dx").ToInt32());
        Console.WriteLine(Marshal.OffsetOf(t, "dy").ToInt32());
        Console.WriteLine(Marshal.OffsetOf(t, "pressure").ToInt32());
    }
}