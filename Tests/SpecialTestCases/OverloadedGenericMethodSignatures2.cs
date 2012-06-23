using System;

public class ItemA { }
public class ItemB : ItemA { }

public interface Interface { void Test2 (ref ItemA obj); }
public interface Interface<T> : Interface { void Test (ref T obj); }

public class A<T> : Interface<T> where T : ItemA {
    public virtual void Test (ref T obj) {
        Console.WriteLine("A");
    }

    public void Test2 (ref ItemA obj) {
        Console.WriteLine("A2");
        var objT = default(T);
        Test(ref objT);
    }
}

public class B<T> : A<T> where T : ItemA, new() {
    public override void Test (ref T obj) {
        Console.WriteLine("B");
    }
}

public static class Program {
    public static void Main (string[] args) {
        var test = (Interface)new B<ItemB>();
        ItemA test2 = new ItemB();
        test.Test2(ref test2);
    }
}