using System;
using System.Text;

public static class Program {
    public static void Main (string[] args) {
        Common.PrintString(
            Encoding.ASCII.GetString(Common.ASCIIBytes)
        );

        Common.PrintString(
            Encoding.ASCII.GetString(Common.UTF8Bytes)
        );
    }
}