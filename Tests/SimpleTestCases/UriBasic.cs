using System;

public static class Program {
    public static void Main (string[] args) {
        var src = new Uri("file://dir");
        var dst = new Uri(src, "file");
        Console.WriteLine(src.LocalPath);
        Console.WriteLine(dst.LocalPath);
    }
}