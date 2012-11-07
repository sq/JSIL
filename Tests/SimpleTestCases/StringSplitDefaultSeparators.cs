using System;

public static class Program {
  public static void Main(string[] args) {
    string s = "abc def\tghi\u00a0jkl";
    string[] split = s.Split(null);

    Console.WriteLine(split.Length);
    Console.WriteLine(split[0]);
    Console.WriteLine(split[1]);
    Console.WriteLine(split[2]);
    Console.WriteLine(split[3]);
    }
}