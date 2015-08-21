//@useroslyn
//@compileroption /optimize
using System;

public static class Program {
    public static void Main (string[] args) {
        if (GetZerro() != 0)
        {
            Console.WriteLine("0l != 0");
        }
    }

    public static long GetZerro()
    {
        return 0l;
    }
}