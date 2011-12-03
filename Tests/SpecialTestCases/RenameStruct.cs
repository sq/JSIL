using System;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine((new MyStruct()).ToString());
    }
}

[JSChangeName("RenamedStruct")]
public struct MyStruct {
    public string ToString () {
        return String.Format("{0} {1}", "MyStruct", this.GetType());
    }
}