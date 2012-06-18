using System;
using System.Text;

public static class Program {
    public static void Main (string[] args) {
        Common.PrintByteArray(
            Encoding.ASCII.GetBytes(Common.ASCIIString)
        );

        Common.PrintByteArray(
            Encoding.ASCII.GetBytes(Common.UTF8String)
        );
    }
}