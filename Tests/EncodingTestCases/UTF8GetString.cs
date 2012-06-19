using System;
using System.Text;

public static class Program {
    public static void Main (string[] args) {
        Common.PrintString(
            Encoding.UTF8.GetString(Common.ASCIIBytes)
        );

        Common.PrintString(
            Encoding.UTF8.GetString(Common.UTF8Bytes)
        );
    }
}