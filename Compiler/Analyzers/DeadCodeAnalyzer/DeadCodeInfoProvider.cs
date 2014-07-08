using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.Utils;
using JSIL.Internal;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Linker.Steps;

namespace JSIL.Compiler.Extensibility.DeadCodeAnalyzer {
    public class DeadCodeInfoProvider {
        private readonly HashSet<AssemblyDefinition> assemblies;
        private readonly HashSet<FieldDefinition> fields;
        private readonly HashSet<MethodDefinition> methods;
        private readonly HashSet<TypeDefinition> types;

        private readonly TypeMapStep typeMapStep = new TypeMapStep();
        private readonly Configuration configuration;
        private readonly List<Regex> whiteListCache; 

        public DeadCodeInfoProvider(Configuration configuration) {
            this.configuration = configuration;

            types = new HashSet<TypeDefinition>();
            methods = new HashSet<MethodDefinition>();
            fields = new HashSet<FieldDefinition>();
            assemblies = new HashSet<AssemblyDefinition>();

            if (configuration.WhiteList != null &
                configuration.WhiteList.Count > 0) {
                whiteListCache = new List<Regex>(configuration.WhiteList.Count);
                foreach (var pattern in configuration.WhiteList) {
                    Regex compiledRegex = new Regex(pattern, RegexOptions.ECMAScript | RegexOptions.Compiled);
                    whiteListCache.Add(compiledRegex);
                }
            }
        }

        internal TypeInfoProvider TypeInfoProvider { get; set; }

        public bool IsUsed(MemberReference member) {
            var typeReference = member as TypeReference;
            if (typeReference != null)
            {
                var defenition = typeReference.Resolve();
                return types.Contains(defenition);
            }

            var methodReference = member as MethodReference;
            if (methodReference != null) {
                var defenition = methodReference.Resolve();
                return methods.Contains(defenition);
            }

            var fieldReference = member as FieldReference;
            if (fieldReference != null) {
                var defenition = fieldReference.Resolve();
                return fields.Contains(defenition);
            }

            throw new ArgumentException("Unexpected member reference type");
        }

        public void WalkMethod(MethodReference methodReference) {
            if (!AddMethod(methodReference))
            {
                return;
            }

            var method = methodReference.Resolve();

            List<Instruction> foundInstructions = (from instruction in method.Body.Instructions
                                                   where method.HasBody && method.Body.Instructions != null && instruction.Operand != null
                                                   select instruction).ToList();

            IEnumerable<TypeReference> typesFound = from instruction in foundInstructions
                                                     let tRef = instruction.Operand as TypeReference
                                                     where tRef != null
                                                     select tRef;

            IEnumerable<MethodReference> methodsFound = from instruction in foundInstructions
                                                         let mRef = instruction.Operand as MethodReference
                                                         where mRef != null && mRef.DeclaringType != null
                                                         select mRef;

            IEnumerable<FieldDefinition> fieldsFound = from instruction in foundInstructions
                                                       let fRef = instruction.Operand as FieldReference
                                                       where fRef != null && fRef.FieldType != null
                                                       let fRefResolved = fRef.Resolve()
                                                       where fRefResolved != null
                                                       select fRefResolved;

            foreach (TypeReference typeDefinition in typesFound)
            {
                AddType(typeDefinition);
            }

            foreach (FieldDefinition fieldDefinition in fieldsFound) {
                AddField(fieldDefinition);
            }

            foreach (MethodReference methodDefinition in methodsFound)
            {
                if (methodDefinition != method) {
                    WalkMethod(methodDefinition);
                }
            }
        }

        public void ResolveVirtualMethodsCycle()
        {
            var inintialMemberCount = 0;
            var endMemberCount = 0;
            do
            {
                inintialMemberCount = fields.Count + methods.Count + types.Count;
                ResolveVirtualMethods();
                endMemberCount = fields.Count + methods.Count + types.Count;
            } while (endMemberCount != inintialMemberCount);
        }

        public void ResolveVirtualMethods() {
            MethodDefinition[] tempMethods = new MethodDefinition[methods.Count];
            methods.CopyTo(tempMethods);

            for (int i = 0; i < tempMethods.Length; i++) {
                MethodDefinition method = tempMethods[i];
                if (method.IsVirtual)
                    ResolveVirtualMethod(method);
            }
        }

        private void ResolveVirtualMethod(MethodDefinition method) {
            HashSet<MethodDefinition> overrides = new HashSet<MethodDefinition>();
            GetAllOverrides(method, overrides);
            foreach (MethodDefinition methodDefinition in overrides) {
                if (IsUsed(methodDefinition.DeclaringType)) {
                    WalkMethod(methodDefinition);
                }
            }
        }

        private void AddType(TypeReference type) {
            if (type == null || IsIgnored(type))
            {
                return;
            }

            TypeDefinition resolvedType;

            if (type.IsGenericInstance)
            {
                var genericType = (GenericInstanceType)type;
                foreach (var genericArgument in genericType.GenericArguments)
                {
                    resolvedType = genericArgument.Resolve();

                    if (resolvedType != null)
                    {
                        AddType(resolvedType);
                    }
                }
            }

            resolvedType = type.Resolve();

            if (resolvedType != null)
            {
                AddType(resolvedType);
            }
        }

