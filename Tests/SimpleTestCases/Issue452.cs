using System;
using System.Collections.Generic;

public interface IInterface {
}

public class CustomType : IInterface {
}

public static class Program {
    public static void Main (string[] args) {
        var list = new CustomType[] { new CustomType() };
        var collection = (ICollection<CustomType>)list;
        foreach (var value in (ICollection<IInterface>)collection) {
            Console.WriteLine(value);
        }
    }
}