using System;
using System.Collections.Generic;

public class CustomType {
    public int Value;
    public readonly List<int> Numbers = new List<int>();

    public override string ToString () {
        var result = Value.ToString() + "\n";
        foreach (var number in Numbers)
            result += number.ToString() + "\n";

        return result;
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = new CustomType {
            Value = 1,
            Numbers = {
                1, 2, 3, 4, 5
            }
        };

        Console.WriteLine(a.ToString());
    }
}