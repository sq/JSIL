using System;
using System.Text;

public static class Program {
    public static void Main (string[] args) {
        Common.PrintByteArray(
            Encoding.UTF8.GetBytes(Common.ASCIIString)
        );

        Common.PrintByteArray(
            Encoding.UTF8.GetBytes(Common.UTF8String)
        );
    }
}