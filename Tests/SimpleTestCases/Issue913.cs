using System;

public static class Program
{
    public static class TmpClss<T>
    {
        public static T CallSite;
    };

    public static void Main(string[] args)
    {
        if (TmpClss<Action>.CallSite == null)
        {
            TmpClss<Action>.CallSite = () => Console.WriteLine("!");
        }
        var action = TmpClss<Action>.CallSite;
        RunMe();

        action();
    }

    public static void RunMe()
    {
        TmpClss<Action>.CallSite = () => Console.WriteLine("!!");
    }
}