        private void AddType(TypeDefinition resolvedType)
        {
            if (resolvedType == null)
            {
                return;
            }

            AddType(resolvedType.BaseType);

            if (types.Add(resolvedType))
            {
                if (resolvedType.HasCustomAttributes)
                {
                    foreach (CustomAttribute attribute in resolvedType.CustomAttributes)
                    {
                        if (attribute.HasConstructorArguments)
                            WalkMethod(attribute.Constructor.Resolve());
                    }
                }

                // HACK: force analyze static constructor
                MethodDefinition cctor = resolvedType.Methods.FirstOrDefault(m => m.Name == ".cctor");
                if (cctor != null && cctor.HasBody)
                {
                    WalkMethod(cctor);
                }
            }
        }

        private bool IsIgnored(TypeReference type)
        {
            if (type == null)
            {
                return false;
            }

            var typeDefenition = type.Resolve();
            if (typeDefenition != null)
            {
                var typeInformation = TypeInfoProvider.GetTypeInformation(type);
                if (typeInformation.IsIgnored)
                {
                    return true;
                }
            }

            if (type.IsGenericInstance)
            {
                var genericType = (GenericInstanceType) type;
                if (genericType.GenericArguments.Any(IsIgnored))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsIgnored(FieldReference field)
        {
            var fieldDefenition = field.Resolve();
            if (fieldDefenition != null)
            {
                var fieldInfo = TypeInfoProvider.GetMemberInformation<FieldInfo>(fieldDefenition);
                if (fieldInfo.IsIgnored)
                {
                    return true;
                }
            }

            if (field.DeclaringType.IsGenericInstance)
            {
                if (IsIgnored(field.DeclaringType))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsIgnored(MethodReference method)
        {
            var methodDefenition = method.Resolve();
            if (methodDefenition != null)
            {
                var methodInfo = TypeInfoProvider.GetMemberInformation<MethodInfo>(methodDefenition);
                if (methodInfo.IsIgnored && !methodInfo.IsLambda)
                {
                    return true;
                }
            }

            if (method.IsGenericInstance)
            {
                var genericMethod = (GenericInstanceMethod) method;
                if (genericMethod.GenericArguments.Any(IsIgnored))
                {
                    return true;
                }
            }

            if (method.DeclaringType.IsGenericInstance)
            {
                if (IsIgnored(method.DeclaringType))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsStubOrExternal(MethodReference method)
        {
            var methodDefenition = method.Resolve();
            if (methodDefenition != null)
            {
                var methodInfo = TypeInfoProvider.GetMemberInformation<MethodInfo>(methodDefenition);
                if (methodInfo.IsExternal || methodInfo.DeclaringType.IsExternal || methodInfo.DeclaringType.IsStubOnly)
                {
                    return true;
                }
            }

            return false;
        }

        private bool AddMethod(MethodReference method) {
            if (method == null || method.DeclaringType.IsArray) {
                return false;
            }

            if (IsIgnored(method))
            {
                return false;
            }

            if (method.IsGenericInstance)
            {
                var genericMethod = (GenericInstanceMethod)method;
                foreach (var genericParameter in genericMethod.GenericArguments)
                {
                    AddType(genericParameter);
                }
            }

            MethodDefinition resolvedMethod = method.Resolve();

            if (resolvedMethod == null)
            {
                return false;
            }

            AddType(resolvedMethod.DeclaringType);
            AddType(resolvedMethod.ReturnType);
            foreach (var parameterDefinition in resolvedMethod.Parameters)
            {
                AddType(parameterDefinition.ParameterType);
            }

            if (methods.Add(resolvedMethod) && resolvedMethod.HasBody) {
                //if (resolvedMethod.HasCustomAttributes) {
                //    foreach (CustomAttribute attribute in resolvedMethod.CustomAttributes) {
                //        AddType(attribute.AttributeType);
                //    }
                //}
                
                if (IsStubOrExternal(method))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private void AddField(FieldReference field) {
            if (field == null || IsIgnored(field)) {
                return;
            }

            AddType(field.FieldType);
            FieldDefinition resolvedField = field.Resolve();

            fields.Add(resolvedField);
        }

        public void AddAssemblies(IEnumerable<AssemblyDefinition> assemblies) {
            IEnumerable<ModuleDefinition> modules = from assembly in assemblies
                                                    from module in assembly.Modules
                                                    select module;
            
            foreach (ModuleDefinition module in modules) {
                typeMapStep.ProcessModule(module);

                if (whiteListCache.Count > 0) {
                    foreach (var type in module.Types) {
                        ProcessWhiteList(type);
                    }
                }
            }

            this.assemblies.UnionWith(assemblies);
        }

        private bool IsMemberWhiteListed(MemberReference member) {
            if (configuration.WhiteList == null)
                return false;

            foreach (var regex in whiteListCache) {
                if (regex.IsMatch(member.FullName))
                    return true;
            }

            IEnumerable<CustomAttribute> customAttributes = null;

            if (member is TypeReference)
            {
                TypeDefinition type = ((TypeReference)member).Resolve();
                if (type != null)
                {
                    customAttributes = type.CustomAttributes;
                }
            }
            else if (member is MethodReference)
            {
                MethodDefinition method = ((MethodReference) member).Resolve();
                if (method != null)
                {
                    customAttributes = method.CustomAttributes;
                }
            }
            else if (member is FieldReference)
            {
                FieldDefinition field = ((FieldReference)member).Resolve();
                if (field != null)
                {
                    customAttributes = field.CustomAttributes;
                }
            }

            if (customAttributes != null && customAttributes.Any(item => item.AttributeType.Name == "JSDeadCodeEleminationEntryPoint"))
            {
                return true;
            }

            TypeReference declaringType = null;
            if (member is TypeReference)
            {
                declaringType = ((TypeReference)member).DeclaringType;
            }
            else if (member is MethodReference)
            {
                declaringType = ((MethodReference) member).DeclaringType;
            }
            else if (member is FieldReference)
            {
                declaringType = ((FieldReference)member).DeclaringType;
            }

            if (declaringType != null)
            {
                var declaringTypeDefenition = declaringType.Resolve();
                if (declaringTypeDefenition != null && declaringTypeDefenition.CustomAttributes.Any(item => item.AttributeType.Name == "JSDeadCodeEleminationClassEntryPoint"))
                {
                    return true;
                }

                while (declaringTypeDefenition != null)
                {
                    if (declaringTypeDefenition.CustomAttributes.Any(
                            item => item.AttributeType.Name == "JSDeadCodeEleminationHierarchyEntryPoint"))
                    {
                        return true;
                    }

                    declaringTypeDefenition = declaringTypeDefenition.BaseType != null
                                                  ? declaringTypeDefenition.BaseType.Resolve()
                                                  : null;
                }
            }

            return false;
        }

        private void ProcessWhiteList(MemberReference member) {
            if (member is TypeReference) {
                TypeDefinition type = (member as TypeReference).Resolve();
                
                if (type != null) {
                    if (IsMemberWhiteListed(type))
                        AddType(type);

                    if (type.HasNestedTypes) {
                        foreach (var nestedType in type.NestedTypes) {
                            ProcessWhiteList(nestedType);
                        }
                    }
                    if (type.HasMethods) {
                        foreach (var method in type.Methods) {
                            if (IsMemberWhiteListed(method))
                                ProcessWhiteList(method);
                        }
                    }
                    if (type.HasFields)
                    {
                        foreach (var field in type.Fields)
                        {
                            if (IsMemberWhiteListed(field))
                                ProcessWhiteList(field);
                        }
                    }
                }

                return;
            }
            if (member is MethodReference) {
                if (IsMemberWhiteListed(member))
                    WalkMethod(member as MethodDefinition);

                return;
            }
            if (member is FieldReference) {
                if (IsMemberWhiteListed(member))
                    AddField(member as FieldReference);

                return;
            }

            throw new ArgumentException("Unexpected member reference type");
        }

        private void GetAllOverrides(MethodDefinition method, HashSet<MethodDefinition> deepOverrides) {
            if (method == null)
                return;

            HashSet<MethodDefinition> overrides = typeMapStep.Annotations.GetOverrides(method);

            if (overrides == null)
                return;

            deepOverrides.UnionWith(overrides);
            foreach (MethodDefinition overrideMethod in overrides) {
                GetAllOverrides(overrideMethod, deepOverrides);
            }
        }

        private static IEnumerable<TypeDefinition> FindDerivedTypes(TypeDefinition type, IEnumerable<ModuleDefinition> assemblies) {
            foreach (ModuleDefinition module in assemblies) {
                foreach (TypeDefinition td in TreeTraversal.PreOrder(module.Types, t => t.NestedTypes)) {
                    if (type.IsInterface && td.HasInterfaces) {
                        foreach (TypeReference typeRef in td.Interfaces) {
                            if (IsSameType(typeRef, type)) {
                                yield return td;
                            }
                        }
                    } else if (!type.IsInterface && td.BaseType != null && IsSameType(td.BaseType, type)) {
                        yield return td;
                    }
                }
            }
        }

        private static bool IsSameType(TypeReference typeRef, TypeDefinition type) {
            if (typeRef.FullName == type.FullName) {
                return true;
            }
            if (typeRef.Name != type.Name || type.Namespace != typeRef.Namespace) {
                return false;
            }
            if (typeRef.IsNested || type.IsNested) {
                if (!typeRef.IsNested || !type.IsNested || !IsSameType(typeRef.DeclaringType, type.DeclaringType)) {
                    return false;
                }
            }
            var genericTypeRef = typeRef as GenericInstanceType;
            if (genericTypeRef != null || type.HasGenericParameters) {
                if (genericTypeRef == null || !type.HasGenericParameters || genericTypeRef.GenericArguments.Count != type.GenericParameters.Count) {
                    return false;
                }
            }
            return true;
        }
    }
}