using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {

      var al = new ArrayList();      

      al.Add("zero");
      al.Add("one");
      al.Add("two");

      al.IndexOf("zero");
      Console.WriteLine(al.Count);
      Console.WriteLine(al[0]);
      Console.WriteLine(al[1]);
      Console.WriteLine(al[2]);

      al[1] = "one-updated"; 
      Console.WriteLine(al[1]); // this used to fail

    }

    public static void PrintBool(bool b)
    {
      Console.WriteLine(b ? 1 : 0);
    }

}