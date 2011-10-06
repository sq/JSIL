using System;
using System.Collections;
using System.Collections.Generic;

public static class Program
{
    class A : IEnumerable
    {
        public void Add(string a, string b)
        {
            Console.WriteLine(a);
            Console.WriteLine(b);
        }

        public IEnumerator GetEnumerator()
        {
            throw new Exception();
        }
    }

    class B
    {
        public B()
        {
            Collection = new A();
        }
        public A Collection { get; set; }
    }

    private static B fieldB;

    public static void Main(string[] args)
    {
        fieldB = new B
        {
            Collection = { 
                { "first", "a" },
                { "second", "b" }
            }
        };

        var b = new B
        {
            Collection = new A { 
                { "first", "a" },
                { "second", "b" }
            }
        };
    }
}