using System;

public static class Program {
    public static bool HasTCctorRun = false;

    public static void Main (string[] args) {
        Console.WriteLine("StaticField = {0}", T.StaticField);
        Console.WriteLine("HasTCctorRun = {0}", HasTCctorRun);
    }
}

public static class T {
    public static int StaticField = 1;

    static T () {
        StaticField = 2;
        Program.HasTCctorRun = true;
    }
}