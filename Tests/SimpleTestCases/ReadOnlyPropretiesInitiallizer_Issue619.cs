using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var instance = new ClassWithReadOnlyProperty
        {
            Property = { SomeString = "Modified" }
        };

        Console.WriteLine(instance.Property.SomeString);
    }
}

public class ClassWithReadOnlyProperty
{
    private ComplexProperty _property = new ComplexProperty {SomeString = "Initial"};

    public ComplexProperty Property
    {
        get { return _property; }
    }
}


public class ComplexProperty 
{
    public string SomeString { get; set; }
}

