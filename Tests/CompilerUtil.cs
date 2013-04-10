using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Microsoft.CSharp;
using Microsoft.FSharp.Compiler.CodeDom;
using Microsoft.VisualBasic;

namespace JSIL.Tests {
    public static class CompilerUtil {
        public static string TempPath;

        // Attempt to clean up stray assembly files from previous test runs
        //  since the assemblies would have remained locked and undeletable 
        //  due to being loaded
        static CompilerUtil () {
            TempPath = Path.Combine(Path.GetTempPath(), "JSIL Tests");
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);

            foreach (var filename in Directory.GetFiles(TempPath))
                try {
                    File.Delete(filename);
                } catch {
                }
        }

        public static Assembly Compile (
            IEnumerable<string> filenames, string assemblyName
        ) {
            var extension = Path.GetExtension(filenames.First()).ToLower();
            Func<CodeDomProvider> provider = null;

            switch (extension) {
                case ".cs":
                    provider = () => new CSharpCodeProvider(new Dictionary<string, string>() { 
                        { "CompilerVersion", "v4.0" } 
                    });
                    break;

                case ".vb":
                    provider = () => new VBCodeProvider(new Dictionary<string, string>() { 
                        { "CompilerVersion", "v4.0" } 
                    });
                    break;

                case ".fs":
                    provider = () => {
                        var result = new FSharpCodeProvider();
                        return result;
                    };
                    break;
                default:
                    throw new NotImplementedException("Extension '" + extension + "' cannot be compiled for test cases");
            }

            return Compile(
                provider, filenames, assemblyName
            );
        }

        private static bool CheckCompileManifest (IEnumerable<string> inputs, string outputDirectory) {
            var manifestPath = Path.Combine(outputDirectory, "compileManifest.json");
            if (!File.Exists(manifestPath))
                return false;

            var jss = new JavaScriptSerializer();
            var manifest = jss.Deserialize<Dictionary<string, string>>(File.ReadAllText(manifestPath));

            foreach (var input in inputs) {
                var fi = new FileInfo(input);
                var key = Path.GetFileName(input);

                if (!manifest.ContainsKey(key))
                    return false;

                var previousTimestamp = DateTime.Parse(manifest[key]);

                var delta = fi.LastWriteTime - previousTimestamp;
                if (Math.Abs(delta.TotalSeconds) >= 1) {
                    return false;
                }
            }

            return true;
        }

        private static void WriteCompileManifest (IEnumerable<string> inputs, string outputDirectory) {
            var manifest = new Dictionary<string, string>();

            foreach (var input in inputs) {
                var fi = new FileInfo(input);
                var key = Path.GetFileName(input);
                manifest[key] = fi.LastWriteTime.ToString("O");
            }

            var manifestPath = Path.Combine(outputDirectory, "compileManifest.json");
            var jss = new JavaScriptSerializer();
            File.WriteAllText(manifestPath, jss.Serialize(manifest));
        }

        private static Assembly Compile (
            Func<CodeDomProvider> getProvider, IEnumerable<string> filenames, string assemblyName
        ) {
            var tempPath = Path.Combine(TempPath, assemblyName);
            Directory.CreateDirectory(tempPath);

            var outputAssembly = Path.Combine(
                tempPath,
                Path.GetFileNameWithoutExtension(assemblyName) + ".dll"
            );

            if (
                File.Exists(outputAssembly) &&
                CheckCompileManifest(filenames, tempPath)
            ) {
                return Assembly.LoadFile(outputAssembly);
            }

            var files = Directory.GetFiles(tempPath);
            foreach (var file in files) {
                try {
                    File.Delete(file);
                } catch {
                }
            }

            var references = new List<string> {
                "System.dll", 
                "System.Core.dll", "System.Xml.dll", 
                "Microsoft.CSharp.dll",
                typeof(JSIL.Meta.JSIgnore).Assembly.Location
            };

            var compilerOptions = "";

            foreach (var sourceFile in filenames) {
                var sourceText = File.ReadAllText(sourceFile);
                foreach (var metacomment in Metacomment.FromText(sourceText)) {
                    switch (metacomment.Command.ToLower()) {
                        case "reference":
                            references.Add(metacomment.Arguments);
                            break;

                        case "compileroption":
                            compilerOptions += metacomment.Arguments;
                            break;
                    }
                }
            }

            var parameters = new CompilerParameters(references.ToArray()) {
                CompilerOptions = compilerOptions,
                GenerateExecutable = false,
                GenerateInMemory = false,
                IncludeDebugInformation = true,
                TempFiles = new TempFileCollection(tempPath, true),
                OutputAssembly = outputAssembly
            };

            CompilerResults results;
            using (var provider = getProvider()) {
                results = provider.CompileAssemblyFromFile(
                    parameters,
                    filenames.ToArray()
                );
            }

            if (results.Errors.Count > 0) {
                throw new Exception(
                    String.Join(Environment.NewLine, results.Errors.Cast<CompilerError>().Select((ce) => ce.ToString()).ToArray())
                );
            }

            WriteCompileManifest(filenames, tempPath);

            return results.CompiledAssembly;
        }
    }
}
