using System;
using System.Collections.Generic;

public class CustomType {
    public int Field;
    public readonly List<int> List = new List<int>();

    public int Property {
        get;
        set;
    }

    public override string ToString () {
        var result = Field.ToString() + "\n";
        foreach (var number in List)
            result += number.ToString() + "\n";
        result += Property.ToString() + "\n";

        return result;
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = new CustomType {
            Field = 1,
            Property = 2,
            List = {
                1, 2, 3, 4, 5
            }
        };

        Console.WriteLine(a.ToString());
    }
}