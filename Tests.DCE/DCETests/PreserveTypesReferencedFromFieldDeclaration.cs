using System;

public static class Program
{
    public static PreservedType Field;
    public static void Main(string[] args) {
        Console.WriteLine(Field);
    }

}

public class PreservedType
{
}

public class StrippedType
{
}