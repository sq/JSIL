using System;

public static class Program {
    public static void Main ()
    {
        Console.WriteLine(typeof(TestClass).GetType());
        Console.WriteLine(typeof(TestClass).Assembly.GetType());
        Console.WriteLine(typeof(TestClass).GetConstructor(new Type[0]).GetType());
        Console.WriteLine(typeof(TestClass).GetMethod("Method").GetType());
        Console.WriteLine(typeof(TestClass).GetMethod("Method").GetParameters()[0].GetType());
        Console.WriteLine(typeof(TestClass).GetProperty("Property").GetType());
        Console.WriteLine(typeof(TestClass).GetEvents()[0].GetType());

        var fieldRuntimeType = typeof (TestClass).GetField("Field").GetType().FullName;
        Console.WriteLine(fieldRuntimeType == "System.Reflection.RuntimeFieldInfo" ||
                          fieldRuntimeType == "System.Reflection.RtFieldInfo"
            ? "System.Reflection.RuntimeFieldInfo"
            : fieldRuntimeType);
    }
}

public class TestClass
{
    public int Field;

    public event EventHandler Event;

    public TestClass()
    {
    }

    public int Property { get; set; }

    public void Method(int arg)
    {
        
    }
}