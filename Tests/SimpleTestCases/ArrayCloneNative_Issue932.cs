using System;
using JSIL;

public static class Program {
    public static void Main(string[] args)
    {
        var arr = ((object[]) Verbatim.Expression("[1,2,3]")) ?? new object[] {1, 2, 3};
        var arr2 = (object[]) arr.Clone();
        foreach (var item in arr2)
        {
            Console.Write(item);
        }
    }
}