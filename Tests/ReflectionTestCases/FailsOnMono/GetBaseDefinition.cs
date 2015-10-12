using System;
using System.Linq;
using System.Reflection;

public static class Program
{
    public static void Main(string[] args)
    {
        WriteMethods(typeof(Derived2));
        //WriteMethods(typeof(Derived2<,>)); //TODO: Look on generic arguments for open generics.
        WriteMethods(typeof(Derived2<Base, Base>));
        WriteMethods(typeof(PartialGenericHolder<>).GetField("Holder").FieldType);
    }

    public static void WriteMethods(Type type)
    {
        WriteInfo(GetParent(type.GetMethod("Method1")));
        WriteInfo(GetParent(type.GetMethod("Method2")));
        WriteInfo(GetParent(type.GetMethod("NonVirtualMethod")));
        WriteInfo(GetParent(type.GetMethod("GenericMethod")));
        WriteInfo(GetParent(type.GetMethod("GenericMethod").MakeGenericMethod(typeof(Base), typeof(Base))));
        Console.WriteLine();
    }

    public static void WriteInfo(MethodInfo item)
    {
        if (item == null)
        {
            Console.WriteLine("null");
            return;
        }

        Console.WriteLine(
            "{0} {3}.{1}({2})",
            item.ReturnType.Name,
            item.Name,
            string.Join(", ", item.GetParameters().Select(parameter => parameter.ParameterType.Name).ToArray()),
            item.DeclaringType.IsGenericType
                ? string.Format(
                    "{0}<{1}>",
                    item.DeclaringType.Name,
                    string.Join(",", item.DeclaringType.GetGenericArguments().Select(arg => arg.Name).ToArray()))
                : item.DeclaringType.Name);
    }

    private static MethodInfo GetParent(MethodInfo item)
    {
        return item.GetBaseDefinition();
    }
}

public class Base
{
    public virtual void Method1() { }
    public virtual void Method2() { }

    public void NonVirtualMethod() { }
    public virtual void GenericMethod<TA, TB>(TA in1, TB in2) { }
}

public class Derived1 : Base
{
    public override void Method1() { }
}

public class Derived2 : Derived1
{
    public sealed override void Method1() { }
    public override void Method2() { }


    public new void NonVirtualMethod() { }
    public override void GenericMethod<TA, TB>(TA in1, TB in2) { }
}

public class Base<T1, T2>
{
    public virtual void Method1() { }
    public virtual void Method2() { }

    public void NonVirtualMethod() { }
    public virtual void GenericMethod<TA, TB>(TA in1, TB in2) { }
}

public class Derived1<T1, T2> : Base<T1, T2>
{
    public override void Method1() { }
}

public class Derived2<T1, T2> : Derived1<T1, T2>
{
    public sealed override void Method1() { }
    public override void Method2() { }


    public new void NonVirtualMethod() { }
    public override void GenericMethod<TA, TB>(TA in1, TB in2) { }
}

public class PartialGenericHolder<TH>
{
    public Derived2<Base, TH> Holder;
}