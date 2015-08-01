using JSIL.Meta;

public static class Program
{
    [JSDeadCodeEleminationEntryPoint]
    public static int UnusedFieldWithAttribute;

    public static void Main()
    {
    }

    [JSDeadCodeEleminationEntryPoint]
    public static void UnusedMethodWithAttribute()
    {
    }
}

[JSDeadCodeEleminationEntryPoint]
public class UnusedClassWithAttribute
{
    public void MethodInUnusedClassWithAttribute()
    {
    }
}

[JSDeadCodeEleminationClassEntryPoint]
public class UnusedClassWithClassEntryPointAttribute
{
    public void MethodInUnusedClassWithClassEntryPointAttribute()
    {
    }
}

[JSDeadCodeEleminationHierarchyEntryPoint]
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