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

        // Equals
        System.Console.WriteLine(i1.Equals(i2) ? "true" : "false");
        System.Console.WriteLine(i2.Equals(i1) ? "true" : "false");
        System.Console.WriteLine(i1.Equals(i1) ? "true" : "false");
        System.Console.WriteLine(i2.Equals(i2) ? "true" : "false");

        System.Console.WriteLine(c1.Equals(c2) ? "true" : "false");
        System.Console.WriteLine(c2.Equals(c1) ? "true" : "false");
        System.Console.WriteLine(c1.Equals(c1) ? "true" : "false");
        System.Console.WriteLine(c2.Equals(c2) ? "true" : "false");
    }
}

public class BaseClass
{
    public override int GetHashCode()
    {
        Console.WriteLine("BaseClass.GetHashCode");
        return base.GetHashCode();
    }

    public override bool Equals(object other)
    {
        Console.WriteLine("BaseClass.Equals");
        return base.Equals(other);
    }

    public bool Equals(BaseClass other)
    {
        Console.WriteLine("BaseClass.Equals");
        return base.Equals(other);
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

    public override bool Equals(object other)
    {
        Console.WriteLine("DerivedClass.Equals");
        return base.Equals(other);
    }
}