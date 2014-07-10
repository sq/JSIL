using System;
using System.Collections.Generic;

public static class Program {
    public static int I = 0;

    public static void Main () {
        Console.WriteLine(Time(Test));
    }

    public static int Test () {
        int numIterations = 4096000;
        var instance = new Derived2();

        I = 0;
        for (var i = 0; i < numIterations; i++)
            instance.Method();

        return I;
    }

    public static int Time (Func<int> func) {
        var started = Environment.TickCount;

        int result = func();

        Console.WriteLine(result);

        var ended = Environment.TickCount;
        return ended - started;
    }
}

public class Base {
    public void Method () {
        Program.I += 1;
    }
}

public class Derived : Base {
    new public void Method () {
        base.Method();
        Program.I += 2;
    }
}

public class Derived2 : Derived {
    new public void Method () {
        base.Method();
        Program.I += 4;
    }
}