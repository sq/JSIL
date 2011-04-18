using System;

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        instance.FireEvent(1);
        instance.Event += PrintNumber;
        instance.FireEvent(2);
        instance.Event += PrintNumberTimes2;
        instance.FireEvent(3);
        instance.Event -= PrintNumber;
        instance.FireEvent(4);
        instance.Event -= PrintNumberTimes2;
        instance.FireEvent(5);
    }

    public static void PrintNumber (int x) {
        Console.WriteLine("Event({0})", x);
    }

    public static void PrintNumberTimes2 (int x) {
        Console.WriteLine("Event({0} / 2)", x * 2);
    }
}

public class CustomType {
    public event Action<int> Event;

    public void FireEvent (int x) {
        if (Event != null)
            Event(x);
    }
}