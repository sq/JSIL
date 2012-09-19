using System;

public static class Program { 
    public static void Main (string[] args) {
        var a = new CustomType(1);
        object b = a;
        object c = a;
        a.Value += 1;

        Console.WriteLine("{0} {1} {2}", a, b, c);

        var d = (CustomType)b;
        d.Value += 1;
        Console.WriteLine("{0} {1} {2} {3}", a, b, c, d);

        Console.WriteLine(
            "{0} {1} {2}",
            Object.ReferenceEquals(a, b) ? "true" : "false",
            Object.ReferenceEquals(b, c) ? "true" : "false",
            Object.ReferenceEquals(a, a) ? "true" : "false"
        );
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