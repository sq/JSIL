using System;

class Program {
    private static char ToHex (int b) {
        return (char)((b < 0xA) ? ('0' + b) : ('a' + b - 0xA));
    }

    public static void Main () {
        for (int b = 0; b < 16; b++) {
            Console.WriteLine(ToHex(b));
        }
    }
}