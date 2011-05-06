using System;
using JSIL;

public static class Program {  
    public static void Main (string[] args) {
        var objs = new object[] { 
            "a", null, "b", null, "c"
        };

        foreach (var obj in objs) {
            var s = obj as string;

            if (s != null)
                Console.WriteLine(s);
        }
    }
}