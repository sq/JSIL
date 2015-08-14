using System;

public interface IInterface {
    void InterfaceMethod (int x);
}

public class CustomType1 : IInterface {
    public void InterfaceMethod (int x) {
        Console.WriteLine("CustomType1.InterfaceMethod({0})", x);
    }
}

public class CustomType2 : IInterface {
    void IInterface.InterfaceMethod (int x) {
        Console.WriteLine("CustomType2.IInterface.InterfaceMethod({0})", x);
    }
}

public class CustomType3 : CustomType2 {
}

public static class Program {
    public static void Main (string[] args) {
        var instances = new object[] { 
            new CustomType1(), new CustomType2(), new CustomType3(), null
        };

        foreach (var instance in instances) {
            var ii = instance as IInterface;

            if (ii != null)
                ii.InterfaceMethod(1);
        }

        object n = "test";
        try {
            var ii = (IInterface)n;
            Console.WriteLine("Cast successful");
        } catch (Exception ex) {
            // We have to limit ourselves to the first line of the exception string,
            //  because JS exceptions don't have consistent support for tracebacks
            var text = ex.ToString().Split('\n')[0];
            Console.WriteLine("Cast failed: {0}", text);
        }

        Console.WriteLine("Is IInterface: {0}", instances[0] is IInterface);
    }
}