using System;

class Program {
    static void Categorize (char c) {
        Console.WriteLine((int) c);
        
        if (char.IsControl(c))
            Console.WriteLine("IsControl");
        
        if (char.IsDigit(c))
            Console.WriteLine("IsDigit");
        
        if (char.IsLetter(c))
            Console.WriteLine("IsLetter");
        
        if (char.IsLetterOrDigit(c))
            Console.WriteLine("IsLetterOrDigit");
        
        if( char.IsSurrogate(c))
            Console.WriteLine("IsSurrogate");
        
        if (char.IsWhiteSpace(c))
            Console.WriteLine("IsWhiteSpace");
        
        Console.WriteLine();
    }
    
    public static void Main () {
        Categorize(' ');
        Categorize('4');
        Categorize('J');
        Categorize('s');
        Categorize((char) 0x0D);
        Categorize((char) 0xD801);
    }
}
