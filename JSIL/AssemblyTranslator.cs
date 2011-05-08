using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Transforms;
using Mono.Cecil;
using ICSharpCode.Decompiler;
using Mono.Cecil.Pdb;

namespace JSIL {
    public class AssemblyResolver : BaseAssemblyResolver {
        public readonly Dictionary<string, AssemblyDefinition> Cache = new Dictionary<string, AssemblyDefinition>();

        public AssemblyResolver (IEnumerable<string> dirs) {
            foreach (var dir in dirs)
                AddSearchDirectory(dir);
        }

        public override AssemblyDefinition Resolve (AssemblyNameReference name) {
            if (name == null)
                throw new ArgumentNullException("name");

            AssemblyDefinition assembly;
            if (Cache.TryGetValue(name.FullName, out assembly))
                return assembly;

            assembly = base.Resolve(name);
            Cache[name.FullName] = assembly;

            return assembly;
        }

        protected void RegisterAssembly (AssemblyDefinition assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var name = assembly.Name.FullName;
            if (Cache.ContainsKey(name))
                return;

            Cache[name] = assembly;
        }
    }

    public class AssemblyTranslator : ITypeInfoSource {
        public const int LargeMethodThreshold = 1024;

        public readonly Dictionary<string, TypeInfo> TypeInformation = new Dictionary<string, TypeInfo>();
        public readonly Dictionary<string, ModuleInfo> ModuleInformation = new Dictionary<string, ModuleInfo>();
        public readonly HashSet<string> GeneratedFiles = new HashSet<string>();
        public readonly List<Regex> IgnoredAssemblies = new List<Regex>();
        public readonly HashSet<string> DeclaredTypes = new HashSet<string>();

        public event Action<string> StartedLoadingAssembly;
        public event Action<string> StartedDecompilingAssembly;
        public event Action<string> StartedTranslatingAssembly;

        public event Action<string> StartedDecompilingMethod;
        public event Action<string> FinishedDecompilingMethod;

        public event Action<string, Exception> CouldNotLoadSymbols;
        public event Action<string, Exception> CouldNotResolveAssembly;
        public event Action<string, Exception> CouldNotDecompileMethod;

        public string OutputDirectory = Environment.CurrentDirectory;

        public bool EliminateTemporaries = true;
        public bool SimplifyOperators = true;
        public bool IncludeDependencies = true;
        public bool UseSymbols = true;

        protected static ReaderParameters GetReaderParameters (bool useSymbols, string mainAssemblyPath = null) {
            var readerParameters = new ReaderParameters {
                ReadingMode = ReadingMode.Deferred,
                ReadSymbols = useSymbols
            };

            if (mainAssemblyPath != null) {
                readerParameters.AssemblyResolver = new AssemblyResolver(new string[] { 
                    Path.GetDirectoryName(mainAssemblyPath) 
                });
            }

            if (useSymbols)
                readerParameters.SymbolReaderProvider = new PdbReaderProvider();

            return readerParameters;
        }

