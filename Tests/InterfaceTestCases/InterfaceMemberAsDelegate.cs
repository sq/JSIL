using System;

interface IHelloPrinter {
    void Print ();
}

class HelloPrinter : IHelloPrinter {
    public void Print () {
        Console.WriteLine("Hello!");
    }
}

class ActionCaller {
    public static void Call (Action action) {
        action();
    }
}

public static class Program {
    public static void Main () {
        // This works:
        HelloPrinter printer = new HelloPrinter();
        ActionCaller.Call(printer.Print);

        // This doesn't:
        ActionCaller.Call(((IHelloPrinter)printer).Print);
    }
}