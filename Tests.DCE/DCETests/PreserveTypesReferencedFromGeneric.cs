using System;

public static class Program
{
    public static PreservedType Field;
    public static void Main(string[] args) {
        Console.WriteLine(new GenericTypeGen1<GenericTypeGen2<PreservedType>>());
    }

}

public class GenericTypeGen1<T>
{
}

public class GenericTypeGen2<T>
{
}

public class PreservedType
{
}

public class StrippedType
{
}