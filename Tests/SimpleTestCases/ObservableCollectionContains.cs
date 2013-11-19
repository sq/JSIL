using System;
using System.Collections.ObjectModel;

public static class Program {
    public static void Main (string[] args) {

        var list = new ObservableCollection<string> { "zero", "one", "two", "three" };

      PrintBool(list.Contains("two"));
      PrintBool(list.Contains("zero"));
      PrintBool(list.Contains("two-shouldNotExits"));
      PrintBool(list.Contains("three"));

    }

     static void PrintBool(bool b)
    {
      Console.WriteLine(b ? 1 : 0);
    }
}