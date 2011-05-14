using System;

public interface InterfaceA {
    int Value { get; }
}

public interface InterfaceB {
    int Value { get; }
}

public class CustomType : InterfaceA, InterfaceB {
    int InterfaceA.Value {
        get {
            return 2;
        }
    }

    int InterfaceB.Value {
        get {
            return 4;
        }
    }
}

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        Console.WriteLine("{0}, {1}", ((InterfaceA)instance).Value, ((InterfaceB)instance).Value);
    }
}