using System;

public static class Program {
    public static void Main (string[] args) {
        TestSByteOverflow(100, 91);
        TestInt16Overflow(24051, 23192);
        TestInt32Overflow(2000543210, 1234567890);
    }

    public static void TestSByteOverflow (SByte a, SByte b) {
        try {
            checked {
                SByte sum = (SByte)(a + b);
                Console.WriteLine(sum);
            }
        } catch (OverflowException) {
            Console.WriteLine("Overflow detected");
        }
    }

    public static void TestInt16Overflow (short a, short b) {
        try {
            checked {
                short sum = (short)(a + b);
                Console.WriteLine(sum);
            }
        } catch (OverflowException) {
            Console.WriteLine("Overflow detected");
        }
    }

    public static void TestInt32Overflow (int a, int b) {
        try {
            checked {
                int sum = a + b;
                Console.WriteLine(sum);
            }
        } catch (OverflowException) {
            Console.WriteLine("Overflow detected");
        }
    }
}