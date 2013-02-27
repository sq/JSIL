using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Threading;
using JSIL.Transforms;
using Mono.Cecil;

namespace JSIL {
    public interface IFunctionSource {
        JSFunctionExpression GetExpression (QualifiedMemberIdentifier method);
        FunctionAnalysis1stPass GetFirstPass (QualifiedMemberIdentifier method, QualifiedMemberIdentifier forMethod);
        FunctionAnalysis2ndPass GetSecondPass (JSMethod method, QualifiedMemberIdentifier forMethod);
        void InvalidateFirstPass (QualifiedMemberIdentifier method);
        void InvalidateSecondPass (QualifiedMemberIdentifier method);
    }

    public class FunctionCache : IFunctionSource, IDisposable {
        public class Entry {
            public readonly TrackedLock StaticAnalysisDataLock, TransformPipelineLock;

            public readonly QualifiedMemberIdentifier Identifier;
            public MethodInfo Info;
            public MethodReference Reference;

            public SpecialIdentifiers SpecialIdentifiers;
            public HashSet<string> ParameterNames;
            public Dictionary<string, JSVariable> Variables;

            public JSFunctionExpression Expression;
            public FunctionAnalysis1stPass FirstPass;
            public FunctionAnalysis2ndPass SecondPass;

            public bool TransformPipelineHasCompleted = false;

            public MethodDefinition Definition {
                get {
                    return Info.Member;
                }
            }

            public Entry (QualifiedMemberIdentifier identifier, TrackedLockCollection lockCollection) {
                Identifier = identifier;

                StaticAnalysisDataLock = new TrackedLock(lockCollection, () => String.Format("Static Analysis Data {0}", this.Identifier.ToString()));
                TransformPipelineLock = new TrackedLock(lockCollection, () => String.Format("Transform Pipeline {0}", this.Identifier.ToString()));
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
        protected readonly TrackedLockCollection Locks = new TrackedLockCollection();

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

                return new Entry(id, Locks) {
                    Info = method.Method,
                    Reference = method.Reference,
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

                return new Entry(id, Locks) {
                    Info = args.Info,
                    Reference = args.Method,
                    Expression = result,
                    Variables = args.Translator.Variables,
                    ParameterNames = args.Translator.ParameterNames,
                    SpecialIdentifiers = args.Translator.SpecialIdentifiers
                };
            };

            MakeNullCacheEntry = (id, args) => {
                return new Entry(id, Locks) {
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

        private FunctionAnalysis1stPass _GetOrCreateFirstPass (Entry entry) {
            if (entry.FirstPass == null) {
                var analyzer = new StaticAnalyzer(entry.Definition.Module.TypeSystem, this);
                entry.FirstPass = analyzer.FirstPass(entry.Identifier, entry.Expression);
            }

            return entry.FirstPass;
        }

        private static bool TryAcquireStaticAnalysisDataLock (Entry entry, QualifiedMemberIdentifier method) {
            var result = entry.StaticAnalysisDataLock.TryBlockingEnter();
            // FIXME: Detect deadlock and throw restart exception
            if (!result.Success) {
                if (result.FailureReason == TrackedLockFailureReason.Deadlock)
                    throw new StaticAnalysisDataTemporarilyUnavailableException(method);
                else
                    return false;
            }

            return true;
        }

        public FunctionAnalysis1stPass GetFirstPass (QualifiedMemberIdentifier method, QualifiedMemberIdentifier forMethod) {
            var entry = GetCacheEntry(method, false);

            if ((entry == null) || (entry.Expression == null))
                return null;

            if (!TryAcquireStaticAnalysisDataLock(entry, method))
                return null;

            try {
                return _GetOrCreateFirstPass(entry);
            } finally {
                entry.StaticAnalysisDataLock.Exit();
            }
        }

        private FunctionAnalysis2ndPass _GetOrCreateSecondPass (Entry entry) {
            if ((entry.SecondPass == null) && (entry.Expression != null)) {
                entry.SecondPass = new FunctionAnalysis2ndPass(this, entry.FirstPass);
            }

            return entry.SecondPass;
        }

        public FunctionAnalysis2ndPass GetSecondPass (JSMethod method, QualifiedMemberIdentifier forMethod) {
            var id = method.QualifiedIdentifier;
            Entry entry = Cache.GetOrCreate(
                id, method, MakeCacheEntry
            );

            if (entry == null)
                return null;

            GetFirstPass(id, forMethod);

            if (!TryAcquireStaticAnalysisDataLock(entry, method.QualifiedIdentifier))
                return null;

            try {
                return _GetOrCreateSecondPass(entry);
            } finally {
                entry.StaticAnalysisDataLock.Exit();
            }
        }

        public void InvalidateFirstPass (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGet(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            entry.StaticAnalysisDataLock.BlockingEnter();
            entry.FirstPass = null;
            entry.SecondPass = null;
            entry.StaticAnalysisDataLock.Exit();
        }

        public void InvalidateSecondPass (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGet(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            entry.StaticAnalysisDataLock.BlockingEnter();
            entry.SecondPass = null;
            entry.StaticAnalysisDataLock.Exit();
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
