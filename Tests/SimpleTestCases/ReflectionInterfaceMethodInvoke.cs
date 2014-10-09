using System;
using System.Reflection;

public static class Program
{
    public static void Main(string[] args)
    {
        var obj = new NonGeneric();
        var mi = typeof(INonGeneric).GetMethod("InstanceMethod");
        mi.Invoke(obj, new object[] { });
        var action = (Action)Delegate.CreateDelegate(typeof(Action), obj, mi);
        action();
    }
}

public interface INonGeneric
{
    void InstanceMethod();
}

public class NonGeneric : INonGeneric
{
    private readonly string _field = "field";

    public void InstanceMethod()
    {
        Console.WriteLine(_field);
    }
}
