using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var str = "\01";
        Console.WriteLine(str.Length);

        Console.WriteLine(str[0] == (char)0 ? "true" : "false");
        Console.WriteLine(str[1] == '1' ? "true" : "false");
    }

}
