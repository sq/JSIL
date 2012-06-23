using System;
using System.Collections.Generic;

public static class Program {

  public class CustomClass
  {
    public string Field1;
    public string Field2;

    public override string ToString()
    {
      return Field1 + "/" + Field2;
    }
  }
    public static void Main (string[] args) {

      List<string> slist;

      slist = new List<string>();
      Console.WriteLine(slist.Count);

      Console.WriteLine("String list with 10 elements");
      slist = new List<string>(10);
      Console.WriteLine(slist.Count);

      
      for (int i = 0; i < slist.Count; i++)
      {
        PrintBool(slist[i]==null);
      }


      Console.WriteLine("Intlist with 10 elements");
      List<int> intList= new List<int>(10);
      Console.WriteLine(intList.Count);

      
      for (int i = 0; i < intList.Count; i++)
      {
        PrintBool(intList[i] == 0);
      }


      Console.WriteLine("String list with initializer ");
      slist=new List<string> { "zero","one","two","three" };


      for (int i = 0; i < slist.Count; i++)
      {
        Console.WriteLine(slist[i]);
      }


      Console.WriteLine("String list with array constructor ");

      string[] sArray = { "apple","orange","plum"};

      slist=new List<string>(sArray);  
      for (int i = 0; i < slist.Count; i++)
      {
        Console.WriteLine(slist[i]);
      }

      Console.WriteLine("ListWith custom Class Initializer");

      
      List<CustomClass> ccList=new List<CustomClass>() { new CustomClass() { Field1="1.first", Field2 = "1.Second" }, new CustomClass() { Field1="2.first", Field2 = "2.Second" } };

      for (int i = 0; i < ccList.Count; i++)
      {
        Console.WriteLine(ccList[i].ToString());
      }

      var cc= new CustomClass() { Field1="one", Field2 = "two" };
      ccList= new List<CustomClass>() { cc };
      PrintBool(ccList[0]==cc);

    }

    public static void PrintBool(bool b)
    {
      Console.WriteLine(b ? 1 : 0);
    }
}