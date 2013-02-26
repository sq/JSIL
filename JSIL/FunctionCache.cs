using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        FunctionAnalysis1stPass GetFirstPass (QualifiedMemberIdentifier method);
        FunctionAnalysis2ndPass GetSecondPass (JSMethod method);
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

            public Thread InProgressThread = null;

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

            public bool RunLocked (int passLevel, Action callback) {
                var thisThread = Thread.CurrentThread;

                while (true) {
                    var previousThread = Interlocked.CompareExchange(ref InProgressThread, thisThread, null);

                    if (previousThread == thisThread) {
                        // Avert deadlock.
                        // LogPassState(passLevel, "exiting due to deadlock");
                        return false;
                    } else if (previousThread != null) {
                        lock (this)
                            ;
                    } else {
                        try {
                            callback();
                        } finally {
                            if (Interlocked.CompareExchange(ref InProgressThread, null, thisThread) != thisThread)
                                throw new ThreadStateException("InProgressThread changed while inside RunLocked");
                        }

                        break;
                    }
                }

                return true;
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

        public readonly MethodTypeFactory MethodTypes;
        public readonly ConcurrentHashQueue<QualifiedMemberIdentifier> PendingTransformsQueue;
        protected readonly ConcurrentCache<QualifiedMemberIdentifier, Entry> Cache;
        protected readonly ConcurrentCache<QualifiedMemberIdentifier, Entry>.CreatorFunction<JSMethod> MakeCacheEntry;
        protected readonly ConcurrentCache<QualifiedMemberIdentifier, Entry>.CreatorFunction<PopulatedCacheEntryArgs> MakePopulatedCacheEntry;
        protected readonly ConcurrentCache<QualifiedMemberIdentifier, Entry>.CreatorFunction<NullCacheEntryArgs> MakeNullCacheEntry; 

        public FunctionCache (ITypeInfoSource typeInfo) {
            var comparer = new QualifiedMemberIdentifier.Comparer(typeInfo);
            Cache = new ConcurrentCache<QualifiedMemberIdentifier, Entry>(
                Environment.ProcessorCount, 4096, comparer
            );
            PendingTransformsQueue = new ConcurrentHashQueue<QualifiedMemberIdentifier>(
                Math.Max(1, Environment.ProcessorCount / 4), 4096, comparer
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

        public FunctionAnalysis1stPass GetFirstPass (QualifiedMemberIdentifier method) {
            var entry = GetCacheEntry(method, false);

            if ((entry == null) || (entry.Expression == null))
                return null;

            if (!entry.RunLocked(1, () => {
                if (entry.FirstPass == null) {
                    // entry.LogPassState(1, "entering static analysis");
                    var analyzer = new StaticAnalyzer(entry.Definition.Module.TypeSystem, this);
                    entry.FirstPass = analyzer.FirstPass(entry.Expression);
                    // entry.LogPassState(1, "exiting static analysis");
                }
            }))
                return null;

            return entry.FirstPass;
        }

        public FunctionAnalysis2ndPass GetSecondPass (JSMethod method) {
            var id = method.QualifiedIdentifier;

            Entry entry = Cache.GetOrCreate(
                id, method, MakeCacheEntry
            );

            if (entry == null)
                return null;

            var firstPass = GetFirstPass(id);
            if (firstPass == null) {
                return entry.SecondPass;
            }

            if (!entry.RunLocked(2, () => {
                if ((entry.SecondPass == null) && (entry.Expression != null)) {
                    // entry.LogPassState(2, "entering static analysis");
                    entry.SecondPass = new FunctionAnalysis2ndPass(this, firstPass);
                    // entry.LogPassState(2, "exiting static analysis");
                }
            }))
                return null;

            return entry.SecondPass;
        }

        public void InvalidateFirstPass (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGet(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            if (!entry.RunLocked(1, () => {
                // entry.LogPassState(1, "invalidated");
                entry.FirstPass = null;
                entry.SecondPass = null;
            }))
                throw new ThreadStateException("Deadlock detected when invalidating first pass");
        }

        public void InvalidateSecondPass (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGet(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            if (!entry.RunLocked(2, () => {
                // entry.LogPassState(2, "invalidated");
                entry.SecondPass = null;
            }))
                throw new ThreadStateException("Deadlock detected when invalidating second pass");
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
    }
}
