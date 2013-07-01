using System;

public static class Program {
    public static StreamingContext? _context;
    public static StreamingContext DefaultContext;

    public static void Main (string[] args) {
        _context = new StreamingContext(1);
        DefaultContext = new StreamingContext(2);

        Console.WriteLine(X());

        _context = null;

        Console.WriteLine(X());
    }

    public static StreamingContext X () {
        StreamingContext? context = _context;
        if (!context.HasValue) {
            return DefaultContext;
        }
        return context.GetValueOrDefault();
    }
}

public struct StreamingContext {
    public int Index;

    public StreamingContext (int index) {
        Index = index;
    }

    public override string ToString () {
        return Index.ToString();
    }
}