        public AssemblyDefinition[] LoadAssembly (string path) {
            var readerParameters = GetReaderParameters(UseSymbols, path);

            if (StartedLoadingAssembly != null)
                StartedLoadingAssembly(path);

            var assembly = AssemblyDefinition.ReadAssembly(
                path, readerParameters
            );

            var result = new List<AssemblyDefinition>();
            result.Add(assembly);

            if (IncludeDependencies) {
                var assemblyNames = new HashSet<string>();
                foreach (var module in assembly.Modules) {
                    foreach (var reference in module.AssemblyReferences) {
                        bool ignored = false;
                        foreach (var ia in IgnoredAssemblies) {
                            if (ia.IsMatch(reference.FullName)) {
                                ignored = true;
                                break;
                            }
                        }

                        if (ignored)
                            continue;
                        if (assemblyNames.Contains(reference.FullName))
                            continue;

                        var childParameters = new ReaderParameters {
                            ReadingMode = ReadingMode.Deferred,
                            ReadSymbols = true,
                            SymbolReaderProvider = new PdbReaderProvider()
                        };

                        if (StartedLoadingAssembly != null)
                            StartedLoadingAssembly(reference.FullName);

                        assemblyNames.Add(reference.FullName);
                        try {
                            result.Add(module.AssemblyResolver.Resolve(reference, readerParameters));
                        } catch (Exception ex) {
                            if (UseSymbols) {
                                try {
                                    result.Add(module.AssemblyResolver.Resolve(reference, GetReaderParameters(false, path)));
                                    if (CouldNotLoadSymbols != null)
                                        CouldNotLoadSymbols(reference.FullName, ex);
                                } catch (Exception ex2) {
                                    if (CouldNotResolveAssembly != null)
                                        CouldNotResolveAssembly(reference.FullName, ex2);
                                }
                            } else {
                                if (CouldNotResolveAssembly != null)
                                    CouldNotResolveAssembly(reference.FullName, ex);
                            }
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public void Translate (string assemblyPath, Stream outputStream = null) {
            if (GeneratedFiles.Contains(assemblyPath))
                return;

            var assemblies = LoadAssembly(assemblyPath);
            GeneratedFiles.Add(assemblyPath);

            if (outputStream == null) {
                if (!Directory.Exists(OutputDirectory))
                    Directory.CreateDirectory(OutputDirectory);

                foreach (var assembly in assemblies) {
                    var outputPath = Path.Combine(OutputDirectory, assembly.Name + ".js");

                    if (File.Exists(outputPath))
                        File.Delete(outputPath);


                    using (outputStream = File.OpenWrite(outputPath))
                        Translate(assembly, outputStream);
                }
            } else {
                foreach (var assembly in assemblies) {
                    var bytes = Encoding.ASCII.GetBytes(String.Format("// {0}{1}", assembly.Name, Environment.NewLine));
                    outputStream.Write(bytes, 0, bytes.Length);

                    Translate(assembly, outputStream);
                }
            }
        }

        internal void Translate (AssemblyDefinition assembly, Stream outputStream) {
            var context = new DecompilerContext(assembly.MainModule);

            context.Settings.YieldReturn = false;
            context.Settings.AnonymousMethods = true;
            context.Settings.QueryExpressions = false;
            context.Settings.LockStatement = false;
            context.Settings.FullyQualifyAmbiguousTypeNames = true;
            context.Settings.ForEachStatement = false;

            if (StartedDecompilingAssembly != null)
                StartedDecompilingAssembly(assembly.MainModule.FullyQualifiedName);

            var tw = new StreamWriter(outputStream, Encoding.ASCII);
            var formatter = new JavascriptFormatter(tw, this);

            foreach (var module in assembly.Modules)
                TranslateModule(context, formatter, module);

            tw.Flush();
        }

        public ModuleInfo GetModuleInformation (ModuleDefinition module) {
            if (module == null)
                throw new ArgumentNullException("module");

            var fullName = module.FullyQualifiedName;

            ModuleInfo result;
            if (!ModuleInformation.TryGetValue(fullName, out result))
                ModuleInformation[fullName] = result = new ModuleInfo(module);

            return result;
        }

        public TypeInfo GetTypeInformation (TypeReference type) {
            if (type == null)
                throw new ArgumentNullException("type");

            type = JSExpression.ResolveGenericType(type, type, type.DeclaringType);

            var fullName = type.FullName;

            TypeInfo result;
            if (!TypeInformation.TryGetValue(fullName, out result)) {
                var resolvedType = (type as TypeDefinition) ?? type.Resolve();

                if (resolvedType != null)
                    TypeInformation[fullName] = result = new TypeInfo(
                        GetModuleInformation(type.Module), resolvedType, type
                    );
                else
                    return null;
            }

            return result;
        }

        ModuleInfo ITypeInfoSource.Get (ModuleDefinition module) {
            return GetModuleInformation(module);
        }

        TypeInfo ITypeInfoSource.Get (TypeReference type) {
            return GetTypeInformation(type);
        }

        public bool IsIgnored (ModuleDefinition module) {
            var moduleInformation = GetModuleInformation(module);
            return moduleInformation.IsIgnored;
        }

        public bool IsIgnored (TypeReference type) {
            var typeInformation = GetTypeInformation(type);

            if (typeInformation != null)
                return typeInformation.IsIgnored;
            else
                return false;
        }

        public bool IsIgnored (MemberReference member) {
            var typeInformation = GetTypeInformation(member.DeclaringType);

            if (typeInformation != null)
                return typeInformation.IgnoredMembers.Contains(member);
            else
                return false;
        }

        protected void TranslateModule (DecompilerContext context, JavascriptFormatter output, ModuleDefinition module) {
            if (IsIgnored(module))
                return;

            context.CurrentModule = module;

            foreach (var typedef in module.Types)
                ForwardDeclareType(context, output, typedef);

            foreach (var typedef in module.Types)
                TranslateTypeDefinition(context, output, typedef);

            foreach (var typedef in module.Types)
                SealType(context, output, typedef);
        }

        protected string GetParent (TypeReference type) {
            var fullname = Util.EscapeIdentifier(type.FullName, false);
            var index = fullname.LastIndexOf('.');
            if (index < 0)
                return "this";
            else
                return fullname.Substring(0, index);
        }

        protected void TranslateInterface (DecompilerContext context, JavascriptFormatter output, TypeDefinition iface) {
            output.Identifier("JSIL.MakeInterface", true);
            output.LPar();
            output.NewLine();

            output.Identifier(GetParent(iface), true);
            output.Comma();
            output.Value(Util.EscapeIdentifier(iface.Name, false));
            output.Comma();
            output.Value(iface.FullName);
            output.Comma();

            output.OpenBrace();

            bool isFirst = true;
            foreach (var m in iface.Methods) {
                if (!isFirst) {
                    output.Comma();
                    output.NewLine();
                }

                output.Value(Util.EscapeIdentifier(m.Name));
                output.Token(": ");
                output.Identifier("Function");

                isFirst = false;
            }

            foreach (var p in iface.Properties) {
                if (!isFirst) {
                    output.Comma();
                    output.NewLine();
                }

                output.Value(Util.EscapeIdentifier(p.Name));
                output.Token(": ");
                output.Identifier("Property");

                isFirst = false;
            }

            output.NewLine();
            output.CloseBrace(false);

            output.RPar();
            output.Semicolon();
            output.NewLine();
        }

        protected void TranslateEnum (DecompilerContext context, JavascriptFormatter output, TypeDefinition enm) {
            output.Identifier("JSIL.MakeEnum", true);
            output.LPar();
            output.NewLine();

            output.Identifier(GetParent(enm), true);
            output.Comma();
            output.Value(Util.EscapeIdentifier(enm.Name, false));
            output.Comma();
            output.Value(enm.FullName);
            output.Comma();
            output.OpenBrace();

            var typeInformation = GetTypeInformation(enm);
            if (typeInformation == null)
                throw new InvalidOperationException();

            bool isFirst = true;
            foreach (var em in typeInformation.EnumMembers.Values) {
                if (!isFirst) {
                    output.Comma();
                    output.NewLine();
                }

                output.Identifier(em.Name);
                output.Token(": ");
                output.Value(em.Value);

                isFirst = false;
            }

            output.NewLine();
            output.CloseBrace(false);
            output.Comma();
            output.Value(typeInformation.IsFlagsEnum);
            output.NewLine();

            output.RPar();
            output.Semicolon();
            output.NewLine();
        }

        protected void ForwardDeclareType (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef) {
            if (IsIgnored(typedef))
                return;

            if (DeclaredTypes.Contains(typedef.FullName)) {
                Debug.WriteLine("Cycle in type references detected: {0}", typedef);
                return;
            }

            context.CurrentType = typedef;

            output.DeclareNamespace(typedef.Namespace);

            if (typedef.IsInterface) {
                TranslateInterface(context, output, typedef);
                return;
            } else if (typedef.IsEnum) {
                TranslateEnum(context, output, typedef);
                return;
            }

            var baseClass = typedef.Module.TypeSystem.Object;
            if (typedef.BaseType != null) {
                baseClass = typedef.BaseType;

                var resolved = baseClass.Resolve();
                if (!DeclaredTypes.Contains(baseClass.FullName) &&
                    (resolved != null) &&
                    (resolved.Module.Assembly == typedef.Module.Assembly)) {

                    ForwardDeclareType(context, output, resolved);
                }
            }

            bool isStatic = typedef.IsAbstract && typedef.IsSealed;

            if (isStatic) {
                output.Identifier("JSIL.MakeStaticClass", true);
                output.LPar();
                output.Identifier(GetParent(typedef), true);
                output.Comma();
                output.Value(Util.EscapeIdentifier(typedef.Name, true));
                output.Comma();
                output.Value(typedef.FullName);
                output.RPar();
                output.Semicolon();
            } else {
                if (typedef.IsValueType)
                    output.Identifier("JSIL.MakeStruct", true);
                else
                    output.Identifier("JSIL.MakeClass", true);

                output.LPar();
                if (!typedef.IsValueType) {
                    output.Identifier(baseClass);
                    output.Comma();
                }
                output.Identifier(GetParent(typedef), true);
                output.Comma();
                output.Value(Util.EscapeIdentifier(typedef.Name, true));
                output.Comma();
                output.Value(typedef.FullName);
                output.RPar();
                output.Semicolon();
            }

            DeclaredTypes.Add(typedef.FullName);

            foreach (var nestedTypedef in typedef.NestedTypes)
                ForwardDeclareType(context, output, nestedTypedef);

            output.NewLine();
        }

        protected void SealType (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef) {
            if (IsIgnored(typedef))
                return;

            context.CurrentType = typedef;

            if (typedef.IsInterface)
                return;
            else if (typedef.IsEnum)
                return;

            foreach (var nestedTypedef in typedef.NestedTypes)
                SealType(context, output, nestedTypedef);

            var typeInfo = GetTypeInformation(typedef);

            if (typeInfo.StaticConstructor != null) {
                output.Identifier("JSIL.SealType", true);
                output.LPar();
                output.Identifier(GetParent(typedef), true);
                output.Comma();
                output.Value(Util.EscapeIdentifier(typedef.Name, true));
                output.RPar();
                output.Semicolon();
            }
        }

        protected void TranslateTypeDefinition (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef) {
            if (IsIgnored(typedef))
                return;

            context.CurrentType = typedef;

            if (typedef.IsInterface)
                return;
            else if (typedef.IsEnum)
                return;

            var info = GetTypeInformation(typedef);
            if (info == null)
                throw new InvalidOperationException();

            foreach (var method in typedef.Methods)
                TranslateMethod(context, output, method);

            foreach (var methodGroup in info.MethodGroups)
                TranslateMethodGroup(context, output, methodGroup);

            foreach (var property in typedef.Properties)
                TranslateProperty(context, output, property);

            var interfaces = (from i in typedef.Interfaces
                              where !IsIgnored(i)
                              select i).ToArray();

            if (interfaces.Length > 0) {
                output.Identifier("JSIL.ImplementInterfaces", true);
                output.LPar();
                output.Identifier(typedef);
                output.Comma();
                output.OpenBracket(true);
                output.CommaSeparatedList(interfaces, ListValueType.Identifier);
                output.CloseBracket(true);
                output.RPar();
                output.Semicolon();
            }

            var structFields = 
                (from field in typedef.Fields
                where !IsIgnored(field) && !field.HasConstant &&
                    EmulateStructAssignment.IsStruct(field.FieldType) &&
                    !field.IsStatic
                select field).ToArray();

            if (structFields.Length > 0) {
                output.Identifier(typedef);
                output.Dot();
                output.Identifier("prototype");
                output.Dot();
                output.Identifier("__StructFields__");
                output.Token(" = ");
                output.OpenBrace();

                bool isFirst = true;
                foreach (var sf in structFields) {
                    if (!isFirst) {
                        output.Comma();
                        output.NewLine();
                    }

                    output.Identifier(sf.Name);
                    output.Token(": ");
                    output.Identifier(sf.FieldType);

                    isFirst = false;
                }

                output.NewLine();
                output.CloseBrace(false);
                output.Semicolon();
            }

            TranslateTypeStaticConstructor(context, output, typedef, info.StaticConstructor);

            output.NewLine();

            foreach (var nestedTypedef in typedef.NestedTypes)
                TranslateTypeDefinition(context, output, nestedTypedef);
        }

        protected void TranslateMethodGroup (DecompilerContext context, JavascriptFormatter output, MethodGroupInfo methodGroup) {
            int i = 0;

            output.Identifier("JSIL.OverloadedMethod", true);
            output.LPar();

            output.Identifier(methodGroup.DeclaringType);
            if (!methodGroup.IsStatic) {
                output.Dot();
                output.Keyword("prototype");
            }

            output.Comma();
            output.Value(Util.EscapeIdentifier(methodGroup.Name));
            output.Comma();
            output.OpenBracket(true);

            bool isFirst = true;
            i = 0;
            foreach (var method in methodGroup.Items) {
                if (IsIgnored(method.Method))
                    continue;

                if (!isFirst) {
                    output.Comma();
                    output.NewLine();
                }

                output.OpenBracket();
                output.Value(Util.EscapeIdentifier(method.MangledName));
                output.Comma();

                output.OpenBracket();
                output.CommaSeparatedList(
                    from p in method.Method.Parameters select p.ParameterType
                );
                output.CloseBracket();

                output.CloseBracket();
                isFirst = false;
            }

            output.CloseBracket(true);
            output.RPar();
            output.Semicolon();
        }

        internal JSFunctionExpression TranslateMethod (DecompilerContext context, MethodDefinition method, Action<JSFunctionExpression> bodyTransformer = null) {
            var oldMethod = context.CurrentMethod;
            try {
                context.CurrentMethod = method;

                if (method.Body.Instructions.Count > LargeMethodThreshold)
                    this.StartedDecompilingMethod(method.FullName);

                ILBlock ilb;
                var decompiler = new ILAstBuilder();
                var optimizer = new ILAstOptimizer();

                try {
                    ilb = new ILBlock(decompiler.Build(method, true));
                    optimizer.Optimize(context, ilb);
                } catch (Exception exception) {
                    if (CouldNotDecompileMethod != null)
                        CouldNotDecompileMethod(method.ToString(), exception);

                    return null;
                }

                var allVariables = ilb.GetSelfAndChildrenRecursive<ILExpression>().Select(e => e.Operand as ILVariable)
                    .Where(v => v != null && !v.IsParameter).Distinct();

                foreach (var v in allVariables)
                    if (v.Type.IsPointer)
                        return null;

                NameVariables.AssignNamesToVariables(context, decompiler.Parameters, allVariables, ilb);

                var translator = new ILBlockTranslator(
                    this, context, method, ilb, decompiler.Parameters, allVariables
                );
                var body = translator.Translate();

                if (body == null)
                    return null;

                var function = new JSFunctionExpression(
                    method,
                    translator.Variables,
                    from p in translator.ParameterNames select translator.Variables[p], 
                    body
                );

                if (EliminateTemporaries)
                    new EliminateSingleUseTemporaries(
                        context.CurrentModule.TypeSystem,
                        translator.Variables
                    ).Visit(function);

                new EmulateStructAssignment(
                    context.CurrentModule.TypeSystem,
                    translator.CLR
                ).Visit(function);

                new IntroduceVariableDeclarations(
                    translator.Variables,
                    this
                ).Visit(function);

                new IntroduceVariableReferences(
                    translator.JSIL,
                    translator.Variables,
                    translator.ParameterNames
                ).Visit(function);

                // Temporary elimination makes it possible to simplify more operators, so do it last
                if (SimplifyOperators)
                    new SimplifyOperators(
                        translator.JSIL,
                        context.CurrentModule.TypeSystem
                    ).Visit(function);

                if (bodyTransformer != null)
                    bodyTransformer(function);

                if (method.Body.Instructions.Count > LargeMethodThreshold)
                    this.FinishedDecompilingMethod(method.FullName);

                return function;
            } finally {
                context.CurrentMethod = oldMethod;
            }
        }

        protected static bool NeedsStaticConstructor (TypeReference type) {
            if (EmulateStructAssignment.IsStruct(type))
                return true;
            else if (type.IsPrimitive)
                return false;

            var resolved = type.Resolve();
            if (resolved == null)
                return true;

            if (resolved.IsEnum)
                return false;
            if (!resolved.IsValueType)
                return false;

            return true;
        }

        protected JSExpression TranslateField (FieldDefinition field) {
            JSDotExpression target;
            
            if (field.IsStatic)
                target = JSDotExpression.New(
                    new JSType(field.DeclaringType), new JSField(field)
                );
            else
                target = JSDotExpression.New(
                    new JSType(field.DeclaringType), new JSIdentifier("prototype"), new JSField(field)
                );

            if (field.HasConstant) {
                return new JSInvocationExpression(
                    JSDotExpression.New(
                        new JSIdentifier("Object"), new JSIdentifier("defineProperty")
                    ),
                    target.Target, target.Member.ToLiteral(),
                    new JSObjectExpression(new JSPairExpression(
                        JSLiteral.New("value"),
                        JSLiteral.New(field.Constant as dynamic)
                    ))
                );
            } else {
                return new JSBinaryOperatorExpression(
                    JSOperator.Assignment, target,
                    new JSDefaultValueLiteral(field.FieldType), 
                    field.FieldType
                );
            }
        }

        protected void TranslateTypeStaticConstructor (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef, MethodDefinition cctor) {
            var typeSystem = context.CurrentModule.TypeSystem;
            var fieldsToEmit =
                (from f in typedef.Fields
                 where f.IsStatic && NeedsStaticConstructor(f.FieldType)
                 select f).ToArray();

            // We initialize all static fields in the cctor to avoid ordering issues
            Action<JSFunctionExpression> fixupCctor = (f) => {
                int insertPosition = 0;

                foreach (var field in fieldsToEmit) {
                    var expr = TranslateField(field);
                    if (expr != null) {
                        var stmt = new JSExpressionStatement(expr);
                        f.Body.Statements.Insert(insertPosition++, stmt);
                    }
                }
            };

            // Default values for instance fields of struct types are handled
            //  by the instance constructor.
            // Default values for static fields of struct types are handled
            //  by the cctor.
            // Everything else is emitted inline.

            var emitter = new JavascriptAstEmitter(output, new JSILIdentifier(typeSystem), typeSystem, this);
            foreach (var f in typedef.Fields) {
                if (f.IsStatic && NeedsStaticConstructor(f.FieldType))
                    continue;

                if (EmulateStructAssignment.IsStruct(f.FieldType))
                    continue;

                var expr = TranslateField(f);
                if (expr != null)
                    emitter.Visit(new JSExpressionStatement(expr));
            }

            if ((cctor != null) && !IsIgnored(cctor)) {
                TranslateMethod(context, output, cctor, fixupCctor);
            } else if (fieldsToEmit.Length > 0) {
                var fakeCctor = new MethodDefinition(".cctor", Mono.Cecil.MethodAttributes.Static, typeSystem.Void);
                fakeCctor.DeclaringType = typedef;

                GetTypeInformation(typedef).StaticConstructor = fakeCctor;

                TranslateMethod(context, output, fakeCctor, fixupCctor);
            }
        }

        protected void TranslateMethod (DecompilerContext context, JavascriptFormatter output, MethodDefinition method, Action<JSFunctionExpression> bodyTransformer = null) {
            if (IsIgnored(method))
                return;
            if (!method.HasBody)
                return;

            var typeInfo = GetTypeInformation(method.DeclaringType);
            if (typeInfo == null)
                throw new InvalidOperationException();

            MetadataCollection methodMetadata;
            if (typeInfo.MemberMetadata.TryGetValue(method, out methodMetadata)) {
                if (methodMetadata.HasAttribute("JSIL.Meta.JSReplacement"))
                    return;
            }

            output.Identifier(method.DeclaringType);
            if (!method.IsStatic) {
                output.Dot();
                output.Keyword("prototype");
            }
            output.Dot();

            MethodGroupItem mgi;
            if (typeInfo.MethodToMethodGroupItem.TryGetValue(method, out mgi))
                output.Identifier(mgi.MangledName);
            else
                output.Identifier(method, false);

            output.Token(" = ");

            var emitter = new JavascriptAstEmitter(
                output, new JSILIdentifier(context.CurrentModule.TypeSystem),
                context.CurrentModule.TypeSystem, this
            );

            var function = TranslateMethod(context, method, (f) => {
                if (bodyTransformer != null)
                    bodyTransformer(f);

                emitter.Visit(f);
                output.Semicolon();
            });

            if (function == null) {
                output.Identifier("JSIL.UntranslatableFunction", true);
                output.LPar();
                output.Value(method.Name);
                output.RPar();
                output.Semicolon();
            }

            output.NewLine();
        }

        protected void TranslateProperty (DecompilerContext context, JavascriptFormatter output, PropertyDefinition property) {
            if (IsIgnored(property))
                return;

            output.Identifier("JSIL.MakeProperty", true);
            output.LPar();

            var isStatic = !(property.SetMethod ?? property.GetMethod).IsStatic;

            output.Identifier(property.DeclaringType);
            if (isStatic) {
                output.Dot();
                output.Keyword("prototype");
            }
            output.Comma();

            var over = (property.SetMethod ?? property.GetMethod).Overrides.FirstOrDefault();

            var separators = new char[] { '.', '+', '/', ':' };
            var lastDot = property.Name.LastIndexOfAny(separators);
            if (over != null) {
                // For some reason property.Name is fully qualified, unlike method.Name, when it privately implements an interface property.
                var declaringType = over.DeclaringType;
                var shortName = property.Name.Substring(lastDot + 1);
                var identifier = String.Format("{0}.{1}", declaringType.Name, shortName);
                output.Value(Util.EscapeIdentifier(identifier));
            } else {
                output.Value(property.Name);
            }

            output.Comma();
            output.NewLine();

            if (property.GetMethod != null) {
                output.Identifier(property.DeclaringType);
                if (isStatic) {
                    output.Dot();
                    output.Keyword("prototype");
                }
                output.Dot();
                output.Identifier(property.GetMethod, false);
            } else {
                output.Keyword("null");
            }

            output.Comma();

            if (property.SetMethod != null) {
                output.Identifier(property.DeclaringType);
                if (isStatic) {
                    output.Dot();
                    output.Keyword("prototype");
                }
                output.Dot();
                output.Identifier(property.SetMethod, false);
            } else {
                output.Keyword("null");
            }

            output.RPar();
            output.Semicolon();
        }
    }
}
