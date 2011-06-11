using System;

public static class Program {
    public static CustomType Prop {
        get {
            return new CustomType(1);
        }
    }

    public static void Main (string[] args) {
        Console.WriteLine("{0}", Prop);
        Console.WriteLine("{0}", Prop.Value);
        Console.WriteLine("{0}", Prop.ToString());
        Console.WriteLine("{0}", Prop.Self.Value);
        Console.WriteLine("{0}", Prop.Self.ToString());
    }
}

public struct CustomType {
    public int Value;
  
    public CustomType (int value) {
        Value = value;
    }

    public CustomType Self {
        get {
            return this;
        }
    }
    
    public override string ToString () {
        return String.Format("{0}", Value);
    }
}