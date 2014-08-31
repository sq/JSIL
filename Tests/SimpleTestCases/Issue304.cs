using System;

class Program {
    public static void Main () {
        var b = new B();
        b.CallStuffIndirectly();
        b.CallStuffIndirectly2();
    }
}

class A {
    public void CallStuffIndirectly() {
        Action a = Stuff;
        a();
    }

    protected virtual void Stuff() {
       Console.WriteLine("A");
    }
}

class B : A {
    public void CallStuffIndirectly2 () {
        Action a = Stuff, b = base.Stuff;
        a();
        b();
    }

    protected override void Stuff () {
       Console.WriteLine("B");
    }
}