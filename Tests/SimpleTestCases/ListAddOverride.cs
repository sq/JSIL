using System;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var l = new GUIHandler();
        l.Add(new GUIElement());
        l.Add(new GUIElement());
        Console.WriteLine(l.Count);
    }
}

public class GUIElement {
    public void AttachToContext () {
        Console.WriteLine("GUIElement.AttachToContext");
    }
}

public class GUIHandler : List<GUIElement> {
    public new void Add (GUIElement item) {
        base.Add(item);
        item.AttachToContext();
    }
}