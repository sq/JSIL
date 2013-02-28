using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using JSIL.Threading;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class ThreadingTests {
        private TrackedLockCollection Locks;

        protected T RunOnThread<T> (Func<T> func) {
            var result = new T[1];

            var thread = new Thread(
                () => { result[0] = func(); }
            );

            thread.Start();
            thread.Join();

            return result[0];
        }

        [SetUp]
        public void SetUp () {
            Locks = new TrackedLockCollection();
        }

        [TearDown]
        public void TearDown () {
            Locks.Dispose();
        }

        [Test]
        public void AcquireAndRelease () {
            var l = new TrackedLock(Locks, "A");

            Assert.IsTrue(l.TryEnter());
            Assert.IsTrue(l.IsHeld);
            l.Exit();
            Assert.IsFalse(l.IsHeld);
        }

        [Test]
        public void DisposeLock () {
            var l = new TrackedLock(Locks, "A");

            Assert.IsTrue(l.TryEnter());
            Assert.IsTrue(l.IsHeld);
            l.Dispose();
            Assert.IsFalse(l.IsHeld);
            Assert.IsTrue(l.IsDisposed);
        }

        [Test]
        public void DisposeLockCollection () {
            var l = new TrackedLock(Locks, "A");

            Assert.IsTrue(l.TryEnter());
            Assert.IsTrue(l.IsHeld);
            Locks.Dispose();
            Assert.IsFalse(l.IsHeld);
            Assert.IsTrue(l.IsDisposed);
        }

        [Test]
        public void HeldLockTracking () {
            var l = new TrackedLock(Locks, "A");

            Assert.IsTrue(l.TryEnter());

            Assert.AreEqual(Thread.CurrentThread, l.HeldBy);

            l.Exit();

            Assert.AreEqual(null, l.HeldBy);
        }

        [Test]
        public void AcquireOnSameThreadFails () {
            var l = new TrackedLock(Locks, "A");

            Assert.IsTrue(l.TryEnter());

            Thread previousOwner;
            Assert.IsFalse(l.TryEnter(out previousOwner));
            Assert.AreEqual(Thread.CurrentThread, previousOwner);
            Assert.AreEqual(l.TryEnter().FailureReason, TrackedLockFailureReason.HeldByCurrentThread);
        }

        [Test]
        public void AcquireOnOtherThreadFails () {
            var l = new TrackedLock(Locks, "A");

            Assert.IsTrue(l.TryEnter());

            Assert.AreEqual(Thread.CurrentThread, RunOnThread(
                () => {
                    Thread previousOwner;
                    if (l.TryEnter(out previousOwner))
                        return null;
                    else
                        return previousOwner;
                }
            ));
        }

        [Test]
        public void WaitBlocksUntilReleased () {
            var log = new List<string>();
            var l = new TrackedLock(Locks, "A");

            lock (log)
                log.Add("Main+TryEnter");
            Assert.IsTrue(l.TryEnter());
            lock (log)
                log.Add("Main-TryEnter");

            var threadStartedSignal = new AutoResetEvent(false);
            var waiterThread = new Thread(
                () => {
                    threadStartedSignal.Set();
                    lock (log)
                        log.Add("Waiter+Enter");
                    l.BlockingEnter();
                    lock (log)
                        log.Add("Waiter-Enter");
                    l.Exit();
                }
            );
            waiterThread.Priority = ThreadPriority.Highest;
            waiterThread.Name = "Waiter";
            waiterThread.Start();

            threadStartedSignal.WaitOne();

            // FIXME: There's a race here since WaitingThreadCount actually increases
            //  slightly before the thread begins waiting. Bleh.
            while (l.WaitingThreadCount == 0)
                Thread.Sleep(1);

            lock (log)
                log.Add("Main+Exit");
            l.Exit();
            lock (log)
                log.Add("Main-Exit");

            waiterThread.Join();

            Assert.Less(log.IndexOf("Waiter+Enter"), log.IndexOf("Main+Exit"));
            Assert.Greater(log.IndexOf("Waiter-Enter"), log.IndexOf("Main-Exit"));
        }

        [Test]
        public void WaitThrowsIfAlreadyHeld () {
            var l = new TrackedLock(Locks, "A");

            l.BlockingEnter();

            Assert.Throws<LockAlreadyHeldException>(l.BlockingEnter);

            l.Exit();
        }

        [Test]
        public void WaitThrowsIfSimpleDeadlockWouldHaveOccurred () {
            var lA = new TrackedLock(Locks, "A");
            var lB = new TrackedLock(Locks, "B");

            lA.BlockingEnter();

            var threadStartedSignal = new AutoResetEvent(false);
            var bOwnerThread = new Thread(
                () => {
                    lB.BlockingEnter();
                    threadStartedSignal.Set();
                    lA.BlockingEnter();
                }
            );
            bOwnerThread.Priority = ThreadPriority.Highest;
            bOwnerThread.Name = "B Owner";
            bOwnerThread.Start();

            threadStartedSignal.WaitOne();
            while (lA.WaitingThreadCount == 0)
                Thread.Sleep(1);

            Assert.Throws<DeadlockAvertedException>(lB.BlockingEnter);

            lA.Exit();

            bOwnerThread.Join();
        }

        [Test]
        public void WaitThrowsIfNestedDeadlockWouldHaveOccurred () {
            var lA = new TrackedLock(Locks, "A");
            var lB = new TrackedLock(Locks, "B");
            var lC = new TrackedLock(Locks, "C");

            lA.BlockingEnter();

            var cOwnerThread = new Thread(
                () => {
                    lC.BlockingEnter();
                    lA.BlockingEnter();
                    lC.Exit();
                    lA.Exit();
                }
            );
            cOwnerThread.Priority = ThreadPriority.Highest;
            cOwnerThread.Name = "C Owner";
            cOwnerThread.Start();

            var bThreadStartedSignal = new AutoResetEvent(false);
            var bOwnerThread = new Thread(
                () => {
                    lB.BlockingEnter();
                    bThreadStartedSignal.Set();
                    lC.BlockingEnter();
                    lB.Exit();
                    lC.Exit();
                }
            );
            bOwnerThread.Priority = ThreadPriority.Highest;
            bOwnerThread.Name = "B Owner";
            bOwnerThread.Start();

            bThreadStartedSignal.WaitOne();
            while (lA.WaitingThreadCount == 0)
                Thread.Sleep(1);

            Assert.Throws<DeadlockAvertedException>(lB.BlockingEnter);

            lA.Exit();

            bOwnerThread.Join();
            cOwnerThread.Join();
        }

        [Test]
        public void WaitDoesNotThrowTheSecondTimeForAGivenThread () {
            var l = new TrackedLock(Locks, "A");

            l.BlockingEnter();

            var exc = new Exception[1];

            var secondTimeSignal = new AutoResetEvent(false);
            var waiterThread = new Thread(
                () => {
                    try {
                        l.BlockingEnter();
                        l.Exit();

                        secondTimeSignal.WaitOne();

                        l.BlockingEnter();
                        l.Exit();
                    } catch (Exception e) {
                        exc[0] = e;
                    }
                }
            );
            waiterThread.Priority = ThreadPriority.Highest;
            waiterThread.Start();

            while (
                (l.WaitingThreadCount == 0) && 
                (waiterThread.ThreadState != ThreadState.Stopped) &&
                (waiterThread.ThreadState != ThreadState.Aborted)
            )
                Thread.Sleep(1);

            l.Exit();
            l.BlockingEnter();

            secondTimeSignal.Set();

            while (
                (l.WaitingThreadCount == 0) &&
                (waiterThread.ThreadState != ThreadState.Stopped) &&
                (waiterThread.ThreadState != ThreadState.Aborted)
            )
                Thread.Sleep(1);

            l.Exit();

            waiterThread.Join();

            if (exc[0] != null)
                throw new Exception("Worker thread failed", exc[0]);
        }
    }
}
