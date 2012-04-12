using System;

public class Program
{
    public static void Main(string[] args)
    {
        var d = new D();
        d.slots = "Before"; 
        string t;
        d.Do_Transform(out t, s => "After");
        Console.WriteLine(t);
    }
}

public class D
{
    public string slots;

    public delegate TRet Transform<TRet>(string key);

    public void Do_Transform<TRet, TElem>(out TElem result, Transform<TRet> transform)
        where TRet : TElem
    {
        result = transform(slots);
    }
}
