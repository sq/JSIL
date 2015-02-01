using System;
using System.Text;
using JSIL.Runtime;

using System.Runtime.InteropServices;

public static unsafe class Program {
    [StructLayout(LayoutKind.Sequential)]
    public struct TestStruct {
        public int I;
        public float F;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate TestStruct TReturnStructArgument (TestStruct arg);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int TBinaryOperator (int a, int b);

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int CallBinaryOperator (IntPtr fp, int a, int b);

    [DllImport("common.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern TestStruct CallReturnStructArgument (IntPtr fp, TestStruct s);

    public static void Main () {
        {
            var d = new TBinaryOperator(Multiply);
            var fp = Marshal.GetFunctionPointerForDelegate(d);

            Console.WriteLine(CallBinaryOperator(fp, 5, 3));
        }

        {
            var d = new TReturnStructArgument(IncrementFields);
            var fp = Marshal.GetFunctionPointerForDelegate(d);

            var a = new TestStruct { I = 1, F = 2.5f };
            var b = CallReturnStructArgument(fp, a);

            Console.WriteLine("i={0} f={1:F4}", b.I, b.F);
        }
    }

    public static TestStruct IncrementFields (TestStruct s) {
        s.I += 1;
        s.F += 1.3f;
        return s;
    }

    public static int Multiply (int a, int b) {
        return a * b;
    }
}