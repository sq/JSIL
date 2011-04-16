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
    override public int A {
        get {
            return _Base_A * 2;
        }
        set {
            _Base_A = value * 2;
        }
    }

    public int BaseA {
        get {
            return _Base_A;
        }
        set {
            _Base_A = value;
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