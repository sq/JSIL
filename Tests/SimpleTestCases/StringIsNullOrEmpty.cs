using System;

public static class Program {

  public static void Main(string[] args) 
  {
    string s=null;
    PrintBool(string.IsNullOrEmpty(s)); 

    s="";
    PrintBool(string.IsNullOrEmpty(s)); 

    s="xxx";
    PrintBool(string.IsNullOrEmpty(s)); 

    PrintBool(string.IsNullOrEmpty("")); 
    PrintBool(string.IsNullOrEmpty(null)); 
    PrintBool(string.IsNullOrEmpty("zzz")); 

  }

  public static void PrintBool(bool b)
  {
    Console.WriteLine(b ? 1 : 0);
  }


}