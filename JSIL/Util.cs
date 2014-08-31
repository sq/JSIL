using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ICSharpCode.Decompiler;
using ICSharpCode.NRefactory.CSharp;
using JSIL.Ast;
using Mono.Cecil;

using TypeInfo = JSIL.Internal.TypeInfo;

namespace JSIL.Internal {
    public enum EscapingMode {
        None,
        MemberIdentifier,
        TypeIdentifier,
        String
    }

    public static class Util {
        public static readonly HashSet<string> ReservedWords = new HashSet<string> {
            "break", "do", "instanceof", "typeof",
            "case", "else", "new", "var",
            "catch", "finally", "return", "void",
            "continue", "for", "switch", "while",
            "debugger", "function", "this", "with",
            "default", "if", "throw", "delete",
            "in", "try", "import", "class", "enum",
            "export", "extends", "super", "let",
            "package", "interface", "implements", "private",
            "protected", "public", "static", "yield",
            "const", "true", "false", "null",
        };

        // We need to flag these names as reserved because they are properties of
        //  Function in many browsers.
        public static readonly HashSet<string> ReservedIdentifiers = new HashSet<string> {
            "name", "length", "arity", "constructor",
            "caller", "arguments", "call", "apply", "bind"
        };

        public static Regex ValidIdentifier = new Regex(
            "$[A-Za-z_$]([A-Za-z_$0-9]*)^", 
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        private static ThreadLocal<StringBuilder> EscapeStringBuilder = new ThreadLocal<StringBuilder>(
            () => new StringBuilder(10240)
        );

        public static string GetPathOfAssembly (Assembly assembly) {
            var uri = new Uri(assembly.CodeBase);
            var result = Uri.UnescapeDataString(uri.AbsolutePath);

            if (String.IsNullOrWhiteSpace(result))
                result = assembly.Location;

            result = result.Replace('/', System.IO.Path.DirectorySeparatorChar);

            return result;
        }

        public static string EscapeIdentifier (string identifier, EscapingMode escapingMode = EscapingMode.MemberIdentifier) {
            if (escapingMode == EscapingMode.None)
                return identifier;

            bool isEscaped = false;
            string result = identifier;

            var sb = EscapeStringBuilder.Value;
            sb.Clear();

            for (int i = 0, l = identifier.Length; i < l; i++) {
                var ch = identifier[i];

                switch (ch) {
                    case '.':
                        if (escapingMode != EscapingMode.MemberIdentifier)
                            sb.Append(".");
                        else {
                            sb.Append("_");
                            isEscaped = true;
                        }
                    break;
                    case '/':
                        if (escapingMode == EscapingMode.MemberIdentifier) {
                            sb.Append("_");
                            isEscaped = true;
                        } else if (escapingMode == EscapingMode.TypeIdentifier) {
                            sb.Append("_");
                            isEscaped = true;
                        } else
                            sb.Append("/");
                    break;
                    case '+':
                        if (escapingMode == EscapingMode.MemberIdentifier) {
                            sb.Append("_");
                            isEscaped = true;
                        } else if (escapingMode == EscapingMode.TypeIdentifier) {
                            sb.Append("_");
                            isEscaped = true;
                        } else
                            sb.Append("+");
                    break;
                    case '`':
                        if (escapingMode != EscapingMode.String) {
                            sb.Append("$b");
                        } else {
                            sb.Append("`");
                        }
                        isEscaped = true;
                    break;
                    case '~':
                        sb.Append("$t");
                        isEscaped = true;
                    break;
                    case ':':
                        sb.Append("$co");
                        isEscaped = true;
                    break;
                    case '<':
                        sb.Append("$l");
                        isEscaped = true;
                    break;
                    case '>':
                        sb.Append("$g");
                        isEscaped = true;
                    break;
                    case '(':
                        sb.Append("$lp");
                        isEscaped = true;
                    break;
                    case ')':
                        sb.Append("$rp");
                        isEscaped = true;
                    break;
                    case '{':
                        sb.Append("$lc");
                        isEscaped = true;
                    break;
                    case '}':
                        sb.Append("$rc");
                        isEscaped = true;
                    break;
                    case '[':
                        sb.Append("$lb");
                        isEscaped = true;
                    break;
                    case ']':
                        sb.Append("$rb");
                        isEscaped = true;
                    break;
                    case '@':
                        sb.Append("$at");
                        isEscaped = true;
                    break;
                    case '-':
                        sb.Append("$da");
                        isEscaped = true;
                    break;
                    case '=':
                        sb.Append("$eq");
                        isEscaped = true;
                    break;
                    case ' ':
                        sb.Append("$sp");
                        isEscaped = true;
                    break;
                    case '?':
                        sb.Append("$qu");
                        isEscaped = true;
                    break;
                    case '!':
                        sb.Append("$ex");
                        isEscaped = true;
                    break;
                    case '*':
                        sb.Append("$as");
                        isEscaped = true;
                    break;
                    case '&':
                        sb.Append("$am");
                        isEscaped = true;
                    break;
                    case ',':
                        sb.Append("$cm");
                        isEscaped = true;
                    break;
                    case '|':
                        sb.Append("$vb");
                        isEscaped = true;
                    break;
                    case '\'':
                        sb.Append("$q");
                        isEscaped = true;
                    break;
                    default:
                        if ((ch <= 32) || (ch >= 127)) {
                            sb.AppendFormat("${0:x}", (int)ch);
                            isEscaped = true;
                        } else
                            sb.Append(ch);
                    break;
                }
            }

            if (isEscaped)
                result = sb.ToString();

            bool isReservedWord = ReservedWords.Contains(result);

            if (isReservedWord)
                result = "$" + result;

            return result;
        }

        public static string EscapeCharacter (char character, bool forJson) {
            switch (character) {
                case '\0':
                    return @"\0";
                case '\'':
                    return @"\'";
                case '\\':
                    return @"\\";
                case '"':
                    return "\\\"";
                case '\t':
                    return @"\t";
                case '\r':
                    return @"\r";
                case '\n':
                    return @"\n";
                default: {
                    if (forJson || (character > 255))
                        return String.Format(@"\u{0:x4}", (int)character);
                    else
                        return String.Format(@"\x{0:x2}", (int)character);
                }
            }
        }

        public static string EscapeString (string text, char quoteCharacter = '\"', bool forJson = false) {
            if (text == null)
                return "null";

            var sb = EscapeStringBuilder.Value;

            sb.Clear();
            sb.Append(quoteCharacter);

            foreach (var ch in text) {
                if (ch == quoteCharacter)
                    sb.Append(EscapeCharacter(ch, forJson));
                else if (ch == '\\')
                    sb.Append(@"\\");
                else if ((ch < ' ') || (ch > 127))
                    sb.Append(EscapeCharacter(ch, forJson));
                else
                    sb.Append(ch);
            }

            sb.Append(quoteCharacter);

            return sb.ToString();
        }

        public static string DemangleCecilTypeName (string typeName) {
            return typeName.Replace("/", "+");
        }

        public sealed class ListSkipAdapter<T> : IList<T> {
            public readonly IList<T> List;
            public readonly int Offset;

            public ListSkipAdapter (IList<T> list, int offset) {
                List = list;
                Offset = offset;
            }

            public int IndexOf (T item) {
                throw new NotImplementedException("ListSkipAdapter.IndexOf not implemented");
            }

            public void Insert (int index, T item) {
                List.Insert(index + Offset, item);
            }

            public void RemoveAt (int index) {
                List.RemoveAt(index + Offset);
            }

            public T this[int index] {
                get {
                    return List[index + Offset];
                }
                set {
                    List[index + Offset] = value;
                }
            }

            public void Add (T item) {
                List.Add(item);
            }

            public void Clear () {
                throw new NotImplementedException("ListSkipAdapter.Clear not implemented");
            }

            public bool Contains (T item) {
                throw new NotImplementedException("ListSkipAdapter.Contains not implemented");
            }

            public void CopyTo (T[] array, int arrayIndex) {
                for (int i = 0, c = Count; i < c; i++)
                    array[i + arrayIndex] = List[i + Offset];
            }

            public int Count {
                get { return List.Count - Offset; }
            }

            public bool IsReadOnly {
                get { return List.IsReadOnly; }
            }

            public bool Remove (T item) {
                throw new NotImplementedException("ListSkipAdapter.Remove not implemented");
            }

            public IEnumerator<T> GetEnumerator () {
                return (List as IEnumerable<T>).Skip(Offset).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
                return (List as IEnumerable<T>).Skip(Offset).GetEnumerator();
            }
        }

        public static IList<T> Skip<T> (this IList<T> list, int offset) {
            return new ListSkipAdapter<T>(list, offset);
        }

        public static string Indent (object inner) {
            if (inner == null)
                return "";

            var text = inner.ToString();

            return String.Join(
                Environment.NewLine,
                (from l in text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                 select "    " + l).ToArray()
            );
        }
    }

