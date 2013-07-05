using System;

enum Letter {
    A, B, C
}

class Program {
    public static void Main () {
        Console.WriteLine(typeof(Letter).BaseType); // JSIL prints System.Type, .NET prints System.Enum
        object e = Letter.A;
        Console.WriteLine((e is Enum) ? "true" : "false");
    }
}