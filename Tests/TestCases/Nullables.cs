using System;

public static class Program {
    public static void PrintNullable (int? i) {
        Console.WriteLine("{0} * 2 = {1}", i, i * 2);
    }

    public static void Main(string[] args)
    {
        int? d = new Nullable<int>();
        int? one = 1;

        Console.WriteLine("{0} {1}", d.HasValue, one.HasValue);
        Console.WriteLine("{0} {1}", d.GetValueOrDefault(), one.GetValueOrDefault());
        Console.WriteLine("{0}", one.Value);

        PrintNullable(one);
        PrintNullable(2);
    }
}