using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var Data = new Data();
        Action<string> TestAction = Data.SetValue;
        TestAction("Hello World");
        Console.WriteLine(Data.Name);
    }
}

public sealed class Data
{
    public string Name { get; set; }
}

public static class Helper
{
    public static void SetValue(this Data Self, string Value)
    {
        Self.Name = Value;
    }
}