using System;
using System.Collections.ObjectModel;

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

      ObservableCollection<string> slist;

      slist = new ObservableCollection<string>();
      Console.WriteLine(slist.Count);

      var list = new System.Collections.Generic.List<string>(10);
      Console.WriteLine("String ObservableCollection with 10 elements");
      slist = new ObservableCollection<string>(list);
      Console.WriteLine(slist.Count);

      
      for (int i = 0; i < slist.Count; i++)
      {
        PrintBool(slist[i]==null);
      }


      Console.WriteLine("Int ObservableCollection with 10 elements");
      System.Collections.Generic.List<int> intList= new System.Collections.Generic.List<int>(10);
      var intObs = new ObservableCollection<int>(intList);
      Console.WriteLine(intObs.Count);

      
      for (int i = 0; i < intObs.Count; i++)
      {
        PrintBool(intObs[i] == 0);
      }


      Console.WriteLine("String ObservableCollection with initializer ");
      slist = new ObservableCollection<string> { "zero", "one", "two", "three" };


      for (int i = 0; i < slist.Count; i++)
      {
        Console.WriteLine(slist[i]);
      }


      Console.WriteLine("String ObservableCollection with array constructor ");

      string[] sArray = { "apple","orange","plum"};

      slist = new ObservableCollection<string>(sArray);  
      for (int i = 0; i < slist.Count; i++)
      {
        Console.WriteLine(slist[i]);
      }

      Console.WriteLine("ListWith custom Class Initializer");


      ObservableCollection<CustomClass> ccList = new ObservableCollection<CustomClass>() { new CustomClass() { Field1 = "1.first", Field2 = "1.Second" }, new CustomClass() { Field1 = "2.first", Field2 = "2.Second" } };

      for (int i = 0; i < ccList.Count; i++)
      {
        Console.WriteLine(ccList[i].ToString());
      }

      var cc= new CustomClass() { Field1="one", Field2 = "two" };
      ccList = new ObservableCollection<CustomClass>() { cc };
      PrintBool(ccList[0]==cc);

    }

     static void PrintBool(bool b)
    {
      Console.WriteLine(b ? 1 : 0);
    }
}