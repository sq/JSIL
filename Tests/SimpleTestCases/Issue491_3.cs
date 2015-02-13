using System.Collections.Generic;

public static class Program {
    public static Dictionary<string, Generic<int>> _runCache = new Dictionary<string,Generic<int>>();

    public static void Main () {
        Run();
    }

    public static Generic<int> Run () {
        return _runCache[string.Empty] = new Generic<int>();
    }
}

public class Generic<T> {
}