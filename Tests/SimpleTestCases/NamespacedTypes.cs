using System;

namespace A {
    public class A {
    }
}

namespace B {
    public class A {
    }
}

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(new A.A());
        Console.WriteLine(new B.A());
    }
}