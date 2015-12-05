using System;

public static class Program
{
    public static void Main(string[] args)
    {
        ProcessIUsedInterface(new UsedType());
    }

    public static void ProcessIUsedInterface(IUsedInterface item)
    {
        item.Run();
    }
}

public interface IUnUsedInterface
{ }

public interface IUsedInterface : IUnUsedInterface
{
    void Run();
}

public class UsedType : IUsedInterface
{
    public void Run()
    {
        Console.WriteLine("Run");
    }
}