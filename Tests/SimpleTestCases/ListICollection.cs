using System;
using System.Collections.Generic;

public static class Program
{


  public static void Main(string[] args)
  {


    var slist = new List<string> { "zero", "one", "two", "three" };


    for (int i = 0; i < slist.Count; i++)
    {
      Console.WriteLine(slist[i]);
    }


    var collection = (ICollection<string>)slist;
    Console.WriteLine(collection.Count);
    PrintBool(collection.Contains("two"));
    PrintBool(collection.Contains("two-shouldNotExits"));

    // The following line passes even if IsReadonly is undefined in JSIL.Bootsrap.js, because !undefined seems to be true in JavaScript
    // I can not write better unit test because of https://github.com/kevingadd/JSIL/issues/91
    // Manual tests (through JavaScript debugger) shows, that the property correctly returns false
    PrintBool(collection.IsReadOnly);

    Console.WriteLine("ICollection<T>.Add('four)");
    collection.Add("four");

    for (int i = 0; i < slist.Count; i++)
    {
      Console.WriteLine(slist[i]);
    }

    Console.WriteLine("ICollection<T>.Remove('one')");
    collection.Remove("one");

    for (int i = 0; i < slist.Count; i++)
    {
      Console.WriteLine(slist[i]);
    }

    Console.WriteLine("ICollection<T>.CopyTo");

    string[] array = new string[collection.Count];
    collection.CopyTo(array, 0);

    for (int i = 0; i < array.Length; i++)
    {
      Console.WriteLine(array[i]);
    }

    Console.WriteLine("ICollection<T>.Clear");

    Console.WriteLine(collection.Count);
    PrintBool(collection.Count == slist.Count);

    collection.Clear();

    Console.WriteLine(collection.Count);
    PrintBool(collection.Count == slist.Count);


  }

  public static void PrintBool(bool b)
  {
      Console.WriteLine(b ? 1 : 0);
  }
}