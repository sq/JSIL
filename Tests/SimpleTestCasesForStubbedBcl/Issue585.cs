using System;
using System.Collections.ObjectModel;

class Program {
    public static void Main () {
        Collection<string> collection = new Collection<string> {
            "a",
            "b"
        };

        foreach (var s in collection) {
            Console.WriteLine(s);
        }

        Console.WriteLine(collection.Count);

        var test = collection.GetEnumerator();
    }
}
