using System;

public static class Program {
    public class Container {
        private int _x;

        public Container (int x) {
            Console.WriteLine("Container constructor called");
            _x = x;
        }
    }

    public class Sample {
        private static Container c;

        static Sample () {
            Console.WriteLine("Sample static constructor called");
            //                Dummy();   // Add this line and it works!
            c = new Container(7);
            Func(c);
            Console.WriteLine("Sample static constructor ends");

        }

        static private void Func (Container x) {
            Console.WriteLine("Sample Func called");
        }

        static private void Dummy () {
            Console.WriteLine("Sample Dummy called");
        }

    }

    public static void Main () {
        Sample sample = new Sample();
    }
}