using System;
using System.Collections.Generic;

public static class Program { 
    public static void Main (string[] args) {
        ISample sample = new Sample();
        sample.MethodTest(1, 2);
    }

    public interface ISample {
        void MethodTest (int p1);
        void MethodTest (int p1, int p2);
    }

    public class Sample : ISample {
        public void MethodTest (int p1) {
            Console.WriteLine(1);
        }

        public void MethodTest (int p1, int p2) {
            Console.WriteLine(2);
        }
    }
}