using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JSIL.Internal;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Linker.Steps;

namespace JSIL.Compiler.Extensibility.DeadCodeAnalyzer {
    using ICSharpCode.Decompiler;
    using ICSharpCode.Decompiler.ILAst;

    public class DeadCodeInfoProvider {
        private class MethodUsageInfo
        {
            private bool _hasNonVirualUsage;

            private List<TypeDefinition> _virtualUseFromType;

            private MethodDefinition _method;

            public MethodUsageInfo(MethodDefinition method)
            {
                if (method.IsVirtual) {
                    _virtualUseFromType = new List<TypeDefinition>();

                    // We need always preserve interface method signature.
                    if (method.DeclaringType.IsInterface) {
                        _hasNonVirualUsage = true;
                    }
                }
                else {
                    _hasNonVirualUsage = true;
                }

                _method = method;
            }

            private static bool IsDerivedType(TypeDefinition baseType, TypeDefinition probableDerived)
            {
                if (baseType == null || baseType.IsInterface)
                {
                    return true;
                }

                TypeReference testType = probableDerived;
                while (testType != null)
                {
                    var testTypeDef = testType.Resolve();
                    if (testTypeDef == baseType)
                    {
                        return true;
                    }

                    testType = testTypeDef.BaseType;
                }

                return false;
            }

            public void RegisterNonVirtualUsage()
            {
                _hasNonVirualUsage = true;
            }

            public void AddVirtualUsageType(TypeReference typeReference)
            {
                if (typeReference == null) {
                    _virtualUseFromType.Clear();
                    _virtualUseFromType.Add(null);
                }
                else {
                    var typeDefToAdd = typeReference.Resolve();

                    if (_virtualUseFromType.All(typeDefInList => !IsDerivedType(typeDefInList, typeDefToAdd))) {
                        var typesToDelete =
                            _virtualUseFromType.Where(typeDefInList => IsDerivedType(typeDefToAdd, typeDefInList))
                                .ToList();
                        _virtualUseFromType.Add(typeDefToAdd);
                        foreach (var typeToDelete in typesToDelete) {
                            _virtualUseFromType.Remove(typeToDelete);
                        }
                    }
                }
            }

            public bool HasVirtualUsage
            {
                get
                {
                    return _virtualUseFromType != null && _virtualUseFromType.Count > 0;
                }
            }

            public bool PresentRealMethodUsage
            {
                get
                {
                    return _hasNonVirualUsage || 
                           (_virtualUseFromType.Count == 1 &&
                            _virtualUseFromType[0] == null || _virtualUseFromType[0] == _method.DeclaringType);
                }
            }

            public bool IsIncluded(TypeDefinition typeToTest)
            {
                return _virtualUseFromType.Any(typeDefinition => IsDerivedType(typeDefinition, typeToTest));
            }
        }

        private readonly HashSet<AssemblyDefinition> Assemblies = new HashSet<AssemblyDefinition>();
        private readonly HashSet<FieldDefinition> Fields = new HashSet<FieldDefinition>();
        private readonly Dictionary<MethodDefinition, MethodUsageInfo> Methods = new Dictionary<MethodDefinition,MethodUsageInfo>();
        private readonly HashSet<TypeDefinition> Types = new HashSet<TypeDefinition>();
        private readonly HashSet<PropertyDefinition> Properties = new HashSet<PropertyDefinition>();
        private readonly HashSet<EventDefinition> Events = new HashSet<EventDefinition>();

        private readonly TypeMapStep TypeMapStep = new TypeMapStep();
        private readonly List<Regex> WhiteListCache;
        private readonly Configuration Configuration; 

        public DeadCodeInfoProvider(Configuration configuration) {
            Configuration = configuration;
            if (configuration.WhiteList != null &&
                configuration.WhiteList.Count > 0) {
                WhiteListCache = new List<Regex>(configuration.WhiteList.Count);
                foreach (var pattern in configuration.WhiteList) {
                    var compiledRegex = new Regex(pattern, RegexOptions.ECMAScript | RegexOptions.Compiled);
                    WhiteListCache.Add(compiledRegex);
                }
            }
        }

        internal TypeInfoProvider TypeInfoProvider { get; set; }

