using System;

public static class Program { 
    public static void Main (string[] args) {
        (new A()).Method(1);
    }    
}

public class A {
    public void Method (int @this) {
        Console.WriteLine("this={0}, @this={1}", this, @this);
    }
}
