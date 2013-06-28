// #issue #204

using System;

interface ITypePrinter {
    void Print (Type type);

    void Print<T> ();
}

class TypePrinter : ITypePrinter {
    public void Print (Type type) {
        Console.WriteLine(type);
    }

    public void Print<T> () {
        Print(typeof(T));
    }
}

class Program {
    public static void Main () {
        // This works
        TypePrinter printer = new TypePrinter();
        printer.Print<object>();

        // This doesn't work
        ((ITypePrinter)printer).Print<object>();
    }
}