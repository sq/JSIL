using System;

public static class Program {

  public static void Main(string[] args) 
  {
    // Based on System.Array.REsize documentation (http://msdn.microsoft.com/en-us/library/bb348051.aspx)

    // Create and initialize a new string array.
    String[] myArr = {"The", "quick", "brown", "fox", "jumps", 
            "over", "the", "lazy", "dog"};

    // Display the values of the array.
    Console.WriteLine(
        "The string array initially contains the following values:");
    PrintIndexAndValues(myArr);

    // Resize the array to a bigger size (five elements larger).
    Array.Resize(ref myArr, myArr.Length + 5);

    // Display the values of the array.
    Console.WriteLine("After resizing to a larger size, ");
    Console.WriteLine("the string array contains the following values:");
    PrintIndexAndValues(myArr);

    // Resize the array to a smaller size (four elements).
    Array.Resize(ref myArr, 4);

    // Display the values of the array.
    Console.WriteLine("After resizing to a smaller size, ");
    Console.WriteLine("the string array contains the following values:");
    PrintIndexAndValues(myArr);

    // Test integer arrays:

    int[] intArr = {10,20,30,40};
    Console.WriteLine(intArr[0]);
    Console.WriteLine(intArr[1]);
    Array.Resize(ref intArr, 5);
    Console.WriteLine(intArr[4]);  // should print out "0"



  }

  public static void PrintIndexAndValues(String[] myArr)
  {
    for (int i = 0; i < myArr.Length; i++)
    {
      string s = myArr[i];
      if (s == null)
      {
        s = ""; // JSIL prints out "null" when C# prints out String.Empty. Fix this, since this "feature" is not subject of this ubit test
      }
      Console.WriteLine("   [{0}] : {1}", i, s);
    }
    Console.WriteLine();
  }


}