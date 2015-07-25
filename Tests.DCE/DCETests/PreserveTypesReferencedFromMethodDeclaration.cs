using System;

public static class Program
{
    public static void Main(string[] args)
    {
        PreservedMethodDeclarationType.TestMethod(null, null);
    }

}

public class PreservedMethodDeclarationType
{
    public static PreservedMethodReturnType TestMethod(
        PreservedMethodFirstArgumentType arg1,
        PreservedMethodSecondArgumentType arg2)
    {
        Console.WriteLine("TestMethod");
        return null;
    }
}

public class PreservedMethodReturnType
{
}

public class PreservedMethodFirstArgumentType
{
}

public class PreservedMethodSecondArgumentType
{
}

public class StrippedType
{
}