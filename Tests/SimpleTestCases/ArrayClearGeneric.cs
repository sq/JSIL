using System;
using System.Threading;

public class GenericType<T>
{
    private InnerType[] innerArray = new InnerType[1] { new InnerType() };

    public void ClearInnerArray()
    {
        Array.Clear(innerArray, 0, 1);
    }

    private struct InnerType
    {
    }
}

public static class Program {
    public static void Main (string[] args) {
        new GenericType<object>().ClearInnerArray();
    }
}