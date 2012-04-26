using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JSIL.Translator;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace JSIL.Try {
    public static class SnippetCompiler {
        public static readonly string[] AssemblyReferences = new[] {
            "mscorlib.dll", "System.dll", 
            "System.Core.dll", "System.Xml.dll", 
            "Microsoft.CSharp.dll",
        };

        /// <summary>
        /// Compiles the provided C# and then translates it into JavaScript.
        /// On success, returns the JS. On failure, throws.
        /// </summary>
        public static string Compile (string csharp) {
            var tempPath = Path.Combine(Path.GetTempPath(), "JSIL.Try");
            Directory.CreateDirectory(tempPath);

            Assembly resultAssembly = null;
            string resultPath = null;
            string compilerOutput = null;

            using (var provider = new CSharpCodeProvider(new Dictionary<string, string>() { 
                { "CompilerVersion", "v4.0" } 
            })) {

                var parameters = new CompilerParameters(
                    AssemblyReferences
                ) {
                    CompilerOptions = "",
                    GenerateExecutable = false,
                    GenerateInMemory = false,
                    IncludeDebugInformation = true,
                    TempFiles = new TempFileCollection(tempPath, false)
                };

                var results = provider.CompileAssemblyFromSource(parameters, csharp);
                compilerOutput = String.Join(Environment.NewLine, results.Output.OfType<string>().ToArray());

                if (results.Errors.Count > 0) {
                    throw new Exception(String.Format(
                        "Compile failed with {0} error(s):{1}{2}{1}{3}",
                        results.Errors.Count, Environment.NewLine,
                        compilerOutput ?? "",
                        String.Join(
                            Environment.NewLine,
                            (from CompilerError err in results.Errors select err.ToString()).ToArray()
                        )
                    ));
                }

                resultAssembly = results.CompiledAssembly;
                resultPath = results.PathToAssembly;
            }

            if ((resultPath == null) || !File.Exists(resultPath)) {
                throw new Exception("Compile failed." + Environment.NewLine + (compilerOutput ?? ""));
            }

            try {
                var translatorConfiguration = new Configuration {
                    ApplyDefaults = false,
                    Assemblies = {
                        Stubbed = {
                          "mscorlib,",
                          "System.*",
                          "Microsoft.*"
                        },
                        Ignored = {
                          "Microsoft.VisualC,",
                          "Accessibility,",
                          "SMDiagnostics,",
                          "System.EnterpriseServices,",
                          "JSIL.Meta,"
                        }
                    },
                    FrameworkVersion = 4.0,
                    GenerateSkeletonsForStubbedAssemblies = false,
                    GenerateContentManifest = false,
                    IncludeDependencies = false,
                    UseSymbols = true,
                    UseThreads = false
                };

                using (var translator = new AssemblyTranslator(
                    translatorConfiguration
                )) {
                    var result = translator.Translate(resultPath, true);

                    AssemblyTranslator.GenerateManifest(
                        translator.Manifest, Path.GetDirectoryName(resultPath), result
                    );

                    return result.WriteToString();
                }
            } finally {
                try {
                    Directory.Delete(tempPath, true);
                } catch (Exception exc) {
                    Console.WriteLine("Failed to empty temporary directory: {0}", exc.Message);
                }
            }
        }
    }
}
