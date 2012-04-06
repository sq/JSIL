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

        public readonly MethodTypeFactory MethodTypes;
        public readonly ConcurrentHashQueue<QualifiedMemberIdentifier> OptimizationQueue;
        protected readonly ConcurrentCache<QualifiedMemberIdentifier, Entry> Cache;

        public FunctionCache (ITypeInfoSource typeInfo) {
            var comparer = new QualifiedMemberIdentifier.Comparer(typeInfo);
            Cache = new ConcurrentCache<QualifiedMemberIdentifier, Entry>(Environment.ProcessorCount, 4096, comparer);
            OptimizationQueue = new ConcurrentHashQueue<QualifiedMemberIdentifier>(Environment.ProcessorCount, 4096, comparer);
            MethodTypes = new MethodTypeFactory();
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

        public Entry GetCacheEntry (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGet(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            return entry;
        }

        public JSFunctionExpression GetExpression (QualifiedMemberIdentifier method) {
            var entry = GetCacheEntry(method);
            return entry.Expression;
        }

        public FunctionAnalysis1stPass GetFirstPass (QualifiedMemberIdentifier method) {
            var entry = GetCacheEntry(method);

            if (entry.Expression == null)
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
                id, () => {
                    OptimizationQueue.TryEnqueue(id);

                    return new Entry {
                        Info = method.Method,
                        Reference = method.Reference, 
                        Identifier = id,
                        ParameterNames = new HashSet<string>(from p in method.Method.Parameters select p.Name),
                        SecondPass = new FunctionAnalysis2ndPass(this, method.Method)
                    };
                }
            );

            if (entry.SecondPass == null) {
                if (entry.InProgress)
                    return null;

                if (entry.Expression == null)
                    return null;
                else
                    entry.SecondPass = new FunctionAnalysis2ndPass(this, GetFirstPass(id));
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
            return Cache.GetOrCreate(identifier, () => {
                var result = new JSFunctionExpression(
                    new JSMethod(method, info, MethodTypes),
                    translator.Variables,
                    parameters,
                    body,
                    MethodTypes
                );

                OptimizationQueue.TryEnqueue(identifier);

                return new Entry {
                    Identifier = identifier,
                    Info = info,
                    Reference = method,
                    Expression = result,
                    Variables = translator.Variables,
                    ParameterNames = translator.ParameterNames,
                    SpecialIdentifiers = translator.SpecialIdentifiers
                };
            }).Expression;
        }

        internal void CreateNull (
            MethodInfo info, MethodReference method, 
            QualifiedMemberIdentifier identifier
        ) {
            Cache.TryCreate(identifier, () => {
                return new Entry {
                    Identifier = identifier,
                    Info = info,
                    Reference = method,
                    Expression = null
                };
            });
        }

        public void Dispose () {
            Cache.Clear();
            OptimizationQueue.Clear();
            MethodTypes.Dispose();
        }
    }
}
