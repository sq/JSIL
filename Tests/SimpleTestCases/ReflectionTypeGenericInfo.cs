using System;
using System.Linq.Expressions;

public static class Program
{
    public static void Main()
    {
        Write(typeof(Base<,>), 1);
        Write(typeof(Derived<>), 2);
        Write(typeof(Derived<>).BaseType, 3);
        Write(typeof(Derived<>).GetField("Field1").FieldType, 4);
        Write(typeof(Derived<>).GetField("Field2").FieldType, 5);
        Write(typeof(Derived<>).GetField("Field3").FieldType, 6);
        Write(typeof(Derived<>.Nested), 7);

        Write(typeof(Base<SomeClass, SomeClass>), 8);
        Write(typeof(Derived<SomeClass>), 9);
        Write(typeof(Derived<SomeClass>).BaseType, 10);
        Write(typeof(Derived<SomeClass>).GetField("Field1").FieldType, 11);
        Write(typeof(Derived<SomeClass>).GetField("Field2").FieldType, 12);
        Write(typeof(Derived<SomeClass>).GetField("Field3").FieldType, 13);
        Write(typeof(Derived<SomeClass>.Nested), 14);
        Write(typeof(Derived<SomeClass>[]), 15);
    }

    public static void Write(Type type, int index)
    {
        Console.WriteLine("{1} IGT: {0}", type.IsGenericType, index);
        Console.WriteLine("{1} IGTD: {0}", type.IsGenericTypeDefinition, index);
        Console.WriteLine("{1} CGP: {0}", type.ContainsGenericParameters, index);
        Console.WriteLine("{1} IGP: {0}", type.IsGenericParameter, index);
    }
}

public class Base<T, U>
{
    public static T M1Base(U u) { return default(T); }
}

public class Derived<V> : Base<SomeClass, V>
{
    public V Field1;
    public G<V> Field2;
    public G<G<V>> Field3;


    public class Nested
    {
        void M1Nested() { }
    }

    public static void M1Derived<W>() { }
}

public class G<T> { }

public class SomeClass {}