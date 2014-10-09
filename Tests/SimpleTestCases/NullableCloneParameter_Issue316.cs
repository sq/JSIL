using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var a = new TestClass();
        a.Prop = 10;
        Console.WriteLine(a.Prop);

        a.SetMutableAndUpdate();

        Console.WriteLine(a.Mutable.Value.Value);
    }
}

public class TestClass
{
    private int? _field;
    private MutableStruct? _mutable;

    public int? Prop
    {
        get { return _field; }
        set { ProcessUpdate(ref _field, value); }
    }

    public MutableStruct? Mutable
    {
        get { return _mutable; }
        set { ProcessUpdate(ref _mutable, value); }
    }

    public void ProcessUpdate<T>(ref T target, ref T input)
    {
        target = input;
    }

    public void ProcessUpdate<T>(ref T target, T input)
    {
        target = input;
    }

    public void SetMutableAndUpdate()
    {
        // Here we test of JSIL.CloneParameter for nullable struct with null and not null value.
        MutableStruct? mutable = new MutableStruct?();
        ProcessUpdate(ref _mutable, ref mutable);
        mutable = new MutableStruct();
        ProcessUpdate(ref _mutable, ref mutable);
        mutable.Value.Increment();
    }
}

public struct MutableStruct
{
    public int Value;

    public void Increment()
    {
        Value++;
    }
}