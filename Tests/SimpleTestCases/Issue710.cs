using System;

public static class Program {
    public static void Main (string[] args) {
        double[] _proj = new double[6];

        const uint D = 3;
        Console.WriteLine(D);
        Console.WriteLine(_proj[3]);
        Console.WriteLine(_proj[D]);      // FAILS    
    }
}