using System;

public class T
{
    public float[] values = new float[3];

    public float this[int x]
    {
        get
        {
            Console.WriteLine("get[" + x + "]" + this.values[x]);
            return this.values[x];
        }
        set
        {
            Console.WriteLine("set[" + x + "]" + value);
            this.values[x] = value;
        }
    }
}

public static class Program {
    public static void Main (string[] args) {
        int q = 0;
        float variable = 100;
        var store = new T();
        variable += store[0] = store[1] = 1.0f;

        Console.WriteLine(store[0]);
        Console.WriteLine(store[1]);
        Console.WriteLine(variable);
        Console.WriteLine(q);

        variable = store[0] += 50;
        Console.WriteLine(store[0]);
        Console.WriteLine(store[1]);
        Console.WriteLine(variable);
        Console.WriteLine(q);

        variable = store[q += 1] += 70;
        Console.WriteLine(store[0]);
        Console.WriteLine(store[1]);
        Console.WriteLine(variable);
        Console.WriteLine(q);
    }
}