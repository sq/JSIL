using System;

public static class Program {
    public static double ManyBits = 10101010101.0101010101010101010;

    public static int I;
    public static float F;
    public static double D;
    public static double LongD;

    public static int f_as_i, d_as_i, ld_as_i;
    public static float i_as_f, d_as_f, ld_as_f;
    public static double i_as_d, f_as_d;

    public static void Main (string[] args) {
        Init();

        i_as_f = (float)I;
        i_as_d = (double)I;

        d_as_f = (float)D;
        d_as_i = (int)D;

        f_as_d = (double)F;
        f_as_i = (int)F;

        Console.WriteLine("{0:0.0} {1:0.0}", i_as_f, i_as_d);
        Console.WriteLine("{0:0} {1:0.0}", d_as_i, d_as_f);
        Console.WriteLine("{0:0} {1:0.0}", f_as_i, f_as_d);

        unchecked {
            ld_as_f = (float)LongD;
            // FIXME: This appears to be impossible to reproduce in javascript???
            // ld_as_i = (int)LongD;
        }

        // FIXME: Double->float truncation in js rounds differently than the MS CLR.
        // As a result we can't print the truncated value, merely assert that it is different
        //  from the double value. Bleh.
        var ld_as_s = String.Format("{0:0.00000}", LongD);
        var ld_as_f_as_s = String.Format("{0:0.00000}", ld_as_f);

        Console.WriteLine(ld_as_s);
        Console.WriteLine(ld_as_s != ld_as_f_as_s ? "truncated" : "not truncated");
    }

    public static void Init () {
        I = 1;
        F = 1.5f;
        D = 2.5f;
        LongD = ManyBits;
    }
}