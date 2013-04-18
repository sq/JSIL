using System;
using System.Reflection;

public interface A<T> {
    void MethodA ();
    void MethodA (T i);
    int MethodB ();
}

public interface B<T> : A<T> {
    T MethodC ();
}

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine("A<int>");
        Common.Util.ListMethods(
            typeof(A<int>),
            BindingFlags.Instance
        );

        Common.Util.ListMethods(
            typeof(A<int>),
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );

        Console.WriteLine("B<float>");
        Common.Util.ListMethods(
            typeof(B<float>),
            BindingFlags.Instance
        );

        Common.Util.ListMethods(
            typeof(B<float>),
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
        );
    }
}