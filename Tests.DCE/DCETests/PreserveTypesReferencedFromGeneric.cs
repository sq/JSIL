using System;

public static class Program
{
    public static void Main(string[] args)
    {
        new PreservedFromGenericTypeConstructorGenericTypeGen1
            <PreservedFromGenericTypeConstructorGenericTypeGen2<PreservedFromGenericTypeConstructor>>();
        PreservedFromGenericTypeStaticMethodGenericTypeGen1
            <PreservedFromGenericTypeStaticMethodGenericTypeGen2<PreservedFromGenericTypeStaticMethod>>.Run();
        GenericMethod
            <PreservedFromGenericMethodMethodGenericTypeGen1
                    <PreservedFromGenericMethodGenericTypeGen2<PreservedFromGenericMethod>>>();
    }

    public static void GenericMethod<T>()
    {
        Console.WriteLine("GenericMethod");
    }

}

public class PreservedFromGenericTypeConstructorGenericTypeGen1<T>
{
}

public class PreservedFromGenericTypeConstructorGenericTypeGen2<T>
{
}

public class PreservedFromGenericTypeConstructor
{
}

public class PreservedFromGenericTypeStaticMethodGenericTypeGen1<T>
{
    public static void Run()
    {
        Console.WriteLine("PreservedFromGenericTypeStaticMethodGenericTypeGen1");
    }
}

public class PreservedFromGenericTypeStaticMethodGenericTypeGen2<T>
{
}

public class PreservedFromGenericTypeStaticMethod
{
}

public class PreservedFromGenericMethodMethodGenericTypeGen1<T>
{
}

public class PreservedFromGenericMethodGenericTypeGen2<T>
{
}

public class PreservedFromGenericMethod
{
}

public class StrippedType
{
}