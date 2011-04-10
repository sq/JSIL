using System;

namespace HelloWorld {
    public static class Program {
        public static void Main (string[] args) {
            Console.WriteLine("Hello, World!");
            Console.WriteLine("You provided the following arguments:");
            foreach (var arg in args)
                Console.WriteLine(new CustomType(arg));
        }
    }

    public class CustomType {
        public string Text;
        public int Numeric;

        public CustomType (string text) {
            Text = text;
            Numeric = int.Parse(text);
        }

        public override string ToString () {
            return Text;
        }
    }
}
