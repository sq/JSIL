using System;

public static class Program {
    public static void Main (string[] args) {
        // TODO: Throw appropriately.
        /*
        Console.WriteLine("Checked");

        checked {
            {
                uint x = uint.MaxValue;
                x = x * x;
                Console.WriteLine(x);
            }

            {
                uint x = (uint.MaxValue / 3);
                x = x * x;
                Console.WriteLine(x);
            }
        }
         */

        Console.WriteLine("Unchecked");

        unchecked {
            {
                uint x = uint.MaxValue;
                x = x * x;
                Console.WriteLine(x);
            }

            {
                uint x = (uint.MaxValue / 3);
                x = x * x;
                Console.WriteLine(x);
            }
        }
    }
}