        public bool IsUsed(MemberReference member)
        {
            bool? result = null;
            var typeReference = member as TypeReference;
            if (typeReference != null)
            {
                var definition = typeReference.Resolve();
                result = Types.Contains(definition);
            }

            var methodReference = member as MethodReference;
            if (methodReference != null) {
                var definition = methodReference.Resolve();
                result = Methods.ContainsKey(definition);
            }

            var fieldReference = member as FieldReference;
            if (fieldReference != null) {
                var definition = fieldReference.Resolve();
                result = Fields.Contains(definition);
            }

            var propertyReference = member as PropertyReference;
            if (propertyReference != null)
            {
                var definition = propertyReference.Resolve();
                result = Properties.Contains(definition);
            }

            var eventReference = member as EventReference;
            if (eventReference != null)
            {
                var definition = eventReference.Resolve();
                result = Events.Contains(definition);
            }

            if (!result.HasValue) {
                throw new ArgumentException("Unexpected member reference type");
            }

            // HACK to allow whitelist proxy members.
            if (!result.Value) {
                if (typeReference == null) {
                    typeReference = member.DeclaringType;
                }

                if (typeReference != null)
                {
                    var definition = typeReference.Resolve();
                    if (definition.HasCustomAttributes &&
                        definition.CustomAttributes.Any(item => item.AttributeType.FullName == "JSIL.Proxy.JSProxy")) {
                        return IsMemberWhiteListed(member);
                    }
                }
            }

            return result.Value;
        }

        public void WalkMethod(MethodReference methodReference, TypeReference targetType = null, bool virt = false)
        {
            if (!AddMethod(methodReference, targetType, virt)) {
                return;
            }

            var method = methodReference.Resolve();

            var context = new DecompilerContext(method.Module)
            {
                Settings =
                {
                    AnonymousMethods = true,
                    AsyncAwait = false,
                    YieldReturn = false,
                    QueryExpressions = false,
                    LockStatement = false,
                    FullyQualifyAmbiguousTypeNames = true,
                    ForEachStatement = false,
                    ExpressionTrees = false,
                    ObjectOrCollectionInitializers = false,
                },
                CurrentModule = method.Module,
                CurrentMethod = method,
                CurrentType = method.DeclaringType
            };

            List<Instruction> foundInstructions = (from instruction in method.Body.Instructions
                where method.HasBody && method.Body.Instructions != null && instruction.Operand != null
                select instruction).ToList();

            IEnumerable<TypeReference> typesFound = from instruction in foundInstructions
                let tRef = instruction.Operand as TypeReference
                where tRef != null
                select tRef;

            IEnumerable<FieldReference> fieldsFound = from instruction in foundInstructions
                let fRef = instruction.Operand as FieldReference
                where fRef != null && fRef.FieldType != null
                select fRef;

            foreach (TypeReference typeDefinition in typesFound) {
                AddType(typeDefinition);
            }

            foreach (FieldReference fieldDefinition in fieldsFound)
            {
                AddField(fieldDefinition);
            }

            ILBlock ilb = null;
            bool useSimpleMethodWalk = Configuration.NonAggressiveVirtualMethodElimination;
            if (!useSimpleMethodWalk) {
                var virtCalls = from instruction in foundInstructions
                    let mRef = instruction.Operand as MethodReference
                    where (instruction.OpCode == OpCodes.Callvirt || instruction.OpCode == OpCodes.Ldvirtftn) &&
                          mRef.Resolve().IsVirtual
                    select instruction;

                if (virtCalls.Any()) {
                    try {
                        var decompiler = new ILAstBuilder();
                        var optimizer = new ILAstOptimizer();

                        ilb = new ILBlock(decompiler.Build(method, false, context));
                        optimizer.Optimize(context, ilb);
                    }
                    catch (Exception) {
                        useSimpleMethodWalk = true;

                    }
                }
                else {
                    useSimpleMethodWalk = true;
                }
            }

            if (!useSimpleMethodWalk) {
                var expressions = ilb.GetSelfAndChildrenRecursive<ILExpression>();

                foreach (var ilExpression in expressions) {
                    var mRef = ilExpression.Operand as MethodReference;
                    if (mRef != null && mRef.DeclaringType != null) {
                        bool isVirtual = false;
                        TypeReference thisArg = null;

                        switch (ilExpression.Code)
                        {
                            case ILCode.Ldftn:
                            case ILCode.Newobj:
                            case ILCode.Jmp:
                            case ILCode.Call:
                            case ILCode.CallGetter:
                            case ILCode.CallSetter:
                                break;

                            case ILCode.CallvirtGetter:
                            case ILCode.CallvirtSetter:
                            case ILCode.Callvirt:
                            case ILCode.Ldvirtftn:
                                isVirtual = true;
                                thisArg = ilExpression.Arguments.Count >0 ? ilExpression.Arguments[0].InferredType : null;
                                break;

                            case ILCode.Ldtoken:
                                isVirtual = true;
                                break;
                        }

                        WalkMethod(mRef, thisArg, isVirtual);
                    }
                }                
            }
            else {
                IEnumerable<MethodReference> methodsFound = from instruction in foundInstructions
                                                            let mRef = instruction.Operand as MethodReference
                                                            where mRef != null && mRef.DeclaringType != null
                                                            select mRef;

                foreach (MethodReference methodDefinition in methodsFound) {
                    if (methodDefinition != method) {
                        WalkMethod(methodDefinition, null, true);
                    }
                }
            }
        }

