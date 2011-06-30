using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Transforms;
using Mono.Cecil;

namespace JSIL {
    public class FunctionCache : IFunctionSource {
        public class Entry {
            public QualifiedMemberIdentifier Identifier;
            public MethodDefinition Definition;
            public MethodReference Reference;

            public JSFunctionExpression Expression;
            public FunctionAnalysis1stPass FirstPass;
            public FunctionAnalysis2ndPass SecondPass;
        }

        public readonly Dictionary<QualifiedMemberIdentifier, Entry> Cache = new Dictionary<QualifiedMemberIdentifier, Entry>();

        public JSFunctionExpression GetExpression (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGetValue(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            return entry.Expression;
        }

        public FunctionAnalysis1stPass GetFirstPass (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGetValue(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            if (entry.FirstPass == null) {
                var analyzer = new StaticAnalyzer(entry.Definition.Module.TypeSystem, this);
                entry.FirstPass = analyzer.FirstPass(entry.Expression);
            }

            return entry.FirstPass;
        }

        public FunctionAnalysis2ndPass GetSecondPass (QualifiedMemberIdentifier method) {
            Entry entry;
            if (!Cache.TryGetValue(method, out entry))
                throw new KeyNotFoundException("No cache entry for method '" + method + "'.");

            if (entry.FirstPass == null)
                throw new InvalidOperationException("First pass not complete");

            if (entry.SecondPass == null)
                entry.SecondPass = new FunctionAnalysis2ndPass(this, entry.FirstPass);

            return entry.SecondPass;
        }

        internal JSFunctionExpression Create (
            MethodDefinition methodDef, MethodReference method, 
            QualifiedMemberIdentifier identifier, ILBlockTranslator translator, 
            IEnumerable<JSVariable> parameters, JSBlockStatement body
        ) {
            var result = new JSFunctionExpression(
                methodDef, method, identifier,
                translator.Variables,
                parameters,
                body
            );

            var entry = new Entry {
                Identifier = identifier,
                Definition = methodDef,
                Reference = method,
                Expression = result
            };
            Cache.Add(identifier, entry);

            return result;
        }
    }
}
