using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JSIL.Internal;
using JSIL.Translator;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace JSIL.Try {
    public class CompiledSnippet {
        public string JavaScript;
        public string OriginalSource;
        public string EntryPoint;
        public string Warnings;

        // Seconds
        public double CompileElapsed;
        public double TranslateElapsed;
    }

    public interface ICompileWorker {
        void CompileAssembly (
            string tempPath, string csharp,
            out string compilerOutput, out string resultPath,
            out string resultFullName, out string entryPoint
        );
    }

    public class CompileWorker : MarshalByRefObject, ICompileWorker {
        public static readonly string[] AssemblyReferences = new[] {
            "mscorlib.dll", "System.dll", 
            "System.Core.dll", "System.Xml.dll", 
            "Microsoft.CSharp.dll", "JSIL.Meta.dll",
            "System.Drawing.dll"
        };

        static string GetFullName (Type type) {
            if (type.DeclaringType != null)
                return String.Format("{0}_{1}", GetFullName(type.DeclaringType), type.Name);
            else if (!String.IsNullOrWhiteSpace(type.Namespace))
                return String.Format("{0}.{1}", type.Namespace, type.Name);
            else
                return type.Name;
        }

        public void CompileAssembly (
            string tempPath, string csharp,
            out string compilerOutput, out string resultPath,
            out string resultFullName, out string entryPoint
        ) {
            using (var provider = new CSharpCodeProvider(new Dictionary<string, string>() { 
                { "CompilerVersion", "v4.0" } 
            })) {

                var parameters = new CompilerParameters(
                    AssemblyReferences
                ) {
                    CompilerOptions = "/unsafe",
                    GenerateExecutable = true,
                    GenerateInMemory = false,
                    IncludeDebugInformation = true,                    
                    TempFiles = new TempFileCollection(tempPath, false),
                };
                var results = provider.CompileAssemblyFromSource(parameters, csharp);

                int numWarnings = (from CompilerError err in results.Errors where err.IsWarning select err).Count();
                int numErrors = results.Errors.Count - numWarnings;

                if (numErrors > 0) {
                    compilerOutput = String.Format(
                        "Compile failed with {0} error(s):{1}{2}",
                        numErrors, Environment.NewLine,
                        String.Join(
                            Environment.NewLine,
                            (from CompilerError err in results.Errors select err.ToString()).ToArray()
                        )
                    );

                    entryPoint = resultFullName = resultPath = null;

                    return;
                } else if (numWarnings > 0) {
                    compilerOutput = String.Join(
                        Environment.NewLine,
                        (from CompilerError err in results.Errors select err.ToString()).ToArray()
                    );
                } else {
                    compilerOutput = String.Join(Environment.NewLine, results.Output.OfType<string>().ToArray());
                }

                var resultAssembly = results.CompiledAssembly;

                entryPoint = String.Format(
                    "{0}.{1}",
                    GetFullName(resultAssembly.EntryPoint.DeclaringType),
                    resultAssembly.EntryPoint.Name
                );

                resultFullName = results.CompiledAssembly.FullName;
                resultPath = results.PathToAssembly;
            }
        }
    }

    public static class SnippetCompiler {
        public const int MaxPendingCompiles = 4;

        public static int NextDomainId = 0;
        public static int NextTempDirId = 0;
        public static int PendingCompiles = 0;
        public static readonly AssemblyCache AssemblyCache = new AssemblyCache();

        public static ThreadLocal<TypeInfoProvider> CachedTypeInfo = new ThreadLocal<TypeInfoProvider>();

        private static void CompileAssembly (
            string tempPath, string csharp, 
            out string compilerOutput, out string resultPath, 
            out string resultFullName, out string entryPoint
        ) {
            if (PendingCompiles >= MaxPendingCompiles)
                throw new Exception(String.Format(
                    "Sorry, the server is currently busy compiling code for {0} people. Please try again later."
                ));

            Interlocked.Increment(ref PendingCompiles);
            try {
                int id = Interlocked.Increment(ref NextDomainId);
                var domainName = String.Format("CompileDomain{0}", id);

                // Copy domain setup information from the current domain
                var currentDomain = AppDomain.CurrentDomain;
                var currentSetup = currentDomain.SetupInformation;
                var domainSetup = new AppDomainSetup {
                    ApplicationBase = currentSetup.ApplicationBase,
                    ApplicationName = currentSetup.ApplicationName,
                    ApplicationTrust = currentSetup.ApplicationTrust,
                    CachePath = currentSetup.CachePath,
                    ConfigurationFile = currentSetup.ConfigurationFile,
                    DisallowCodeDownload = true,
                    DynamicBase = currentSetup.DynamicBase,
                    PrivateBinPath = currentSetup.PrivateBinPath,
                    PrivateBinPathProbe = currentSetup.PrivateBinPathProbe,
                    ShadowCopyDirectories = currentSetup.ShadowCopyDirectories,
                    ShadowCopyFiles = currentSetup.ShadowCopyFiles,
                    LoaderOptimization = LoaderOptimization.MultiDomain
                };

                var domain = AppDomain.CreateDomain(domainName, null, domainSetup);

                var tWorker = typeof(CompileWorker);
                domain.Load(tWorker.Assembly.GetName());

                try {
                    var worker = (ICompileWorker)domain.CreateInstanceAndUnwrap(
                        tWorker.Assembly.FullName,
                        "JSIL.Try.CompileWorker"
                    );

                    worker.CompileAssembly(
                        tempPath, csharp, 
                        out compilerOutput, out resultPath, 
                        out resultFullName, out entryPoint
                    );
                } finally {
                    AppDomain.Unload(domain);
                }

            } finally {
                Interlocked.Decrement(ref PendingCompiles);
            }
        }

        /// <summary>
        /// Compiles the provided C# and then translates it into JavaScript.
        /// On success, returns the JS. On failure, throws.
        /// </summary>
        public static CompiledSnippet Compile (string csharp, bool deleteTempFiles) {
            var result = new CompiledSnippet {
                OriginalSource = csharp
            };

            int tempDirId = Interlocked.Increment(ref NextTempDirId);
            var tempPath = Path.Combine(Path.GetTempPath(), "JSIL.Try", tempDirId.ToString());

            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);

            try {
                string resultPath, entryPointName, compilerOutput, resultFullName;

                long compileStarted = DateTime.UtcNow.Ticks;

                CompileAssembly(
                    tempPath, csharp, 
                    out compilerOutput, out resultPath, 
                    out resultFullName, out entryPointName
                );

                result.CompileElapsed = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - compileStarted).TotalSeconds;

                if ((resultPath == null) || !File.Exists(resultPath)) {
                    if (String.IsNullOrWhiteSpace(compilerOutput))
                        throw new Exception("Compile failed with unknown error.");
                    else
                        throw new Exception(compilerOutput);
                }

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
                    CodeGenerator = {
                        EnableUnsafeCode = true
                    },
                    FrameworkVersion = 4.0,
                    GenerateSkeletonsForStubbedAssemblies = false,
                    GenerateContentManifest = false,
                    IncludeDependencies = false,
                    UseSymbols = true,
                    UseThreads = false,
                    RunBugChecks = false,
                };

                var translatorOutput = new StringBuilder();

                var typeInfo = CachedTypeInfo.Value;

                // Don't use a cached type provider if this snippet contains a proxy.
                bool disableCaching = csharp.Contains("JSProxy");
                
                using (var translator = new AssemblyTranslator(
                    translatorConfiguration,
                    // Reuse the cached type info provider, if one exists.
                    disableCaching ? null : typeInfo,
                    // Can't reuse a manifest meaningfully here.
                    null,
                    // Reuse the assembly cache so that mscorlib doesn't get loaded every time.
                    AssemblyCache
                )) {
                    translator.CouldNotDecompileMethod += (s, exception) => {
                        lock (translatorOutput)
                            translatorOutput.AppendFormat(
                                "Could not decompile method '{0}': {1}{2}",
                                s, exception.Message, Environment.NewLine
                            );
                    };

                    translator.CouldNotResolveAssembly += (s, exception) => {
                        lock (translatorOutput)
                            translatorOutput.AppendFormat(
                                "Could not resolve assembly '{0}': {1}{2}",
                                s, exception.Message, Environment.NewLine
                            );
                    };

                    translator.Warning += (s) => {
                        lock (translatorOutput)
                            translatorOutput.AppendLine(s);
                    };

                    var translateStarted = DateTime.UtcNow.Ticks;
                    var translationResult = translator.Translate(resultPath, true);

                    AssemblyTranslator.GenerateManifest(
                        translator.Manifest, Path.GetDirectoryName(resultPath), translationResult
                    );

                    result.EntryPoint = String.Format(
                        "{0}.{1}",
                        translator.Manifest.GetPrivateToken(resultFullName).IDString,
                        entryPointName
                    );

                    result.Warnings = translatorOutput.ToString().Trim();
                    result.TranslateElapsed = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - translateStarted).TotalSeconds;
                    result.JavaScript = translationResult.WriteToString();

                    if (typeInfo != null) {
                        // Remove the temporary assembly from the type info provider.
                        typeInfo.Remove(translationResult.Assemblies.ToArray());
                    } else if (!disableCaching) {
                        // We didn't have a type info provider to reuse, so store the translator's.
                        CachedTypeInfo.Value = typeInfo = translator.GetTypeInfoProvider();
                    }

                    /*
                    result.Warnings += String.Format(
                        "{1} assemblies loaded{0}",
                        Environment.NewLine, AppDomain.CurrentDomain.GetAssemblies().Length
                    );
                     */

                    /*
                    result.Warnings += String.Format(
                        "TypeInfo.Count = {1}{0}AssemblyCache.Count = {2}{0}",
                        Environment.NewLine, TypeInfo.Count, AssemblyCache.Count
                    );
                        */
                }

                /*

                GC.Collect();
                
                result.Warnings += String.Format(
                    "{1} byte(s) GC heap {0}",
                    Environment.NewLine, GC.GetTotalMemory(true)
                );
                 */

                return result;
            } finally {

                try {
                    if (deleteTempFiles)
                        Directory.Delete(tempPath, true);
                } catch (Exception exc) {
                    Console.WriteLine("Failed to empty temporary directory: {0}", exc.Message);
                }
            }
        }
    }
}