        public void AddAssemblies(IEnumerable<AssemblyDefinition> assemblies)
        {
            IEnumerable<ModuleDefinition> modules = from assembly in assemblies
                                                    from module in assembly.Modules
                                                    select module;

            foreach (ModuleDefinition module in modules)
            {
                TypeMapStep.ProcessModule(module);

                foreach (var type in module.Types)
                {
                    ProcessWhiteList(type);
                }
            }

            Assemblies.UnionWith(assemblies);
        }

        public void FinishProcessing()
        {
            RepeatUntilStable(() =>
            {
                ResolveVirtualMethodsCycle();
                BuildPropertiesAndEventsList();
                ProcessMetaAttributes();
            });

            var methodsToRemove =
                Methods
                    .Where(item => !item.Value.PresentRealMethodUsage)
                    .Select(item => item.Key)
                    .ToList();

            foreach (var methodDefinition in methodsToRemove) {
                Methods.Remove(methodDefinition);
            }
        }

        private void RepeatUntilStable(Action action)
        {
            var inintialMemberCount = 0;
            var endMemberCount = 0;
            do
            {
                inintialMemberCount = Fields.Count + Methods.Count + Types.Count + Properties.Count + Events.Count;
                action();
                endMemberCount = Fields.Count + Methods.Count + Types.Count + Properties.Count + Events.Count;
            } while (endMemberCount != inintialMemberCount);
        }

        private void ResolveVirtualMethodsCycle()
        {
            RepeatUntilStable(ResolveVirtualMethods);

            if (!Configuration.NonAggressiveVirtualMethodElimination)
            {
                foreach (var type in Types.Where(item => !item.IsInterface))
                {
                    var baseMap = new HashSet<MethodDefinition>();

                    var currentType = type;
                    do
                    {
                        foreach (var method in currentType.Methods.Where(item => item.IsVirtual))
                        {
                            MethodUsageInfo methodUsageInfo;

                            if (Methods.TryGetValue(method, out methodUsageInfo))
                            {
                                if (!baseMap.Contains(method) && methodUsageInfo.IsIncluded(type))
                                {

                                    methodUsageInfo.RegisterNonVirtualUsage();
                                }
                            }

                            var localBase = TypeMapStep.Annotations.GetBaseMethods(method);
                            if (localBase != null)
                            {
                                baseMap.UnionWith(localBase);
                            }
                        }

                        currentType = currentType.BaseType != null ? currentType.BaseType.Resolve() : null;
                    } while (currentType != null);
                }
            }
        }

