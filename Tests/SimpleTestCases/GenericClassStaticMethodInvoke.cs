using System;

public class Program {
    public static void Main (string[] args)
    {
        typeof(GenericClass<ClassA>).GetMethod("StaticMethod").Invoke(null, new object[0]);
    }
}

public class GenericClass<A>
{
    public static void StaticMethod()
    {
        Console.WriteLine(typeof(A));
    }
}

public class ClassA {}