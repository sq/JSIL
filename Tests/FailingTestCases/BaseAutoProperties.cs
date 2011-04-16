using System;

public class CustomTypeBase {
    public int A {
        get;
        set;
    }

    public CustomTypeBase () {
        A = 4;
    }
}

public class CustomType : CustomTypeBase {
    new public int A {
        get;
        set;
    }

    public int BaseA {
        get {
            return base.A;
        }
        set {
            base.A = value;
        }
    }

    public CustomType () {
        A = 8;
    }
}

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();
        Console.WriteLine(
            "instance.A = {0}, instance.BaseA = {1}", 
            instance.A, instance.BaseA
        );
    }
}