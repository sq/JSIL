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
        public readonly Dictionary<string, TypeInfo> TypeInformation = new Dictionary<string, TypeInfo>();
        public readonly HashSet<string> GeneratedFiles = new HashSet<string>();
        public readonly List<Regex> IgnoredAssemblies = new List<Regex>();

        public event Action<string> StartedLoadingAssembly;
        public event Action<string> StartedDecompilingAssembly;
        public event Action<string> StartedTranslatingAssembly;

        public event Action<string, Exception> CouldNotLoadSymbols;
        public event Action<string, Exception> CouldNotResolveAssembly;

        public string OutputDirectory = Environment.CurrentDirectory;
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

        public TypeInfo GetTypeInformation (TypeReference type) {
            if (type == null) {
                Debugger.Break();
                throw new ArgumentNullException("type");
            }

            var fullName = type.FullName;

            TypeInfo result;
            if (!TypeInformation.TryGetValue(fullName, out result))
                TypeInformation[fullName] = result = new TypeInfo(type.ResolveOrThrow());

            return result;
        }

        TypeInfo ITypeInfoSource.Get (TypeReference type) {
            return GetTypeInformation(type);
        }

        public static bool IsIgnored (Mono.Cecil.ICustomAttributeProvider attributedNode) {
            foreach (var attribute in attributedNode.CustomAttributes) {
                var attributeName = attribute.AttributeType.FullName;

                switch (attributeName) {
                    case "JSIL.Meta.JSIgnore":
                        return true;
                }
            }

            var ms = attributedNode as IMetadataScope;
            var md = attributedNode as IMemberDefinition;
            string fullName;
            string name;

            if (md != null) {
                fullName = md.FullName;
                name = md.Name;
            } else if (ms != null) {
                fullName = name = ms.Name;
            } else {
                return false;
            }

            if (name.EndsWith("__BackingField"))
                return false;
            else if (name == "<Module>")
                return true;
            else if (name.StartsWith("<") && name.Contains("__SiteContainer"))
                return true;
            else if (name.StartsWith("CS$<"))
                return true;
            else if (name.Contains("<PrivateImplementationDetails>"))
                return true;

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

            output.NewLine();
            output.CloseBrace();

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

            bool isFirst = true;
            foreach (var em in GetTypeInformation(enm).EnumMembers.Values) {
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
            output.CloseBrace();
            output.Comma();
            output.Value(ILBlockTranslator.IsFlagsEnum(enm));
            output.NewLine();

            output.RPar();
            output.Semicolon();
            output.NewLine();
        }

        protected void ForwardDeclareType (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef) {
            if (IsIgnored(typedef))
                return;

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
            if (typedef.BaseType != null)
                baseClass = typedef.BaseType;

            bool isStatic = typedef.IsAbstract && typedef.IsSealed;

            if (isStatic) {
                output.DeclareNamespace(typedef.FullName);
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

            foreach (var nestedTypedef in typedef.NestedTypes)
                ForwardDeclareType(context, output, nestedTypedef);

            output.NewLine();
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

            foreach (var field in typedef.Fields) {
                if (IsIgnored(field))
                    continue;

                EmitFieldDefault(context, output, field);
            }

            foreach (var method in typedef.Methods)
                TranslateMethod(context, output, method);

            foreach (var methodGroup in info.MethodGroups)
                TranslateMethodGroup(context, output, methodGroup);

            var cctor = (from m in typedef.Methods where m.Name == ".cctor" select m).FirstOrDefault();
            if (cctor != null) {
                output.Identifier(cctor, true);
                output.LPar();
                output.RPar();
                output.Semicolon();
            }

            foreach (var iface in typedef.Interfaces) {
                output.Identifier(typedef);
                output.Dot();
                output.Identifier("prototype");
                output.Dot();
                output.Identifier("__ImplementInterface__");
                output.LPar();
                output.Identifier(iface);
                output.RPar();
                output.Semicolon();
            }

            output.NewLine();

            foreach (var nestedTypedef in typedef.NestedTypes)
                TranslateTypeDefinition(context, output, nestedTypedef);
        }

        protected void EmitFieldDefault (DecompilerContext context, JavascriptFormatter output, FieldDefinition field) {
            output.Identifier(field.DeclaringType);
            output.Dot();

            if (!field.IsStatic) {
                output.Identifier("prototype");
                output.Dot();
            }

            output.Identifier(field.Name);
            output.Token(" = ");

            if (field.HasConstant) {
                output.Value(field.Constant as dynamic);
            } else {
                output.DefaultValue(field.FieldType);
            }

            output.Semicolon();
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

        public static JSFunctionExpression TranslateMethod (DecompilerContext context, MethodDefinition method, ITypeInfoSource typeInfo, JavascriptFormatter output = null) {
            var oldMethod = context.CurrentMethod;
            try {
                context.CurrentMethod = method;

                var decompiler = new ILAstBuilder();
                var ilb = new ILBlock(decompiler.Build(method, true));

                var optimizer = new ILAstOptimizer();
                optimizer.Optimize(context, ilb);

                var allVariables = ilb.GetSelfAndChildrenRecursive<ILExpression>().Select(e => e.Operand as ILVariable)
                    .Where(v => v != null && !v.IsParameter).Distinct();

                NameVariables.AssignNamesToVariables(context, decompiler.Parameters, allVariables, ilb);

                var translator = new ILBlockTranslator(context, method, ilb, typeInfo, allVariables);
                var body = translator.Translate();

                var function = new JSFunctionExpression(
                    new JSMethod(method), 
                    from p in translator.ParameterNames select translator.Variables[p], 
                    body
                );

                new EmulateStructAssignment(
                    context.CurrentModule.TypeSystem, 
                    translator.CLR
                ).Visit(function);
                new IntroduceVariableDeclarations(
                    translator.Variables
                ).Visit(function);
                new IntroduceVariableReferences(
                    translator.JSIL,
                    translator.Variables,
                    translator.ParameterNames
                ).Visit(function);

                if (output != null) {
                    var emitter = new JavascriptAstEmitter(output, translator.JSIL);
                    emitter.Visit(function);
                }

                return function;
            } finally {
                context.CurrentMethod = oldMethod;
            }
        }

        protected void TranslateMethod (DecompilerContext context, JavascriptFormatter output, MethodDefinition method) {
            if (IsIgnored(method))
                return;
            if (!method.HasBody)
                return;

            output.Identifier(method.DeclaringType);
            if (!method.IsStatic) {
                output.Dot();
                output.Keyword("prototype");
            }
            output.Dot();

            MethodGroupItem mgi;
            if (GetTypeInformation(method.DeclaringType).MethodToMethodGroupItem.TryGetValue(method, out mgi))
                output.Identifier(mgi.MangledName);
            else
                output.Identifier(method.Name);

            output.Token(" = ");

            TranslateMethod(context, method, this, output);
        }
    }
}
