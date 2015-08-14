using System;

public class GenericClass<T> {
    public GenericStruct<T> Field;
}

public struct GenericStruct<T> {
}

public static class Program {
    public static void Main (string[] args) {
        WriteClassName(new GenericClass<string>().Field);
        WriteClassName(new GenericClass<int>().Field);
    }

    public static void WriteClassName(object input) {
        Console.WriteLine(GetTypeName(input.GetType()));
    }

    public static string GetTypeName(Type type) {
        var name = (string.IsNullOrEmpty(type.Namespace) ? string.Empty : (type.Namespace + ".")) + type.Name;
        if (type.IsGenericType) {
            name += "[";
            bool isFirst = true;
            foreach (var genericTypeArgument in type.GetGenericArguments()) {
                if (!isFirst) {
                    name += ", ";
                }
                isFirst = false;

                name += GetTypeName(genericTypeArgument);
            }
            name += "]";
        }
        return name;
    }
}