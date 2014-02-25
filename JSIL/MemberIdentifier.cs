using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL.Internal {
    public struct MemberIdentifier {
        public class Comparer : IEqualityComparer<MemberIdentifier> {
            public readonly ITypeInfoSource TypeInfo;

            public Comparer (ITypeInfoSource typeInfo) {
                TypeInfo = typeInfo;
            }

            public bool Equals (MemberIdentifier x, MemberIdentifier y) {
                /*
                if (x == null)
                    return x == y;
                 */

                return x.Equals(y, TypeInfo);
            }

            public int GetHashCode (MemberIdentifier obj) {
                return obj.GetHashCode();
            }
        }

        public enum MemberType : byte {
            Field = 0,
            Method = 1,
            Property = 2,
            Event = 3,
        }

        public readonly MemberType Type;
        public readonly string Name;
        public readonly TypeReference ReturnType;
        public readonly TypeReference[] ParameterTypes;
        public readonly int GenericArgumentCount;

        private readonly int HashCode;

        public static readonly TypeReference[] AnyParameterTypes = { };

        public static MemberIdentifier New (ITypeInfoSource ti, MemberReference mr) {
            MethodReference method;
            PropertyReference property;
            EventReference evt;
            FieldReference field;

            if ((method = mr as MethodReference) != null)
                return new MemberIdentifier(ti, method);
            else if ((field = mr as FieldReference) != null)
                return new MemberIdentifier(ti, field);
            else if ((property = mr as PropertyReference) != null)
                return new MemberIdentifier(ti, property);
            else if ((evt = mr as EventReference) != null)
                return new MemberIdentifier(ti, evt);
            else
                throw new NotImplementedException(String.Format(
                    "Unsupported member reference type: {0}",
                    mr
                ));
        }

        public MemberIdentifier (ITypeInfoSource ti, MethodReference mr, string newName = null) {
            Type = MemberType.Method;
            Name = newName ?? mr.Name;
            ReturnType = mr.ReturnType;
            ParameterTypes = GetParameterTypes(mr.Parameters);

            if (mr is GenericInstanceMethod)
                GenericArgumentCount = ((GenericInstanceMethod)mr).GenericArguments.Count;
            else if (mr.HasGenericParameters)
                GenericArgumentCount = mr.GenericParameters.Count;
            else
                GenericArgumentCount = 0;

            ti.CacheProxyNames(mr);

            HashCode = Type.GetHashCode() ^ Name.GetHashCode();
        }

        public MemberIdentifier (ITypeInfoSource ti, PropertyReference pr) {
            Type = MemberType.Property;
            Name = pr.Name;
            ReturnType = pr.PropertyType;
            GenericArgumentCount = 0;
            ParameterTypes = null;
            ti.CacheProxyNames(pr);

            var pd = pr.Resolve();
            if (pd != null) {
                if (pd.GetMethod != null) {
                    ParameterTypes = GetParameterTypes(pd.GetMethod.Parameters);
                } else if (pd.SetMethod != null) {
                    ParameterTypes = GetParameterTypes(pd.SetMethod.Parameters)
                        .Take(pd.SetMethod.Parameters.Count - 1).ToArray();
                }
            }

            HashCode = Type.GetHashCode() ^ Name.GetHashCode();
        }

        public MemberIdentifier (ITypeInfoSource ti, FieldReference fr) {
            Type = MemberType.Field;
            Name = fr.Name;
            ReturnType = fr.FieldType;
            GenericArgumentCount = 0;
            ParameterTypes = null;
            ti.CacheProxyNames(fr);

            HashCode = Type.GetHashCode() ^ Name.GetHashCode();
        }

        public MemberIdentifier (ITypeInfoSource ti, EventReference er) {
            Type = MemberType.Event;
            Name = er.Name;
            ReturnType = er.EventType;
            GenericArgumentCount = 0;
            ParameterTypes = null;
            ti.CacheProxyNames(er);

            HashCode = Type.GetHashCode() ^ Name.GetHashCode();
        }

        static TypeReference[] GetParameterTypes (IList<ParameterDefinition> parameters) {
            if (parameters.Count == 1) {
                var p = parameters[0];
                for (int c = p.CustomAttributes.Count, i = 0; i < c; i++) {
                    var ca = p.CustomAttributes[i];
                    if ((ca.AttributeType.Name == "ParamArrayAttribute") && (ca.AttributeType.Namespace == "System")) {
                        var t = JSExpression.DeReferenceType(parameters[0].ParameterType);
                        var at = t as ArrayType;
                        if ((at != null) && IsAnyType(at.ElementType))
                            return AnyParameterTypes;
                    }
                }
            }

            {
                int c = parameters.Count;
                var result = new TypeReference[c];
                for (int i = 0; i < c; i++) {
                    TypeReference parameterType = parameters[i].ParameterType, expandedParameterType;

                    if (TypeUtil.ExpandPositionalGenericParameters(parameterType, out expandedParameterType))
                        result[i] = expandedParameterType;
                    else
                        result[i] = parameterType;
                }

                return result;
            }
        }

        public static bool IsAnyType (TypeReference t) {
            if (t == null)
                return false;

            return (t.Name == "AnyType" && t.Namespace == "JSIL.Proxy");
        }

        public static bool TypesAreEqual (ITypeInfoSource typeInfo, TypeReference lhs, TypeReference rhs) {
            if (lhs == rhs)
                return true;
            else if (lhs == null || rhs == null)
                return false;

            var lhsReference = lhs as ByReferenceType;
            var rhsReference = rhs as ByReferenceType;

            if ((lhsReference != null) || (rhsReference != null)) {
                if ((lhsReference == null) || (rhsReference == null))
                    return false;

                return TypesAreEqual(typeInfo, lhsReference.ElementType, rhsReference.ElementType);
            }

            var lhsArray = lhs as ArrayType;
            var rhsArray = rhs as ArrayType;

            if ((lhsArray != null) || (rhsArray != null)) {
                if ((lhsArray == null) || (rhsArray == null))
                    return false;

                return TypesAreEqual(typeInfo, lhsArray.ElementType, rhsArray.ElementType);
            }

            var lhsGit = lhs as GenericInstanceType;
            var rhsGit = rhs as GenericInstanceType;

            if ((lhsGit != null) && (rhsGit != null)) {
                if (lhsGit.GenericArguments.Count != rhsGit.GenericArguments.Count)
                    return false;

                if (!TypesAreEqual(typeInfo, lhsGit.ElementType, rhsGit.ElementType))
                    return false;

                using (var eLeft = lhsGit.GenericArguments.GetEnumerator())
                using (var eRight = rhsGit.GenericArguments.GetEnumerator())
                    while (eLeft.MoveNext() && eRight.MoveNext()) {
                        if (!TypesAreEqual(typeInfo, eLeft.Current, eRight.Current))
                            return false;
                    }

                return true;
            }

            string[] proxyTargets;
            if (
                typeInfo.TryGetProxyNames(lhs.FullName, out proxyTargets) &&
                (proxyTargets != null) &&
                proxyTargets.Contains(rhs.FullName)
            ) {
                return true;
            } else if (
                typeInfo.TryGetProxyNames(rhs.FullName, out proxyTargets) &&
                (proxyTargets != null) &&
                proxyTargets.Contains(lhs.FullName)
            ) {
                return true;
            }

            if (IsAnyType(lhs) || IsAnyType(rhs))
                return true;

            return TypeUtil.TypesAreEqual(lhs, rhs);
        }

        public bool Equals (MemberIdentifier rhs, ITypeInfoSource typeInfo) {
            /*
            if (this == rhs)
                return true;
             */

            if (Type != rhs.Type)
                return false;

            if (!String.Equals(Name, rhs.Name))
                return false;

            if (!TypesAreEqual(typeInfo, ReturnType, rhs.ReturnType))
                return false;

            if (GenericArgumentCount != rhs.GenericArgumentCount)
                return false;

            if ((ParameterTypes == AnyParameterTypes) || (rhs.ParameterTypes == AnyParameterTypes)) {
            } else if ((ParameterTypes == null) || (rhs.ParameterTypes == null)) {
                if (ParameterTypes != rhs.ParameterTypes)
                    return false;
            } else {
                if (ParameterTypes.Length != rhs.ParameterTypes.Length)
                    return false;

                for (int i = 0, c = ParameterTypes.Length; i < c; i++) {
                    if (!TypesAreEqual(typeInfo, ParameterTypes[i], rhs.ParameterTypes[i]))
                        return false;
                }
            }

            return true;
        }

        public override bool Equals (object obj) {
            throw new InvalidOperationException("Use MemberIdentifier.Equals(...) explicitly.");
        }

        public override int GetHashCode () {
            return HashCode;
        }

        public string ToString (string name) {
            if (GenericArgumentCount != 0)
                name = String.Format("{0}`{1}", name, GenericArgumentCount);

            if (ParameterTypes != null)
                return String.Format(
                    "{0} {1} ( {2} )", ReturnType, name,
                    String.Join(", ", (from p in ParameterTypes select p.ToString()).ToArray())
                );
            else
                return String.Format(
                    "{0} {1}", ReturnType, name
                );
        }

        public override string ToString () {
            return ToString(Name);
        }
    }

    public struct QualifiedMemberIdentifier {
        public class Comparer : IEqualityComparer<QualifiedMemberIdentifier> {
            public readonly ITypeInfoSource TypeInfo;

            public Comparer (ITypeInfoSource typeInfo) {
                TypeInfo = typeInfo;
            }

            public bool Equals (QualifiedMemberIdentifier x, QualifiedMemberIdentifier y) {
                /*
                if (x == null)
                    return x == y;
                 */

                return x.Equals(y, TypeInfo);
            }

            public int GetHashCode (QualifiedMemberIdentifier obj) {
                return obj.GetHashCode();
            }
        }

        public readonly TypeIdentifier Type;
        public readonly MemberIdentifier Member;

        public QualifiedMemberIdentifier (TypeIdentifier type, MemberIdentifier member) {
            Type = type;
            Member = member;
        }

        public override int GetHashCode () {
            return Type.GetHashCode() ^ Member.GetHashCode();
        }

        public bool Equals (MemberReference lhs, MemberReference rhs, ITypeInfoSource typeInfo) {
            if ((lhs == null) || (rhs == null))
                return lhs == rhs;

            if (lhs == rhs)
                return true;

            var declaringType = rhs.DeclaringType.Resolve();
            if (declaringType == null)
                return false;

            var rhsType = new TypeIdentifier(declaringType);

            if (!Type.Equals(rhsType))
                return false;

            var rhsMember = MemberIdentifier.New(typeInfo, rhs);

            return Member.Equals(rhsMember, typeInfo);
        }

        public bool Equals (QualifiedMemberIdentifier rhs, ITypeInfoSource typeInfo) {
            if (!Type.Equals(rhs.Type))
                return false;

            return Member.Equals(rhs.Member, typeInfo);
        }

        public override bool Equals (object obj) {
            throw new InvalidOperationException("Use QualifiedMemberIdentifier.Equals(...) explicitly.");
        }

        public override string ToString () {
            return String.Format("{0} {1}", Type, Member);
        }
    }
}
