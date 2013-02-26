using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Transforms;
using Mono.Cecil;

namespace JSIL {
    public interface IFunctionSource {
        JSFunctionExpression GetExpression (QualifiedMemberIdentifier method);
        FunctionAnalysis1stPass GetFirstPass (QualifiedMemberIdentifier method, QualifiedMemberIdentifier forMethod, bool suspendIfNotReady);
        FunctionAnalysis2ndPass GetSecondPass (JSMethod method, QualifiedMemberIdentifier forMethod, bool suspendIfNotReady);
        void InvalidateFirstPass (QualifiedMemberIdentifier method);
        void InvalidateSecondPass (QualifiedMemberIdentifier method);
    }

    public class FunctionCache : IFunctionSource, IDisposable {
        public class Entry {
            public QualifiedMemberIdentifier Identifier;
            public MethodInfo Info;
            public MethodReference Reference;

            public SpecialIdentifiers SpecialIdentifiers;
            public HashSet<string> ParameterNames;
            public Dictionary<string, JSVariable> Variables;

            public JSFunctionExpression Expression;
            public FunctionAnalysis1stPass FirstPass;
            public FunctionAnalysis2ndPass SecondPass;

            public object[] PassLocks = new [] { new object(), new object() };

            public int IsLockedForTransformPipeline = 0;
            public bool TransformPipelineHasCompleted = false;
            public Thread[] InProgressThreads = new Thread[2];

            public MethodDefinition Definition {
                get {
                    return Info.Member;
                }
            }

            internal void LogPassState (int passLevel, string state) {
                var thisThread = Thread.CurrentThread;
                Console.WriteLine(
                    "{0}: {1} {2} pass {3}", 
                    thisThread.ManagedThreadId, state,
                    this.Identifier.Member.Name, passLevel
                );
            }

            public bool RunLocked<T> (int passLevel, out T result, Func<Entry, T> callback)
                where T : class {
                var thisThread = Thread.CurrentThread;

                result = default(T);

                var theLock = PassLocks[passLevel];

                while (true) {
                    var previousThread = Interlocked.CompareExchange(ref InProgressThreads[passLevel], thisThread, null);

                    if (previousThread == thisThread) {
                        // Avert deadlock.
                        // LogPassState(passLevel, "exiting due to deadlock");
                        return false;
                    } else if (previousThread != null) {
                        if (!Monitor.TryEnter(theLock, 2000)) {
                            // Possible deadlock?
                            Console.WriteLine("Wait for '{0}' timed out after 2 seconds", Identifier);
                        } else {
                            Monitor.Exit(theLock);
                        }
                    } else {
                        try {
                            lock (theLock)
                                result = callback(this);

                            return true;
                        } finally {
                            if (Interlocked.CompareExchange(ref InProgressThreads[passLevel], null, thisThread) != thisThread)
                                throw new ThreadStateException("InProgressThread changed while inside RunLocked");
                        }
                    }
                }
            }
        }

        protected struct PopulatedCacheEntryArgs {
            public MethodInfo Info;
            public MethodReference Method;
            public ILBlockTranslator Translator;
            public IEnumerable<JSVariable> Parameters;
            public JSBlockStatement Body;
        }

        protected struct NullCacheEntryArgs {
            public MethodInfo Info;
            public MethodReference Method;
        }

        public readonly ITypeInfoSource TypeInfo;
        public readonly MethodTypeFactory MethodTypes;
        public readonly ConcurrentHashQueue<QualifiedMemberIdentifier> PendingTransformsQueue;
        public readonly ConcurrentDictionary<QualifiedMemberIdentifier, FunctionTransformPipeline> ActiveTransformPipelines;
        protected readonly ConcurrentCache<QualifiedMemberIdentifier, Entry> Cache;
        protected readonly ConcurrentCache<QualifiedMemberIdentifier, Entry>.CreatorFunction<JSMethod> MakeCacheEntry;
        protected readonly ConcurrentCache<QualifiedMemberIdentifier, Entry>.CreatorFunction<PopulatedCacheEntryArgs> MakePopulatedCacheEntry;
        protected readonly ConcurrentCache<QualifiedMemberIdentifier, Entry>.CreatorFunction<NullCacheEntryArgs> MakeNullCacheEntry;
        protected readonly QualifiedMemberIdentifier.Comparer Comparer;

        protected readonly static ThreadLocal<Stack<QualifiedMemberIdentifier>> InFlightStack = new ThreadLocal<Stack<QualifiedMemberIdentifier>>(
            () => new Stack<QualifiedMemberIdentifier>(128)
        );

