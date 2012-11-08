using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            public bool InProgress;

            public MethodDefinition Definition {
                get {
                    return Info.Member;
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

            if (entry.InProgress)
                return null;

            if (entry.FirstPass == null) {
                entry.InProgress = true;
                try {
                    var analyzer = new StaticAnalyzer(entry.Definition.Module.TypeSystem, this);
                    entry.FirstPass = analyzer.FirstPass(entry.Expression);
                } finally {
                    entry.InProgress = false;
                }
            }

            return entry.FirstPass;
        }

        public FunctionAnalysis2ndPass GetSecondPass (JSMethod method) {
            var id = method.QualifiedIdentifier;

            Entry entry = Cache.GetOrCreate(
                id, method, MakeCacheEntry
            );

            if (entry.SecondPass == null) {
                if (entry.InProgress)
                    return null;

                if (entry.Expression == null)
                    return null;
                else {
                    var firstPass = GetFirstPass(id);
                    try {
                        entry.InProgress = true;
                        entry.SecondPass = new FunctionAnalysis2ndPass(this, firstPass);
                    } finally {
                        entry.InProgress = false;
                    }
                }
            }

            return entry.SecondPass;
        }

        public void InvalidateFirstPass (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGet(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            entry.FirstPass = null;
            entry.SecondPass = null;
        }

        public void InvalidateSecondPass (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGet(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            entry.SecondPass = null;
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