    public class ConcurrentHashQueue<TValue> {
        protected readonly ConcurrentDictionary<TValue, int> Counts;
        protected readonly ConcurrentQueue<TValue> Queue;

        public ConcurrentHashQueue (IEqualityComparer<TValue> comparer) {
            Queue = new ConcurrentQueue<TValue>();
            Counts = new ConcurrentDictionary<TValue, int>(comparer);
        }

        public ConcurrentHashQueue (int concurrencyLevel, int capacity, IEqualityComparer<TValue> comparer) {
            Queue = new ConcurrentQueue<TValue>();
            Counts = new ConcurrentDictionary<TValue, int>(concurrencyLevel, capacity, comparer);
        }

        public void Clear () {
            Counts.Clear();

            TValue temp;
            while (Queue.Count > 0)
                Queue.TryDequeue(out temp);
        }

        public bool TryEnqueue (TValue value) {
            if (Counts.TryAdd(value, 1)) {
                Queue.Enqueue(value);
                return true;
            } else {
                int existingCount;
                int tryCount = 10;

                while (Counts.TryGetValue(value, out existingCount)) {
                    var newCount = existingCount + 1;

                    if (Counts.TryUpdate(value, newCount, existingCount))
                        return true;

                    // Abort after a few tries.
                    if ((tryCount--) <= 0)
                        return false;
                }
            }

            return false;
        }

