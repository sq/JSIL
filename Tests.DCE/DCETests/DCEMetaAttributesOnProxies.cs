using JSIL.Meta;
using JSIL.Proxy;

public static class Program
{
    public static int UnusedFieldWithAttribute;

    public static void Main()
    {
    }

    public static void UnusedMethodWithAttribute()
    {
    }
}

public class UnusedClassWithAttribute
{
    public void MethodInUnusedClassWithAttribute()
    {
    }
}

public class UnusedClassWithClassEntryPointAttribute
{
    public void MethodInUnusedClassWithClassEntryPointAttribute()
    {
    }
}

public class UnusedClassWithHierarchyEntryPointAttribute
{
    public void MethodInUnusedClassWithHierarchyEntryPointAttribute()
    {
    }
}

public class DerivedUnusedClassWithAttribute : UnusedClassWithAttribute
{
    public void MethodInDerivedUnusedClassWithAttribute()
    {
    }
}

public class DerivedUnusedClassWithClassEntryPointAttribute : UnusedClassWithClassEntryPointAttribute
{
    public void MethodInDerivedUnusedClassWithClassEntryPointAttribute()
    {
    }
}

public class DerivedUnusedClassWithHierarchyEntryPointAttribute : UnusedClassWithHierarchyEntryPointAttribute
{
    public void MethodInDerivedUnusedClassWithHierarchyEntryPointAttribute()
    {
    }
}

[JSProxy(typeof(Program), JSProxyMemberPolicy.ReplaceNone, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceDeclared, false)]
public static class ProxyForProgram
{
    [JSDeadCodeEleminationEntryPoint]
    public static int UnusedFieldWithAttribute;


    [JSDeadCodeEleminationEntryPoint]
    public static void UnusedMethodWithAttribute()
    {
    }
}

[JSProxy(typeof(UnusedClassWithAttribute), JSProxyMemberPolicy.ReplaceDeclared, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceDeclared, false)]
[JSDeadCodeEleminationEntryPoint]
public class ProxyForUnusedClassWithAttribute
{
}

[JSProxy(typeof(UnusedClassWithClassEntryPointAttribute), JSProxyMemberPolicy.ReplaceDeclared, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceDeclared, false)]
[JSDeadCodeEleminationClassEntryPoint]
public class ProxyForUnusedClassWithClassEntryPointAttribute
{
}

[JSProxy(typeof(UnusedClassWithHierarchyEntryPointAttribute), JSProxyMemberPolicy.ReplaceDeclared, JSProxyAttributePolicy.ReplaceDeclared, JSProxyInterfacePolicy.ReplaceDeclared, false)]
[JSDeadCodeEleminationHierarchyEntryPoint]
public class ProxyForUnusedClassWithHierarchyEntryPointAttribute
{
}