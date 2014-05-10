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
        Console.WriteLine(
            ((IEquatable<ICustomType>)new CustomType()).Equals(new CustomType())
            ? 1 : 0
        );
    }
}