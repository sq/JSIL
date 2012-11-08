using System;

public class Program
{
    public static void Main()
    {

        int days = 100, hours = 20, minutes = 1, seconds = 1, milliseconds = 20;
        int hrssec = (hours * 3600); // break point at (Int32.MaxValue - 596523)
        int minsec = (minutes * 60);
        long t = ((long)(hrssec + minsec + seconds) * 1000L + (long)milliseconds);
        t *= 10000;
        Console.WriteLine(t);

    }
}