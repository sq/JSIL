using System;

public static class Program
{
    public static void Main(string[] args)
    {
        new OuterType.InnerType().Run();
    }
}

public class OuterType
{
    public class InnerType
    {
        public void Run()
        {
            Console.WriteLine("Run");
        }
    }
}

public class StrippedType
{
}