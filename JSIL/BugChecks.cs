using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Internal {
    public static class BugChecks {
        internal class MyKey {
            public readonly int HashCode;

            public MyKey (int hashCode) {
                HashCode = hashCode;
            }

            public override bool Equals (object obj) {
                return Object.ReferenceEquals(this, obj);
            }

            public override int GetHashCode () {
                return HashCode;
            }
        }

        private static void Fail (string message) {
            Console.Error.WriteLine("Bug check failed: {0}", message);
            throw new PlatformNotSupportedException(message);
        }

        public static void RunBugChecks () {
            BrokenConcurrentDictionary();
        }

        public static void BrokenConcurrentDictionary () {
            const string msg = "Broken ConcurrentDictionary: https://bugzilla.xamarin.com/show_bug.cgi?id=6225";
            var keyA = new MyKey(1);
            var keyB = new MyKey(1);
            var dict = new ConcurrentDictionary<MyKey, string>(1, 2);

            if (!dict.TryAdd(keyA, "a"))
                Fail(msg);

            string foundValue;
            if (!dict.TryGetValue(keyA, out foundValue) || foundValue != "a")
                Fail(msg);

            if (dict.TryGetValue(keyB, out foundValue))
                Fail(msg);
        }
    }
}