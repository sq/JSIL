using System;

public static class Program {
    public static void Main (string[] args) {
        int i = 0;
        a:
            i += 1;
            Console.WriteLine("a");
        
        for (; i < 16; i++) {
            if (i == 8)
                goto a;
            else
                goto c;
            
            c:
                Console.WriteLine("c");
        }
    }
}