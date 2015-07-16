using System;

class Program
{
    public static void Main()
    {
        // Check raw type GetHashCode
        int i1 = 0;
        object i2 = 0;
        System.Console.WriteLine(i1.GetHashCode() == i2.GetHashCode()  ? "true" : "false");

        // Check type with several base.GetHashCode
        var c1 = new DerivedClass();
        object c2 = new DerivedClass();

        System.Console.WriteLine(c1.GetHashCode() == c2.GetHashCode() ? "true" : "false");
        System.Console.WriteLine(c1.GetHashCode() == c1.GetHashCode() ? "true" : "false");
        System.Console.WriteLine(c2.GetHashCode() == c2.GetHashCode() ? "true" : "false");
    }
}

public class BaseClass
{
    public override int GetHashCode()
    {
        Console.WriteLine("BaseClass.GetHashCode");
        return base.GetHashCode();
    }
}

public class DerivedClass : BaseClass
{
    public void GetHashCode(string a)
    {
        Console.WriteLine("DerivedClass.GetHashCode(string)");
    }

    public override int GetHashCode()
    {
        Console.WriteLine("DerivedClass.GetHashCode");
        return base.GetHashCode();
    }

    public void GetHashCode(int a)
    {
        Console.WriteLine("DerivedClass.GetHashCode(int)");
    }
}