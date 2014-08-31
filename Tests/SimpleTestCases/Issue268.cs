using System;

class Program {
    public static void Main () {
        object o1 = new object();
        object o2 = new object();

        var result = DoStuff(ref o1, o2);

        Console.WriteLine(object.ReferenceEquals(o1, o2) ? "true" : "false");
        Console.WriteLine(object.ReferenceEquals(result, o2) ? "true" : "false");
    }

    private static T DoStuff<T> (ref T t1, T t2) {
        T ret = t2;
        ret.ToString(); // prevent variable elimination

        T result = t1 = ret;
        result.ToString(); // prevent variable elimination

        return result;
    }
}