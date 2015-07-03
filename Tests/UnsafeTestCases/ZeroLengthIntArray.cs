using System;
using System.Text;

using System.Runtime.InteropServices;

public static class Program {
    public static unsafe void Main () {
        int[] arr = new int[0];
        fixed (int* pArr = arr) {
            Console.WriteLine("{0}", (int)pArr);
        }
    }
}