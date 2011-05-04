using System;

public static class Program {
    public static void Main (string[] args) {
        Func<int> zero = () => 0, one = () => 1, two = () => 2, three = () => 3;
        Func<int> four = () => 4, five = () => 5, six = () => 6, seven = () => 7;
        Func<bool> True = () => true;

        int i = 0;
        Console.WriteLine(i);
        Console.WriteLine(--i);
        Console.WriteLine(i--);
        Console.WriteLine(i);
        Console.WriteLine(++i);
        Console.WriteLine(i++);
        Console.WriteLine(i);

        Console.WriteLine("+ {0} - {1} / {2} * {3}", one() + two(), one() - two(), four() / two(), four() * two());
        Console.WriteLine("% {0} & {1} | {2} ^ {3}", five() % two(), one() & three(), one() | two(), one() ^ three());
        Console.WriteLine("! {0} ~ {1} ~ {2} - {3}", !True(), ~zero(), ~(-one()), -one());
    }
}