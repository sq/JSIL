using System;
using System.Collections.Generic;

public static class Program {
    public struct Sample {
        public int _a;

        public Sample (int a) {
            _a = a;
        }

        public int A { get { return _a; } }
    }

    public static void Main() {
        List<Sample> list = new List<Sample>();
        list.Add(new Sample(1));
        list.Add(new Sample(2));
        list.Add(new Sample(3));
        list.Add(new Sample(4));
        list.Add(new Sample(5));
        list.Add(new Sample(6));

        Console.WriteLine(list.Contains(new Sample(3)).ToString().ToLower());     // WORKS

        list.Remove(new Sample(3));

        foreach (var s in list)
            Console.WriteLine(s.A);        // Broken: List still contains all 6 elements. 

        // WORKS also
        Console.WriteLine(new Sample(3).Equals(new Sample(3)).ToString().ToLower());
    }
}