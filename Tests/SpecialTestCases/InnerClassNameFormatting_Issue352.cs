using JSIL.Meta;

public static class Program
{
    public static void Main()
    {
    }

    [JSStubOnly]
    public class InnerGenericClass<T>
    {
        public T ReturnT()
        {
            return default(T);
        }
    }
}
