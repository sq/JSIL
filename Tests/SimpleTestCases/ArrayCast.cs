using System;
using System.Collections;
using System.Collections.Generic;


public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("TestClass[]");
        TestObject(new TestClass[0]);
        Console.WriteLine();

        Console.WriteLine("TestStruct[]");
        TestObject(new TestStruct[0]);
        Console.WriteLine();

        Console.WriteLine("object[]");
        TestObject(new object[0]);
        Console.WriteLine();

        Console.WriteLine("TestClass[,]");
        TestObject(new TestClass[0,0]);
        Console.WriteLine();

        Console.WriteLine("TestStruct[,]");
        TestObject(new TestStruct[0, 0]);
        Console.WriteLine();

        Console.WriteLine("object[,]");
        TestObject(new object[0, 0]);
        Console.WriteLine();

        Console.WriteLine("null");
        TestObject(null);
    }

    public static void TestObject(object c)
    {
        Console.WriteLine("IEnumerable: " + (c is IEnumerable ? "true" : "false"));
        Console.WriteLine("ICollection: " + (c is ICollection ? "true" : "false"));
        Console.WriteLine("IList: " + (c is IList ? "true" : "false"));

        Console.WriteLine("IEnumerable<object>: " + (c is IEnumerable<object> ? "true" : "false"));
        Console.WriteLine("ICollection<object>: " + (c is ICollection<object> ? "true" : "false"));
        Console.WriteLine("IList<object>: " + (c is IList<object> ? "true" : "false"));

        Console.WriteLine("IEnumerable<TestClass>: " + (c is IEnumerable<TestClass> ? "true" : "false"));
        Console.WriteLine("ICollection<TestClass>: " + (c is ICollection<TestClass> ? "true" : "false"));
        Console.WriteLine("IList<TestClass>: " + (c is IList<TestClass> ? "true" : "false"));

        Console.WriteLine("IEnumerable<TestStruct>: " + (c is IEnumerable<TestStruct> ? "true" : "false"));
        Console.WriteLine("ICollection<TestStruct>: " + (c is ICollection<TestStruct> ? "true" : "false"));
        Console.WriteLine("IList<TestStruct>: " + (c is IList<TestStruct> ? "true" : "false"));

        Console.WriteLine("IEnumerable<OtherTestClass>: " + (c is IEnumerable<OtherTestClass> ? "true" : "false"));
        Console.WriteLine("ICollection<OtherTestClass>: " + (c is ICollection<OtherTestClass> ? "true" : "false"));
        Console.WriteLine("IList<OtherTestClass>: " + (c is IList<OtherTestClass> ? "true" : "false"));

        Console.WriteLine("Array: " + (c is Array ? "true" : "false"));
        Console.WriteLine("TestClass[]: " + (c is TestClass[] ? "true" : "false"));
        Console.WriteLine("TestStruct[]: " + (c is TestStruct[] ? "true" : "false"));
        Console.WriteLine("object[]: " + (c is object[] ? "true" : "false"));
        Console.WriteLine("OtherTestClass[]: " + (c is OtherTestClass[] ? "true" : "false"));
        Console.WriteLine("TestClass[,]: " + (c is TestClass[,] ? "true" : "false"));
        Console.WriteLine("TestStruct[,]: " + (c is TestStruct[,] ? "true" : "false"));
        Console.WriteLine("object[,]: " + (c is object[,] ? "true" : "false"));
        Console.WriteLine("OtherTestClass[,]: " + (c is OtherTestClass[,] ? "true" : "false"));

    }
}

public class TestClass
{
}

public struct TestStruct
{
}

public class OtherTestClass
{
}