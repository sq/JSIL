using System;
using System.Reflection;
using System.Runtime.InteropServices;

public static class Program {
    public static unsafe void Main (string[] args) {
        var types = new Type[] {
            typeof(EmptyStruct),
            typeof(TwoBytes),
            typeof(TwoBytesOneInt),
            typeof(TwoBytesShortDouble),
            typeof(DoubleTwoBytes),
            typeof(ByteNestedByte)
        };

        foreach (var type in types) {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)) {
                Console.WriteLine(
                    "offsetof({0}.{1}) == {2}", 
                    type.Name, field.Name, 
                    Marshal.OffsetOf(type, field.Name).ToInt32()
                );
            }
        }
    }
}