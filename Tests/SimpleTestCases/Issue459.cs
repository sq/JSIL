using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var listT = new List<string>() { "hello", "world" };
        var list = (IList)listT;

        for (var index = 0; index < list.Count; index++) {
            Console.WriteLine(list[index]);
        }
    }
}