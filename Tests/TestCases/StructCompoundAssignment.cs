using System;

public static class Program {
    static CustomType[] cts = new CustomType[3];

    public static void Main (string[] args) {
        cts[0] = new CustomType(1);
        cts[1] = new CustomType(2.5f);
        cts[2] = cts[0];
        Console.WriteLine("1={0}, 2={1}, 3={2}", cts[0], cts[1], cts[2]);
        cts[0] *= 3;
        Console.WriteLine("1={0}, 2={1}, 3={2}", cts[0], cts[1], cts[2]);
        cts[2].Value = 16;
        Console.WriteLine("1={0}, 2={1}, 3={2}", cts[0], cts[1], cts[2]);
    }
}

public struct CustomType {
    public float Value;
  
    public CustomType (float value) {
        Value = value;
    }

    public static CustomType operator * (CustomType lhs, float rhs) {
        return new CustomType(lhs.Value * rhs);
    }
    
    public override string ToString () {
        return String.Format("{0}", Value);
    }
}