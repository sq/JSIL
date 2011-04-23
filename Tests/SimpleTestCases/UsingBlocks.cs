using System;
using System.Collections.Generic;

public static class Program {
    public class TestClass : IDisposable {
        public TestClass () {
            System.Console.WriteLine("new TestClass()");
        }

        public void Dispose () {
            System.Console.WriteLine("TestClass.Dispose()");
        }
    }

    public static void Main (string[] args) {
        using (var instance = new TestClass()) {
            Console.WriteLine("Body");
        }
    }
}