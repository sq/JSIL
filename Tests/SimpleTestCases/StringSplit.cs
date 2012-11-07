using System;

public static class Program {

  public static void Main(string[] args) 
  {

    string s = "abc,defg";
    string[] split = s.Split(',');

    Console.WriteLine(split.Length);
    Console.WriteLine(split[0]);
    Console.WriteLine(split[1]);

    ComplexTest(); // currently not supported: JSIL.Bootstrap.js:730: Error: Split cannot handle more than one separator
  }

  public static void ComplexTest() {
    // From MSDN sample from String.Split
    string words = "This is a list of words, with: a bit of punctuation" +
                   "\tand a tab character.";

    string[] split = words.Split(new Char[] { ' ', ',', '.', ':', '\t' });

    foreach (string s in split)
    {

      if (s.Trim() != "")
        Console.WriteLine(s);
    }

  }

}