using System;
using System.Collections.ObjectModel;

public static class Program {
    public static void Main (string[] args) {


        var list = new ObservableCollection<string> { "zero", "one", "two", "three" };

      Console.WriteLine(list.IndexOf("two"));
      Console.WriteLine(list.IndexOf("zero"));
      Console.WriteLine(list.IndexOf("three"));

      list.Insert(0, "newOne");
      Console.WriteLine(list.IndexOf("three"));
      Console.WriteLine(list.IndexOf("newOne"));

      list.Insert(3, "anotherNewOne");
      Console.WriteLine(list.IndexOf("three"));
      Console.WriteLine(list.IndexOf("anotherNewOne"));      
      Console.WriteLine(list.IndexOf("one"));


    }

}