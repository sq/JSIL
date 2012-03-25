using System;

class Base {
}

class A : Base {
}

class B : Base {
}

public static class Program {
    public static void Main (string[] args) {
        var flag = true;
        var a = new A();
        var b = new B();
        Console.WriteLine(flag ? a as Base : b as Base);
        Console.WriteLine(!flag ? a as Base : b as Base);
    }
}