        public bool TryDequeue (out TValue value) {
            if (Queue.TryDequeue(out value)) {
                int existingCount;
                int tryCount = 10;

                while (Counts.TryGetValue(value, out existingCount)) {
                    int newCount = existingCount - 1;

                    if (newCount <= 0) {
                        if (Counts.TryRemove(value, out existingCount))
                            return true;
                    } else {
                        if (Counts.TryUpdate(value, existingCount, newCount))
                            return true;
                    }

                    // Abort after a few tries.
                    if ((tryCount--) <= 0)
                        return false;
                }
            }

            return false;
        }

        public int Count {
            get {
                return Queue.Count;
            }
        }

        public IEnumerable<TValue> TryDequeueAll {
            get {
                TValue value;

                while (TryDequeue(out value))
                    yield return value;
            }
        }
    }

    public class ConcurrentCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IDisposable {
        public delegate TValue CreatorFunction (TKey key);
        public delegate TValue CreatorFunction<in TUserData> (TKey key, TUserData userData);

        protected class ConstructionState : IDisposable {
            private volatile bool IsDisposed;
            private int WaiterCount = 0, DisposePending = 0;
            private readonly ManualResetEventSlim Signal = new ManualResetEventSlim(false);

            public readonly Thread ConstructingThread = Thread.CurrentThread;

            public bool Wait () {
                if (ConstructingThread == Thread.CurrentThread)
                    throw new InvalidOperationException("Recursive construction of cache entry");

                try {
                    Interlocked.Increment(ref WaiterCount);
                    if (IsDisposed)
                        return true;

                    Signal.Wait();
                    return true;
                } catch (ObjectDisposedException) {
                    return false;
                } finally {
                    var newCount = Interlocked.Decrement(ref WaiterCount);

                    if (newCount <= 0) {

                        if (Interlocked.CompareExchange(ref DisposePending, 0, 1) == 1) {
                            IsDisposed = true;
                            Thread.MemoryBarrier();
                            Signal.Dispose();
                        }
                    }
                }
            }

            public void Set () {
                try {
                    if (!IsDisposed)
                        Signal.Set();
                } catch (ObjectDisposedException) {
                    // Threading is hard and I'm lazy.
                }
            }

            public void Dispose () {
                if (Interlocked.Exchange(ref DisposePending, 1) == 0) {
                    if (WaiterCount <= 0) {
                        DisposePending = 0;
                        IsDisposed = true;
                        Thread.MemoryBarrier();
                        Signal.Dispose();
                    }
                }
            }
        }

        protected readonly ConcurrentDictionary<TKey, TValue> Storage;
        protected readonly ConcurrentDictionary<TKey, ConstructionState> States;
        protected readonly IEqualityComparer<TKey> Comparer;

