using System;

public static class Program {
    public static void SomeRefFunction2(ref int[] x) {
        if (x == null)
            Console.WriteLine("null");
        else
            Console.WriteLine(x[0]);
    }

    public static int[] MyTest(bool dummy) {
        int[] array;
        if (dummy) {
            array = new int[1] { 1 };
            SomeRefFunction2(ref array);
            return array;
        }
        array = new int[1] { 2 };
        SomeRefFunction2(ref array);
        return array;
    }

    public static void Main (string[] arguments) {
        MyTest(false);
        MyTest(true);
    }
}