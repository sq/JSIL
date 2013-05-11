using System;


namespace TestNamespace
{
    public interface I1 {
        int Get(); }

    namespace Inner
    {
        public interface I1 {
            int Get(); }
    }

    public static class TestClass {

        public class Implementer : TestNamespace.I1, TestNamespace.Inner.I1
        {
            public int Get() {
                return 0;
            }
            int TestNamespace.Inner.I1.Get() {
                return 1;
            }
        }
    }
}

public static class Program {
    public static void Main (string[] args) {
        var implementer = new TestNamespace.TestClass.Implementer();
        
        Console.WriteLine(((TestNamespace.I1)implementer).Get());
        Console.WriteLine(implementer.Get());
        Console.WriteLine(((TestNamespace.Inner.I1)implementer).Get());
    }
}