using System;

public struct MyType {
}

public static class Program {
    public interface IMyInterface {
        MyType GetValue ();
    }

    public static void Main () {
        var interfaceType = typeof(IMyInterface);

        foreach (var methodInfo in interfaceType.GetMethods()) {
            Console.WriteLine(methodInfo.ToString());
        }
    }
}
