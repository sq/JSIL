using System;

public static class Program {
    public static void Main (string[] args) {
        var a = new CustomType(8);
        var b = a.Mutate((v) => v * 2);
        var c = a.Mutate(
            (v) => v * (
                b.Mutate((n) => v).Value
            )
        );

        Console.WriteLine("a={0}, b={1}, c={2}", a, b, c);
    }
}

public class CustomType {
    public int Value;

    public CustomType (int value) {
        Value = value;
    }

    public CustomType Mutate (Func<int, int> mutator) {
        return new CustomType(mutator(Value));
    }

    public override string ToString () {
        return Value.ToString();
    }
}