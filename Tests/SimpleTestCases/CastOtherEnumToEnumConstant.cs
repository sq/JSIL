using System;

enum Letter {
    A = 37, B = 96, C = 128
}

enum OtherEnum {
    J = 192, K = 37, L = 256, Twelve = 10240
}

class Program {
    private const Letter ValidLetterCast = (Letter)OtherEnum.K;
    private const Letter InvalidLetterCast = (Letter)OtherEnum.Twelve;

    public static void PrintLetter (Letter l) {
        Console.WriteLine(l);
    }

    public static void Main () {
        PrintLetter(ValidLetterCast);
        PrintLetter(InvalidLetterCast);
    }
}