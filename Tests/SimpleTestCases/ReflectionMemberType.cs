using System;

public static class Program {
    public static void Main (string[] args)
    {
        Console.WriteLine(typeof(TestClass).GetConstructor(new Type[0]).MemberType);
        Console.WriteLine(typeof(TestClass).GetField("Field").MemberType);
        Console.WriteLine(typeof(TestClass).GetProperty("Prop").MemberType);
        Console.WriteLine(typeof(TestClass).GetEvents()[0].MemberType);
        Console.WriteLine(typeof(TestClass).GetMethod("Method").MemberType);
    }
}

public class TestClass
{
    public TestClass()
    {
    }

    public int Field;
    public int Prop {
        get { return 0; }
    }
    public event EventHandler Event;

    public void Method()
    {
    }
}