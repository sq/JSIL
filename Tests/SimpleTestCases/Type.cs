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
        Console.WriteLine("Namespace: {0}", type.Namespace.Replace(typeof(object).Assembly.ToString(), "JSIL.Core"));
        Console.WriteLine("Name: {0}", type.Name.Replace(typeof(object).Assembly.ToString(), "JSIL.Core"));
        Console.WriteLine("FullName: {0}", type.FullName.Replace(typeof(object).Assembly.ToString(), "JSIL.Core"));
        Console.WriteLine("AssemblyQualifiedName: {0}", type.AssemblyQualifiedName.Replace(typeof(object).Assembly.ToString(), "JSIL.Core"));

        if (type.IsGenericType)
            Console.WriteLine("GenericTypeDefinition: {0}", type.GetGenericTypeDefinition().Name);

        Console.Write("Attributes: ");

        if (type.IsValueType)
            Console.Write("ValueType ");
        else if (!type.IsValueType)
            Console.Write("ReferenceType ");

        if (type.IsArray)
            Console.Write("Array ");
        else if (!type.IsArray)
            Console.Write("NotArray ");

        if (type.IsGenericType)
            Console.Write("GenericType ");
        else if (!type.IsGenericType)
            Console.Write("NotGenericType ");

        if (type.IsGenericTypeDefinition)
            Console.Write("GenericTypeDefinition ");
        else if (!type.IsGenericTypeDefinition)
            Console.Write("NotGenericTypeDefinition ");

        // Not implemented yet
        //if (type.ContainsGenericParameters)
        //    Console.WriteLine("ContainsGenericParameters");
        //else if (!type.ContainsGenericParameters)
        //    Console.WriteLine("ContainsGenericParameters");

        if (type.IsEnum)
            Console.Write("Enum ");
        else if (!type.IsEnum)
            Console.Write("NotEnum ");

        Console.WriteLine();
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