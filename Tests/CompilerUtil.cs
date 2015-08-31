using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using FSharp.Compiler.CodeDom;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace JSIL.Tests {
    using System.Runtime.Serialization;
    using System.Security.Cryptography;

    public static class CompilerUtil {
        public static string TempPath;

        // Attempt to clean up stray assembly files from previous test runs
        //  since the assemblies would have remained locked and undeletable 
        //  due to being loaded
        static CompilerUtil () {
            TempPath = Path.Combine(Path.GetTempPath(), "JSIL Tests");
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }

        public static CompileResult Compile (
            IEnumerable<string> filenames, string assemblyName, string compilerOptions, string currentMetaRevision
        ) {
            var extension = Path.GetExtension(filenames.First()).ToLower();
            Func<CompileOptions, CodeDomProvider> provider = null;

            switch (extension) {
                case ".cs":
                    provider = options =>
                    {
                        var providerOptions = new Dictionary<string, string>();
                        if (options.UseRoslyn)
                        {
                            providerOptions.Add(
                                "CompilerDirectoryPath", 
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "packages", "Microsoft.Net.Compilers.1.0.0", "tools"));
                        }
                        else
                        {
                            providerOptions.Add("CompilerVersion", "v4.0");
                        }

                        return new CSharpCodeProvider(providerOptions);
                    };
                    break;

                case ".vb":
                    provider = options =>
                    {
                        var providerOptions = new Dictionary<string, string>();
                        if (options.UseRoslyn)
                        {
                            providerOptions.Add(
                                "CompilerDirectoryPath",
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "packages", "Microsoft.Net.Compilers.1.0.0", "tools"));
                        }
                        else
                        {
                            providerOptions.Add("CompilerVersion", "v4.0");
                        }

                        return new VBCodeProvider(providerOptions);
                    };
                    break;

                case ".fs":
                    provider = options => new FSharpCodeProvider();
                    break;

                case ".il":
                    provider = options => new CILCodeProvider();
                    break;

                case ".cpp":
                    provider = options => new CPPCodeProvider();
                    break;
                default:
                    throw new NotImplementedException("Extension '" + extension + "' cannot be compiled for test cases");
            }

            return Compile(
                provider, filenames, assemblyName, compilerOptions, currentMetaRevision
            );
        }

        private static string GetFileMD5 (string path) {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(path)) {
                var hashBytes = md5.ComputeHash(stream);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private static Dictionary<string, string> BuildCompileManifest (IEnumerable<string> inputs, string metaVersion) {
            var manifest = new Dictionary<string, string>();

            manifest["metaVersion"] = metaVersion;

            foreach (var input in inputs) {
                var hash = GetFileMD5(input);
                manifest["md5-" + input] = hash;
            }

            return manifest;
        }

        private static bool CheckCompileManifest (string outputDirectory, Dictionary<string, string> expectedManifest) {
            var manifestPath = Path.Combine(outputDirectory, "compileManifest.json");
            if (!File.Exists(manifestPath))
                return false;

            var jss = new JavaScriptSerializer();
            var manifest = jss.Deserialize<Dictionary<string, string>>(File.ReadAllText(manifestPath));

            var expectedKeys = expectedManifest.Keys.ToArray();
            Array.Sort(expectedKeys);

            var actualKeys = manifest.Keys.ToArray();
            Array.Sort(actualKeys);

            if (!expectedKeys.SequenceEqual(actualKeys))
                return false;

            foreach (var key in expectedKeys) {
                if (expectedManifest[key] != manifest[key])
                    return false;
            }

            return true;
        }

        private static void WriteCompileManifest (string outputDirectory, Dictionary<string, string> manifest) {
            var manifestPath = Path.Combine(outputDirectory, "compileManifest.json");
            var jss = new JavaScriptSerializer();
            File.WriteAllText(manifestPath, jss.Serialize(manifest));
        }

        public static bool TryGetMetaVersion (out string version) {
            string stderr, stdout;
            var metaSourceDirectory = Path.Combine(ComparisonTest.JSILFolder, "..", "Meta"); 

            try {
                var exitCode = ProcessUtil.Run(
                    "git", "rev-parse --verify HEAD", null, out stderr, out stdout,
                    cwd: metaSourceDirectory
                );

                if (exitCode != 0) {
                    Console.WriteLine("revparse exited with code {0}. stdout='{1}' stderr='{2}'", exitCode, stdout, stderr);
                    version = null;
                    return false;
                }

                version = stdout.Trim();

                exitCode = ProcessUtil.Run(
                    "git", "diff-files --name-only --ignore-submodules", null, out stderr, out stdout,
                    cwd: metaSourceDirectory
                );

                if (exitCode != 0) {
                    Console.WriteLine("diff-files exited with code {0}. stdout='{1}' stderr='{2}'", exitCode, stdout, stderr);
                    version = null;
                    return false;
                } else if (stdout.Trim().Length > 0) {
                    Console.WriteLine("Suppressing Meta caching because of local modifications");
                    version = null;
                    return false;
                }

                return true;
            } catch {
                Console.WriteLine("Looked for meta submodule in {0}", metaSourceDirectory);
                throw;
            }
            return false;
        }

        private static CompileResult Compile (
            Func<CompileOptions, CodeDomProvider> getProvider, IEnumerable<string> _filenames, string assemblyName, string compilerOptions, string currentMetaRevision
        ) {
            var filenames = _filenames.ToArray();
            var tempPath = Path.Combine(TempPath, assemblyName);
            Directory.CreateDirectory(tempPath);

            var warningTextPath = Path.Combine(
                tempPath, "warnings.txt"
            );

            var references = new List<string> {
                "System.dll", 
                "System.Core.dll", "System.Xml.dll", 
                "Microsoft.CSharp.dll",
                typeof(JSIL.Meta.JSIgnore).Assembly.Location
            };

            bool generateExecutable = false;

            var compilerCreationOptions = new CompileOptions();

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

                        case "generateexecutable":
                            generateExecutable = true;
                            break;

                        case "useroslyn":
                            compilerCreationOptions.UseRoslyn = true;
                            break;
                    }
                }

                metacomments.AddRange(localMetacomments);
            }

            var outputAssembly = Path.Combine(
                tempPath,
                Path.GetFileNameWithoutExtension(assemblyName) + 
                (generateExecutable ? ".exe" : ".dll")
            );

            var manifest = BuildCompileManifest(filenames, currentMetaRevision);

            if (
                File.Exists(outputAssembly) &&
                CheckCompileManifest(tempPath, manifest)
            ) {
                if (File.Exists(warningTextPath))
                    Console.Error.WriteLine(File.ReadAllText(warningTextPath));

                return new CompileResult(
                    generateExecutable ? Assembly.ReflectionOnlyLoadFrom(outputAssembly) : Assembly.LoadFile(outputAssembly),
                    metacomments.ToArray(),
                    true
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
                GenerateExecutable = generateExecutable,
                GenerateInMemory = false,
                IncludeDebugInformation = true,
                TempFiles = new TempFileCollection(tempPath, true),
                OutputAssembly = outputAssembly,
                WarningLevel = 4,
                TreatWarningsAsErrors = false,
            };

            CompilerResults results;
            using (var provider = getProvider(compilerCreationOptions)) {
                results = provider.CompileAssemblyFromFile(
                    parameters,
                    filenames.ToArray()
                );
            }

            var compileErrorsAndWarnings = results.Errors.Cast<CompilerError>().ToArray();
            var compileWarnings = (from ce in compileErrorsAndWarnings where ce.IsWarning select ce).ToArray();
            var compileErrors = (from ce in compileErrorsAndWarnings where !ce.IsWarning select ce).ToArray();

            // Mono incorrectly trats some warnings as errors;
            if (Type.GetType("Mono.Runtime") != null)
            {
                if (compileErrors.Length > 0 && File.Exists(outputAssembly))
                {
                    try
                    {
                        results.CompiledAssembly = Assembly.LoadFrom(outputAssembly);
                        compileErrors = new CompilerError[0];
                        compileWarnings = compileErrorsAndWarnings;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

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

            WriteCompileManifest(tempPath, manifest);

            return new CompileResult(
                results.CompiledAssembly,
                metacomments.ToArray(),
                false
            );
        }

        private class CompileOptions
        {
            public bool UseRoslyn { get; set; }
        }
    }

    public class CompileResult {
        public readonly Assembly      Assembly;
        public readonly Metacomment[] Metacomments;
        public readonly bool          WasCached;

        public CompileResult (Assembly assembly, Metacomment[] metacomments, bool wasCached) {
            Assembly = assembly;
            Metacomments = metacomments;
            WasCached = wasCached;
        }
    }

    [Serializable]
    public class CompilerNotFoundException : Exception
    {
        public CompilerNotFoundException()
        {
        }

        public CompilerNotFoundException(string message) : base(message)
        {
        }

        public CompilerNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CompilerNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
