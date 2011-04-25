using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.Ast.Transforms;
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

    public class AssemblyTranslator {
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

        protected static ReaderParameters GetReaderParameters (bool useSymbols) {
            var readerParameters = new ReaderParameters {
                ReadingMode = ReadingMode.Deferred,
                ReadSymbols = useSymbols
            };

            if (useSymbols)
                readerParameters.SymbolReaderProvider = new PdbReaderProvider();

            return readerParameters;
        }

        public AssemblyDefinition[] LoadAssembly (string path) {
            var readerParameters = GetReaderParameters(UseSymbols);

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
                                if (CouldNotLoadSymbols != null)
                                    CouldNotLoadSymbols(reference.FullName, ex);

                                try {
                                    result.Add(module.AssemblyResolver.Resolve(reference, GetReaderParameters(false)));
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

            if (StartedDecompilingAssembly != null)
                StartedDecompilingAssembly(assembly.MainModule.FullyQualifiedName);

            var decompiler = new AstBuilder(context);
            decompiler.AddAssembly(assembly);

            decompiler.RunTransformations();

            if (StartedTranslatingAssembly != null)
                StartedTranslatingAssembly(assembly.MainModule.FullyQualifiedName);

            var tw = new StreamWriter(outputStream, Encoding.ASCII);
            var astCompileUnit = decompiler.CompilationUnit;
            var output = new PlainTextOutput(tw);
            var outputFormatter = new TextOutputFormatter(output);
            var outputVisitor = new JavascriptOutputVisitor(outputFormatter);

            astCompileUnit.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = true }, null);
            astCompileUnit.AcceptVisitor(outputVisitor, null);
            tw.Flush();
        }
    }
}
