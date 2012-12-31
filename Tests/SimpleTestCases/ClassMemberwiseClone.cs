using System;

public class CustomType {
    public int Value;

    public CustomType Clone () {
        CustomType c = (CustomType)MemberwiseClone();
        return c;
    }
}

public static class Program {
    public static void Main (string[] args) {
        var c = new CustomType { Value = 123 };
        Console.WriteLine(c.Clone().Value);
    }
}