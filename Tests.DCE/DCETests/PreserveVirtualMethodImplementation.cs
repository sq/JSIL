using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var obj = new UsedDerivedType();
        RunFromBase(obj);
        RunFromIterface(obj);
    }

    public static void RunFromBase(BaseType obj)
    {
        obj.MethodFromBaseType();
        obj.Method();
    }

    public static void RunFromIterface(IIterface obj)
    {
        obj.MethodFromIIterface();
    }
}

public interface IIterface
{
    void MethodFromIIterface();
}

public interface IIterfaceNotUsed
{
    void UnusedMethodFromIIterfaceNotUsed();
    void UsedMethodFromIIterfaceNotUsed();

}

public class BaseType
{
    public virtual void Method()
    {
        Console.WriteLine("BaseType.Method - used");
    }

    public virtual void MethodFromBaseType()
    {
        Console.WriteLine("BaseType.MethodFromBaseType - used");
    }

    public virtual void UnusedMethodFromBaseType()
    {
        Console.WriteLine("BaseType.UnusedMethodFromBaseType - used");
    }
}

public class UsedDerivedType : BaseType, IIterface, IIterfaceNotUsed
{
    public new void Method()
    {
        Console.WriteLine("UsedDerivedType.Method - used");
    }

    public void MethodFromIIterface()
    {
        Console.WriteLine("UsedDerivedType.MethodFromIIterface - used");
    }

    public override void MethodFromBaseType()
    {
        Console.WriteLine("UsedDerivedType.MethodFromBaseType - used");
    }

    public void UnusedMethodFromBaseType()
    {
        Console.WriteLine("UsedDerivedType.UnusedMethodFromBaseType - used");
    }

    public void UnusedMethodFromIIterfaceNotUsed()
    {
        Console.WriteLine("UsedDerivedType.UnusedMethodFromIIterfaceNotUsed - used");
    }

    public void UsedMethodFromIIterfaceNotUsed()
    {
        Console.WriteLine("UsedDerivedType.UsedMethodFromIIterfaceNotUsed - used");
    }
}

public class UnusedDerivedType : BaseType, IIterface
{
    public new void Method()
    {
        Console.WriteLine("UnusedDerivedType.Method - used");
    }

    public void MethodFromIIterface()
    {
        Console.WriteLine("UnusedDerivedType.MethodFromIIterface - used");
    }

    public override void MethodFromBaseType()
    {
        Console.WriteLine("UnusedDerivedType.MethodFromBaseType - used");
    }
}