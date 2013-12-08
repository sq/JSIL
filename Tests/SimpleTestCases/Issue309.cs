using System;
using System.Collections.Generic;

public static class Program {
    class GenericClass<T>
        where T : new() {
        Dictionary<uint, T> trackedObjects = new Dictionary<uint, T>();

        public T GetOrAdd (uint handle) {
            T result;
            if (this.trackedObjects.TryGetValue(handle, out result)) {
                return result;
            } else {
                result = new T();
                this.trackedObjects.Add(handle, result);
                return result;
            }
        }
    }

    public static void Main (string[] args) {
        var genericInstance = new GenericClass<MyClass>();
        Console.WriteLine(genericInstance.GetOrAdd(0));
    }
}

class MyClass {
}