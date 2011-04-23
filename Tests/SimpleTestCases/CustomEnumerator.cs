using System;
using System.Collections;
using System.Collections.Generic;

public static class Program {
    public static void Main (string[] args) {
        foreach (var i in new CustomEnumerator())
            Console.WriteLine("{0}", i);
    }

    public class CustomEnumerator : IEnumerator<int>, IEnumerable<int> {
        public int CurrentValue = -1;

        public bool MoveNext () {
            CurrentValue += 1;
            return (CurrentValue < 10);
        }

        public void Reset () {
            throw new NotImplementedException();
        }

        public int Current {
            get {
                return CurrentValue;
            }
        }

        object IEnumerator.Current {
            get {
                return CurrentValue;
            }
        }

        public IEnumerator<int> GetEnumerator () {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator () {
            return this;
        }

        public void Dispose () {
            Console.WriteLine("Disposed");
        }
    }
}