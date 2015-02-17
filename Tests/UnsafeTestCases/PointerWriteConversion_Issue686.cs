using System;
using JSIL.Meta;

public static class Program
{
    [JSPackedArray]
    public static TestStuct[] PackedArray = new TestStuct[] { 5, 4, 3 };

    public static unsafe void Main(string[] args)
    {
        fixed (TestStuct* p0 = PackedArray)
        {
            Transform(p0);
        }

        Console.WriteLine(PackedArray[0]);
        Console.WriteLine(PackedArray[1]);
        Console.WriteLine(PackedArray[2]);
    }

    private static unsafe void Transform(TestStuct* state)
    {
        var b = state[1];
        state[1] += b;
    }
}

public struct TestStuct
{
    public int Field;

    public TestStuct(int field)
    {
        Field = field;
    }

    public static TestStuct operator +(TestStuct a, TestStuct b)
    {
        return new TestStuct();
    }

    public static implicit operator TestStuct(int input)
    {
        return new TestStuct(input);
    }

    public override string ToString()
    {
        return Field.ToString();
    }
}