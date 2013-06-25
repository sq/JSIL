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

        public static CompileResult Compile (
            IEnumerable<string> filenames, string assemblyName, string compilerOptions = ""
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
                provider, filenames, assemblyName, compilerOptions
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

        private static CompileResult Compile (
            Func<CodeDomProvider> getProvider, IEnumerable<string> _filenames, string assemblyName, string compilerOptions
        ) {
            var filenames = _filenames.ToArray();
            var tempPath = Path.Combine(TempPath, assemblyName);
            Directory.CreateDirectory(tempPath);

            var outputAssembly = Path.Combine(
                tempPath,
                Path.GetFileNameWithoutExtension(assemblyName) + ".dll"
            );

            var warningTextPath = Path.Combine(
                tempPath, "warnings.txt"
            );

            var references = new List<string> {
                "System.dll", 
                "System.Core.dll", "System.Xml.dll", 
                "Microsoft.CSharp.dll",
                typeof(JSIL.Meta.JSIgnore).Assembly.Location
            };

            var metacomments = new List<Metacomment>();
            foreach (var sourceFile in filenames) {
                var sourceText = File.ReadAllText(sourceFile);

                var localMetacomments = Metacomment.FromText(sourceText);
                foreach (var metacomment in localMetacomments) {
                    switch (metacomment.Command.ToLower()) {
                        case "reference":
                            references.Add(metacomment.Arguments);
                            break;

                        case "compileroption":
                            compilerOptions += " " + metacomment.Arguments;
                            break;
                    }
                }

                metacomments.AddRange(localMetacomments);
            }
            if (
                File.Exists(outputAssembly) &&
                CheckCompileManifest(filenames, tempPath)
            ) {
                if (File.Exists(warningTextPath))
                    Console.Error.WriteLine(File.ReadAllText(warningTextPath));

                return new CompileResult(
                    Assembly.LoadFile(outputAssembly),
                    metacomments.ToArray()
                );
            }

            var files = Directory.GetFiles(tempPath);
            foreach (var file in files) {
                try {
                    File.Delete(file);
                } catch {
                }
            }

            var parameters = new CompilerParameters(references.ToArray()) {
                CompilerOptions = compilerOptions,
                GenerateExecutable = false,
                GenerateInMemory = false,
                IncludeDebugInformation = true,
                TempFiles = new TempFileCollection(tempPath, true),
                OutputAssembly = outputAssembly,
                WarningLevel = 4,
                TreatWarningsAsErrors = false
            };

            CompilerResults results;
            using (var provider = getProvider()) {
                results = provider.CompileAssemblyFromFile(
                    parameters,
                    filenames.ToArray()
                );
            }

            var compileErrorsAndWarnings = results.Errors.Cast<CompilerError>().ToArray();
            var compileWarnings = (from ce in compileErrorsAndWarnings where ce.IsWarning select ce).ToArray();
            var compileErrors = (from ce in compileErrorsAndWarnings where !ce.IsWarning select ce).ToArray();

            if (compileWarnings.Length > 0) {
                var warningText = String.Format(
                    "// C# Compiler warning(s) follow //\r\n{0}\r\n// End of C# compiler warning(s) //",
                    String.Join(Environment.NewLine, compileWarnings.Select(Convert.ToString).ToArray())
                );
                Console.Error.WriteLine(
                    warningText
                );
                File.WriteAllText(
                    warningTextPath, warningText
                );
            } else {
                if (File.Exists(warningTextPath))
                    File.Delete(warningTextPath);
            }

            if (compileErrors.Length > 0) {
                throw new Exception(
                    String.Join(Environment.NewLine, compileErrors.Select(Convert.ToString).ToArray())
                );
            }

            WriteCompileManifest(filenames, tempPath);

            return new CompileResult(
                results.CompiledAssembly,
                metacomments.ToArray()
            );
        }
    }

    public class CompileResult {
        public readonly Assembly Assembly;
        public readonly Metacomment[] Metacomments;

        public CompileResult (Assembly assembly, Metacomment[] metacomments) {
            Assembly = assembly;
            Metacomments = metacomments;
        }
    }
}
