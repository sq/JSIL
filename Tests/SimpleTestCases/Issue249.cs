using System;
using System.Collections.Generic;

public static class Program { 
    public static void Main (string[] args) {
        var strct = new A();
        strct.Field = new B();
        strct.Field.Field = "str";
        Console.WriteLine(strct.Field.Field);
    }

    public class A {
        public B Field { get; set; } 
    }

    public class B {
        public string Field { get; set; }
    }
}