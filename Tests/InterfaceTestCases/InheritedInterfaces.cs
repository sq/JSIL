using System;

public interface IInterface1 {
    void Interface1Method (int x);
}

public interface IInterface2 {
    void Interface2Method (int x);
}

public class CustomType1 : IInterface1 {
    public void Interface1Method (int x) {
        Console.WriteLine("CustomType1.Interface1Method({0})", x);
    }
}

public class CustomType2 : CustomType1, IInterface2 {
    public void Interface2Method (int x) {
        Console.WriteLine("CustomType2.Interface1Method({0})", x);
    }
}

public static class Program {
    public static void Main (string[] args) {
        object[] instances = new[] { new CustomType1(), new CustomType2() };

        Console.WriteLine("{0} {1}", instances[0] is IInterface1, instances[0] is IInterface2);
        Console.WriteLine("{0} {1}", instances[1] is IInterface1, instances[1] is IInterface2);
    }
}