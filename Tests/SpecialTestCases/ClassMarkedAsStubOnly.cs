using JSIL.Meta;

class Program {
    public static void Main ()
    {
        System.Console.WriteLine(ClassThatShouldBeStubbed.StubMethod());
    }

    [JSStubOnly]
    public class ClassThatShouldBeStubbed
    {
        public static string StubMethod()
        {
            return "undefined";
        }
    }
}