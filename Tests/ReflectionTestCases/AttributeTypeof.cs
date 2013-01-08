using System;

public class X : Attribute
{
    public Type T { get; set; }
    public X(Type t) { T = t; }
}

public class A
{
    [X(typeof(Exception))]
    public void Aa() { }
}

public static class Program
{

    public static void Main()
    {
        var m = typeof(A).GetMethod("Aa");
        var a = m.GetCustomAttributes(false)[0];
        Console.WriteLine(
            ((X)a).T == typeof(Exception) ? "True" : "False");
    }
}