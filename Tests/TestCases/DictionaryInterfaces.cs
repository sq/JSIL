using System;
using System.Collections.Generic;
using System.Collections;


public static class Program {
    public static void Main (string[] args)
    {
        var genericIDictionary = GetFilledGenericIDictionary();
        Console.WriteLine(genericIDictionary.Count);
        foreach (var pair in genericIDictionary)
        {
            Console.WriteLine(pair.Key);
            Console.WriteLine(pair.Value);
        }
        foreach (var value in genericIDictionary.Values)
        {
            Console.WriteLine(value);
        }
        foreach (var value in genericIDictionary.Keys)
        {
            Console.WriteLine(value);
        }

        var iDictionary = GetFilledIDictionary();
        Console.WriteLine(iDictionary.Count);
        foreach (var pair in genericIDictionary)
        {
            Console.WriteLine(pair.Key);
            Console.WriteLine(pair.Value);
        } 
        foreach (var value in iDictionary.Values)
        {
            Console.WriteLine(value);
        }
        foreach (var value in iDictionary.Keys)
        {
            Console.WriteLine(value);
        }

        var genericICollection = GetFilledGenericICollection();
        Console.WriteLine(genericICollection.Count);
        foreach (var pair in genericICollection)
        {
            Console.WriteLine(pair.Key);
            Console.WriteLine(pair.Value);
        }

        var iCollection = GetFilledICollection();
        Console.WriteLine(iCollection.Count);
        foreach (var obj in iCollection)
        {
            var pair = (KeyValuePair<string, int>) obj;
            Console.WriteLine(pair.Key);
            Console.WriteLine(pair.Value);
        }

        var genericIEnumerable = GetFilledGenericIEnumerable();
        foreach (var pair in genericIEnumerable)
        {
            Console.WriteLine(pair.Key);
            Console.WriteLine(pair.Value);
        }

        var iEnumerable = GetFilledIEnumerable();
        foreach (var obj in iEnumerable)
        {
            var pair = (KeyValuePair<string, int>)obj;
            Console.WriteLine(pair.Key);
            Console.WriteLine(pair.Value);
        }
    }

    public static IEnumerable GetFilledIEnumerable()
    {
        return CreateAndFill();
    }

    public static IEnumerable<KeyValuePair<string, int>> GetFilledGenericIEnumerable()
    {
        return CreateAndFill();
    }
    
    public static ICollection GetFilledICollection()
    {
        return CreateAndFill();
    }

    public static ICollection<KeyValuePair<string, int>> GetFilledGenericICollection()
    {
        return CreateAndFill();
    }

    public static IDictionary GetFilledIDictionary()
    {
        return CreateAndFill();
    }

    public static IDictionary<string, int> GetFilledGenericIDictionary()
    {
        return CreateAndFill();
    }

    public static Dictionary<string, int> CreateAndFill()
    {
        return new Dictionary<string, int> {
            {"a", 1},
            {"b", 2},
            {"z", 3},
            {"c", 4}
        };
    }
}