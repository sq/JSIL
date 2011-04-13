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

namespace JSIL {
    public class AssemblyTranslator {
        public string Translate (string assemblyPath) {
            var assembly = AssemblyDefinition.ReadAssembly(
                assemblyPath,
                new ReaderParameters {
                    ReadingMode = ReadingMode.Deferred
                }
            );

            using (var outputStream = new StringWriter()) {
                Translate(assembly, outputStream);

                return outputStream.ToString();
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
                new DynamicCallSites(context),
                new ReplacementFinder(context),
                new JumpTargeter(context),
                new GotoConverter(context)
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
