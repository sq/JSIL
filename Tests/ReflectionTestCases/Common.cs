using System;
using System.Reflection;

namespace Common {
    public static class Util {
        public static string[] GetMethodNames (Type type, BindingFlags flags) {
            var methods = type.GetMethods(flags);
            var methodNames = new string[methods.Length];
            for (int i = 0; i < methodNames.Length; i++)
                methodNames[i] = methods[i].Name;

            Array.Sort(methodNames);

            return methodNames;
        }

        public static void AssertMethods (Type type, BindingFlags flags, params string[] names) {
            var methodNames = GetMethodNames(type, flags);

            foreach (var name in names)
                if (System.Array.IndexOf(methodNames, name) < 0)
                    Console.WriteLine("{0} not in methods of {1}", name, type);
        }

        public static void ListMethods (Type type, BindingFlags flags) {
            var methodNames = GetMethodNames(type, flags);

            Console.WriteLine();
            foreach (var methodName in methodNames)
                Console.WriteLine(methodName);
        }
    }
}