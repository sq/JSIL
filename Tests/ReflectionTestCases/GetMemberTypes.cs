using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var t = typeof(T);

        Console.WriteLine(t.GetField("Field").FieldType);
        Console.WriteLine(t.GetProperty("Property").PropertyType);
        Console.WriteLine(t.GetProperty("ReadOnlyProperty").PropertyType);
        Console.WriteLine(t.GetProperty("WriteOnlyProperty").PropertyType);
        Console.WriteLine(t.GetMethod("Method").ReturnType);
    }
}

public class T {
    public float Field;
    public int Property { get; set; }
    public byte ReadOnlyProperty { get { return default(byte); } }
    public double WriteOnlyProperty { set { ; } }
    public string Method () {
        return null;
    }
}