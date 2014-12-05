using System;
using System.Collections.Generic;
using JSIL.Meta;
using System.Linq;

public static class Program {
    public static void Main (string[] args)
    {
        var type = typeof (List<ClassA>);
        var method = type.GetMethods().Where(it => it.Name == "GetEnumerator").First();
        Console.WriteLine(method.ReturnType.FullName);
    }


    public class ClassA
    {
    }
}