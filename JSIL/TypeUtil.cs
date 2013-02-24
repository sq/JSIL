using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using JSIL.Ast;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL {
    public static class TypeUtil {
        public static string GetLocalName (TypeDefinition type) {
            var result = new List<string>();
            result.Add(type.Name);

            type = type.DeclaringType;
            while (type != null) {
                result.Insert(0, type.Name);
                type = type.DeclaringType;
            }

            return String.Join("_", result);
        }

        public static bool IsArray (TypeReference type) {
            var at = type as ArrayType;
            if (at != null)
                return true;

            if (type == null)
                return false;

            if (type.FullName == "System.Array")
                return true;

            return false;
        }

        public static bool IsReferenceType (TypeReference type) {
            if (IsStruct(type))
                return false;
            else if (IsIntegralOrEnum(type))
                return false;

            var gp = type as GenericParameter;
            if (gp != null) {
                foreach (var constraint in gp.Constraints)
                    if (IsReferenceType(constraint))
                        return true;
            }

            switch (type.MetadataType) {
                case MetadataType.Object:
                case MetadataType.Class:
                case MetadataType.String:
                case MetadataType.Array:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsStruct (TypeReference type) {
            if (type == null)
                return false;

            type = DereferenceType(type);
            MetadataType etype = type.MetadataType;

            if (IsEnum(type))
                return false;

            var git = type as GenericInstanceType;
            if (git != null)
                return git.IsValueType;

            var gp = type as GenericParameter;
            if (gp != null) {
                foreach (var constraint in gp.Constraints)
                    if (IsStruct(constraint))
                        return true;
            }

            // System.ValueType's MetadataType is Class... WTF.
            if ((type.Namespace == "System") && (type.Name == "ValueType"))
                return true;

            return (etype == MetadataType.ValueType);
        }

        public static bool IsNumeric (TypeReference type) {
            type = DereferenceType(type);

            switch (type.MetadataType) {
                case MetadataType.Byte:
                case MetadataType.SByte:
                case MetadataType.Double:
                case MetadataType.Single:
                case MetadataType.Int16:
                case MetadataType.Int32:
                case MetadataType.Int64:
                case MetadataType.UInt16:
                case MetadataType.UInt32:
                case MetadataType.UInt64:
                    return true;
                    // Blech
                case MetadataType.UIntPtr:
                case MetadataType.IntPtr:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsIntegral (TypeReference type) {
            type = DereferenceType(type);

            switch (type.MetadataType) {
                case MetadataType.Byte:
                case MetadataType.SByte:
                case MetadataType.Int16:
                case MetadataType.Int32:
                case MetadataType.Int64:
                case MetadataType.UInt16:
                case MetadataType.UInt32:
                case MetadataType.UInt64:
                    return true;
                    // Blech
                case MetadataType.UIntPtr:
                case MetadataType.IntPtr:
                    return true;
                default:
                    return false;
            }
        }

        public static bool Is32BitIntegral (TypeReference type) {
            type = DereferenceType(type);

            switch (type.MetadataType) {
                case MetadataType.Byte:
                case MetadataType.SByte:
                case MetadataType.UInt16:
                case MetadataType.Int16:
                case MetadataType.UInt32:
                case MetadataType.Int32:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNullable (TypeReference type) {
            int temp;
            type = FullyDereferenceType(type, out temp);

            var git = type as GenericInstanceType;
            if ((git != null) && (git.Name == "Nullable`1"))
                return true;

            return false;
        }

        public static TypeReference StripNullable (TypeReference type) {
            int temp;
            type = FullyDereferenceType(type, out temp);

            var git = type as GenericInstanceType;
            if ((git != null) && (git.Name == "Nullable`1"))
                return git.GenericArguments[0];

            return type;
        }

        public static bool IsEnum (TypeReference type) {
            var typedef = GetTypeDefinition(type);
            return (typedef != null) && (typedef.IsEnum);
        }

        public static bool IsBoolean (TypeReference type) {
            type = DereferenceType(type);
            return type.MetadataType == MetadataType.Boolean;
        }

        public static bool IsIgnoredType (TypeReference type, bool enableUnsafeCode = true) {
            type = DereferenceType(type);

            if (type == null)
                return false;

            if (!enableUnsafeCode) {
                if (type.IsPointer)
                    return true;
                else if (type.IsPinned)
                    return true;
            }

            if (type.IsFunctionPointer)
                return true;
            else
                return false;
        }

        public static bool IsDelegateType (TypeReference type) {
            type = DereferenceType(type);

            var typedef = GetTypeDefinition(type);
            if (typedef == null)
                return false;
            if (typedef.FullName == "System.Delegate")
                return true;

            if (
                (typedef != null) && (typedef.BaseType != null) &&
                (
                    (typedef.BaseType.FullName == "System.Delegate") ||
                    (typedef.BaseType.FullName == "System.MulticastDelegate")
                )
                ) {
                    return true;
                }

            return false;
        }

        public static bool IsOpenType (TypeReference type) {
            type = DereferenceType(type);

            var gp = type as GenericParameter;
            var git = type as GenericInstanceType;
            var at = type as ArrayType;
            var byref = type as ByReferenceType;

            if (gp != null)
                return true;

            if (git != null) {
                var elt = git.ElementType;

                foreach (var ga in git.GenericArguments) {
                    if (IsOpenType(ga))
                        return true;
                }

                return IsOpenType(elt);
            }

            if (at != null)
                return IsOpenType(at.ElementType);

            if (byref != null)
                return IsOpenType(byref.ElementType);

            return false;
        }

        public static TypeDefinition GetTypeDefinition (TypeReference typeRef, bool mapAllArraysToSystemArray = true) {
            if (typeRef == null)
                return null;

            var ts = typeRef.Module.TypeSystem;
            typeRef = DereferenceType(typeRef);

            bool unwrapped = false;
            do {
                var rmt = typeRef as RequiredModifierType;
                var omt = typeRef as OptionalModifierType;

                if (rmt != null) {
                    typeRef = rmt.ElementType;
                    unwrapped = true;
                } else if (omt != null) {
                    typeRef = omt.ElementType;
                    unwrapped = true;
                } else {
                    unwrapped = false;
                }
            } while (unwrapped);

            var at = typeRef as ArrayType;
            if (at != null) {
                if (mapAllArraysToSystemArray)
                    return new TypeReference(ts.Object.Namespace, "Array", ts.Object.Module, ts.Object.Scope).ResolveOrThrow();

                var inner = GetTypeDefinition(at.ElementType, mapAllArraysToSystemArray);
                if (inner != null)
                    return (new ArrayType(inner, at.Rank)).Resolve();
                else
                    return null;
            }

            var gp = typeRef as GenericParameter;
            if ((gp != null) && (gp.Owner == null))
                return null;

            else if (IsIgnoredType(typeRef))
                return null;
            else
                return typeRef.Resolve();
        }

        public static TypeReference FullyDereferenceType (TypeReference type, out int depth) {
            depth = 0;

            var brt = type as ByReferenceType;
            while (brt != null) {
                depth += 1;
                type = brt.ElementType;
                brt = type as ByReferenceType;
            }

            return type;
        }

        public static TypeReference DereferenceType (TypeReference type, bool dereferencePointers = false) {
            var brt = type as ByReferenceType;
            if (brt != null)
                return brt.ElementType;

            if (dereferencePointers) {
                var pt = type as PointerType;
                if (pt != null)
                    return pt.ElementType;
            }

            return type;
        }

        private static bool TypeInBases (TypeReference haystack, TypeReference needle, bool explicitGenericEquality) {
            if ((haystack == null) || (needle == null))
                return haystack == needle;

            var dToWalk = haystack.Resolve();
            var dSource = needle.Resolve();

            if ((dToWalk == null) || (dSource == null))
                return TypesAreEqual(haystack, needle, explicitGenericEquality);

            var t = haystack;
            while (t != null) {
                if (TypesAreEqual(t, needle, explicitGenericEquality))
                    return true;

                var dT = t.Resolve();

                if ((dT != null) && (dT.BaseType != null)) {
                    var baseType = dT.BaseType;

                    t = baseType;
                } else
                    break;
            }

            return false;
        }

        public static bool TypesAreEqual (TypeReference target, TypeReference source, bool strictEquality = false) {
            if (target == source)
                return true;
            else if ((target == null) || (source == null))
                return (target == source);

            bool result;

            int targetDepth, sourceDepth;
            FullyDereferenceType(target, out targetDepth);
            FullyDereferenceType(source, out sourceDepth);

            var targetGp = target as GenericParameter;
            var sourceGp = source as GenericParameter;

            if ((targetGp != null) || (sourceGp != null)) {
                if ((targetGp == null) || (sourceGp == null))
                    return false;

                TypeReference temp;

                // Generic parameters may be in positional (!0) form; expand them so they compare
                //  correctly with named (T) form.

                if (IsPositionalGenericParameter(sourceGp)) {
                    ExpandPositionalGenericParameters(sourceGp, out temp);
                    sourceGp = (GenericParameter)temp;
                }

                if (IsPositionalGenericParameter(targetGp)) {
                    ExpandPositionalGenericParameters(targetGp, out temp);
                    targetGp = (GenericParameter)temp;
                }

                var targetOwnerType = targetGp.Owner as TypeReference;
                var sourceOwnerType = sourceGp.Owner as TypeReference;

                // https://github.com/jbevain/cecil/issues/97

                if ((targetOwnerType != null) || (sourceOwnerType != null)) {
                    var basesEqual = false;

                    if (TypeInBases(targetOwnerType, sourceOwnerType, strictEquality))
                        basesEqual = true;
                    else if (TypeInBases(sourceOwnerType, targetOwnerType, strictEquality))
                        basesEqual = true;

                    if (!basesEqual)
                        return false;
                } else {
                    // FIXME: Can't do an exact comparison here since we get called by MemberIdentifier comparisons.
                }

                if (targetGp.Type != sourceGp.Type)
                    return false;

                if (targetGp.Name != sourceGp.Name)
                    return false;

                if (targetGp.Position != sourceGp.Position)
                    return false;

                return true;
            }

            var targetArray = target as ArrayType;
            var sourceArray = source as ArrayType;

            if ((targetArray != null) || (sourceArray != null)) {
                if ((targetArray == null) || (sourceArray == null))
                    return false;

                if (targetArray.Rank != sourceArray.Rank)
                    return false;

                return TypesAreEqual(targetArray.ElementType, sourceArray.ElementType, strictEquality);
            }

            var targetGit = target as GenericInstanceType;
            var sourceGit = source as GenericInstanceType;

            if ((targetGit != null) || (sourceGit != null)) {
                if (!strictEquality) {
                    if ((targetGit != null) && TypesAreEqual(targetGit.ElementType, source))
                        return true;
                    if ((sourceGit != null) && TypesAreEqual(target, sourceGit.ElementType))
                        return true;
                }

                if ((targetGit == null) || (sourceGit == null))
                    return false;

                if (targetGit.GenericArguments.Count != sourceGit.GenericArguments.Count)
                    return false;

                for (var i = 0; i < targetGit.GenericArguments.Count; i++) {
                    if (!TypesAreEqual(targetGit.GenericArguments[i], sourceGit.GenericArguments[i], strictEquality))
                        return false;
                }

                return TypesAreEqual(targetGit.ElementType, sourceGit.ElementType, strictEquality);
            }

            if ((target.IsByReference != source.IsByReference) || (targetDepth != sourceDepth))
                result = false;
            else if (target.IsPointer != source.IsPointer)
                result = false;
            else if (target.IsFunctionPointer != source.IsFunctionPointer)
                result = false;
            else if (target.IsPinned != source.IsPinned)
                result = false;
            else {
                if (
                    (target.Name == source.Name) &&
                    (target.Namespace == source.Namespace) &&
                    (target.Module == source.Module) &&
                    TypesAreEqual(target.DeclaringType, source.DeclaringType, strictEquality)
                )
                    return true;

                var dTarget = GetTypeDefinition(target);
                var dSource = GetTypeDefinition(source);

                if (Equals(dTarget, dSource) && (dSource != null))
                    result = true;
                else if (Equals(target, source))
                    result = true;
                else if (
                    (dTarget != null) && (dSource != null) &&
                    (dTarget.FullName == dSource.FullName)
                )
                    result = true;
                else
                    result = false;
            }

            return result;
        }

        public static IEnumerable<TypeDefinition> AllBaseTypesOf (TypeDefinition type) {
            if (type == null)
                yield break;

            var baseType = GetTypeDefinition(type.BaseType);

            while (baseType != null) {
                yield return baseType;

                baseType = GetTypeDefinition(baseType.BaseType);
            }
        }

        public static bool TypesAreAssignable (ITypeInfoSource typeInfo, TypeReference target, TypeReference source) {
            if ((target == null) || (source == null))
                return false;

            // All values are assignable to object
            if (target.FullName == "System.Object")
                return true;

            int targetDepth, sourceDepth;
            if (TypesAreEqual(FullyDereferenceType(target, out targetDepth), FullyDereferenceType(source, out sourceDepth))) {
                if (targetDepth == sourceDepth)
                    return true;
            }

            // HACK: System.Array and T[] do not implement IEnumerable<T>
            if (
                source.IsArray &&
                (target.Namespace == "System.Collections.Generic")
            ) {
                var targetGit = target as GenericInstanceType;
                if (
                    (targetGit != null) &&
                    (targetGit.Name == "IEnumerable`1") &&
                    (targetGit.GenericArguments.FirstOrDefault() == source.GetElementType())
                )
                    return true;
            }

            // HACK: The .NET type system treats pointers and ints as assignable to each other
            if (TypeUtil.IsIntegral(target) && source.IsPointer)
                return true;

            var cacheKey = new Tuple<string, string>(target.FullName, source.FullName);
            return typeInfo.AssignabilityCache.GetOrCreate(
                cacheKey, (key) => {
                    bool result = false;

                    var dSource = GetTypeDefinition(source);

                    if (TypeInBases(source, target, false))
                        result = true;
                    else if (dSource == null)
                        result = false;
                    else if (TypesAreEqual(target, dSource))
                        result = true;
                    else if ((dSource.BaseType != null) && TypesAreAssignable(typeInfo, target, dSource.BaseType))
                        result = true;
                    else {
                        foreach (var iface in dSource.Interfaces) {
                            if (TypesAreAssignable(typeInfo, target, iface)) {
                                result = true;
                                break;
                            }
                        }
                    }

                    return result;
                }
            );
        }

        public static bool IsIntegralOrEnum (TypeReference type) {
            var typedef = GetTypeDefinition(type);
            return IsIntegral(type) || ((typedef != null) && typedef.IsEnum);
        }

        public static bool IsNumericOrEnum (TypeReference type) {
            var typedef = GetTypeDefinition(type);
            return IsNumeric(type) || ((typedef != null) && typedef.IsEnum);
        }

        public static JSExpression[] GetArrayDimensions (TypeReference arrayType) {
            var at = arrayType as ArrayType;
            if (at == null)
                return null;

            var result = new List<JSExpression>();
            for (var i = 0; i < at.Dimensions.Count; i++) {
                var dim = at.Dimensions[i];
                if (dim.IsSized)
                    result.Add(JSLiteral.New(dim.UpperBound.Value));
                else
                    return null;
            }

            return result.ToArray();
        }

        public static bool ContainsGenericParameter (TypeReference type) {
            type = DereferenceType(type);

            var gp = type as GenericParameter;
            var git = type as GenericInstanceType;
            var at = type as ArrayType;
            var byref = type as ByReferenceType;

            if (gp != null)
                return true;

            if (git != null) {
                var elt = git.ElementType;

                foreach (var ga in git.GenericArguments) {
                    if (ContainsGenericParameter(ga))
                        return true;
                }

                return ContainsGenericParameter(elt);
            }

            if (at != null)
                return ContainsGenericParameter(at.ElementType);

            if (byref != null)
                return ContainsGenericParameter(byref.ElementType);

            return false;
        }

        public static bool ContainsPositionalGenericParameter (TypeReference type) {
            type = DereferenceType(type);

            var gp = type as GenericParameter;
            var git = type as GenericInstanceType;
            var at = type as ArrayType;
            var byref = type as ByReferenceType;

            if (gp != null)
                return IsPositionalGenericParameter(gp);

            if (git != null) {
                var elt = git.ElementType;

                foreach (var ga in git.GenericArguments) {
                    if (IsPositionalGenericParameter(ga))
                        return true;
                }

                return IsPositionalGenericParameter(elt);
            }

            if (at != null)
                return IsPositionalGenericParameter(at.ElementType);

            if (byref != null)
                return IsPositionalGenericParameter(byref.ElementType);

            return false;
        }

        public static bool IsPositionalGenericParameter (TypeReference type) {
            var gp = type as GenericParameter;
            if (gp != null)
                return IsPositionalGenericParameter(gp);
            else
                return false;
        }

        public static bool IsPositionalGenericParameter (GenericParameter gp) {
            return gp.Name.StartsWith("!!") || gp.Name.StartsWith("!");
        }

        public static bool ExpandPositionalGenericParameters (TypeReference type, out TypeReference expanded) {
            var gp = type as GenericParameter;
            var git = type as GenericInstanceType;
            var at = type as ArrayType;
            var byref = type as ByReferenceType;

            TypeReference _expanded;

            if (gp != null) {
                if (IsPositionalGenericParameter(gp)) {
                    var ownerType = gp.Owner as TypeReference;
                    var ownerMethod = gp.Owner as MethodReference;

                    if (ownerType != null) {
                        var resolvedOwnerType = ownerType.Resolve();
                        if (resolvedOwnerType == null) {
                            expanded = type;
                            return false;
                        }

                        expanded = resolvedOwnerType.GenericParameters[int.Parse(gp.Name.Replace("!", ""))];
                        return true;
                    } else if (ownerMethod != null) {
                        var resolvedOwnerMethod = ownerMethod.Resolve();
                        if (resolvedOwnerMethod == null) {
                            expanded = type;
                            return false;
                        }

                        expanded = resolvedOwnerMethod.GenericParameters[int.Parse(gp.Name.Replace("!", ""))];
                        return true;
                    } else {
                        throw new NotImplementedException("Unknown positional generic parameter type");
                    }
                }
            }

            if (git != null) {
                var elt = git.ElementType;

                if (ContainsPositionalGenericParameter(elt) || git.GenericArguments.Any(ContainsPositionalGenericParameter)) {
                    ExpandPositionalGenericParameters(elt, out _expanded);
                    var result = new GenericInstanceType(_expanded);

                    foreach (var ga in git.GenericArguments) {
                        ExpandPositionalGenericParameters(ga, out _expanded);
                        result.GenericArguments.Add(_expanded);
                    }

                    expanded = result;
                    return true;
                }
            }

            if (at != null) {
                if (ExpandPositionalGenericParameters(at.ElementType, out _expanded))
                    ;
            }

            if (byref != null) {
                if (ExpandPositionalGenericParameters(byref.ElementType, out _expanded))
                    ;
            }

            expanded = type;
            return false;
        }

        public static bool IsNestedInside (TypeReference needle, TypeReference haystack) {
            var tr = needle;

            while (tr != null) {
                if (TypesAreEqual(tr, haystack, true))
                    return true;

                tr = tr.DeclaringType;
            }

            return false;
        }
    }
}
