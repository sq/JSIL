using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.Ast.Transforms;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;
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

            if (StartedDecompilingAssembly != null)
                StartedDecompilingAssembly(assembly.MainModule.FullyQualifiedName);

            var tw = new StreamWriter(outputStream, Encoding.ASCII);
            var formatter = new JavascriptFormatter(tw);

            if (false) {
                context.Transforms = new IAstTransform[] {
				    new PushNegation(),
				    new DelegateConstruction(context),
				    new PatternStatementTransform(context),
				    new ReplaceMethodCallsWithOperators(),
				    new IntroduceUnsafeModifier(),
				    new AddCheckedBlocks(),
				    new DeclareVariables(context), // should run after most transforms that modify statements
				    new ConvertConstructorCallIntoInitializer(), // must run after DeclareVariables
				    new IntroduceUsingDeclarations(context),
                    new OverloadRenamer(context),
                    new DynamicCallSites(context),
                    new ReplacementFinder(context),
                    new EventOperatorConverter(context),
                    new PropertyAccessConverter(context),
                    new ParameterModifierTransformer(context),
                    new BlockTranslator(context),
                    new JumpTargeter(context),
                    new GotoConverter(context),
                };

                var decompiler = new AstBuilder(context);

                decompiler.RunTransformations();

                if (StartedTranslatingAssembly != null)
                    StartedTranslatingAssembly(assembly.MainModule.FullyQualifiedName);

                var astCompileUnit = decompiler.CompilationUnit;
                var outputVisitor = new JavascriptOutputVisitor(formatter.PlainTextFormatter);

                astCompileUnit.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = true }, null);
                astCompileUnit.AcceptVisitor(outputVisitor, null);
            } else {
                foreach (var module in assembly.Modules)
                    TranslateModule(context, formatter, module);
            }

            tw.Flush();
        }

        public TypeInfo GetTypeInformation (TypeReference type) {
            var fullName = type.FullName;

            TypeInfo result;
            if (!TypeInformation.TryGetValue(fullName, out result))
                TypeInformation[fullName] = result = new TypeInfo(type.ResolveOrThrow());

            return result;
        }

        TypeInfo ITypeInfoSource.Get (TypeReference type) {
            return GetTypeInformation(type);
        }

        protected bool IsIgnored (Mono.Cecil.ICustomAttributeProvider attributedNode) {
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

            if (name.StartsWith("CS$<"))
                return true;
            else if (name.StartsWith("<"))
                return true;

            return false;
        }

        protected void TranslateModule (DecompilerContext context, JavascriptFormatter output, ModuleDefinition module) {
            if (IsIgnored(module))
                return;

            context.CurrentModule = module;

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
            output.Identifier(GetParent(iface), true);
            output.Comma();
            output.Value(iface.Name);
            output.RPar();
            output.Semicolon();
            output.NewLine();
        }

        protected void TranslateEnum (DecompilerContext context, JavascriptFormatter output, TypeDefinition enm) {
            output.Identifier("JSIL.MakeEnum", true);
            output.LPar();
            output.Identifier(GetParent(enm), true);
            output.Comma();
            output.Value(enm.Name);
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

            output.CloseBrace();
            output.RPar();
            output.Semicolon();
            output.NewLine();
        }

        protected void TranslateTypeDefinition (DecompilerContext context, JavascriptFormatter output, TypeDefinition typedef) {
            if (IsIgnored(typedef))
                return;

            context.CurrentType = typedef;

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
                output.Identifier(typedef);
                output.Token(" = {}");
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
                output.Value(typedef.Name);
                output.RPar();
            }
            output.Semicolon();

            foreach (var field in typedef.Fields) {
                if (IsIgnored(field))
                    continue;

                EmitFieldDefault(context, output, field);
            }

            foreach (var methodGroup in (
                from m in typedef.Methods
                group m by m.Name into mg
                select mg
            )) {
                if (methodGroup.Count() == 1)
                    TranslateMethod(context, output, methodGroup.First());
                else
                    TranslateMethodGroup(context, output, methodGroup);
            }

            var cctor = (from m in typedef.Methods where m.Name == ".cctor" select m).FirstOrDefault();
            if (cctor != null) {
                output.Identifier(cctor, true);
                output.LPar();
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

            output.DefaultValue(field.FieldType);
            output.Semicolon();
        }

        protected void TranslateMethodGroup (DecompilerContext context, JavascriptFormatter output, IGrouping<string, MethodDefinition> methodGroup) {
            int i = 0;
            foreach (var method in methodGroup)
                TranslateMethod(context, output, method, String.Format("_{0}", i++));

            output.Identifier("JSIL.OverloadedMethod", true);
            output.LPar();

            var firstMethod = methodGroup.First();

            output.Identifier(firstMethod.DeclaringType);
            if (!firstMethod.IsStatic) {
                output.Dot();
                output.Keyword("prototype");
            }

            output.Comma();
            output.Value(firstMethod.Name);
            output.Comma();
            output.OpenBracket(true);

            bool isFirst = true;
            i = 0;
            foreach (var method in methodGroup) {
                string name = String.Format("{0}_{1}", method.Name, i++);

                if (IsIgnored(method))
                    continue;

                if (!isFirst) {
                    output.Comma();
                    output.NewLine();
                }

                output.OpenBracket();
                output.Value(name);
                output.Comma();

                output.OpenBracket();
                output.CommaSeparatedList(
                    from p in method.Parameters select p.ParameterType
                );
                output.CloseBracket();

                output.CloseBracket();
                isFirst = false;
            }

            output.CloseBracket(true);
            output.RPar();
            output.Semicolon();
        }

        protected void TranslateMethod (DecompilerContext context, JavascriptFormatter output, MethodDefinition method, string nameSuffix = null) {
            if (IsIgnored(method))
                return;
            if (!method.HasBody)
                return;

            context.CurrentMethod = method;

            var decompiler = new ILAstBuilder();
            var ilb = new ILBlock(decompiler.Build(method, true));

            var optimizer = new ILAstOptimizer();
            optimizer.Optimize(context, ilb);

            var allVariables = ilb.GetSelfAndChildrenRecursive<ILExpression>().Select(e => e.Operand as ILVariable)
				.Where(v => v != null && !v.IsParameter).Distinct();

            NameVariables.AssignNamesToVariables(context, decompiler.Parameters, allVariables, ilb);

            output.Identifier(method.DeclaringType);
            if (!method.IsStatic) {
                output.Dot();
                output.Keyword("prototype");
            }
            output.Dot();
            output.Identifier(method.Name);
            if (nameSuffix != null)
                output.Identifier(nameSuffix);

            output.Token(" = ");

            output.OpenFunction(from p in method.Parameters select p.Name);

            var translator = new ILBlockTranslator(context, method, ilb, output, this);
            translator.Translate();

            output.CloseBrace(true);
        }
    }
}
