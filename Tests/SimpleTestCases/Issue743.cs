using System;

public static class Program {
    public static void Main (string[] args) {
        var tt = new TestType();
        tt[1, 2] = 3.5;

        Console.WriteLine(tt._00);
    }
}

public class TestType {
    public double _00;

    public double this[int row, int col]
    {
        set
        {
            _00 = value;
        }
    }
}