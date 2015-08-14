using System;

enum Letter {
    A, B, C
}

class Program {
    public static void Main () {
        Console.WriteLine(Convert.ToInt32(Letter.B));
        Console.WriteLine(Convert.ToInt64(Letter.C));
    }
}
