using System;
using System.Reflection;

public static class Program {
    public static void Main (string[] args) {
        var fields = typeof(T).GetFields();
        foreach (var field in fields)
            Console.WriteLine(field.Name);
    }
}

public class T {
    // Real code out there depends on the order of fields from GetFields() matching the
    //  order they were declared in code. :/
    public int FieldC;
    public int FieldB;
    public int FieldA;
}