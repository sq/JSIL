using System;
using System.Threading;
using System.Threading.Tasks;

public static class Program
{
    public static void Main(string[] args)
    {
        if (ShouldRun()) {
            var source1 = new TaskCompletionSource<object>();
            var source2 = new TaskCompletionSource<object>();

            Console.WriteLine("Main thread");
            AsyncMethod(source1.Task, source2.Task).ContinueWith((Task pre) => Console.WriteLine("Continuation:" + ((Task<string>)pre).Result));
            Console.WriteLine("Main: Delay 1 complete");
            source1.TrySetResult(string.Empty);
            Console.WriteLine("Main: Delay 2 complete");
            source2.TrySetResult(string.Empty);

            if (!JSIL.Builtins.IsJavascript)
            {
                Thread.Sleep(100);
            }
        }
    }

    // HACK: async/await support is not merged to trunk yet
    public static bool ShouldRun () {
        return false;
    }

    public static async Task<string> AsyncMethod(Task task1, Task task2)
    {
        await task1;
        Console.WriteLine("After delay 1");
        await task2;
        Console.WriteLine("After delay 2");
        return "AsyncMethod result";
    }
}