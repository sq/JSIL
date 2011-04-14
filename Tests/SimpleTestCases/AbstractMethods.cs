using System;

public abstract class CustomTypeBase {
    public abstract void Method ();
}

public class CustomType : CustomTypeBase {
    override public void Method () {
        Console.WriteLine("Method");
    }
}

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        instance.Method();
    }
}