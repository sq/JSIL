using System;
using System.Collections.Generic;

public static class Program {
    public struct Sample {
        public int _a;
        public bool _b;

        public Sample (int a, bool b) {
            _a = a;
            _b = b;
        }
    }

    public static void Main () {
        List<Sample> list = new List<Sample>();
        list.Add(new Sample(1, false));
        list.Add(new Sample(2, true));

        // WORKS -- direct access from Sample instance
        Sample s1 = new Sample(1, false);
        Console.WriteLine("{0} {1}", s1._a, s1._b);
        Console.WriteLine("");

        // WORKS -- access from List via specific element
        Console.WriteLine("{0} {1}", list[0]._a, list[0]._b);
        Console.WriteLine("Equals:{0}", s1.Equals(list[0]));
        Console.WriteLine("");
        Console.WriteLine("");

        // BROKEN.  _b acts like an int.  Equality against s1 also fails.
        foreach (var element in list) {
            Console.WriteLine("{0} {1}", element._a, element._b);
            Console.WriteLine("Equals:{0}", s1.Equals(element));
            Console.WriteLine("");
        }
    }
}