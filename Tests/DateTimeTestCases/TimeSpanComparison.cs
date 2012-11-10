using System;

public class Program {
    public static void Main() {
        var ticksZero = TimeSpan.FromTicks(0);
        var secondsZero = TimeSpan.FromSeconds(0);
        var secondsOne = TimeSpan.FromSeconds(1);
        var msOneThousand = TimeSpan.FromMilliseconds(1000);

        Console.WriteLine(ticksZero == secondsZero ? 1 : 0);
        Console.WriteLine(ticksZero != secondsZero ? 1 : 0);
        Console.WriteLine(ticksZero != secondsOne ? 1 : 0);
        Console.WriteLine(msOneThousand > ticksZero ? 1 : 0);
        Console.WriteLine(msOneThousand < ticksZero ? 1 : 0);
        Console.WriteLine(msOneThousand < secondsOne ? 1 : 0);
    }
}