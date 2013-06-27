// issue #203

interface ITypePrinter {
    void Print<T> ();
}

class TypePrinter : ITypePrinter {
    public void Print<T> () {
        System.Console.WriteLine(typeof(T));
    }
}

class Program {
    public static void Main () {
        TypePrinter printer = new TypePrinter();
        printer.Print<object>();

        ((ITypePrinter)printer).Print<object>();
    }
}