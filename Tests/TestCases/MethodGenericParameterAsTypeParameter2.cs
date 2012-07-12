using System;

public class MyList<T> {
    public static void MyMethod1 (MyList<T> param) {
    }

    public static void MyMethod1 (int param)  // another overload to force JSIL to use  runtime dispatch
    {
    }
}

public static class Program {

    public static void MyToList10<TSource> (MyList<TSource> source) // a generic method
    {
        MyList<TSource>.MyMethod1(source); //call void class MyList`1<!!TSource>::MyMethod1(class MyList`1<!0>)
    }

    public static void Main (string[] args) {
        var s = new MyList<string>();
        MyToList10(s);  // fails
        Console.WriteLine("Finished");
    }
}