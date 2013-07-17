using System;

enum Letter {
    A, B, C
}

class Program {
    public static void Main () {
        IConvertible letter = Letter.B;

        Console.WriteLine(letter.ToInt32(null));
        Console.WriteLine(letter.ToInt64(null));
    }
}