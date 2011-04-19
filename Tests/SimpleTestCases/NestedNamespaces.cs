using System;
using JSIL.Meta;

namespace A {
    namespace B {
        public static class C {
            public static void StaticMethod () {
                Console.WriteLine("StaticMethod");
            }
        }
    }
}

public static class Program { 
    public static void Main (string[] args) {
        A.B.C.StaticMethod();
    }
}