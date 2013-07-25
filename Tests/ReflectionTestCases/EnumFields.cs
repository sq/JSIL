using System;
using System.Reflection;

enum Letter {
    A, B, C
}

class Program {
    public static void Main () {
        FieldInfo[] fields = typeof(Letter).GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (FieldInfo field in fields) {
            var c = field.GetRawConstantValue();
            Console.WriteLine("{0} = {1} {2}", field.Name, c.GetType(), c);
        }
    }
}