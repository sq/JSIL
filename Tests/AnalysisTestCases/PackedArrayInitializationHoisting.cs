using System;
using JSIL.Meta;
using JSIL.Runtime;

public static class Program {
    public static void Main (string[] args) {
        var structs = InitSomeStructs(8);

        foreach (var s in structs)
            Console.WriteLine(s);
    }

    [JSPackedArrayReturnValue]
    public static TestStruct[] InitSomeStructs (int count) {
        var result = PackedArray.New<TestStruct>(count);

        for (var i = 0; i < count; i++)
            result[i] = new TestStruct(i);

        return result;
    }
}

public struct TestStruct {
    public readonly int Value;

    public TestStruct (int value) {
        Value = value;
    }

    public override string ToString () {
        return Value.ToString();
    }
}