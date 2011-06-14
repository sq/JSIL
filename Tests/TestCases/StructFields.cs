using System;

public struct CustomType {
    public int Field;
    public int Property {
        get;
        set;
    }
}

public static class Program {
    public static CustomType Field;

    public static void Main (string[] args) {
        Console.WriteLine("Field.Field = {0}, Field.Property = {1}", Field.Field, Field.Property);
        var a = Field;
        a.Field = 1;
        a.Property = 2;
        Console.WriteLine("Field.Field = {0}, Field.Property = {1}", Field.Field, Field.Property);
        Field = a;
        Console.WriteLine("Field.Field = {0}, Field.Property = {1}", Field.Field, Field.Property);
    }
}
