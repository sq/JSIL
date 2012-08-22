using System;
using System.Text;

public static class Program {
    private static void Test (string text) {
        var bytes = Encoding.ASCII.GetBytes(text);
        var base64 = System.Convert.ToBase64String(bytes);
        Common.PrintString(base64);
    }

    public static void Main (string[] args) {
        Test("any carnal pleasure.");
        Test("any carnal pleasure");
        Test("any carnal pleasur");
        Test("any carnal pleasu");

        Test("any");
        Test("an");
        Test("a");
        Test("");
    }
}