using System;
using System.Collections.Generic;

public struct CustomStruct {
    public int Value;

    public override string ToString () {
        return Value.ToString();
    }
}

public class CustomType {
    public CustomStruct Field;
    public CustomStruct Property {
        get;
        set;
    }

    public override string ToString () {
        var result = Field.ToString() + "\n";
        result += Property.ToString() + "\n";

        return result;
    }
}

public static class Program {
    public static void Main (string[] args) {
        var a = new CustomType {
            Field = {
                Value = 1
            },
            Property = new CustomStruct {
                Value = 2
            }
        };

        Console.WriteLine(a.ToString());
    }
}