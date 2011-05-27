using System;

public static class Program {
    public delegate void CustomDelegate (float f);

    public static void Main (string[] args) {
        OverloadedMethod(_Action);
        OverloadedMethod(_Action_Int);
        OverloadedMethod((CustomDelegate)_CustomDelegate);
    }

    public static void _Action () {
        Console.WriteLine("_Action");
    }

    public static void _Action_Int (int i) {
        Console.WriteLine("_Action<int>({0})", i);
    }

    public static void _CustomDelegate (float f) {
        Console.WriteLine("_CustomDelegate<float>({0})", f);
    }

    public static void OverloadedMethod (Action a) {
        Console.WriteLine("OverloadedMethod(<Action>)");
        a();
    }

    public static void OverloadedMethod (Action<int> ai) {
        Console.WriteLine("OverloadedMethod(<Action<int>>)");
        ai(1);
    }

    public static void OverloadedMethod (CustomDelegate cd) {
        Console.WriteLine("OverloadedMethod(<CustomDelegate>)");
        cd(1.5f);
    }
}