        private void ProcessMetaAttributes()
        {
            foreach (var definition in Types.ToList()) {
                if (definition.HasCustomAttributes) {
                    ProcessMetaAttributes(definition.CustomAttributes);
                }

                if (definition.HasGenericParameters) {
                    foreach (var parameter in definition.GenericParameters) {
                        if (parameter.HasCustomAttributes) {
                            ProcessMetaAttributes(parameter.CustomAttributes);
                        }
                    }
                }
            }

            foreach (var definition in Fields.ToList()) {
                if (definition.HasCustomAttributes) {
                    ProcessMetaAttributes(definition.CustomAttributes);
                }
            }

            foreach (var methodPair in Methods.ToList()) {
                if (methodPair.Value.PresentRealMethodUsage) {
                    if (methodPair.Key.HasCustomAttributes) {
                        ProcessMetaAttributes(methodPair.Key.CustomAttributes);
                    }

                    if (methodPair.Key.HasParameters) {
                        foreach (var parameter in methodPair.Key.Parameters) {
                            if (parameter.HasCustomAttributes) {
                                ProcessMetaAttributes(parameter.CustomAttributes);
                            }
                        }
                    }

                    if (methodPair.Key.HasGenericParameters) {
                        foreach (var parameter in methodPair.Key.GenericParameters) {
                            if (parameter.HasCustomAttributes) {
                                ProcessMetaAttributes(parameter.CustomAttributes);
                            }
                        }
                    }

                    if (methodPair.Key.MethodReturnType.HasCustomAttributes) {
                        ProcessMetaAttributes(methodPair.Key.MethodReturnType.CustomAttributes);
                    }
                }
            }

            foreach (var definition in Properties.ToList()) {
                if (definition.HasCustomAttributes) {
                    ProcessMetaAttributes(definition.CustomAttributes);
                }
            }

            foreach (var definition in Events.ToList()) {
                if (definition.HasCustomAttributes) {
                    ProcessMetaAttributes(definition.CustomAttributes);
                }
            }
        }

        private void ProcessMetaAttributes(IEnumerable<CustomAttribute> attributes)
        {
            foreach (CustomAttribute attribute in attributes) {
                // Think more about proxy processing.
                if (attribute.AttributeType.Namespace == "JSIL.Proxy" ||
                    attribute.AttributeType.Namespace == "JSIL.Meta") {
                    continue;
                }

                if (!Types.Contains(attribute.AttributeType.Resolve())) {
                    continue;
                }

                var attributeDef = attribute.AttributeType.Resolve();

                if (attribute.Constructor != null) {
                    WalkMethod(attribute.Constructor);
                }

                if (attribute.ConstructorArguments != null) {
                    foreach (var customAttributeArgument in attribute.ConstructorArguments) {
                        ProcessCustomAttributeArgument(customAttributeArgument);
                    }
                }

                if (attribute.Properties != null) {
                    foreach (var argument in attribute.Properties) {
                        ProcessCustomAttributeArgument(argument.Argument);

                        var currentType = attributeDef;
                        do {
                            var members = attributeDef.Properties.Where(item => item.Name == argument.Name).ToArray();
                            if (members.Any()) {
                                foreach (var property in members) {
                                    if (Configuration.NonAggressiveVirtualMethodElimination) {
                                        WalkMethod(property.GetMethod, null, true);
                                    }
                                    else {
                                        var currentMethodSearchType = attributeDef;
                                        do {
                                            var foundMethod =
                                                currentMethodSearchType.Methods.FirstOrDefault(
                                                    item => TypeMapStep.MethodMatch(item, property.GetMethod));
                                            if (foundMethod != null) {
                                                WalkMethod(property.GetMethod, null, false);
                                                break;
                                            }

                                            currentMethodSearchType = currentMethodSearchType.BaseType != null
                                                ? currentMethodSearchType.BaseType.Resolve()
                                                : null;
                                        } while (currentMethodSearchType != null);
                                    }
                                }
                                break;
                            }

                            currentType = currentType.BaseType != null ? currentType.BaseType.Resolve() : null;
                        } while (currentType != null);
                    }
                }

                if (attribute.Fields != null) {
                    foreach (var argument in attribute.Fields) {
                        ProcessCustomAttributeArgument(argument.Argument);

                        var currentType = attributeDef;
                        do {
                            var members = attributeDef.Fields.Where(item => item.Name == argument.Name).ToArray();
                            if (members.Any()) {
                                foreach (var field in members) {
                                    Fields.Add(field);
                                }
                                break;
                            }

                            currentType = currentType.BaseType != null ? currentType.BaseType.Resolve() : null;
                        } while (currentType != null);
                    }
                }
            }
        }

