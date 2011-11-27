using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using JSIL.Translator;

namespace JSIL.Compiler {
    class Program {
        static void ApplyDefaults (Configuration configuration) {
            Console.Error.WriteLine("// Applying default settings. To suppress, use --nodefaults.");

            configuration.Assemblies.Stubbed.Add(@"mscorlib,");
            configuration.Assemblies.Stubbed.Add(@"System.*");
            configuration.Assemblies.Stubbed.Add(@"Microsoft.*");
            configuration.Assemblies.Stubbed.Add(@"Accessibility,");
        }

        // Removes parsed elements from the input list
        static Configuration ParseCommandLine (IEnumerable<string> arguments, out List<string> filenames) {
            var result = new Configuration();
            var includeDefaults = new [] { true };

            var os = new Mono.Options.OptionSet {
                {"o=|out=", "Specifies the output directory for generated javascript and manifests.",
                    (string path) => result.OutputDirectory = Path.GetFullPath(path) },
                {"p=|proxy=", "Loads a type proxy assembly to provide type information for the translator.",
                    (string name) => result.Assemblies.Proxies.Add(Path.GetFullPath(name)) },
                {"i=|ignore=", "Specifies a regular expression pattern for assembly names that should be ignored during the translation process.",
                    (string regex) => result.Assemblies.Ignored.Add(regex) },
                {"s=|stub=", "Specifies a regular expression pattern for assembly names that should be stubbed during the translation process. Stubbing forces all methods to be externals.",
                    (string regex) => result.Assemblies.Stubbed.Add(regex) },
                {"nd|nodeps", "Suppresses the automatic loading and translation of assembly dependencies.",
                    (bool b) => result.IncludeDependencies = !b},
                {"nodefaults", "Suppresses the default list of stubbed assemblies.",
                    (bool b) => includeDefaults[0] = !b},
                {"os", "Suppresses struct copy elimination.",
                    (bool b) => result.Optimizer.EliminateStructCopies = !b},
                {"ot", "Suppresses temporary local variable elimination.",
                    (bool b) => result.Optimizer.EliminateTemporaries = !b},
                {"oo", "Suppresses simplification of operator expressions and special method calls.",
                    (bool b) => result.Optimizer.SimplifyOperators = !b},
                {"ol", "Suppresses simplification of loop blocks.",
                    (bool b) => result.Optimizer.SimplifyLoops = !b},
                {"fv=|frameworkVersion=", "Specifies the version of the .NET framework proxies to use. This ensures that correct type information is provided (as 3.5 and 4.0 use different standard libraries). Accepted values are '3.5' and '4.0'. Default: '4.0'",
                    (string fv) => result.FrameworkVersion = double.Parse(fv)}
            };

            filenames = os.Parse(arguments);

            if (includeDefaults[0])
                ApplyDefaults(result);

            return result;
        }

        static AssemblyTranslator CreateTranslator (Configuration configuration) {
            var translator = new AssemblyTranslator(configuration);

            translator.LoadingAssembly += (fn, progress) => {
                Console.Error.WriteLine("// Loading {0}. ", fn);
            };
            translator.Decompiling += (progress) => {
                Console.Error.Write("// Decompiling ");

                var previous = new int[1] { 0 };

                progress.ProgressChanged += (s, p, max) => {
                    var current = p * 20 / max;
                    if (current != previous[0]) {
                        previous[0] = current;
                        Console.Error.Write(".");
                    }
                };

                progress.Finished += (s, e) => {
                    Console.Error.WriteLine(" done");
                };
            };
            translator.Optimizing += (progress) => {
                Console.Error.Write("// Optimizing ");

                var previous = new int[1] { 0 };

                progress.ProgressChanged += (s, p, max) => {
                    var current = p * 20 / max;
                    if (current != previous[0]) {
                        previous[0] = current;
                        Console.Error.Write(".");
                    }
                };

                progress.Finished += (s, e) => {
                    Console.Error.WriteLine(" done");
                };
            };
            translator.Writing += (progress) => {
                Console.Error.Write("// Generating JS ");

                var previous = new int[1] { 0 };

                progress.ProgressChanged += (s, p, max) => {
                    var current = p * 20 / max;
                    if (current != previous[0]) {
                        previous[0] = current;
                        Console.Error.Write(".");
                    }
                };

                progress.Finished += (s, e) => {
                    Console.Error.WriteLine(" done");
                };
            };
            translator.CouldNotLoadSymbols += (fn, ex) => {
                Console.Error.WriteLine("// {0}", ex.Message);
            };
            translator.CouldNotResolveAssembly += (fn, ex) => {
                Console.Error.WriteLine("// Could not load module {0}: {1}", fn, ex.Message);
            };
            translator.CouldNotDecompileMethod += (fn, ex) => {
                Console.Error.WriteLine("// Could not decompile method {0}: {1}", fn, ex.Message);
            };

            return translator;
        }

