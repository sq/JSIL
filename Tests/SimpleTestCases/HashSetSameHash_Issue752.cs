using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

public static class Program
{
    public static void Main(string[] args)
    {
        var hashSet = new HashSet<SameHash>() { new SameHash(1), new SameHash(2), new SameHash(3), new SameHash(4), new SameHash(5), new SameHash(6) };

        Console.WriteLine(hashSet.Count);

        foreach (var item in hashSet)
        {
            Console.WriteLine(item.Value);
        }
    }


    public class SameHash
    {
        public int Value { get; private set; }

        public SameHash(int value)
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value / 2;
        }
    }
}