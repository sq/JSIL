using System;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(GenericType<NonGenericType>.StaticField);
    }

}

public class GenericType<T>
{
    public static T StaticField;
}

public class NonGenericType
{
}
