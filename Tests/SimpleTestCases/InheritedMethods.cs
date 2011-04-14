using System;

public class CustomTypeBase {
    public void InheritedMethod () {
        Console.WriteLine("InheritedMethod");
    }
}

public class CustomType : CustomTypeBase {
    public void Method () {
        Console.WriteLine("Method");
    }
}

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        instance.Method();
        instance.InheritedMethod();
    }
}