using System;
using JSIL.Meta;

public static class Program { 
    public static void Main (string[] args) {
        for (var i = 0; i < 10; i++)
            Console.WriteLine(i);

        for (var j = 0; j < 10; j++)
            ;

        for (var k = 5; k >= 0; --k) {
            if (k % 2 == 0)
                continue;

            Console.WriteLine(k);
        }
    }
}