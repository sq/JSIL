using System;
using System.Collections.Generic;

public static class Program
{


  public static void Main(string[] args)
  {


    var slist = new List<string> { "zero", "one", "two", "three" };



    // collection test are already implemented in ListiCollection.cs

    // NOTE: JSIL is quite forgiving when implementing the interface - it does not take into account the returning parameter type
    //       so "object get_Item(i)" can also implement "T get_Item(int)". The listExternals implementation as a little bit
    //       loosey goosey. To be strict, different types of collection (ArrayList, List<T> should really have slightly
    //       different method signatures for get_item, set_Item and others - this is currenlty only partially covverred in JSIL.Bootsrap.js)
    Console.WriteLine("----- interface ----");

    var list = (IList<string>) slist;

    Console.WriteLine(list.Count);
    Console.WriteLine(list[0]);
    Console.WriteLine(list[1]);
    Console.WriteLine(list[2]);
    Console.WriteLine(list[3]);
    
    Console.WriteLine(list.IndexOf("two"));
    
    list.Insert(1,"inserted");
    Console.WriteLine(list.Count);
    Console.WriteLine(list[4]);
    
    list.RemoveAt(2);
    Console.WriteLine(list.Count);
    Console.WriteLine(list[3]);

    
    list[2] = "modified";
    Console.WriteLine(list[2]);
     
    // also check item acecssor throw class (not the interface)
    slist = new List<string> { "zero", "one", "two", "three" };

    Console.WriteLine("----- class ----");

    Console.WriteLine(slist.Count);
    Console.WriteLine(slist[0]);
    Console.WriteLine(slist[1]);
    Console.WriteLine(slist[2]);
    Console.WriteLine(slist[3]);

   Console.WriteLine(slist.IndexOf("two"));
    
    slist.Insert(1,"inserted");
    Console.WriteLine(slist.Count);
    Console.WriteLine(slist[4]);
    
    slist.RemoveAt(2);
    Console.WriteLine(slist.Count);
    Console.WriteLine(slist[3]);

    
    slist[2] = "modified";
    Console.WriteLine(slist[2]);

  }


}