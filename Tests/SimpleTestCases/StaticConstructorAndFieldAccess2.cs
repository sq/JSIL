using System;

public static class Program {
    public static void Main (string[] args) {
        T.StaticField = 3;
        Console.WriteLine("StaticField = {0}", T.StaticField);
    }
}

public static class T {
    public static int StaticField = 1;

    static T () {
        MungeStaticField();
    }

    public static void MungeStaticField () {
        StaticField = 2;
    }
}