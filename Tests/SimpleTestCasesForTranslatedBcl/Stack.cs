using System;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        var stack = new Stack<string>();
        stack.Push("test");
        Console.WriteLine(stack.Pop());
    }
}
