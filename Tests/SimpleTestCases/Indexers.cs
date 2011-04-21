using System;

public static class Program {  
    public static void Main (string[] args) {
        var instance = new CustomType();

        Console.WriteLine("{0}, {1}", instance[0], instance[1]);

        instance[2] = 3;
    }
}

public class CustomType {
    public int this[int index] {
        get {
            return index * 2;
        }
        set {
            Console.WriteLine("[{0}] = {1}", index, value);
        }
    }
}