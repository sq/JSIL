using System;
using System.Collections.Generic;

public static class Program {
    public static bool True () {
        return true;
    }
  
    public static void Main (string[] args) {
        bool LocalTrue = true;
      
        if (true)
            Console.WriteLine("true");
        else
            Console.WriteLine("false");
      
        if (LocalTrue)
            Console.WriteLine("true");
        else
            Console.WriteLine("false");
        
        if (True())
            Console.WriteLine("true");
        else
            Console.WriteLine("false");
    }
}