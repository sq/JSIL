using System;

public static class Program
{
    public static int UsedFieldInProgram;
    public static int UnusedFieldInProgram;
    public static int UnusedPropertyInProgram { get; set; }
    public static int UsedPropertyInProgram { get; set; }

    //Assign value to event background field should not be treated as using
    public static event Action UnusedEventInProgram = delegate { };
    public static event Action UsedEventInProgram = delegate { };

    public static void Main(string[] args) {
        UsedFunctionInProgram();
        UsedStaticClass.UsedFunctionInUsedStaticClass();
        UsedClass.UsedStaticFunctionInUsedClass();
        new UsedClass().UsedFunctionInUsedClass();
    }

    public static void UnusedFunctionInProgram()
    {
    }

    public static void UsedFunctionInProgram()
    {
        Console.WriteLine(UsedFieldInProgram);
        Console.WriteLine(UsedPropertyInProgram);
        UsedEventInProgram += () => { };

        // Fire event in c# is not treated as using.
        UnusedEventInProgram();
    }
}

public static class UnusedStaticClass
{
    public static int UnusedFieldInUnusedStaticClass;
    public static int UnusedPropertyInUnusedStaticClass { get; set; }

    public static void UnusedFunctionInUnusedStaticClass()
    {
    }
}

public class UnusedClass
{
    public int UnusedFieldInUnusedClass;
    public static int UnusedStaticPropertyInUnusedClass { get; set; }
    public int UnusedPropertyInUnusedClass { get; set; }

    public void UnusedFunctionInUnusedClass()
    {
    }
}

public static class UsedStaticClass
{
    public static int UsedFieldInUsedStaticClass;
    public static int UnusedFieldInUsedStaticClass;

    public static void UnusedFunctionInUsedStaticClass()
    {
    }

    public static void UsedFunctionInUsedStaticClass()
    {
        Console.WriteLine(UsedFieldInUsedStaticClass);
    }
}

public class UsedClass
{
    public static int UsedStaticFieldInUsedClass;
    public static int UnusedStaticFieldInUsedClass;

    public int UsedFieldInUsedClass;
    public int UnusedFieldInUsedClass;

    public static int UnusedStaticPropertyInUsedClass { get; set; }
    public static int UsedStaticPropertyInUsedClass { get; set; }

    public int UnusedPropertyInUsedClass { get; set; }
    public int UsedPropertyInUsedClass { get; set; }

    public static event Action UnusedStaticEventInUsedClass = delegate { };
    public static event Action UsedStaticEventInUsedClass = delegate { };

    public event Action UnusedEventInUsedClass = delegate { };
    public event Action UsedEventInUsedClass = delegate { };


    public void UnusedFunctionInUsedClass()
    {
    }

    public void UsedFunctionInUsedClass()
    {
        Console.WriteLine(UsedFieldInUsedClass);
        Console.WriteLine(UsedPropertyInUsedClass);
        UsedEventInUsedClass += () => { };

        // Fire event in c# is not treated as using.
        UnusedEventInUsedClass();
    }

    public static void UnusedStaticFunctionInUsedClass()
    {
    }

    public static void UsedStaticFunctionInUsedClass()
    {
        Console.WriteLine(UsedStaticFieldInUsedClass);
        Console.WriteLine(UsedStaticPropertyInUsedClass);
        UsedStaticEventInUsedClass -= () => { };

        // Fire event in c# is not treated as using.
        UnusedStaticEventInUsedClass();
    }
}