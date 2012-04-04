using System;
using System.Reflection;

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

        public static void AssertMembers<T> (Type type, BindingFlags flags, params string[] names) where T : MemberInfo {
            var methodNames = GetMemberNames<T>(type, flags);

            foreach (var name in names)
                if (System.Array.IndexOf(methodNames, name) < 0)
                    Console.WriteLine("{0} not in members of {1}", name, type);
        }

        public static void ListMembers<T> (Type type, BindingFlags flags) where T : MemberInfo {
            var methodNames = GetMemberNames<T>(type, flags);

            Console.WriteLine();
            foreach (var methodName in methodNames)
                Console.WriteLine(methodName);
        }
    }
}