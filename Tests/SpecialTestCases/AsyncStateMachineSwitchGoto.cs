using System;

public static class Program {
    public static void TestStateMachine (int i) {
        try {
            switch (i) {
                case 0:
                    goto Label0;
                case 1:
                    goto Label1;
            }
            Console.WriteLine("DefaultCase");
        Label0:
            Console.WriteLine("Label0");
        Label1:
            Console.WriteLine("Label1");
        } catch {
            Console.WriteLine("Exception");
        }
        Console.WriteLine("End");
    }

    public static void Main (string[] args) {
        TestStateMachine(0);
        TestStateMachine(1);
        TestStateMachine(2);
    }
}