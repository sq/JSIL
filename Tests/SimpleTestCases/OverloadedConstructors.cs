using System;

public static class Program { 
    public static void Main (string[] args) {
        Console.WriteLine(new CustomType());
        Console.WriteLine(new CustomType(1));
        Console.WriteLine(new CustomType("a"));
    }
}

public class CustomType {
    public string Text;

    public CustomType () {
        Text = "CustomType(<void>)";
    }
  
    public CustomType (int i) {
        Text = "CustomType(<int>)";
    }

    public CustomType (string s) {
        Text = "CustomType(<string>)";
    }
    
    public override string ToString () {
        return Text;
    }
}