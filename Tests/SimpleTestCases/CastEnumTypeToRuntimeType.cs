using System;

public static class Program {
    public static void Main (string[] args) {
        var tA = typeof(Enm);
        var tB = typeof(Cls);
        var tC = typeof(Iface);
        var tD = typeof(object);

        var types = new object[] { tA, tB, tC, tD };

        foreach (var type in types) {
            Console.WriteLine("{0}", type is Type ? 1 : 0);
            Console.WriteLine("{0}", (Type)type);
        }
    }
}

public enum Enm {
}

public class Cls {
}

public interface Iface {
}