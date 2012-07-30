using System;

public class CustomClass {
    public int Value = 1;

    public override string ToString () {
        return Value.ToString();
    }
}

public struct CustomStruct {
    public int Value;

    public override string ToString () {
        return Value.ToString();
    }
}

public static class Program {
    public static void Main (string[] args) {
        Func<object> Null = () => null;

        Console.WriteLine("{0}", default(bool));
        Console.WriteLine("{0}", default(char).ToString().Length);
        Console.WriteLine("{0}", default(int));
        Console.WriteLine("{0}", default(float));
        Console.WriteLine("{0}", default(string) == Null());
        Console.WriteLine("{0}", default(CustomClass) == Null());
        Console.WriteLine("{0}", default(CustomStruct));
    }
}