        public ConcurrentCache () {
            Comparer = EqualityComparer<TKey>.Default;
            Storage = new ConcurrentDictionary<TKey, TValue>();
            States = new ConcurrentDictionary<TKey, ConstructionState>();
        }

        public ConcurrentCache (IEqualityComparer<TKey> comparer) {
            Comparer = comparer;
            Storage = new ConcurrentDictionary<TKey, TValue>(comparer);
            States = new ConcurrentDictionary<TKey, ConstructionState>(comparer);
        }

        public ConcurrentCache (int concurrencyLevel, int capacity) {
            Comparer = EqualityComparer<TKey>.Default;
            Storage = new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, capacity);
            States = new ConcurrentDictionary<TKey, ConstructionState>(concurrencyLevel, concurrencyLevel);
        }

        public ConcurrentCache (int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) {
            Comparer = comparer;
            Storage = new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, capacity, comparer);
            States = new ConcurrentDictionary<TKey, ConstructionState>(concurrencyLevel, concurrencyLevel, comparer);
        }

        protected ConcurrentCache (ConcurrentCache<TKey, TValue> cloneSource) {
            // FIXME: Probably not thread-safe?
            Storage = new ConcurrentDictionary<TKey, TValue>(cloneSource.Storage, cloneSource.Comparer);
            States = new ConcurrentDictionary<TKey, ConstructionState>(Environment.ProcessorCount, Environment.ProcessorCount, cloneSource.Comparer);
        }

        public ConcurrentCache<TKey, TValue> Clone () {
            return new ConcurrentCache<TKey, TValue>(this);
        }

        public int Count {
            get {
                return Storage.Count + States.Count;
            }
        }

        public virtual void Dispose () {
            Clear();
        }

        public void Clear () {
            Storage.Clear();

            foreach (var kvp in States)
                kvp.Value.Dispose();

            States.Clear();
        }

        public IEnumerable<TKey> Keys {
            get {
                return Storage.Keys;
            }
        }

        public bool MightContainKey (TKey key) {
            return Storage.ContainsKey(key) || States.ContainsKey(key);
        }

        public bool ContainsKey (TKey key) {
            return Storage.ContainsKey(key);
        }

        public bool TryGet (TKey key, out TValue result) {
            ConstructionState state;

            while (States.TryGetValue(key, out state)) {
                if (!state.Wait()) {
                    result = default(TValue);
                    return false;
                }
            }

            return Storage.TryGetValue(key, out result);
        }

        private bool TryCreateSetup (TKey key, out ConstructionState state) {
            if (Storage.ContainsKey(key)) {
                state = null;
                return false;
            }

            state = new ConstructionState();
            if (States.TryAdd(key, state)) {
                if (Storage.ContainsKey(key)) {
                    TryCreateTeardown(key, state);
                    return false;
                }

                return true;
            } else {
                state.Dispose();
                return false;
            }
        }

        private void TryCreateTeardown (TKey key, ConstructionState state) {
            if (States.TryRemove(key, out state)) {
                state.Set();
                state.Dispose();
            }
        }

        public bool TryCreate (TKey key, CreatorFunction creator) {
            ConstructionState state;
            if (TryCreateSetup(key, out state)) {
                try {
                    var result = creator(key);

                    if (!Storage.TryAdd(key, result))
                        throw new InvalidOperationException("Cache entry was created by someone else while construction lock was held");

                    return true;
                } finally {
                    TryCreateTeardown(key, state);
                }
            }

            return false;
        }

        public bool TryCreate<TUserData> (TKey key, TUserData userData, CreatorFunction<TUserData> creator, Predicate<TValue> shouldAdd = null) {
            ConstructionState state;

            if (TryCreateSetup(key, out state)) {
                try {
                    var result = creator(key, userData);

                    if ((shouldAdd == null) || shouldAdd(result)) {
                        if (!Storage.TryAdd(key, result))
                            throw new InvalidOperationException("Cache entry was created by someone else while construction lock was held");

                        return true;
                    } else {
                        return false;
                    }

                } finally {
                    TryCreateTeardown(key, state);
                }
            }

            return false;
        }

        private bool TryWaitForConstruction (TKey key) {
            ConstructionState state;

            bool waitFailed = false;
            while (States.TryGetValue(key, out state))
                waitFailed = !state.Wait();

            return waitFailed;
        }

