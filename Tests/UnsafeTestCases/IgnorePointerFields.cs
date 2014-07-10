public static class Program
{
    public static void Main(string[] args)
    {
        new MyClass();
    }

    public class MyClass
    {
        internal unsafe byte* charProperties;
    }
}