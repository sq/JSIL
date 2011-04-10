using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cecil.Decompiler.Languages;
using JSIL.Internal;
using JSIL.Languages;
using Mono.Cecil;

namespace JSIL {
    public class AssemblyTranslator {
        public void Translate (string assemblyPath) {
            var assembly = AssemblyDefinition.ReadAssembly(
                assemblyPath,
                new ReaderParameters {
                    ReadingMode = ReadingMode.Deferred
                }
            );
            Translate(assembly);
        }

        internal void Translate (AssemblyDefinition assembly) {
            foreach (var module in assembly.Modules)
                TranslateModule(assembly, module);
        }

        internal void TranslateModule (AssemblyDefinition assembly, ModuleDefinition module) {
            foreach (var type in module.Types)
                TranslateType(assembly, module, type);
        }

        internal void TranslateType (AssemblyDefinition assembly, ModuleDefinition module, TypeDefinition type) {
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
        }
    }
}
