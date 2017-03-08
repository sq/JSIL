using System;
using System.Threading.Tasks;

public static class Program {
    public static void Main (string[] args)
    {
        var task = Task.FromResult(new A(true));
        Console.WriteLine(task.GetType().FullName);
        Console.WriteLine(task.Status);
        Console.WriteLine(task.Result.Value ? "true" : "false");
    }
}

public class A
{
    public readonly bool Value;

    public A(bool value)
    {
        Value = value;
    }
}