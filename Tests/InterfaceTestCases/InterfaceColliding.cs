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
        public interface I1 {
            int Get(); }
        
        public class Implementer : TestNamespace.I1, TestNamespace.TestClass.I1, TestNamespace.Inner.I1
        {
            int TestNamespace.I1.Get() {
                return 0;
            }
            int TestNamespace.TestClass.I1.Get() {
                return 1;
            }
            int TestNamespace.Inner.I1.Get() {
                return 2;
            }
        }
    }
}

public static class Program {
    public static void Main (string[] args) {
        var implementer = new TestNamespace.TestClass.Implementer();
        
        Console.WriteLine(((TestNamespace.I1)implementer).Get());
        Console.WriteLine(((TestNamespace.TestClass.I1)implementer).Get());
        Console.WriteLine(((TestNamespace.Inner.I1)implementer).Get());
    }
}