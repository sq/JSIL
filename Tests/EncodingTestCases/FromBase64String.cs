using System;
using System.Text;

public static class Program {
    private static void Test (string base64) {
        try {
            var bytes = System.Convert.FromBase64String(base64);
            Common.PrintByteArray(bytes);
        } catch (Exception exc) {
            // We have to truncate the exception message because Microsoft randomly changed it between VS2010 and VS2012.
            Console.WriteLine("{0} {1}", exc.GetType().Name, exc.Message.Substring(0, 16));
        }
    }

    public static void Main (string[] args) {
        Test("YW55IGNhcm5hbCBwbGVhc3VyZS4=");
        Test("YW55IGNhcm5hbCBwbGVhc3VyZQ==");
        Test("YW55IGNhcm5hbCBwbGVhc3Vy");
        Test("YW55IGNhcm5hbCBwbGVhc3U=");

        Test("ZS4=");
        Test("ZQ==");
        Test("");

        Test("ZS4");
        Test("ZQ");
        Test("Z");

        Test("....");

        Test("ZS4=\r\n\t ");
        Test("\r\n\t ZQ==");
        Test("\r\n\t ");
    }
}