        public TValue GetOrCreate (TKey key, CreatorFunction creator) {
            while (true) {
                bool waitFailed = TryWaitForConstruction(key);

                TValue result;
                if (Storage.TryGetValue(key, out result))
                    return result;
                else if (waitFailed)
                    throw new ObjectDisposedException("Cache", "The cache was cleared or disposed.");

                TryCreate(key, creator);
            }
        }

        public TValue GetOrCreate<TUserData> (TKey key, TUserData userData, CreatorFunction<TUserData> creator) {
            bool createSuccess = false;

            while (true) {
                bool waitFailed = TryWaitForConstruction(key);

                TValue result;
                if (Storage.TryGetValue(key, out result))
                    return result;
                else if (waitFailed)
                    throw new ObjectDisposedException("Cache", "The cache was cleared or disposed.");
                else if (createSuccess)
                    throw new ThreadStateException("Failed to retrieve cache element after creating it");

                createSuccess |= TryCreate(key, userData, creator);
            }
        }

        public bool TryRemove (TKey key) {
            ConstructionState state;

            while (States.TryGetValue(key, out state)) {
                if (!state.Wait())
                    return false;
            }

            TValue temp;
            return Storage.TryRemove(key, out temp);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator () {
            return Storage.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return Storage.GetEnumerator();
        }
    }

    public class ReferenceComparer<T> : IEqualityComparer<T>
        where T : class {

        public bool Equals (T x, T y) {
            return x == y;
        }

        public int GetHashCode (T obj) {
            return obj.GetHashCode();
        }
    }

    public class TemporaryVariable {
        public static JSRawOutputIdentifier ForFunction (
            JSFunctionExpression function, TypeReference type
        ) {
            return new JSRawOutputIdentifier(
                type,
                "$temp{0:X2}", function.TemporaryVariableCount++
            );
        }
    }

    public struct HashedString {
        public readonly int HashCode;
        public readonly string String;

        public HashedString (string str) {
            String = str;
            HashCode = str.GetHashCode();
        }

        public HashedString (string str, int hashCode) {
            String = str;
            HashCode = hashCode;
        }
    }

    public class HashedStringComparer : IEqualityComparer<HashedString> {
        public bool Equals (HashedString x, HashedString y) {
            return String.Equals(x.String, y.String, StringComparison.Ordinal);
        }

        public int GetHashCode (HashedString obj) {
            return obj.HashCode;
        }
    }

    public static class ImmutableArrayPool<T> {
        private class State {
            public readonly T[] Buffer;
            public int ElementsUsed;

            public State (int capacity) {
                Buffer = new T[capacity];

                ElementsUsed = 0;
            }
        }

        // The large object heap threshold is roughly 85KB so we set our block size small.
        //  this ensures that our blocks start in gen0 and can get collected early, instead
        //  of spending their entire life on the large object heap.
        // This also reduces waste in cases where some but not all of the buffers expire.
        public const int MaxSizeBytes = 1 * 1024;

        public static readonly int Capacity;
        public static readonly ArraySegment<T> Empty = new ArraySegment<T>(new T[0]);

        private readonly static ThreadLocal<State> ThreadLocal = new ThreadLocal<State>();

        static ImmutableArrayPool () {
            // Assume heap reference
            int itemSize = Environment.Is64BitProcess 
                ? 8 
                : 4;

            try {
                // If it's a blittable type, estimate its in-memory size
                if (!typeof(T).IsClass)
                    itemSize = Marshal.SizeOf(typeof(T));
            } catch {
                // Non-blittable struct. Make a rough estimate of size (conservative) so we try to stay below LOH threshold.
                itemSize = 32;
            }

            Capacity = MaxSizeBytes / itemSize;
        }

        public static ArraySegment<T> Allocate (int count) {
            if (count == 0)
                return Empty;

            if (count > Capacity)
                return new ArraySegment<T>(new T[count], 0, count);

            var data = ThreadLocal.Value;

            bool usedUpElements = false;
            bool allocateNew = (data == null) ||
                (usedUpElements = (data.ElementsUsed >= Capacity - count));

            if (allocateNew) {
                data = ThreadLocal.Value = new State(Capacity);
                usedUpElements = false;
            }

            if (usedUpElements)
                data.ElementsUsed = 0;

            var offset = data.ElementsUsed;
            data.ElementsUsed += count;

            return new ArraySegment<T>(data.Buffer, offset, count);
        }

