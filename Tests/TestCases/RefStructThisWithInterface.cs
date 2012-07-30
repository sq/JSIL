using System;

public interface I {
    int Value { get; set; }
    void Method ();
}

public struct A : I {
    public int Value { get; set; }

    public void Method () {
        Method2(ref this);
        I test = this;
        test.Value = 12;
        Console.WriteLine(this.Value);
    }

    public void Method2<T> (ref T a) where T : I {
        I copy = a;
        a.Value += 2;
        Console.WriteLine(copy.Value);
    }
}

public static class Program {
    public static void Test<T> (ref T obj) where T : I {
        obj.Value = 4;
        obj.Method();
        Console.WriteLine(obj.Value);
    }

    public static void Main (string[] args) {
        var obj = new A();
        Test(ref obj);
    }
}