using System;

public static class Program {
  public static void Main(string[] args) {
    var x = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };

    x[0, 1] *= 2;
    x[1, 2] /= 2;

    foreach (int i in x)
        Console.WriteLine(i);
  }
}