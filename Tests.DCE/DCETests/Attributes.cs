using System;

public static class Program
{
    [Test(typeof(Program), new object[] { typeof(Program), "Test arg", new object[] { TestEnum.A, TestEnum.B, null } }, Field2 = typeof(TestAttribute), Prop2 = 99)]
    [Unused("Unused arg", UnusedEnum.A)]
    public static void Main()
    {
        Console.WriteLine(typeof(TestAttribute));
    }
}

public class UnusedAttribute : Attribute
{
    public UnusedAttribute(string input, UnusedEnum input2)
    {
        
    }
}


public class TestAttribute : Attribute
{
    public Type Field1;
    public Type Field2;

    public object Prop1 { get; set; }
    public object Prop2 { get; set; }
    public object Prop3 { get; set; }

    public TestAttribute(Type f1, object p1)
    {
        Field1 = f1;
        Prop1 = p1;
    }
}

public enum TestEnum
{
    A, B, C, D_Usage
}

public enum UnusedEnum
{
    A, B, C_Usage, D
}