        public FunctionCache (ITypeInfoSource typeInfo) {
            TypeInfo = typeInfo;
            Comparer = new QualifiedMemberIdentifier.Comparer(typeInfo);

            Cache = new ConcurrentCache<QualifiedMemberIdentifier, Entry>(
                Environment.ProcessorCount, 4096, Comparer
            );
            PendingTransformsQueue = new ConcurrentHashQueue<QualifiedMemberIdentifier>(
                Math.Max(1, Environment.ProcessorCount / 4), 4096, Comparer
            );
            ActiveTransformPipelines = new ConcurrentDictionary<QualifiedMemberIdentifier, FunctionTransformPipeline>(
                Math.Max(1, Environment.ProcessorCount / 4), 128, Comparer
            );
            MethodTypes = new MethodTypeFactory();

            MakeCacheEntry = (id, method) => {
                PendingTransformsQueue.TryEnqueue(id);

                return new Entry {
                    Info = method.Method,
                    Reference = method.Reference,
                    Identifier = id,
                    ParameterNames = new HashSet<string>(from p in method.Method.Parameters select p.Name),
                    SecondPass = new FunctionAnalysis2ndPass(this, method.Method)
                };
            };

            MakePopulatedCacheEntry = (id, args) => {
                var result = new JSFunctionExpression(
                    new JSMethod(args.Method, args.Info, MethodTypes),
                    args.Translator.Variables,
                    args.Parameters,
                    args.Body,
                    MethodTypes
                );

                PendingTransformsQueue.TryEnqueue(id);

                return new Entry {
                    Identifier = id,
                    Info = args.Info,
                    Reference = args.Method,
                    Expression = result,
                    Variables = args.Translator.Variables,
                    ParameterNames = args.Translator.ParameterNames,
                    SpecialIdentifiers = args.Translator.SpecialIdentifiers
                };
            };

            MakeNullCacheEntry = (id, args) => {
                return new Entry {
                    Identifier = id,
                    Info = args.Info,
                    Reference = args.Method,
                    Expression = null
                };
            };
        }

        public bool TryGetExpression (QualifiedMemberIdentifier method, out JSFunctionExpression function) {
            Entry entry;
            if (!Cache.TryGet(method, out entry)) {
                function = null;
                return false;
            }

            function = entry.Expression;
            return true;
        }

        public Entry GetCacheEntry (QualifiedMemberIdentifier method, bool throwOnFail = true) {
            Entry entry;
            if (!Cache.TryGet(method, out entry)) {
                if (throwOnFail)
                    throw new KeyNotFoundException("No cache entry for method '" + method + "'.");
                else
                    return null;
            }

            return entry;
        }

        public JSFunctionExpression GetExpression (QualifiedMemberIdentifier method) {
            var entry = GetCacheEntry(method);
            return entry.Expression;
        }

        private void ThrowIfStaticAnalysisDataIsNotReadyYet (QualifiedMemberIdentifier method, ref QualifiedMemberIdentifier forMethod) {
            if (method.Equals(forMethod, TypeInfo))
                return;

            var entry = GetCacheEntry(method, false);

            if (entry != null) {
                if (entry.IsLockedForTransformPipeline != 0)
                    throw new StaticAnalysisDataTemporarilyUnavailableException(method);

                if (!Monitor.TryEnter(entry)) {
                    var currentThreadId = Thread.CurrentThread.ManagedThreadId;
                } else {
                    Monitor.Exit(entry);
                }
            }
        }

        private FunctionAnalysis1stPass _GetOrCreateFirstPass (Entry entry) {
            if (entry.FirstPass == null) {
                // entry.LogPassState(1, "entering static analysis");
                var analyzer = new StaticAnalyzer(entry.Definition.Module.TypeSystem, this);
                entry.FirstPass = analyzer.FirstPass(entry.Identifier, entry.Expression);
                // entry.LogPassState(1, "exiting static analysis");
            }

            return entry.FirstPass;
        }

        public FunctionAnalysis1stPass GetFirstPass (QualifiedMemberIdentifier method, QualifiedMemberIdentifier forMethod, bool suspendIfNotReady) {
            var entry = GetCacheEntry(method, false);

            if ((entry == null) || (entry.Expression == null))
                return null;

            if (suspendIfNotReady)
                ThrowIfStaticAnalysisDataIsNotReadyYet(method, ref forMethod);

            FunctionAnalysis1stPass result;
            var runLockedResult = entry.RunLocked(
                0, out result, 
                _GetOrCreateFirstPass
            );

            if (runLockedResult)
                return result;
            else
                return null;
        }

