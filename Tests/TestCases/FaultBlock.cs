using System;
using System.Collections;
using System.Collections.Generic;

public class ADisposable : IDisposable {
    public void Dispose () {
        Console.WriteLine("ADisposable.Dispose");
    }
}

public static class Program {
    public static ADisposable Disposable = new ADisposable();

    public static IEnumerable<int> OneToFive {
        get {
            using (Disposable)
                for (var i = 1; i <= 5; i++)
                    yield return i;
        }
    }

    public static void Main (string[] args) {
        foreach (var i in OneToFive)
            Console.WriteLine("{0}", i);
    }
}