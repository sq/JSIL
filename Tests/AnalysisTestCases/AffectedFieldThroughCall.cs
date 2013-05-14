// Variant of Issue #194
using System;

public class Stack<T> {
    private class Node {
        public T Value;
        public Node Next;
    }

    private Node _top;

    public bool Empty () {
        return _top == null;
    }

    public void Push (T val) {
        _top = new Node {
            Value = val,
            Next = _top
        };
    }

    private void PopTop () {
        _top = _top.Next;
    }

    public T Pop () {
        if (_top == null)
            throw new Exception();

        var result = _top.Value;
        PopTop();
        return result;
    }
}

public static class Program {
    public static void Main () {
        var s = new Stack<int>();

        s.Push(10);
        s.Push(100);

        Console.WriteLine("Expected {0}, actual {1}.", 100, s.Pop());
        Console.WriteLine("Expected {0}, actual {1}.", 10, s.Pop());
    }
}