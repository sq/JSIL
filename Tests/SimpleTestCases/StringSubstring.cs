using System;

public static class Program {

  public static void Main(string[] args) 
  {

    string s = "abcdefg";
  
    Console.WriteLine(s.Substring(0));
    Console.WriteLine(s.Substring(3));

    Console.WriteLine(s.Substring(3,1));
    Console.WriteLine(s.Substring(0, 3));

    // The following throw exceptions in C#, but not in JS
    //Console.WriteLine(s.Substring(0, 100));
    //Console.WriteLine(s.Substring(500));


    ComplexTest();
  }

  public static void ComplexTest()
  {
    // More complex test from MSDN

    string[] info = { "Name: Felica Walker", "Title: Mz.", "Age: 47", "Location: Paris", "Gender: F" };
    int found = 0;

    Console.WriteLine("The initial values in the array are:");
    foreach (string s in info)
      Console.WriteLine(s);

    //Console.WriteLine("{0}We want to retrieve only the key information. That is:", Environment.NewLine);  // It looks like Environment.NewLine prints out "undefined" in JSIL
    Console.WriteLine("We want to retrieve only the key information. That is:");  


    foreach (string s in info)
    {
      found = s.IndexOf(":");
      Console.WriteLine(s.Substring(found + 1).Trim());
    }
  }

}