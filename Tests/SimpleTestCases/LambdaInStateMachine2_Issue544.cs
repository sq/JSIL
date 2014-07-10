using System;
using System.Threading.Tasks;

public static class Program
{
    public static void Main(string[] args)
    {
        new MyClass().Main();
    }

    public static void MethodAcceptingDelegate(Action action)
    {
        action();
    }

    public class MyClass
    {
        private string _str = "str";
        private object _updateChartDataKey;

        public void Main()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.TrySetResult(0);
            MethodWithStateMachine(tcs.Task);
        }

        public async Task MethodWithStateMachine(Task t)
        {
            var updateChartDataKey = _updateChartDataKey = new { };
            await t;
            if (updateChartDataKey == _updateChartDataKey)
            {
                MethodAcceptingDelegate(() => Console.WriteLine(_str));
            }
        }
    }
}