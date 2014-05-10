using System;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(Get<TestStruct>());
    }

    public static T Get<T> () {
        object result = GetFlag() ? (GetValue() ?? default(T)) : default(T);

        return (T)result;
    }

    public static object GetValue () {
        return null;
    }

    public static bool GetFlag () {
        return true;
    }

    public struct TestStruct {
    }
}