        private void ProcessCustomAttributeArgument(CustomAttributeArgument argument)
        {
            if (argument.Value is CustomAttributeArgument) {
                ProcessCustomAttributeArgument((CustomAttributeArgument)argument.Value);
            }
            else if (argument.Value is CustomAttributeArgument[]) {
                var values = (CustomAttributeArgument[])argument.Value;
                foreach (var value in values) {
                    ProcessCustomAttributeArgument(value);                   
                }
            }
            else if (argument.Value is TypeReference)
            {
                var type = ((TypeReference)argument.Value).Resolve();
                if (type != null) {
                    Types.Add(type);
                }
            }
            else if (argument.Value != null){
                var type = argument.Type.Resolve();
                if (type != null)
                {
                    Types.Add(type);
                }
            }
        }

        private void BuildPropertiesAndEventsList()
        {
            foreach (var typeDefinition in Types) {
                foreach (var defenition in typeDefinition.Properties) {
                    if (defenition != null) {
                        if (defenition.GetMethod != null && Methods.ContainsKey(defenition.GetMethod)) {
                            Properties.Add(defenition);
                            continue;
                        }

                        if (defenition.SetMethod != null && Methods.ContainsKey(defenition.SetMethod)) {
                            Properties.Add(defenition);
                            continue;
                        }

                        if (defenition.OtherMethods != null &&
                            defenition.OtherMethods.Any(method => Methods.ContainsKey(method))) {
                            Properties.Add(defenition);
                            continue;
                        }
                    }
                }

                foreach (var defenition in typeDefinition.Events) {
                    if (defenition != null) {
                        if (defenition.AddMethod != null && Methods.ContainsKey(defenition.AddMethod)) {
                            Events.Add(defenition);
                            continue;
                        }

                        if (defenition.RemoveMethod != null && Methods.ContainsKey(defenition.RemoveMethod)) {
                            Events.Add(defenition);
                            continue;
                        }

                        if (defenition.InvokeMethod != null && Methods.ContainsKey(defenition.InvokeMethod)) {
                            Events.Add(defenition);
                            continue;
                        }

                        if (defenition.OtherMethods != null &&
                            defenition.OtherMethods.Any(method => Methods.ContainsKey(method))) {
                            Events.Add(defenition);
                            continue;
                        }
                    }
                }
            }
        }

        private void ResolveVirtualMethods()
        {
            var tempMethods = Methods.Where(pair => pair.Value.HasVirtualUsage).ToList();

            foreach (var pair in tempMethods)
            {
                ResolveVirtualMethod(pair.Key, pair.Value);
            }
        }

        private void ResolveVirtualMethod(MethodDefinition method, MethodUsageInfo usageInfo) {
            var overrides = new HashSet<MethodDefinition>();
            GetAllOverrides(method, overrides);
            foreach (MethodDefinition methodDefinition in overrides) {
                if (IsUsed(methodDefinition.DeclaringType)) {
                    if (usageInfo.IsIncluded(methodDefinition.DeclaringType.Resolve()))
                    {
                        WalkMethod(methodDefinition);
                    }
                }
            }
        }

