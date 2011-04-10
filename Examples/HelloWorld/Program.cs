using System;

namespace HelloWorld {
    public static class Program {
        public static void Main (string[] args) {
            Console.WriteLine("Hello, World!");
            Console.WriteLine("You provided the following arguments:");
            foreach (var arg in args)
                Console.WriteLine(arg);
        }
    }
}
