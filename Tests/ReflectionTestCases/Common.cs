using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Common {
    public static class Util {
        public static string[] GetMemberNames<T> (Type type, BindingFlags flags) where T : MemberInfo {
            var members = type.GetMembers(flags);

            int count = 0;
            for (int i = 0; i < members.Length; i++) {
                if (members[i] is T)
                    count += 1;
            }

            var names = new string[count];
            for (int i = 0, j = 0; i < members.Length; i++) {
                T t = (members[i] as T);
                if ((object)t == null)
                    continue;

                names[j] = t.Name;
                j += 1;
            }

            Array.Sort(names);

            return names;
        }

        public static int AssertMembers<T> (Type type, BindingFlags flags, params string[] names) where T : MemberInfo {
            int result = 0;
            var methodNames = new List<string>(GetMemberNames<T>(type, flags));

            foreach (var name in names) {
                int count = methodNames.FindAll((n) => n == name).Count;

                if (count < 1)
                    Console.WriteLine("{0} not in members of {1}", name, type);

                result += count;
            }

            return result;
        }

        public static void ListMembers<T> (Type type, BindingFlags flags) where T : MemberInfo {
            var methodNames = GetMemberNames<T>(type, flags);

            Console.WriteLine();
            foreach (var methodName in methodNames)
                Console.WriteLine(methodName);
        }

        public static string[] GetTypeNames (Assembly asm, string filterRegex = null) {
            var types = asm.GetTypes();

            Regex regex = null;
            if (filterRegex != null)
                regex = new Regex(filterRegex, RegexOptions.ECMAScript);

            var result = new List<string>();
            for (int i = 0, l = types.Length; i < l; i++) {
                var fullName = types[i].FullName;
                if ((regex == null) || regex.IsMatch(fullName))
                    result.Add(fullName);
            }

            result.Sort();

            return result.ToArray();
        }

        public static void ListTypes (Assembly asm, string filterRegex = null) {
            var typeNames = GetTypeNames(asm, filterRegex);

            Console.WriteLine();
            foreach (var typeName in typeNames)
                Console.WriteLine(typeName);
        }
    }
}