        private void AddType(TypeReference type) {
            if (type == null || IsIgnored(type))
            {
                return;
            }

            if (type.IsGenericInstance)
            {
                var genericType = (GenericInstanceType)type;
                foreach (var genericArgument in genericType.GenericArguments) {
                    AddType(genericArgument);
                }
            }

            TypeDefinition resolvedType = type.Resolve();
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

            if (Types.Add(resolvedType))
            {
                AddType(resolvedType.BaseType);

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
                if (methodInfo.IsIgnored)
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

        private bool AddMethod(MethodReference method, TypeReference targetType, bool isVirt) {
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

            AddType(method.DeclaringType);
            AddType(method.ReturnType);
            ProcessMarshallInfo(method.MethodReturnType.MarshalInfo);
            foreach (var parameterDefinition in method.Parameters)
            {
                AddType(parameterDefinition.ParameterType);
                ProcessMarshallInfo(parameterDefinition.MarshalInfo);
            }

            MethodDefinition resolvedMethod = method.Resolve();

            if (resolvedMethod == null)
            {
                return false;
            }

            if (AddMethodToList(resolvedMethod, targetType, isVirt) && resolvedMethod.HasBody)
            {               
                if (IsStubOrExternal(method))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private void ProcessMarshallInfo(MarshalInfo marshalInfo)
        {
            var customMarshalInfo = marshalInfo as CustomMarshalInfo;
            if (customMarshalInfo != null) {
                AddType(customMarshalInfo.ManagedType);
            }
        }

        private bool AddMethodToList(MethodDefinition method, TypeReference targetType, bool isVirt)
        {
            MethodUsageInfo virualCallsList;
            bool found = true;
            if (!Methods.TryGetValue(method, out virualCallsList)) {
                found = false;
                virualCallsList = new MethodUsageInfo(method);
                Methods.Add(method, virualCallsList);
            }

            if (method.IsVirtual) {
                if (!isVirt) {
                    virualCallsList.RegisterNonVirtualUsage();
                }
                else {
                    virualCallsList.AddVirtualUsageType(targetType);
                }
            }

            return !found;
        }

        private void AddField(FieldReference field) {
            if (field == null || IsIgnored(field)) {
                return;
            }

            AddType(field.FieldType);
            AddType(field.DeclaringType);

            FieldDefinition resolvedField = field.Resolve();

            if (resolvedField != null) {
                Fields.Add(resolvedField);
                ProcessMarshallInfo(resolvedField.MarshalInfo);
            }
        }

        private bool IsMemberWhiteListed(MemberReference member)
        {
            if (WhiteListCache != null) {
                if (WhiteListCache.Any(regex => regex.IsMatch(member.FullName))) {
                    return true;
                }
            }

            MetadataCollection customAttributes = null;

            if (member is TypeReference) {
                var type = TypeInfoProvider.GetTypeInformation((TypeReference) member);
                if (type != null) {
                    customAttributes = type.Metadata;
                }
            }
            else if (member is MethodReference) {
                var method = TypeInfoProvider.GetMemberInformation<MethodInfo>(member);
                if (method != null) {
                    customAttributes = method.Metadata;
                }
            }
            else if (member is FieldReference) {
                var field = TypeInfoProvider.GetMemberInformation<FieldInfo>(member);
                if (field != null) {
                    customAttributes = field.Metadata;
                }
            }

            if (customAttributes != null && customAttributes.HasAttribute("JSIL.Meta.JSDeadCodeEleminationEntryPoint")) {
                return true;
            }

            if (member.DeclaringType != null) {
                var declaringType = TypeInfoProvider.GetTypeInformation(member.DeclaringType);
                if (declaringType.Metadata.HasAttribute("JSIL.Meta.JSDeadCodeEleminationClassEntryPoint")) {
                    return true;
                }

                while (declaringType != null) {
                    if (declaringType.Metadata.HasAttribute("JSIL.Meta.JSDeadCodeEleminationHierarchyEntryPoint")) {
                        return true;
                    }

                    declaringType = declaringType.BaseClass;
                }
            }

            return false;
        }

        private void ProcessWhiteList(MemberReference member)
        {
            if (member is TypeReference) {
                TypeDefinition type = (member as TypeReference).Resolve();

                if (type != null) {
                    if (IsMemberWhiteListed(type)) {
                        AddType(type);
                    }

                    if (type.HasNestedTypes) {
                        foreach (var nestedType in type.NestedTypes) {
                            ProcessWhiteList(nestedType);
                        }
                    }

                    if (type.HasMethods) {
                        foreach (var method in type.Methods) {
                            ProcessWhiteList(method);
                        }
                    }

                    if (type.HasFields) {
                        foreach (var field in type.Fields) {
                            ProcessWhiteList(field);
                        }
                    }
                }

                return;
            }

            if (member is MethodReference) {
                if (IsMemberWhiteListed(member)) {
                    var definition = member as MethodDefinition;
                    if (definition.IsVirtual) {
                        WalkMethod(definition, null, true);
                    }
                    else {
                        WalkMethod(definition);
                    }
                }

                return;
            }

            if (member is FieldReference) {
                if (IsMemberWhiteListed(member)) {
                    AddField(member as FieldReference);
                }

                return;
            }

            throw new ArgumentException("Unexpected member reference type");
        }

        private void GetAllOverrides(MethodDefinition method, HashSet<MethodDefinition> deepOverrides) {
            if (method == null) {
                return;
            }

            HashSet<MethodDefinition> overrides = TypeMapStep.Annotations.GetOverrides(method);

            if (overrides == null) {
                return;
            }

            deepOverrides.UnionWith(overrides);
            foreach (MethodDefinition overrideMethod in overrides) {
                GetAllOverrides(overrideMethod, deepOverrides);
            }
        }
    }
}