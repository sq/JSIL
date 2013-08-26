using System;

public class Program
{
    public static void Main()
    {
        // Issue 162
        unchecked
        {
            var p1 = (long)0xffff030405060708;
            var p2 = 8;
            var s = (long)(p1 >> p2);
            Console.WriteLine(Format(s));

            var up1 = 0xffff030405060708;
            var up2 = 8;
            var us = (up1 >> up2);
            Console.WriteLine(Format(us));
        }
    }

    private static string Format<T>(T t)
    {
        return string.Format("{0} {1}", t, t.GetType());
    }
}

