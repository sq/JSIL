using System;

public interface IMutatable
{
    void Mutate();
    int X { get; }
}

public struct Struct : IMutatable
{
    public int _x;

    public void Mutate()
    {
        _x++;
    }

    public int X { get { return _x; } }
}

public static class Program
{
    public static void Main(string[] args)
    {
        var s = new Struct();

        GenericMethod(s);
        Console.WriteLine(s.X);
    }

    public static void GenericMethod<T>(T s) where T : IMutatable
    {
        s.Mutate();
    }
}