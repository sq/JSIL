using System;
using System.Threading.Tasks;

public static class Program
{
    public static void Main(string[] args)
    {
        TaskCompletionSource<object> t1 = new TaskCompletionSource<object>();
        RunMe(t1.Task);
        t1.TrySetException(new Exception("e1"));

        TaskCompletionSource<int> t2 = new TaskCompletionSource<int>();
        RunMe(t2.Task);
        t2.TrySetException(new Exception("e2"));
    }

    public static async Task<int> RunMe(Task<int> waitFor)
    {
        try
        {
            var result = await waitFor;
            Console.WriteLine(result);
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine("Expected exception: " + e.Message);
        }

        return 0;
    }

    public static async Task RunMe(Task waitFor)
    {
        try
        {
            await waitFor;
        }
        catch (Exception e)
        {
            Console.WriteLine("Expected exception: " + e.Message);
        }
    }
}