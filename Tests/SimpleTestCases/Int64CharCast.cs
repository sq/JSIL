using System;

public static class Program {
    public static void Main (string[] args)
    {
        DoIt((char)(GetIt() + 5));
    }

    public static void DoIt(char input)
    {
        Console.WriteLine(input);
    }

    public static long GetIt()
    {
        return 10;
    }
}