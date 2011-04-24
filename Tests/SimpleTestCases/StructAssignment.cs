using System;

public static class Program { 
    public static void Main (string[] args) {
        var a = new CustomType(1);
        CustomType bstruct = a;
        a.Value = 2;
        Console.WriteLine("a={0}, b={1}", a, bstruct);
        bstruct = a;
        Console.WriteLine("a={0}, b={1}", a, bstruct);
        a.Value = 3;
        Console.WriteLine("a={0}, b={1}", a, bstruct);
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