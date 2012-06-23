using System;
using JSIL;

public static class Program { 
    public static void Main (string[] args) {
        var e = new Element("type");
        var e2 = new Element(5);

        Console.WriteLine("{0} {1}", e.DummyField ?? "null", e.Value);
        Console.WriteLine("{0} {1}", e2.DummyField ?? "null", e2.Value);
    }
}

public class Element {
    public object Value;
    public string DummyField;

    [JSIL.Meta.JSReplacement("$type + 'createElement'")]
    protected static object ParentExpression (string type) {
        return type + "createElement";
    }

    public Element (string type)
        : this(ParentExpression(type)) {
        this.DummyField = "initialized only from ctor 0";
    }

    public Element (object value) {
        this.Value = value;
    }
}