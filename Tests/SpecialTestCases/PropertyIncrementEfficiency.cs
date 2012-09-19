using System;

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        Console.WriteLine(instance.Value);
        instance.Value += 2;
        Console.WriteLine(instance.Value);
        Console.WriteLine(instance.Value += 2);
        Console.WriteLine(instance.Value);
    }
}

public class CustomType {
    int _Value;
    public int Value {
        get {
            return _Value;
        }
        set {
            _Value = value;
        }
    }
}