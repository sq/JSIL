using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        var local = new MyDictionary {
            {"a", "b"},
            {"c", "d"}
        };

        foreach (var kvp in local)
            Console.WriteLine(kvp);
    }
}

public struct StringPair {
    public readonly string Key, Value;

    public StringPair (string key, string value) {
        Key = key;
        Value = value;
    }

    public override string ToString () {
        return String.Format("{0}={1}", Key, Value);
    }
}

public class MyDictionary : IEnumerable<StringPair> {
    protected readonly List<StringPair> Entries = new List<StringPair>();

    public void Add (string key, string value) {
        Entries.Add(new StringPair(key, value));
    }

    IEnumerator IEnumerable.GetEnumerator () {
        return Entries.GetEnumerator();
    }

    public IEnumerator<StringPair> GetEnumerator () {
        return Entries.GetEnumerator();
    }
}