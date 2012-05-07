using System;

public static class Program
{
  public static void Main(string[] args) {
      
      var x = new int[,] { { 1, 2, 3 }, { 4, 5, 6 } };

      Console.WriteLine(x.Length);

      for (int i = 0; i < x.GetLength(0);i++ )
      {
        for (int j = 0; j < x.GetLength(1); j++)
        {
          Console.WriteLine(x[i,j]);
        }

      }

    }
}