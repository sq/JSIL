using System;
using System.Reflection;

public static class Program
{
    public static void Main(string[] args)
    {
        WriteInfo(typeof(Derived).GetProperty("Prop"));
    }

    public static void WriteInfo(MemberInfo item)
    {
        Console.WriteLine(item.Name);
        Console.WriteLine(Attribute.GetCustomAttributes(item, typeof(CustomAttribute), false).Length);
        Console.WriteLine(Attribute.GetCustomAttributes(item, typeof(CustomAttribute), true).Length);

        Console.WriteLine(item.GetCustomAttributes(typeof(CustomAttribute), false).Length);
        Console.WriteLine(item.GetCustomAttributes(typeof(CustomAttribute), true).Length);
    }
}

public class Base
{
    [Custom]
    public virtual int Prop { get; set; }

    public virtual void Method1() { }

    [Custom]
    public virtual void Method2() { }

    [Custom]
    public void NonVirtualMethod() { }

    [Custom]
    public virtual void GenericMethod<TA>(TA in1) { }
}

public class Derived : Base
{
    public override int Prop { get; set; }
}


[AttributeUsage(AttributeTargets.All, Inherited = true)]
public class CustomAttribute : Attribute
{
    public Type T;
}