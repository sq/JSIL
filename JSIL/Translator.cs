using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
        public string Translate (string assemblyPath) {
            var readerParameters = new ReaderParameters {
                ReadingMode = ReadingMode.Deferred
            };

            var assemblyDir = Path.GetDirectoryName(assemblyPath);
            var pdbPath = Path.Combine(assemblyDir, Path.GetFileNameWithoutExtension(assemblyPath) + ".pdb");
            if (File.Exists(pdbPath)) {
                readerParameters.ReadSymbols = true;
                readerParameters.AssemblyResolver = new AssemblyResolver(new string [] {
                    assemblyDir
                });
                readerParameters.SymbolReaderProvider = new PdbReaderProvider();
                readerParameters.SymbolStream = File.OpenRead(pdbPath);
            } else {
            }

            try {
                var assembly = AssemblyDefinition.ReadAssembly(
                    assemblyPath, readerParameters
                );

                using (var outputStream = new StringWriter()) {
                    Translate(assembly, outputStream);

                    return outputStream.ToString();
                }
            } finally {
                if (readerParameters.SymbolStream != null)
                    readerParameters.SymbolStream.Dispose();
            }
        }

        internal void Translate (AssemblyDefinition assembly, TextWriter outputStream) {
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
                new ForeachTranslator(context),
                new JumpTargeter(context),
                new GotoConverter(context),
            };

            var decompiler = new AstBuilder(context);
            decompiler.AddAssembly(assembly);

            decompiler.RunTransformations();

            var astCompileUnit = decompiler.CompilationUnit;
            var output = new PlainTextOutput(outputStream);
            var outputFormatter = new TextOutputFormatter(output);
            var outputVisitor = new JavascriptOutputVisitor(outputFormatter);

            astCompileUnit.AcceptVisitor(new InsertParenthesesVisitor { InsertParenthesesForReadability = true }, null);
            astCompileUnit.AcceptVisitor(outputVisitor, null);
        }
    }
}
