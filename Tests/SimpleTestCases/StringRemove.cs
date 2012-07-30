using System;

public static class Program {
    public static void Main(string[] args) {
        string s = "abcdefg";
  
        Console.WriteLine(s.Remove(0));
        Console.WriteLine(s.Remove(3));
        Console.WriteLine(s.Remove(1, 1));
        Console.WriteLine(s.Remove(1, 2));
        Console.WriteLine(s.Remove(0, 7));
    }
}