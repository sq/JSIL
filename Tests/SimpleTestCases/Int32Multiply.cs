using System;

public static class Program {
    // TODO: What does the C# spec say about this?
    // In C, signed imul overflow is undefined.
    // As a result, JS only exposes unsigned imul, not signed imul.
    public static void Main (string[] args) {
        // TODO: Throw appropriately.
        /*
        Console.WriteLine("Checked");

        checked {
            {
                int x = int.MinValue;
                x = x * x;
                Console.WriteLine(x);
            }

            {
                int x = (int.MaxValue / 3);
                x = x * x;
                Console.WriteLine(x);
            }
        }
         */

        Console.WriteLine("Unchecked");

        unchecked {
            {
                int x = int.MinValue;
                x = x * x;
                Console.WriteLine(x);
            }

            {
                int x = (int.MaxValue / 3);
                x = x * x;
                Console.WriteLine(x);
            }
        }
    }
}