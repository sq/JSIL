using System;
using System.Collections.Generic;
using JSIL.Meta;

public static class Program {
    public static void Main (string[] args) {
        Console.WriteLine(((IMyInterface)new MyClass()).Add());
    }
}

public interface IMyInterface : IList<int> {
    int Add ();
}

public class MyClass : IMyInterface {
    public int Add () {
        return 5;
    }

    public int IndexOf (int item) {
        throw new NotImplementedException();
    }

    public void Insert (int index, int item) {
        throw new NotImplementedException();
    }

    public void RemoveAt (int index) {
        throw new NotImplementedException();
    }

    public int this[int index] {
        get { throw new NotImplementedException(); }
        set { throw new NotImplementedException(); }
    }

    public void Add (int item) {
        throw new NotImplementedException();
    }

    public void Clear () {
        throw new NotImplementedException();
    }

    public bool Contains (int item) {
        throw new NotImplementedException();
    }

    public void CopyTo (int[] array, int arrayIndex) {
        throw new NotImplementedException();
    }

    public int Count {
        get { throw new NotImplementedException(); }
    }

    public bool IsReadOnly {
        get { throw new NotImplementedException(); }
    }

    public bool Remove (int item) {
        throw new NotImplementedException();
    }

    public System.Collections.Generic.IEnumerator<int> GetEnumerator () {
        throw new NotImplementedException();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
        throw new NotImplementedException();
    }
}