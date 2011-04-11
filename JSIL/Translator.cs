using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ICSharpCode.Decompiler.Ast;
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
            context.Transforms.Add(new DynamicCallSites(context));

            var replacements = new ReplacementFinder(context);
            context.Transforms.Add(replacements);

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

        internal void TranslateModule (AssemblyDefinition assembly, ModuleDefinition module) {
            foreach (var type in module.Types)
                TranslateType(assembly, module, type);
        }

        internal void TranslateType (AssemblyDefinition assembly, ModuleDefinition module, TypeDefinition type) {
            /*
            using (var sw = new StringWriter()) {
                var language = new JavaScript();
                var languageWriter = new JavaScriptWriter(
                    language, new PlainTextFormatter(sw)
                );

                languageWriter.PushType(type);

                foreach (var method in type.Methods)
                    languageWriter.Write(method);

                languageWriter.PopType(type);

                Console.Write(sw.ToString());
            }
             */
        }
    }
}
