using System;
using System.Text;

public static class Program {
    public static void Main (string[] args) {
        var retVal = new StringBuilder("Hello");

        retVal[0] = 'J';
        retVal[2] = 'H';
        retVal[4] = 'e';

        Console.WriteLine(retVal.ToString());
    }
}