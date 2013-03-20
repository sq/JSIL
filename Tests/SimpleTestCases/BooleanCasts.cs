using System;

public static class Program {
    public static object ReturnBool (bool b) {
        return b;
    }

    public static void Main (string[] args) {
        object falseValue = ReturnBool(Convert.ToBoolean(0));
        object trueValue = ReturnBool(Convert.ToBoolean(1));

        Console.WriteLine("{0} {1}", falseValue, trueValue);
    }
}