        public static ArraySegment<T> Elements (T a) {
            var result = Allocate(1);
            result.Array[result.Offset + 0] = a;
            return result;
        }

        public static ArraySegment<T> Elements (T a, T b) {
            var result = Allocate(2);
            result.Array[result.Offset + 0] = a;
            result.Array[result.Offset + 1] = b;
            return result;
        }

        public static ArraySegment<T> Elements (T a, T b, T c) {
            var result = Allocate(3);
            result.Array[result.Offset + 0] = a;
            result.Array[result.Offset + 1] = b;
            result.Array[result.Offset + 2] = c;
            return result;
        }

        public static ArraySegment<T> Elements (T a, T b, T c, T d) {
            var result = Allocate(4);
            result.Array[result.Offset + 0] = a;
            result.Array[result.Offset + 1] = b;
            result.Array[result.Offset + 2] = c;
            result.Array[result.Offset + 3] = d;
            return result;
        }
    }

    public static class ImmutableArrayPoolExtensions {
#if TARGETTING_FX_4_5
        public static ArraySegment<T> ToEnumerable<T> (this ArraySegment<T> aseg) {
            return aseg;
        }
#else
        public struct ArraySegmentEnumerable<T> : IEnumerable<T> {
            public struct Enumerator : IEnumerator<T> {
                public readonly ArraySegment<T> ArraySegment;
                private int Index;

                public Enumerator (ArraySegment<T> aseg) {
                    ArraySegment = aseg;
                    Index = -1;
                }

                public bool MoveNext () {
                    Index += 1;
                    return (Index < ArraySegment.Count);
                }

                public T Current {
                    get {
                        return ArraySegment.Array[ArraySegment.Offset + Index];
                    }
                }

                public void Reset () {
                    Index = -1;
                }

                public void Dispose () {
                }

                object System.Collections.IEnumerator.Current {
                    get { 
                        return Current; 
                    }
                }
            }

            public readonly ArraySegment<T> ArraySegment;

            public ArraySegmentEnumerable (ArraySegment<T> aseg) {
                ArraySegment = aseg;
            }

            public Enumerator GetEnumerator () {
                return new Enumerator(ArraySegment);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator () {
                return new Enumerator(ArraySegment);
            }

            IEnumerator IEnumerable.GetEnumerator () {
                return new Enumerator(ArraySegment);
            }
        }

        public static ArraySegmentEnumerable<T> ToEnumerable<T> (this ArraySegment<T> aseg) {
            return new ArraySegmentEnumerable<T>(aseg);
        }
#endif

        public static ArraySegment<T> ToImmutableArray<T> (this IEnumerable<T> enumerable) {
            var collection = enumerable as ICollection<T>;
#if TARGETTING_FX_4_5
            var readOnlyCollection = enumerable as IReadOnlyCollection<T>;
#endif
            var array = enumerable as T[];

            if (collection != null) {
                var count = collection.Count;
                var buffer = ImmutableArrayPool<T>.Allocate(count);
                collection.CopyTo(buffer.Array, buffer.Offset);
                return buffer;
#if TARGETTING_FX_4_5
            } else if (readOnlyCollection != null) {
                return ToImmutableArray(enumerable, readOnlyCollection.Count);
#endif
            } else if (array != null) {
                return new ArraySegment<T>(array);
            } else {
                // Slow path =[
                array = enumerable.ToArray();
                return new ArraySegment<T>(array);
            }
        }

        public static ArraySegment<T> ToImmutableArray<T> (this IEnumerable<T> enumerable, int maximumCount) {
            int count = maximumCount;
            var buffer = ImmutableArrayPool<T>.Allocate(maximumCount);

            using (var e = enumerable.GetEnumerator()) {
                for (var i = 0; i < count; i++) {
                    if (!e.MoveNext()) {
                        count = i;
                        break;
                    }

                    buffer.Array[i + buffer.Offset] = e.Current;
                }

                if (e.MoveNext())
                    throw new ArgumentException("Enumerable was longer", "maximumCount");
            }

            if (buffer.Array == null)
                return ImmutableArrayPool<T>.Empty;
            else
                return new ArraySegment<T>(buffer.Array, buffer.Offset, count);
        }
    }
}
