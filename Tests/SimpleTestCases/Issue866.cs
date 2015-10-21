using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(typeof(IInterface).IsAssignableFrom(typeof(Holder).GetField("Field").FieldType) ? "true" : "false");
    }
}

public interface IInterface {}
public class GenericClass<T> : IInterface
{ }

public class Holder
{
    public GenericClass<object> Field;
}
