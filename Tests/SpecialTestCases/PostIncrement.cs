using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        int i = 0;

        i += 1;
        i = i + 1;
        Console.WriteLine("{0}", i);
        Console.WriteLine("{0}", i = i + 1);
        i -= 1;
        i = i - 1;
        Console.WriteLine("{0}", i);
        Console.WriteLine("{0}", i = i - 1);
        Console.WriteLine("{0}", i);

        var instance = new CustomType(0);
        Console.WriteLine("{0}", instance.Value);
        instance.Increment(1);
        Console.WriteLine("{0}", instance.Value);
    }
}

public class CustomType {
    public int Value;

    public CustomType (int value) {
        Value = value;
    }

    public void Increment (int value) {
        this.Value += value;
    }
}