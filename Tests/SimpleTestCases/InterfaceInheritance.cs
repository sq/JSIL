using System;

public interface IInterface1 {
    void Interface1Method (int x);
}

public interface IInterface2 : IInterface1 {
    void Interface2Method (int x);
}

public class CustomType : IInterface2 {
    public void Interface1Method (int x) {
        Console.WriteLine("CustomType.Interface1Method({0})", x);
    }

    public void Interface2Method (int x) {
        Console.WriteLine("CustomType.Interface1Method({0})", x);
    }
}

public static class Program {
    public static void Main (string[] args) {
        object instance = new CustomType();

        var ii1 = instance as IInterface1;
        var ii2 = instance as IInterface2;

        ii1.Interface1Method(1);
        ii2.Interface1Method(2);
        ii2.Interface2Method(3);
    }
}