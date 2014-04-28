using System;

public interface ICustomType : IEquatable<ICustomType> {
}

public class CustomType : ICustomType {
    public bool Equals (ICustomType other) {
        return false;
    }
}

public static class Program {
    public static void Main (string[] args) {
        var firstType = (ICustomType)new CustomType();
        var secondType = new CustomType();
        Console.WriteLine(
            ((IEquatable<ICustomType>)firstType).Equals(secondType)
            ? 1 : 0
        );
    }
}