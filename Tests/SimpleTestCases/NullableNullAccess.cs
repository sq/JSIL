using System;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            int? empty = null;
            Console.WriteLine(empty.Value);
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Expected InvalidOperationException");
        }

        try
        {
            StructWithMethod? empty2 = null;
            empty2.Value.Method();
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Expected InvalidOperationException");
        }
    }

    public struct StructWithMethod
    {
        public void Method()
        {
            Console.WriteLine("StructWithMethod.Method");
        }
    }

}
