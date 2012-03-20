using System;
using System.Reflection;

namespace Common {
    public static class Util {
        public static void ListMethods (Type type, BindingFlags flags) {
            var methods = type.GetMethods(flags);
            var methodNames = new string[methods.Length];
            for (int i = 0; i < methodNames.Length; i++)
                methodNames[i] = methods[i].Name;

            Array.Sort(methodNames);

            foreach (var methodName in methodNames)
                Console.WriteLine(methodName);
        }
    }
}