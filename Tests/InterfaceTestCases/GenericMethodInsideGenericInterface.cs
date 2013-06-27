// issue #203

interface ITypePrinter<U> {
    void Print<T> ();
}

class TypePrinter : ITypePrinter<string> {
    public void Print<T> () {
        System.Console.WriteLine(typeof(T));
    }
}

class Program {
    public static void Main () {
        TypePrinter printer = new TypePrinter();
        printer.Print<object>();

        ((ITypePrinter<string>)printer).Print<object>();
    }
}