using System;
using JSIL.Meta;

public static class Program {
    [JSIgnore]
    public static event Action Event;
  
    public static void Main (string[] args) {
        Program.Event += () => Console.WriteLine("a");
        Program.Event();
        Program.Event -= () => Console.WriteLine("a");
    }
}