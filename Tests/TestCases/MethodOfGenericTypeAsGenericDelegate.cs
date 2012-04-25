using System;

class X<T> {
    public void Run () {
        Y(actn);
    }

    void Y (Action<T> t) {
        t(default(T));
    }

    void actn (T t) {
        Console.WriteLine("howdy");
    }
}

public class Program {
    public static void Main (string[] args) {
        var x = new X<int>();
        x.Run();
    }
}
