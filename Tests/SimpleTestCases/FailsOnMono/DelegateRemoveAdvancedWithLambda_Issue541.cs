using System;
public static class Program
{
    public static void Main(string[] args)
    {
	    // See http://confluence.jetbrains.com/display/ReSharper/Delegate+subtraction+has+unpredictable+semantics
        Action a = () => Console.Write("A");
        Action b = () => Console.Write("B");
        Action c = () => Console.Write("C");
        Action s = a + b + c + Console.WriteLine;
        s();                  //ABC 
        (s - a)();            //BC 
        (s - b)();            //AC 
        (s - c)();            //AB 
        (s - (a + b))();      //C 
        (s - (b + c))();      //A 
        (s - (a + c))();      //ABC 

        s = a + b + a;
        (s - a)();            // AB
    }
}