using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Tests.ReflectionTestCases
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Type abstractType = typeof(AbstractClass);
            Type nonAbstractType = typeof(NonAbstractClass);

            Console.WriteLine("Should be true: {0}, Should be false: {1}", abstractType.IsAbstract, nonAbstractType.IsAbstract);
        }
    }

    public abstract class AbstractClass
    {

    }

    public class NonAbstractClass
    {

    }
}
