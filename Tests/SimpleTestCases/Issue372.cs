using System;
using System.Collections.Generic;

public static class Program { 
    public static void Main (string[] args) {
        IntType type = new IntType();
        type.Value = 10;
        int result = 5;

        type.Value += result;

        Console.WriteLine(type.Value);
    }
}

public abstract class GenericType<T> where T : struct
{
    protected T value;

    public virtual T Value
    {
        get { return value; }
        set
        {
            if (!Object.Equals(value, this.value))
            {
                this.value = value;
            }
        }
    }
}

class IntType : GenericType<int>
{
    public override int Value
    {
        get { return base.Value; }
        set
        {
            base.Value = value;
        }
    }
}
