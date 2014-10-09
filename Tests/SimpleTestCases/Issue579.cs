using System;
using System.Threading;
using System.Threading.Tasks;

public static class Program
{
    public static void Main(string[] args)
    {
        var signal1 = new ManualResetEventSlim(false);
        var signal2 = new ManualResetEventSlim(false);

        var t1 = new TaskCompletionSource<object>();
        var t2 = new TaskCompletionSource<object>();

        DoWork(t1.Task, 1).ContinueWith(
            pre =>
            {
                if (!JSIL.Builtins.IsJavascript)
                    Console.Out.Flush();

                signal1.Set();
            });

        DoWork(t2.Task, 2).ContinueWith(
            pre =>
            {
                if (!JSIL.Builtins.IsJavascript)
                    Console.Out.Flush();

                signal2.Set();
            });

        t1.TrySetResult(new object());
        t2.TrySetResult(new object());

        signal1.Wait();
        signal2.Wait();
    }

    public static async Task DoWork(Task waitFor, int index)
    {
        Console.WriteLine("Started: " + index);
        await Program.RunTaskWithTwoInterceptors(
            "input",
            async () =>
                {
                    Console.WriteLine("Inner begin");
                    await waitFor;
                });

        Console.WriteLine("Finished: " + index);
    }

    public static async Task TaskInterceptor(object target, Func<Task> func)
    {
        Console.WriteLine(target);
        await func();
    }

    public static Task RunTaskWithTwoInterceptors<TController>(
        TController target,
        Func<Task> func)
    {
        return TaskInterceptor(
            target,
            async () => await TaskInterceptor(target, func));
    }
}
