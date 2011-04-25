using System;

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        instance.Event += PrintNumber;
        instance.Event += PrintNumberTimes2;
        instance.Event -= PrintNumber;
        instance.Event -= PrintNumberTimes2;
    }

    public static void PrintNumber (int x) {
        Console.WriteLine("Event({0})", x);
    }

    public static void PrintNumberTimes2 (int x) {
        Console.WriteLine("Event({0} / 2)", x * 2);
    }
}

public class CustomType {
    public event Action<int> Event {
        add {
            Console.WriteLine("Add {0}", value);
        }
        remove {
            Console.WriteLine("Remove {0}", value);
        }
    }
}