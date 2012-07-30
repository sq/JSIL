using System;
using System.Collections.Generic;

public struct myint {
    public myint (int xx) {
        x = xx;
    }

    public static myint operator+ (myint a, myint b) {
        return new myint(a.x + b.x);
    }

    public int x;

    public static void AddToList (myint i, List<myint> aList) {
        aList.Add(i);
    }
}

public static class Program {
    public static void Main () {
        List<myint> list = new List<myint>();
        for (int i = 0; i < 3; i++) {
            myint myI = new myint(0);
            myI += new myint(i);
            myint.AddToList(myI, list);
        }

        foreach (var mi in list)
            Console.WriteLine(mi.x);
    }
}