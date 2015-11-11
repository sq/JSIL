using System;
using System.Collections.Generic;

public static class Program
{
  public static void Main(string[] args)
  {
    Console.WriteLine("Testing string dictionary");
    var d = new Dictionary<string, int> {
            {"a", 1},
            {"b", 2},
            {"z", 3},
            {"c", 4}
        };

    Console.WriteLine(d["a"]);
    Console.WriteLine(d["b"]);
    Console.WriteLine(d["c"]);



    Console.WriteLine("Testing object dictionary"); // This was not yet supported in December 2011, but it works now (May 2012)
    object o1=new Object();
    object o2=new Object();
    object o3=new Object();

    var od = new Dictionary<object, int> {
      { o1, 1},
      { o2, 2},
      { o3, 3} };

    PrintBool(od[o1] == 1);
    PrintBool(od[o2] == 2);
    PrintBool(od[o3] == 3);      

    Console.WriteLine("performing complex test");
    ComplexTest();
  }

  public static void PrintBool(bool b)
  {
    Console.WriteLine(b ? 1 : 0);
  }

  public static void ComplexTest() // based on MSDN. The stuff not (yet) implemented in JSIL is commented out
  {
        Dictionary<string, string> openWith = 
            new Dictionary<string, string>();

        // Add some elements to the dictionary. There are no 
        // duplicate keys, but some of the values are duplicates.
        openWith.Add("txt", "notepad.exe");
        openWith.Add("bmp", "paint.exe");
        openWith.Add("dib", "paint.exe");
        openWith.Add("rtf", "wordpad.exe");

        try
        {
            openWith.Add("txt", "winword.exe");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("An element with Key = \"txt\" already exists.");
        }

        // The Item property is another name for the indexer, so you 
        // can omit its name when accessing elements. 
        Console.WriteLine("For key = \"rtf\", value = {0}.", 
            openWith["rtf"]);

        // The indexer can be used to change the value associated
        // with a key.
        openWith["rtf"] = "winword.exe";
        Console.WriteLine("For key = \"rtf\", value = {0}.", 
            openWith["rtf"]);

        // If a key does not exist, setting the indexer for that key
        // adds a new key/value pair.
        openWith["doc"] = "winword.exe";

        // The indexer throws an exception if the requested key is
        // not in the dictionary.
        try
        {
            Console.WriteLine("For key = \"tif\", value = {0}.", 
                openWith["tif"]);
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("Key = \"tif\" is not found.");
        }

        // When a program often has to try keys that turn out not to
        // be in the dictionary, TryGetValue can be a more efficient 
        // way to retrieve values.
        string value = "";
        if (openWith.TryGetValue("tif", out value))
        {
            Console.WriteLine("For key = \"tif\", value = {0}.", value);
        }
        else
        {
            Console.WriteLine("Key = \"tif\" is not found.");
        }

        // ContainsKey can be used to test keys before inserting 
        // them.
        if (!openWith.ContainsKey("ht"))
        {
            openWith.Add("ht", "hypertrm.exe");
            Console.WriteLine("Value added for key = \"ht\": {0}", 
                openWith["ht"]);
        }

        // FAILS WITH JSIL - it prints out "undefined" for all keys and values (either Enumerator or .MemberwiseClone() is not doing its work):
        // When you use foreach to enumerate dictionary elements,
        // the elements are retrieved as KeyValuePair objects.
        Console.WriteLine();
        foreach( KeyValuePair<string, string> kvp in openWith )
        {
            Console.WriteLine("Key = {0}, Value = {1}", 
                kvp.Key, kvp.Value);
        }

        // To get the values alone, use the Values property.
        Dictionary<string, string>.ValueCollection valueColl =
            openWith.Values;

        // The elements of the ValueCollection are strongly typed
        // with the type that was specified for dictionary values.
        Console.WriteLine();
        foreach( string s in valueColl )
        {
            Console.WriteLine("Value = {0}", s);
        }

        // To get the keys alone, use the Keys property.
        Dictionary<string, string>.KeyCollection keyColl =
            openWith.Keys;

        // The elements of the KeyCollection are strongly typed
        // with the type that was specified for dictionary keys.
        Console.WriteLine();
        foreach( string s in keyColl )
        {
            Console.WriteLine("Key = {0}", s);
        }

        // Use the Remove method to remove a key/value pair.
        Console.WriteLine("\nRemove(\"doc\")");
        openWith.Remove("doc");

        if (!openWith.ContainsKey("doc"))
        {
            Console.WriteLine("Key \"doc\" is not found.");
        }

  }
}