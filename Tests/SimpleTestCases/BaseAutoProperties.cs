using System;

public class CustomTypeBase {
    public int A {
        get;
        set;
    }

    public virtual int B {
        get;
        set;
    }

    public CustomTypeBase () {
        A = 4;
        B = 8;
    }
}

public class CustomType : CustomTypeBase {
    new public int A {
        get;
        set;
    }

    public override int B {
        get {
            return base.B * 2;
        }
        set {
            base.B = value / 2;
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

    public int BaseB {
        get {
            return base.B;
        }
        set {
            base.B = value;
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
        Console.WriteLine(
            "instance.B = {0}, instance.BaseB = {1}",
            instance.B, instance.BaseB
        );
    }
}