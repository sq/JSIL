using System;

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        Console.WriteLine("{0} {1}", instance[1], instance["2"]);
    }
}

public class CustomType {
    public int this[int key] {
        get {
            return key * 2;
        }
    }

    public int this[string key] {
        get {
            return int.Parse(key) * 4;
        }
    }
}