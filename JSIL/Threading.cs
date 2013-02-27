using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using JSIL.Internal;

namespace JSIL.Threading {
    public class ReferenceComparer<T> : IEqualityComparer<T>
        where T : class {

        public bool Equals (T x, T y) {
            return (x == y);
        }

        public int GetHashCode (T obj) {
            return obj.GetHashCode();
        }
    }

    public class TrackedLockCollection : IDisposable {
        public class DeadlockInfo {
            public readonly Wait A, B;

            public DeadlockInfo (Wait a, Wait b) {
                A = a;
                B = b;
            }
        }

        public class Wait : IDisposable {
            private readonly ManualResetEventSlim Signal = new ManualResetEventSlim(false);
            private readonly TrackedLockCollection Collection;

            public readonly TrackedLock Lock;
            public readonly Thread Thread;

            public Wait (TrackedLockCollection collection, TrackedLock lck, Thread thread) {
                Collection = collection;
                Lock = lck;
                Thread = thread;

                if (!Collection.WaitsByThread.TryAdd(thread, this))
                    throw new ThreadStateException();
            }

            public void Block () {
                Signal.Wait();
            }

            public void Wake () {
                Signal.Set();
            }

            public void Dispose () {
                Signal.Dispose();

                OrderedDictionary<Wait, bool> waits;

                if (!Collection.Waits.TryGet(Lock, out waits))
                    throw new ThreadStateException();

                lock (waits)
                    waits.Remove(this);

                Wait temp;
                if (!Collection.WaitsByThread.TryRemove(Thread, out temp))
                    throw new ThreadStateException();
            }
        }

        protected readonly ConcurrentDictionary<string, TrackedLock> Locks = new ConcurrentDictionary<string, TrackedLock>();
        protected readonly ConcurrentDictionary<Thread, Wait> WaitsByThread = new ConcurrentDictionary<Thread, Wait>(
            new Internal.ReferenceComparer<Thread>()
        );
        protected readonly ConcurrentCache<TrackedLock, OrderedDictionary<Wait, bool>> Waits = new ConcurrentCache<TrackedLock, OrderedDictionary<Wait, bool>>(
            new ReferenceComparer<TrackedLock>()
        );

        private static readonly ConcurrentCache<TrackedLock, OrderedDictionary<Wait, bool>>.CreatorFunction MakeWaitList;

        static TrackedLockCollection () {
            MakeWaitList = (lck) => new OrderedDictionary<Wait, bool>();
        }

        public void Track (string name, TrackedLock lck) {
            if (!Locks.TryAdd(name, lck))
                throw new InvalidOperationException("A lock with this name already exists");
        }

        public void Untrack (string name, TrackedLock lck) {
            TrackedLock temp;

            if (!Locks.TryRemove(name, out temp))
                throw new InvalidOperationException("A lock with this name does not exist");
            if (temp != lck)
                throw new ThreadStateException("The lock with this name does not match the lock passed in");

            OrderedDictionary<Wait, bool> waits;
            if (Waits.TryGet(lck, out waits)) {
                lock (waits) {
                    foreach (var w in waits)
                        w.Key.Dispose();

                    waits.Clear();
                }

                if (!Waits.TryRemove(lck))
                    throw new ThreadStateException();
            }
        }

        public DeadlockInfo DetectDeadlock (Wait wait) {
            var seenLocks = new HashSet<TrackedLock>(new Internal.ReferenceComparer<TrackedLock>());
            var waitToExamine = wait;

            while (waitToExamine != null) {
                seenLocks.Add(waitToExamine.Lock);

                var ownerThread = waitToExamine.Lock.HeldBy;
                if (ownerThread == null)
                    break;

                Wait nextWaitToExamine;
                if (!WaitsByThread.TryGetValue(ownerThread, out nextWaitToExamine))
                    break;

                if (seenLocks.Contains(nextWaitToExamine.Lock))
                    return new DeadlockInfo(waitToExamine, nextWaitToExamine);
                waitToExamine = nextWaitToExamine;
            }

            return null;
        }

        internal Wait CreateWait (TrackedLock lck) {
            var currentThread = Thread.CurrentThread;
            var waits = Waits.GetOrCreate(lck, MakeWaitList);
            var result = new Wait(this, lck, currentThread);

            lock (waits)
                waits.Enqueue(result, false);

            var deadlock = DetectDeadlock(result);
            if (deadlock != null) {
                lock (waits)
                    waits.Remove(result);

                throw new DeadlockAvertedException(deadlock.A.Lock, deadlock.B.Lock);
            }

            return result;
        }

        internal bool TryDequeueOneWait (TrackedLock lck, out Wait wait) {
            wait = null;

            var currentThread = Thread.CurrentThread;
            OrderedDictionary<Wait, bool> waits;
            if (!Waits.TryGet(lck, out waits))
                return false;

            bool temp;
            lock (waits)
                return waits.TryDequeueFirst(out wait, out temp);
        }

