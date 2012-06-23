using System;

public static class Program {

  public static void Main(string[] args) 
  {

    string s = "abcdefg";
    //PrintBool(s.Contains('b'));
    //PrintBool(s.Contains('x'));

    PrintBool(s.Contains("e"));
    PrintBool(s.Contains("fg"));
    PrintBool(s.Contains("xx"));

    PrintBool(s.Contains(""));
  }

  public static void PrintBool(bool b)
  {
    Console.WriteLine(b ? 1 : 0);
  }

}