using JSIL.Meta;

public static class Program
{
    public static void Main()
    {
    }
}

[JSStubOnly]
public class ClassThatShouldBeStubbed
{
    public static string MethodInStubbedClass()
    {
        return null;
    }
}

[JSExternal]
public class ClassThatShouldBeExternal
{
    public static string MethodInExternalClass()
    {
        return null;
    }
}

[JSIgnore]
public class ClassThatShouldBeIgnored
{
    public static string MethodInIgnoredClass()
    {
        return null;
    }
}