using System;

public class CustomTypeBase {
    public int _Base_A = 4;

    public virtual int A {
        get {
            return _Base_A;
        }
        set {
            _Base_A = value;
        }
    }
}

public class CustomType : CustomTypeBase {
    public int _A = 2;

    override public int A {
        get {
            return _A;
        }
        set {
            _A = value;
        }
    }

    public int BaseA {
        get {
            return base.A;
        }
        set {
            base.A = value;
        }
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