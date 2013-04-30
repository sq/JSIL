using System;
using JSIL.Meta;

public interface A<T> {
    T GetValue ();
}

public interface B<in U> {
    void SetValue (U value);
}

public interface C<out V> {
    V GetValue ();
}

public static class Program {
    public static void Main (string[] args) {
    }
}