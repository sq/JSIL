#pragma warning disable 0420

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using JSIL.Internal;

namespace JSIL.Threading {
    public class TrackedLockCollection : IDisposable {
        public class DeadlockInfo {
            public readonly TrackedLock A, B;

            public DeadlockInfo (TrackedLock a, TrackedLock b) {
                A = a;
                B = b;
            }

            public override string ToString () {
                return String.Format("[{0}, {1}]", A, B);
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

            internal bool IsSignaled {
                get {
                    return Signal.IsSet;
                }
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

        protected readonly ConcurrentDictionary<TrackedLock, bool> Locks = new ConcurrentDictionary<TrackedLock, bool>();
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

        internal void Track (TrackedLock lck) {
            if (!Locks.TryAdd(lck, false))
                throw new ThreadStateException();
        }

        internal void Untrack (TrackedLock lck) {
            bool temp;

            if (!Locks.TryRemove(lck, out temp))
                throw new ThreadStateException();

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
                    return new DeadlockInfo(waitToExamine.Lock, nextWaitToExamine.Lock);
                waitToExamine = nextWaitToExamine;
            }

            return null;
        }

        internal bool TryCreateWait (TrackedLock lck, out DeadlockInfo deadlock, out Wait wait) {
            var currentThread = Thread.CurrentThread;
            var waits = Waits.GetOrCreate(lck, MakeWaitList);
            wait = new Wait(this, lck, currentThread);

            lock (waits)
                waits.Enqueue(wait, false);

            deadlock = DetectDeadlock(wait);
            if (deadlock != null) {
                lock (waits)
                    waits.Remove(wait);

                var wasSignaled = wait.IsSignaled;

                wait.Dispose();
                wait = null;

                // It's possible, albeit incredibly unlikely, for someone to call Wake on our wait
                //  before we do the deadlock check.
                // If this happens, try to dequeue another wait from the queue and wake it up in our stead.
                // This can probably still break, though...
                if (wasSignaled) {
                    if (TryDequeueOneWait(lck, out wait))
                        wait.Wake();
                }

                return false;
            }

            return true;
        }

        internal bool TryDequeueOneWait (TrackedLock lck, out Wait wait) {
            wait = null;

            OrderedDictionary<Wait, bool> waits;
            if (!Waits.TryGet(lck, out waits))
                return false;

            bool temp;
            lock (waits)
                return waits.TryDequeueFirst(out wait, out temp);
        }

        public void Dispose () {
            while (Locks.Count > 0) {
                foreach (var lck in Locks.Keys.ToList())
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
        public readonly Func<string> GetName;

        private volatile int _RecursionCount = 0;
        private volatile Thread _HeldBy = null;

        public TrackedLock (TrackedLockCollection collection, string name)
            : this(collection, () => name) {
        }

        public TrackedLock (TrackedLockCollection collection, Func<string> getName = null) {
            Collection = collection;
            GetName = getName;

            collection.Track(this);
        }

        public void Dispose () {
            if (IsDisposed)
                return;

            Collection.Untrack(this);

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

        public int RecursionDepth {
            get {
                return _RecursionCount;
            }
        }

        public int WaitingThreadCount {
            get {
                return Collection.GetWaitingThreadCount(this);
            }
        }

        public TrackedLockResult TryEnter (out Thread previousOwner, bool recursive = false) {
            var currentThread = Thread.CurrentThread;
            previousOwner = Interlocked.CompareExchange(ref _HeldBy, currentThread, null);
            var acquired = previousOwner == null;

            TrackedLockFailureReason? failureReason = null;

            if (!acquired) {
                failureReason = (previousOwner == currentThread)
                    ? TrackedLockFailureReason.HeldByCurrentThread
                    : TrackedLockFailureReason.HeldByOtherThread;
            }

            if (recursive && (failureReason == TrackedLockFailureReason.HeldByCurrentThread)) {
                _RecursionCount += 1;
                return new TrackedLockResult();
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

        public TrackedLockResult TryEnter (bool recursive = false) {
            Thread temp;
            return TryEnter(out temp, recursive);
        }

        public TrackedLockResult TryBlockingEnter (bool recursive = false) {
            TrackedLockCollection.DeadlockInfo temp;
            return TryBlockingEnter(out temp, recursive);
        }

        public TrackedLockResult TryBlockingEnter (out TrackedLockCollection.DeadlockInfo deadlock, bool recursive = false) {
            TrackedLockCollection.Wait wait;
            deadlock = null;

            while (true) {
                var result = TryEnter(recursive);
                if (result.FailureReason != TrackedLockFailureReason.HeldByOtherThread)
                    return result;

                if (Collection.TryCreateWait(this, out deadlock, out wait)) {
                    using (wait) {
                        result = TryEnter(recursive);

                        if (result.FailureReason == TrackedLockFailureReason.HeldByOtherThread) {
                            wait.Block();
                        } else {
                            return result;
                        }
                    }
                } else {
                    return new TrackedLockResult(TrackedLockFailureReason.Deadlock);
                }
            }
        }

        public void BlockingEnter (bool recursive = false) {
            TrackedLockCollection.DeadlockInfo deadlock;
            var result = TryBlockingEnter(out deadlock, recursive);

            if (!result.Success) {
                switch (result.FailureReason) {
                    case TrackedLockFailureReason.HeldByCurrentThread:
                        throw new LockAlreadyHeldException();
                    case TrackedLockFailureReason.Deadlock:
                        throw new DeadlockAvertedException(deadlock.A, deadlock.B);
                }
            }
        }

        public void Exit () {
            var currentThread = Thread.CurrentThread;
            bool released = false;

            if (_HeldBy == currentThread) {
                if (_RecursionCount == 0) {
                    TrackedLockCollection.Wait wait;
                    if (!Collection.TryDequeueOneWait(this, out wait))
                        wait = null;

                    var previousOwner = Interlocked.CompareExchange(ref _HeldBy, null, currentThread);
                    released = previousOwner == currentThread;

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
                } else {
                    _RecursionCount -= 1;
                    released = false;
                }
            } else {
                throw new InvalidOperationException("Lock held by other thread or not held");
            }
        }

        public override string ToString () {
            var hby = _HeldBy;
            return String.Format(
                "<TrackedLock '{0}' held by {1}>",
                (GetName != null)
                    ? GetName()
                    : String.Format("Unnamed {0}", GetHashCode()),
                hby != null
                    ? String.Format("thread #{0} '{1}'", hby.ManagedThreadId, hby.Name)
                    : "nobody"
            );
        }
    }

    public enum TrackedLockFailureReason {
        HeldByCurrentThread,
        HeldByOtherThread,
        Deadlock
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

        public static explicit operator bool (TrackedLockResult result) {
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