        static void Main (string[] arguments) {
            List<string> filenames;
            var configuration = ParseCommandLine(arguments, out filenames);

            if (filenames.Count == 0) {
                var asmName = Assembly.GetExecutingAssembly().GetName();
                Console.WriteLine("==== JSILc v{0}.{1}.{2} ====", asmName.Version.Major, asmName.Version.Minor, asmName.Version.Revision);
                Console.WriteLine("Usage: JSILc [options] ...");
                Console.WriteLine("Specify one or more compiled assemblies (dll/exe) to translate them. Symbols will be loaded if they exist in the same directory.");
                Console.WriteLine("You can also specify Visual Studio solution files (sln) to build them and automatically translate their output(s).");
                Console.WriteLine("Specify the path of a .jsilconfig file to load settings from it.");
                Console.WriteLine("Options:");
                Console.WriteLine("--fv:<version>");
                Console.WriteLine("  Specifies the version of the .NET framework libraries to use. Valid options are '3.5' and '4.0'. The default is '4.0'.");
                Console.WriteLine("--out:<folder>");
                Console.WriteLine("  Specifies the directory into which the generated javascript should be written.");
                Console.WriteLine("--nodeps");
                Console.WriteLine("  Disables translating dependencies.");
                Console.WriteLine("--nodefaults");
                Console.WriteLine("  Disables the built-in default stub list. Use this if you actually want to translate huge Microsoft assemblies like mscorlib.");
                Console.WriteLine("--oS");
                Console.WriteLine("  Disables struct copy optimizations");
                Console.WriteLine("--oO");
                Console.WriteLine("  Disables operator optimizations");
                Console.WriteLine("--oL");
                Console.WriteLine("  Disables loop optimizations");
                Console.WriteLine("--oT");
                Console.WriteLine("  Disables temporary variable elimination");
                Console.WriteLine("--proxy:<assembly>");
                Console.WriteLine("  Specifies the location of a proxy assembly that contains type information for other assemblies.");
                Console.WriteLine("--ignore:<regex>");
                Console.WriteLine("  Specifies a regular expression filter used to ignore certain dependencies.");
                Console.WriteLine("--stub:<regex>");
                Console.WriteLine("  Specifies a regular expression filter used to specify that certain dependencies should only be generated as stubs.");
                return;
            }

            foreach (var filename in filenames.ToArray()) {
                var extension = Path.GetExtension(filename);
                switch (extension.ToLower()) {
                    case ".sln":
                        foreach (var resultFilename in SolutionBuilder.Build(filename)) {
                            filenames.Add(resultFilename);
                        }
                        break;
                    case ".jsilconfig":
                        break;
                }
            }

            var translator = CreateTranslator(configuration);
            while (filenames.Count > 0) {
                var filename = filenames.First();
                filenames.Remove(filename);

                var extension = Path.GetExtension(filename);
                switch (extension.ToLower()) {
                    case ".exe":
                    case ".dll":
                        var result = translator.Translate(filename);
                        Console.Error.Write("// Saving to disk ... ");
                        result.WriteToDirectory(configuration.OutputDirectory);
                        Console.Error.WriteLine("done");
                        break;
                    case ".sln":
                    case ".jsilconfig":
                        break;
                    default:
                        Console.Error.WriteLine("// Don't know what to do with file '{0}'.", filename);
                        break;
                }
            }
        }
    }
}
