using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        var List = new List<Action>();

        foreach (var Index in new int[] { 1, 2, 3 })
        {
            var Local = Index;

            List.Add(() => Console.WriteLine("Delegate Index(" + Index + ") vs Local(" + Local + ")"));
        }

        foreach (var Action in List)
            Action();
    }
}
