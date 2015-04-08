using System;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Sample s = new Sample();
            s.Action();
            Console.WriteLine("ExtraAccessToAvoidException - Success (not expected)");
        }
        catch (Exception)
        {
            Console.WriteLine("ExtraAccessToAvoidException - Failed (expected)");
        }
    }
}

public class Sample
{
    private readonly Sample2[] _points;

    public Sample()
    {
        _points = new Sample2[4];
        _points[0] = new Sample2(1.0);
        _points[1] = new Sample2(4.0);
        _points[2] = new Sample2(4.0);
        _points[3] = new Sample2(1.0);
    }

    public void GetPoint(int index, out Sample2 dest)
    {
        dest = _points[index];
    }


    public void Action()
    {
        Sample2 p0;                         // Must be declared outside of for loop.  (Inside works)
        Sample2 p1 = _points[0];

        int count = 0;

        for (int i = 1; i < _points.Length; i++)
        {
            p0 = p1;
            GetPoint(i, out p1);            // Must be called this way (return value version works fine).

            // Sample2.IsNull(p0);           // Uncomment this line, and the script error goes away.

            if (p0.IsEqual(p1))
            {
                count++;
            }
        }
    }
}

public class Sample2
{
    public readonly double X;

    public Sample2(double x)
    {
        X = x;
    }

    public bool IsEqual(Sample2 pt)
    {
        return (Math.Abs(pt.X - X) < .00000001);
    }

    static public bool IsNull(Sample2 point)
    {
        return (null == point);
    }
}