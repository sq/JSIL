using System;
using System.Collections.ObjectModel;

public static class Program {
    public static void Main (string[] args) {

        var list = new ObservableCollection<string> { "zero", "one", "two", "three" };

      Console.WriteLine(list.IndexOf("two"));
      Console.WriteLine(list.IndexOf("zero"));
      Console.WriteLine(list.IndexOf("two-shouldNotExits"));
      Console.WriteLine(list.IndexOf("three"));

      // Other overloads not implemented yet
    }

}