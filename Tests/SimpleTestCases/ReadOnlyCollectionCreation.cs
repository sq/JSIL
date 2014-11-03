using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class Program
{
    public static void Main(string[] args)
    {
        var readOnlyCollection = new ReadOnlyCollection<int>(new CustomList());

        foreach (var i in readOnlyCollection)
        {
            Console.WriteLine(i);
        }
    }
}

public class CustomList : IList<int>
{
    private int[] _content = new[] {1, 2, 5, 10};

    public IEnumerator<int> GetEnumerator()
    {
        foreach (var i in _content)
        {
            yield return i;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(int item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(int item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(int[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(int item)
    {
        throw new NotImplementedException();
    }

    public int Count
    {
        get { return _content.Length; }
    }

    public bool IsReadOnly
    {
        get { return true; }
    }

    public int IndexOf(int item)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, int item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    public int this[int index]
    {
        get { return _content[index]; }
        set { throw new NotImplementedException(); }
    }
}

