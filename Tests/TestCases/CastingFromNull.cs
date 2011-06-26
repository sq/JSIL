using System;
using System.Collections;
using System.Collections.Generic;

public static class Program
{
    public static void Main(string[] args)
    {
        Object castSource = null;
        SomeClass castDestination = (SomeClass)castSource;
        Console.WriteLine("{0}", castDestination == null);
    }

    public class SomeClass
    {
    }
}