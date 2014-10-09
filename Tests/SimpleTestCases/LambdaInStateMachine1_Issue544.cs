using System;
using System.Collections;
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
            foreach (var q in MethodWithStateMachine())
            {
                Console.WriteLine("A");
            }
        }

        public IEnumerable MethodWithStateMachine()
        {
            var updateChartDataKey = _updateChartDataKey = new { };
            yield return 0;
            Console.WriteLine("a");
            if (updateChartDataKey == _updateChartDataKey)
            {
                MethodAcceptingDelegate(() => Console.WriteLine(_str));
                yield return 1;
            }
        }
    }
}