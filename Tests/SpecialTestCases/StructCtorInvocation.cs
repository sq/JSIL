using System;

public static class Program {
    public static void Main (string[] args) {
        var ct = new CustomType(1);
        var mc = new MyClass();
        mc.UpdateWithNewState(2, ct);
        ct.Value = 3;
        Console.WriteLine("ct={0}, mc={1}", ct, mc);
    }
}

public struct CustomType {
    public int Value;

    public CustomType (int value) {
        Value = value;
    }

    public override string ToString () {
        return String.Format("{0}", Value);
    }
}

public class MyClass {
    public int A;
    public CustomType B;

    public void UpdateWithNewState (int a, CustomType b) {
        A = a;
        B = b;
    }

    public override string ToString () {
        return String.Format("(a={0} b={1})", A, B);
    }
}