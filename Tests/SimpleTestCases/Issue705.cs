using System;

public static class Program {
    public static void Main(string[] args) {
        Resolver.GetResult().RunMe();
    }
}

public static class Resolver
{
    private static object _syncRoot = new object();

    internal static ResultClass GetResult()
    {
        lock (_syncRoot)
        {
            ResultClass factory;
            if (!TryGet(out factory))
            {
                factory = Create();
            }

            return factory;
        }
    }

    private static ResultClass Create()
    {
        return new ResultClass();
    }

    private static bool TryGet(out ResultClass result)
    {
        result = null;
        return false;
    }
}

public class ResultClass
{
    public void RunMe()
    {
        Console.WriteLine("Hura!");
    }
}

