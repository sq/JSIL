using System;

public static class Program {
    public static void Main ()
    {
        Write(new object[] {1, 2, 3});
    }

    public static void Write(object input)
    {
        Console.WriteLine(input.GetType());
    }
}
