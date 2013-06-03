using System;

public static class Program { 
    public static void Main () {
        var tOpenDerived = typeof(GD<>);
        var tClosedDerived = typeof(GD<int>);

        Console.WriteLine("{0} {1}", tOpenDerived, tClosedDerived);
        Console.WriteLine("{0} {1}", tOpenDerived.BaseType, tClosedDerived.BaseType);
    }
}

public class GB<T> {
}

public class GD<U> : GB<U> {
}