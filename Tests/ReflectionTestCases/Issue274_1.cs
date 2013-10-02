using System;

public static class Program {
    public interface IMyInterface {
        int GetValue ();
    }

    public static void Main () {
        var interfaceType = typeof(IMyInterface);

        foreach (var methodInfo in interfaceType.GetMethods()) {
            Console.WriteLine(methodInfo.ToString());
        }
    }
}
