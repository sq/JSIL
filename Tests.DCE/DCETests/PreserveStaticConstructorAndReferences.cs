using System;

public static class Program
{
    public static void Main(string[] args)
    {
        new ClassWithStaticConstructor();
    }
}

public class ClassWithStaticConstructor
{
    static ClassWithStaticConstructor()
    {
        Console.WriteLine("Hello from .cctor");
        Console.WriteLine(typeof(PreservedFromTypeReference));
    }
}

public class PreservedFromTypeReference
{
}

public class StrippedType
{
}