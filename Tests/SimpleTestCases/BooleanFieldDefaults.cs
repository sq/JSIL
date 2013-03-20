using System;

public static class Program {
    public static bool BoolField;
    public static bool FalseField = false, TrueField = true;

    public static object ReturnBool (bool b) {
        return b;
    }

    public static void Main (string[] args) {
        object boolField = ReturnBool(BoolField);
        object falseField = ReturnBool(FalseField);
        object trueField = ReturnBool(TrueField);

        Console.WriteLine("{0} {1} {2}", boolField, falseField, trueField);

        var mc = new MyClass();
        Console.WriteLine("{0} {1} {2}", ReturnBool(mc.BoolField), ReturnBool(mc.FalseField), ReturnBool(mc.TrueField));
    }
}

public class MyClass {
    public bool BoolField;
    public bool FalseField = false, TrueField = true;
}