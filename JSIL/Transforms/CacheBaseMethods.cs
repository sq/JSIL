using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Transforms {
    public class BaseMethodCacher : JSAstVisitor {
        public struct CachedMethodRecord {
            public readonly JSMethod Method;
            public readonly int Index;

            public CachedMethodRecord (JSMethod method, int index) {
                Method = method;
                Index = index;
            }
        }

        public readonly ITypeInfoSource TypeInfo;
        public readonly Dictionary<QualifiedMemberIdentifier, CachedMethodRecord> CachedMethods;
        public readonly TypeDefinition ThisType;
        private int NextID = 0;

        public BaseMethodCacher (ITypeInfoSource typeInfo, TypeDefinition thisType) {
            TypeInfo = typeInfo;
            ThisType = thisType;
            CachedMethods = new Dictionary<QualifiedMemberIdentifier, CachedMethodRecord>(
                new QualifiedMemberIdentifier.Comparer(TypeInfo)
            );
        }

        private JSCachedMethod GetCachedMethod (JSMethod method) {
            if (!IsCacheable(method))
                return null;

            var type = method.Reference.DeclaringType.Resolve();
            if (type == null)
                return null;

            var identifier = new QualifiedMemberIdentifier(
                new TypeIdentifier(type),
                new MemberIdentifier(TypeInfo, method.Reference)
            );

            CachedMethodRecord record;
            if (!CachedMethods.TryGetValue(identifier, out record))
                CachedMethods.Add(identifier, record = new CachedMethodRecord(method, NextID++));

            return new JSCachedMethod(
                method.Reference, method.Method,
                method.MethodTypes, method.GenericArguments,
                record.Index
            );
        }

        public bool IsCacheable (JSMethod method) {
            if (method.Reference == null)
                return false;

            var type = method.Reference.DeclaringType;

            // Same-type calls are excluded
            if (TypeUtil.TypesAreEqual(type, ThisType))
                return false;

            // Exclude any type that isn't in our bases or interfaces
            if (!TypeUtil.TypesAreAssignable(TypeInfo, type, ThisType))
                return false;

            // TODO: Exclude interfaces?

            // Exclude generics
            if (TypeUtil.ContainsGenericParameter(type))
                return false;

            if (TypeUtil.IsOpenType(type))
                return false;

            return true;
        }

        public void VisitNode (JSMethod method) {
            var cm = GetCachedMethod(method);

            if (cm != null) {
                ParentNode.ReplaceChild(method, cm);
                VisitReplacement(cm);
            } else {
                VisitChildren(method);
            }
        }

        public void VisitNode (JSCachedMethod method) {
            VisitChildren(method);
        }

        public void CacheMethodsForFunction (JSFunctionExpression function) {
            Visit(function);
        }
    }
}
