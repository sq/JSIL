using System;

namespace Namespace {
    public class Class {
    }

    public enum Enum {
    }

    public static class GenericStaticClass<T> {
    }

    public class GenericClass<T> {
        public class Nested {
        }
    }
}

public static class Program {
    static void TestType(Type type) {
        // For now, remap JSIL.Core to mscorlib.
        Console.WriteLine(type.Namespace.Replace(typeof(object).Assembly.ToString(), "JSIL.Core"));
        Console.WriteLine(type.Name.Replace(typeof(object).Assembly.ToString(), "JSIL.Core"));
        Console.WriteLine(type.FullName.Replace(typeof(object).Assembly.ToString(), "JSIL.Core"));
        Console.WriteLine(type.AssemblyQualifiedName.Replace(typeof(object).Assembly.ToString(), "JSIL.Core"));
        
        if (type.IsValueType)
            Console.WriteLine("ValueType");
        else if (!type.IsValueType)
            Console.WriteLine("NotValueType");

        if (type.IsArray)
            Console.WriteLine("Array");
        else if (!type.IsArray)
            Console.WriteLine("NotArray");

        if (type.IsGenericType) {
            Console.WriteLine("GenericType");
            Console.WriteLine(type.GetGenericTypeDefinition().Name);
        }
        else if (!type.IsGenericType)
            Console.WriteLine("NotGenericType");

        if (type.IsGenericTypeDefinition)
            Console.WriteLine("GenericTypeDefinition");
        else if (!type.IsGenericTypeDefinition)
            Console.WriteLine("NotGenericTypeDefinition");

        // Not implemented yet
        //if (type.ContainsGenericParameters)
        //    Console.WriteLine("ContainsGenericParameters");
        //else if (!type.ContainsGenericParameters)
        //    Console.WriteLine("ContainsGenericParameters");

        if (type.IsEnum)
            Console.WriteLine("Enum");
        else if (!type.IsEnum)
            Console.WriteLine("NotEnum");
    }

    public static void Main (string[] args) {
        TestType(typeof(object));
        TestType(typeof(int[]));
        TestType(typeof(Namespace.Class));
        TestType(typeof(Namespace.Enum));
        TestType(typeof(Namespace.GenericClass<Namespace.Class>));
        TestType(typeof(Namespace.GenericClass<>));

        // Need to change / to + for nested type support
        //TestType(typeof(Namespace.GenericClass<Namespace.Class>.Nested));
        //TestType(typeof(Namespace.GenericClass<>.Nested));
    }
}