        private FunctionAnalysis2ndPass _GetOrCreateSecondPass (Entry entry) {
            if (entry.FirstPass == null)
                return entry.SecondPass;

            if ((entry.SecondPass == null) && (entry.Expression != null)) {
                // entry.LogPassState(2, "entering static analysis");

                // Detect same-thread static analysis recursion, and abort.
                var ifs = InFlightStack.Value;
                if (ifs.Contains(entry.Identifier, Comparer)) {
                    /*
                    Console.WriteLine("Same-thread recursion for '{0}':", entry.Identifier.Member);
                    foreach (var item in ifs)
                        Console.WriteLine("  {0}", item.Member);
                     */

                    return null;
                }

                ifs.Push(entry.Identifier);
                entry.SecondPass = new FunctionAnalysis2ndPass(this, entry.FirstPass);
                ifs.Pop();
                // entry.LogPassState(2, "exiting static analysis");
            }

            return entry.SecondPass;
        }

        public FunctionAnalysis2ndPass GetSecondPass (JSMethod method, QualifiedMemberIdentifier forMethod, bool suspendIfNotReady) {
            var id = method.QualifiedIdentifier;
            Entry entry = Cache.GetOrCreate(
                id, method, MakeCacheEntry
            );

            if (entry == null)
                return null;

            if (suspendIfNotReady)
                ThrowIfStaticAnalysisDataIsNotReadyYet(id, ref forMethod);

            GetFirstPass(id, forMethod, suspendIfNotReady);

            FunctionAnalysis2ndPass result;
            var runLockedResult = entry.RunLocked(
                1, out result, 
                _GetOrCreateSecondPass
            );

            if (runLockedResult)
                return result;
            else
                return null;
        }

        private object _InvalidateFirstPass (Entry entry) {
            if (entry.Expression == null)
                throw new InvalidOperationException("Attempted to invalidate a function that had no expression");

            // entry.LogPassState(1, "invalidated");
            entry.FirstPass = null;
            return entry.SecondPass = null;
        }

        public void InvalidateFirstPass (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGet(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            object temp;
            entry.RunLocked(0, out temp, _InvalidateFirstPass);
        }

        private object _InvalidateSecondPass (Entry entry) {
            if (entry.Expression == null)
                throw new InvalidOperationException("Attempted to invalidate a function that had no expression");

            // entry.LogPassState(2, "invalidated");
            return entry.SecondPass = null;
        }

        public void InvalidateSecondPass (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGet(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            object temp;
            entry.RunLocked(1, out temp, _InvalidateSecondPass);
        }

        internal JSFunctionExpression Create (
            MethodInfo info, MethodDefinition methodDef, MethodReference method, 
            QualifiedMemberIdentifier identifier, ILBlockTranslator translator, 
            IEnumerable<JSVariable> parameters, JSBlockStatement body
        ) {
            var args = new PopulatedCacheEntryArgs {
                Info = info,
                Method = method,
                Translator = translator,
                Parameters = parameters,
                Body = body,
            };

            return Cache.GetOrCreate(identifier, args, MakePopulatedCacheEntry).Expression;
        }

        internal void CreateNull (
            MethodInfo info, MethodReference method, 
            QualifiedMemberIdentifier identifier
        ) {
            var args = new NullCacheEntryArgs {
                Info = info,
                Method = method
            };

            Cache.TryCreate(identifier, args, MakeNullCacheEntry);
        }

        public void Dispose () {
            Cache.Dispose();
            PendingTransformsQueue.Clear();
            MethodTypes.Dispose();
        }

        public bool LockCacheEntryForTransformPipeline (QualifiedMemberIdentifier method) {
            var entry = GetCacheEntry(method);

            return Interlocked.CompareExchange(ref entry.IsLockedForTransformPipeline, 1, 0) == 0;
        }

        public bool UnlockCacheEntryForTransformPipeline (QualifiedMemberIdentifier method, bool completed) {
            var entry = GetCacheEntry(method);

            entry.TransformPipelineHasCompleted |= completed;

            return Interlocked.CompareExchange(ref entry.IsLockedForTransformPipeline, 0, 1) == 1;
        }
    }

    public abstract class TemporarilySuspendTransformPipelineException : Exception {
        public readonly QualifiedMemberIdentifier Identifier;

        public TemporarilySuspendTransformPipelineException (QualifiedMemberIdentifier identifier) {
            Identifier = identifier;
        }
    }

    public class StaticAnalysisDataTemporarilyUnavailableException : TemporarilySuspendTransformPipelineException {
        public StaticAnalysisDataTemporarilyUnavailableException (QualifiedMemberIdentifier identifier)
            : base (identifier) {
        }

        public override string Message {
            get {
                return String.Format("Static analysis data for the function '{0}' is temporarily unavailable because the function is being transformed. Please re-run this transform later.", Identifier);
            }
        }
    }
}
