using System;

public static class Program
{
    public static void Main(string[] args)
    {
        new NonGenericDerivedType();
    }

}

public class TypeForT
{
}

public class TypeForK
{
}

public class BaseGenericType<T, K>
{
}

public class DerivedGenericType<K> : BaseGenericType<TypeForT, K>
{
}

public class NonGenericDerivedType : DerivedGenericType<TypeForK>
{
}

public class StrippedType
{
}