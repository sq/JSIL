using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class TypeExpressionCacher : JSAstVisitor {
        public readonly Dictionary<GenericTypeIdentifier, JSCachedType> CachedTypes;
        public readonly TypeReference ThisType;
        private int NextToken = 0;

        public TypeExpressionCacher (TypeReference thisType) {
            ThisType = thisType;
            CachedTypes = new Dictionary<GenericTypeIdentifier, JSCachedType>();
        }

        private JSCachedType MakeCachedType (TypeReference type) {
            string token;
            
            if (TypeUtil.TypesAreEqual(ThisType, type, true))
                token = "$Tthis";
            else
                token = String.Format("$T{0:X2}", NextToken++);

            return new JSCachedType(type, token);
        }

        private JSCachedType GetCachedType (TypeReference type) {
            if (!IsCacheable(type))
                return null;

            var resolved = TypeUtil.GetTypeDefinition(type);
            if (resolved == null)
                return null;

            TypeDefinition[] arguments;
            var git = type as GenericInstanceType;

            if (git != null) {
                arguments = (from a in git.GenericArguments select TypeUtil.GetTypeDefinition(a)).ToArray();
            } else {
                arguments = new TypeDefinition[0];
            }

            var identifier = new GenericTypeIdentifier(resolved, arguments);
            JSCachedType result;
            if (!CachedTypes.TryGetValue(identifier, out result))
                CachedTypes.Add(identifier, result = MakeCachedType(type));

            return result;
        }

        public bool IsCacheable (TypeReference type) {
            // Referring to an enclosing type from a nested type creates a cycle
            if (TypeUtil.IsNestedInside(type, ThisType))
                return false;

            if (TypeUtil.ContainsGenericParameter(type))
                return false;

            if (TypeUtil.IsOpenType(type))
                return false;

            return true;
        }

        public void VisitNode (JSType type) {
            var ct = GetCachedType(type.Type);

            if (ct != null) {
                ParentNode.ReplaceChild(type, ct);
                VisitReplacement(ct);
            } else {
                VisitChildren(type);
            }
        }

        public void VisitNode (JSTypeReference tr) {
            VisitChildren(tr);
        }

        public void VisitNode (JSTypeOfExpression toe) {
            // TODO: Cache these types too.
            VisitChildren(toe);
        }

        public void VisitNode (JSCachedType ct) {
            VisitChildren(ct);
        }

        public JSCachedType[] CacheTypesForFunction (JSFunctionExpression function) {
            var currentKeys = new HashSet<GenericTypeIdentifier>(CachedTypes.Keys);

            Visit(function);

            var newKeys = new HashSet<GenericTypeIdentifier>(CachedTypes.Keys);
            newKeys.ExceptWith(currentKeys);

            return (from k in newKeys 
                    let ct = CachedTypes[k]
                    orderby ct.Token
                    select ct).ToArray();
        }
    }
}
