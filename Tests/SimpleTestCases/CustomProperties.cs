using System;

public static class Program {
    static int _A;
    public static int A {
        get {
            return _A / 2;
        }
        set {
            _A = value * 2;
        }
    }
    
    static Program () {
        _A = 2;
    }
  
    public static void Main (string[] args) {
        Console.WriteLine("A = {0}, _A = {1}", A, _A);
        A = 4;
        Console.WriteLine("A = {0}, _A = {1}", A, _A);
      
        var instance = new CustomType();
        Console.WriteLine(instance);
        instance.A = 4;
        Console.WriteLine(instance);      
    }
}

public class CustomType {
    int _A;
    public int A {
        get {
            return _A / 2;
        }
        set {
            _A = value * 2;
        }
    }
    
    public CustomType () {
        A = 2;
    }
    
    public override string ToString () {
        return String.Format("A = {0}, _A = {1}", A, _A);
    }
}