using System;
using JSIL.Meta;
using Interfaces;

namespace Interfaces {
    public interface IInterface1 {
        void A ();
    }

    public interface IInterface2 {
        void A ();
    }

    public interface IInterface3 {
        void A ();
    }
}

public class CustomType : IInterface1, IInterface2, IInterface3 {
    public void A () {
        Console.WriteLine("A");
    }

    void IInterface1.A () {
        Console.WriteLine("IInterface1.A");
    }

    void Interfaces.IInterface2.A () {
        Console.WriteLine("IInterface2.A");
    }
}

public static class Program {
    public static void Main (string[] args) {
        var instance = new CustomType();

        instance.A();
        ((IInterface1)instance).A();
        ((IInterface2)instance).A();
        ((IInterface3)instance).A();
    }
}