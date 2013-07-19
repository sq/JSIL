using System;

public static class Program {
    public static void Main (string[] args) {
        var t = new DerivedType();
        Console.WriteLine(t.Prop);
    }
}

public class BaseType {
    public virtual int Prop {
        get {
            return 1;
        }
    }
}

public class DerivedType : BaseType {
    public override int Prop {
        get {
            return 2 + base.Prop;
        }
    }
}