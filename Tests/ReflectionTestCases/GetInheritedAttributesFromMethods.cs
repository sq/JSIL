using System;
using System.Reflection;

public static class Program
{
    public static void Main(string[] args)
    {
        WriteInfo(typeof(Derived2).GetMethod("Method1"));
        WriteInfo(typeof(Derived2).GetMethod("Method2"));
        WriteInfo(typeof(Derived2).GetMethod("NonVirtualMethod"));
        WriteInfo(typeof(Derived2).GetMethod("GenericMethod"));
        WriteInfo(typeof(Derived2).GetMethod("GenericMethod").MakeGenericMethod(typeof(Base)));

        WriteInfo(typeof(GenericDerived2<,>).GetMethod("Method1"));
        WriteInfo(typeof(GenericDerived2<,>).GetMethod("Method2"));
        WriteInfo(typeof(GenericDerived2<,>).GetMethod("NonVirtualMethod"));
        WriteInfo(typeof(GenericDerived2<,>).GetMethod("GenericMethod"));
        WriteInfo(typeof(GenericDerived2<,>).GetMethod("GenericMethod").MakeGenericMethod(typeof(Base)));

        WriteInfo(typeof(GenericDerived2<Base, Base>).GetMethod("Method1"));
        WriteInfo(typeof(GenericDerived2<Base, Base>).GetMethod("Method2"));
        WriteInfo(typeof(GenericDerived2<Base, Base>).GetMethod("NonVirtualMethod"));
        WriteInfo(typeof(GenericDerived2<Base, Base>).GetMethod("GenericMethod"));
        WriteInfo(typeof(GenericDerived2<Base, Base>).GetMethod("GenericMethod").MakeGenericMethod(typeof(Base)));

        WriteInfo(typeof(PartialGenericHolder<Base>).GetField("Field").FieldType.GetMethod("Method1"));
        WriteInfo(typeof(PartialGenericHolder<Base>).GetField("Field").FieldType.GetMethod("Method2"));
        WriteInfo(typeof(PartialGenericHolder<Base>).GetField("Field").FieldType.GetMethod("NonVirtualMethod"));
        WriteInfo(typeof(PartialGenericHolder<Base>).GetField("Field").FieldType.GetMethod("GenericMethod"));
        WriteInfo(typeof(PartialGenericHolder<Base>).GetField("Field").FieldType.GetMethod("GenericMethod").MakeGenericMethod(typeof(Base)));
    }

    public static void WriteInfo(MemberInfo item)
    {
        Console.WriteLine(item.Name);
        Console.WriteLine(item.GetCustomAttributes(typeof(CustomAttribute), true).Length);
        Console.WriteLine(item.GetCustomAttributes(typeof(CustomAttribute), false).Length);
    }
}

public class Base
{
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
    [Custom]
    public override void Method1() { }
}

public class Derived2 : Derived
{
    public override void Method1() { }
    public override void Method2() { }


    public new void NonVirtualMethod() { }
    public override void GenericMethod<TA>(TA in1) { }
}

public class GenericBase<T1, T2>
{
    public virtual void Method1() { }

    [Custom]
    public virtual void Method2() { }

    [Custom]
    public void NonVirtualMethod() { }

    [Custom]
    public virtual void GenericMethod<TA>(TA in1) { }
}

public class GenericDerived<T1, T2> : GenericBase<T1, T2>
{
    [Custom]
    public override void Method1() { }
}

public class GenericDerived2<T1, T2> : GenericDerived<T1, T2>
{
    public override void Method1() { }
    public override void Method2() { }


    public new void NonVirtualMethod() { }
    public override void GenericMethod<TA>(TA in1) { }
}

public class PartialGenericHolder<T2>
{
    public GenericDerived2<Base, T2> Field;
}

[AttributeUsage(AttributeTargets.All, Inherited = true)]
public class CustomAttribute : Attribute
{
    public Type T;
}