        public void Dispose () {
            while (Locks.Count > 0) {
                foreach (var lck in Locks.Values.ToArray())
                    lck.Dispose();
            }
        }

        internal int GetWaitingThreadCount (TrackedLock lck) {
            OrderedDictionary<Wait, bool> waits;
            if (!Waits.TryGet(lck, out waits))
                return 0;

            lock (waits)
                return waits.Count;
        }
    }

    public class TrackedLock : IDisposable {
        public readonly TrackedLockCollection Collection;
        public readonly string Name;

        private volatile Thread _HeldBy = null;

        public TrackedLock (TrackedLockCollection collection, string name) {
            if (name == null)
                throw new ArgumentNullException("name");

            Collection = collection;
            Name = name;

            collection.Track(name, this);
        }

        public void Dispose () {
            if (IsDisposed)
                return;

            Collection.Untrack(Name, this);

            // FIXME
            _HeldBy = null;
            IsDisposed = true;
        }

        public bool IsDisposed {
            get;
            private set;
        }

        public Thread HeldBy {
            get {
                return _HeldBy;
            }
        }

        public bool IsHeld {
            get {
                return _HeldBy != null;
            }
        }

        public int WaitingThreadCount {
            get {
                return Collection.GetWaitingThreadCount(this);
            }
        }

        public TrackedLockResult TryEnter (out Thread previousOwner) {
            var currentThread = Thread.CurrentThread;
            previousOwner = Interlocked.CompareExchange(ref _HeldBy, currentThread, null);
            var acquired = previousOwner == null;

            TrackedLockFailureReason? failureReason = null;

            if (!acquired) {
                failureReason = (previousOwner == currentThread)
                    ? TrackedLockFailureReason.HeldByCurrentThread
                    : TrackedLockFailureReason.HeldByOtherThread;
            }

            var result = new TrackedLockResult(failureReason);

#if LOCK_TRACING
            Console.WriteLine(
                "TryEnter {0} => {1}; result = {2}", 
                previousOwner != null ? previousOwner.Name : "<null>",
                _HeldBy != null ? _HeldBy.Name : "<null>", 
                result
            );
#endif

            return result;
        }

        public TrackedLockResult TryEnter () {
            Thread temp;
            return TryEnter(out temp);
        }

        public void Enter () {
            while (true) {
                var result = TryEnter();
                if (result.Success)
                    return;

                if (result.FailureReason == TrackedLockFailureReason.HeldByCurrentThread)
                    throw new LockAlreadyHeldException();

                using (var w = Collection.CreateWait(this)) {
                    result = TryEnter();

                    if (result.Success)
                        return;

                    switch (result.FailureReason) {
                        case TrackedLockFailureReason.HeldByCurrentThread:
                            throw new LockAlreadyHeldException();
                        case TrackedLockFailureReason.HeldByOtherThread:
                            w.Block();
                            break;
                    }
                }
            }
        }

        public void Exit () {
            TrackedLockCollection.Wait wait;
            if (!Collection.TryDequeueOneWait(this, out wait))
                wait = null;

            var currentThread = Thread.CurrentThread;
            var previousOwner = Interlocked.CompareExchange(ref _HeldBy, null, currentThread);
            var released = previousOwner == currentThread;

#if LOCK_TRACING
            Console.WriteLine(
                "Exit {0} => {1}; released = {2}",
                previousOwner != null ? previousOwner.Name : "<null>",
                _HeldBy != null ? _HeldBy.Name : "<null>",
                released
            );
#endif

            if (!released)
                throw new InvalidOperationException("Lock not held");

            if (wait != null)
                wait.Wake();
        }

        public override string ToString () {
            var hby = _HeldBy;
            return String.Format(
                "<TrackedLock '{0}' held by {1}>",
                Name,
                hby != null
                    ? String.Format("thread #{0} '{1}'", hby.ManagedThreadId, hby.Name)
                    : "nobody"
            );
        }
    }

    public enum TrackedLockFailureReason {
        HeldByCurrentThread,
        HeldByOtherThread
    }

    public struct TrackedLockResult {
        public readonly TrackedLockFailureReason? FailureReason;

        public TrackedLockResult (TrackedLockFailureReason? failureReason) {
            FailureReason = failureReason;
        }

        public bool Success {
            get {
                return !FailureReason.HasValue;
            }
        }

        public static implicit operator bool (TrackedLockResult result) {
            return result.Success;
        }

        public override string ToString () {
            if (FailureReason.HasValue)
                return FailureReason.Value.ToString();
            else
                return "Success";
        }
    }

    public class LockAlreadyHeldException : Exception {
        public LockAlreadyHeldException ()
            : base("The current thread already holds this lock.") {
        }
    }

    public class DeadlockAvertedException : Exception {
        public TrackedLock Lock1, Lock2;

        public DeadlockAvertedException (TrackedLock lock1, TrackedLock lock2)
            : base("The operation would have caused a deadlock.") {

            Lock1 = lock1;
            Lock2 = lock2;
        }
    }
}
