using System;
public static class Program
{
    public static void Main(string[] args)
    {
        Action a = A;
        Action b = B;
        Action c = C;
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

    public static void A()
    {
        Console.Write("A");
    }

    public static void B()
    {
        Console.Write("B");
    }

    public static void C()
    {
        Console.Write("C");
    }
}
