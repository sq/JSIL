using System;
using System.Collections.Generic;

public struct CustomType {
    public int Field;
    private int _Property;
    public readonly List<int> List;

    public int Property {
        get {
            return _Property;
        }
        set {
            _Property = value;
        }
    }

    public CustomType (int unused) {
        Field = 0;
        _Property = 0;
        List = new List<int>();
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
        var a = new CustomType(0) {
            Field = 1,
            Property = 2,
            List = {
                1, 2, 3, 4, 5
            }
        };

        Console.WriteLine(a.ToString());
    }
}