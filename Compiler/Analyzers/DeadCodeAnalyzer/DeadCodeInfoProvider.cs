using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.NRefactory.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Linker.Steps;

namespace JSIL.Compiler.Extensibility.DeadCodeAnalyzer {
    public class DeadCodeInfoProvider {
        private readonly List<AssemblyDefinition> assemblies;
        private readonly List<FieldDefinition> fields;
        private readonly List<MethodDefinition> methods;
        private readonly List<TypeDefinition> types;

        private readonly TypeMapStep typeMapStep = new TypeMapStep();
        private Configuration configuration;

        public DeadCodeInfoProvider() {
            types = new List<TypeDefinition>();
            methods = new List<MethodDefinition>();
            fields = new List<FieldDefinition>();
            assemblies = new List<AssemblyDefinition>();
        }

        public void SetConfiguration(Configuration configuration) {
            if (configuration == null)
                return;

            this.configuration = configuration;
        }

        public bool IsUsed(MemberReference member) {
            if (member is TypeReference) {
                TypeDefinition typeDefinition = member as TypeDefinition;
                if (typeDefinition != null && typeDefinition.IsInterface)
                    return true; // HACK: always include interfaces

                return types.Contains(member);
            }
            if (member is MethodReference) {
                return methods.Contains(member);
            }
            if (member is FieldReference) {
                return fields.Contains(member);
            }

            throw new ArgumentException("Unexpected member reference type");
        }

        public void WalkMethod(MethodDefinition method) {
            if (!AddMethod(method)) {
                return;
            }

            List<Instruction> foundInstructions = (from instruction in method.Body.Instructions
                                                   where method.HasBody && method.Body.Instructions != null && instruction.Operand != null
                                                   select instruction).ToList();

            IEnumerable<TypeDefinition> typesFound = from instruction in foundInstructions
                                                     let tRef = instruction.Operand as TypeReference
                                                     where tRef != null
                                                     let tRefResolved = tRef.Resolve()
                                                     where tRefResolved != null
                                                     select tRefResolved;

            IEnumerable<MethodDefinition> methodsFound = from instruction in foundInstructions
                                                         let mRef = instruction.Operand as MethodReference
                                                         where mRef != null && mRef.DeclaringType != null
                                                         let mRefResolved = mRef.Resolve()
                                                         where mRefResolved != null
                                                         select mRefResolved;

            IEnumerable<FieldDefinition> fieldsFound = from instruction in foundInstructions
                                                       let fRef = instruction.Operand as FieldReference
                                                       where fRef != null && fRef.FieldType != null
                                                       let fRefResolved = fRef.Resolve()
                                                       where fRefResolved != null
                                                       select fRefResolved;

            foreach (TypeDefinition typeDefinition in typesFound) {
                AddType(typeDefinition);
            }

            foreach (FieldDefinition fieldDefinition in fieldsFound) {
                AddField(fieldDefinition);
            }

            foreach (MethodDefinition methodDefinition in methodsFound) {
                if (methodDefinition != method) {
                    WalkMethod(methodDefinition);
                }
            }
        }

        public void ResolveVirtualMethods() {
            for (int i = 0; i < methods.Count; i++) {
                MethodDefinition method = methods[i];
                if (method.IsVirtual)
                    ResolveVirtualMethod(method);
            }
        }

        private void ResolveVirtualMethod(MethodDefinition method) {
            List<MethodDefinition> overrides = new List<MethodDefinition>();
            GetAllOverrides(method, overrides);
            foreach (MethodDefinition methodDefinition in overrides) {
                if (IsUsed(methodDefinition.DeclaringType)) {
                    WalkMethod(methodDefinition);
                }
            }
        }

        private void AddType(TypeReference type) {
            if (type == null) {
                return;
            }

            TypeDefinition resolvedType = type.Resolve();

            if (resolvedType != null && !types.Contains(resolvedType)) {
                types.Add(resolvedType);

                if (resolvedType.HasCustomAttributes) {
                    foreach (CustomAttribute attribute in resolvedType.CustomAttributes) {
                        if (attribute.HasConstructorArguments)
                            WalkMethod(attribute.Constructor.Resolve());
                    }
                }

                // HACK: force analyze static constructor
                MethodDefinition cctor = resolvedType.Methods.FirstOrDefault((m) => m.Name == ".cctor");
                if ((cctor != null) && (cctor.HasBody)) {
                    WalkMethod(cctor);
                }
            }
        }

        private bool AddMethod(MethodReference method) {
            if (method == null) {
                return false;
            }

            MethodDefinition resolvedMethod = method.Resolve();
            AddType(resolvedMethod.DeclaringType);
            AddType(resolvedMethod.ReturnType);

            if (resolvedMethod.HasBody && !methods.Contains(resolvedMethod)) {
                methods.Add(resolvedMethod);

                //if (resolvedMethod.HasCustomAttributes) {
                //    foreach (CustomAttribute attribute in resolvedMethod.CustomAttributes) {
                //        AddType(attribute.AttributeType);
                //    }
                //}

                return true;
            }

            return false;
        }

        private void AddField(FieldReference field) {
            if (field == null) {
                return;
            }

            FieldDefinition resolvedField = field.Resolve();

            if (!fields.Contains(resolvedField)) {
                fields.Add(resolvedField);
            }
        }

        public void AddAssemblies(AssemblyDefinition[] assemblies) {
            ModuleDefinition[] modules = (from assembly in assemblies
                                          from module in assembly.Modules
                                          select module).ToArray();

            foreach (ModuleDefinition module in modules) {
                typeMapStep.ProcessModule(module);

                foreach (var type in module.Types) {
                    ProcessWhitelist(type);
                }
            }
            this.assemblies.AddRange(assemblies);
        }

        private bool IsMemberWhiteListed(MemberReference member) {
            if (configuration.WhiteList == null)
                return false;

            foreach (var whiteListItem in configuration.WhiteList) {
                if (Regex.IsMatch(member.FullName, whiteListItem, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }

        private void ProcessWhitelist(MemberReference member) {
            if (member is TypeReference) {
                TypeDefinition type = (member as TypeReference).Resolve();
                
                if (type != null) {
                    if (IsMemberWhiteListed(type))
                        AddType(type);

                    if (type.HasNestedTypes) {
                        foreach (var nestedType in type.NestedTypes) {
                            ProcessWhitelist(nestedType);
                        }
                    }
                    if (type.HasMethods) {
                        foreach (var method in type.Methods) {
                            if (IsMemberWhiteListed(method))
                                ProcessWhitelist(method);
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

        private void GetAllOverrides(MethodDefinition method, List<MethodDefinition> deepOverrides) {
            if (method == null)
                return;

            List<MethodDefinition> overrides = typeMapStep.Annotations.GetOverrides(method);

            if (overrides == null)
                return;

            deepOverrides.AddRange(overrides);
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