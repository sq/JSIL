using System;

public static class Program
{
    public static void Main(string[] args)
    {
        var br = new BaseResult();
        var dr = new DerivedResult();

        var bas = new Base();
        var derived = new Derived();

        bas.Method();
        bas.AnotherMethod();
        bas.MethodWithParameter1(br);
        bas.MethodWithParameter1(dr);
        bas.MethodWithParameter2(dr);

        bas = derived;
        bas.Method();
        bas.AnotherMethod();
        bas.MethodWithParameter1(br);
        bas.MethodWithParameter1(dr);
        bas.MethodWithParameter2(dr);

        derived.Method();
        derived.AnotherMethod();
        derived.MethodWithParameter1(br);
        derived.MethodWithParameter1(dr);
        derived.MethodWithParameter2(br);
        derived.MethodWithParameter2(dr);
    }
}

public class Base
{
    public BaseResult Method()
    {
        return null;
    }

    public void MethodWithParameter1(BaseResult r1)
    {
    }

    public void MethodWithParameter2(DerivedResult r1)
    {
    }

    public void AnotherMethod()
    {
    }
}

public class Derived : Base
{
    public DerivedResult Method()
    {
        return null;
    }

    public void MethodWithParameter1(DerivedResult r1)
    {
    }

    public void MethodWithParameter2(BaseResult r1)
    {
    }
}

public class BaseResult
{
}

public class DerivedResult : BaseResult
{
}
