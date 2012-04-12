using System;

public static class Program {
    public static int F () {
        return 1;
    }

    public static void Main (string[] args) {
        switch (F()) {
            case 1:
                Console.WriteLine("1");

                switch (F()) {
                    case 1:
                        Console.WriteLine("1");
                        break;
                    case 2:
                        Console.WriteLine("2");
                        break;
                }

                break;
            case 2:
                Console.WriteLine("2");

                switch (F()) {
                    case 1:
                        Console.WriteLine("1");
                        break;
                    case 2:
                        Console.WriteLine("2");
                        break;
                }

                break;
        }
    }
}