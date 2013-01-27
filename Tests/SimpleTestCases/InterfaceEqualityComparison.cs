using System;

public interface I { }

public class A : I { }

public class B : I {
    public void X (I i) {
        if (this == i)
            Console.WriteLine("equal");
        else
            Console.WriteLine("not equal");
    }
}

public static class Program {

    public static void Main () {
        var b = new B();
        b.X(new A());
    }
}