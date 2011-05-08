using System;

public class CustomType {
}

public class CustomType2 {
}

public static class Program {
    public static void Main (string[] args) {
        var instanceA = new CustomType();
        var instanceB = new CustomType2();

        Console.WriteLine(
            "{0} {1} {2} {3}", instanceA, instanceA.GetType(), 
            instanceB, instanceB.GetType()
        );
    }
}