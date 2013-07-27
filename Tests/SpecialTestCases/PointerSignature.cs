//@compileroption /unsafe
using System;

public static class Program {
    public static void Main (string[] args) {        
    }

    public static void Method (int i) {
    }

    public static unsafe void Method (int * i) {
    }

    public static void Method (float f) {
    }

    public static unsafe void Method (float* f) {
    }
}
