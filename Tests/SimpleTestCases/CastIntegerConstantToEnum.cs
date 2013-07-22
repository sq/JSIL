using System;

enum Letter {
    A, B, C
}

class Program {
    private const Letter ValidLetterCast = (Letter)1;
    private const Letter InvalidLetterCast = (Letter)127;

    public static void Main () {
        Console.WriteLine("{0} {1}", ValidLetterCast, InvalidLetterCast);
    }
}