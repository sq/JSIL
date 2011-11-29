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

        static Configuration ParseCommandLine (IEnumerable<string> arguments, out List<string> filenames) {
            var result = new Configuration();

            var os = new Mono.Options.OptionSet {
                {"o=|out=", "Specifies the output directory for generated javascript and manifests.",
                    (string path) => result.OutputDirectory = Path.GetFullPath(path) },
                "Solution Builder options",
                {"configuration=", "When building one or more solution files, specifies the build configuration to use (like 'Debug').",
                    (string v) => result.SolutionBuilder.Configuration = v },
                {"platform=", "When building one or more solution files, specifies the build platform to use (like 'x86').",
                    (string v) => result.SolutionBuilder.Platform = v },
                "Assembly options",
                {"p=|proxy=", "Loads a type proxy assembly to provide type information for the translator.",
                    (string name) => result.Assemblies.Proxies.Add(Path.GetFullPath(name)) },
                {"i=|ignore=", "Specifies a regular expression pattern for assembly names that should be ignored during the translation process.",
                    (string regex) => result.Assemblies.Ignored.Add(regex) },
                {"s=|stub=", "Specifies a regular expression pattern for assembly names that should be stubbed during the translation process. Stubbing forces all methods to be externals.",
                    (string regex) => result.Assemblies.Stubbed.Add(regex) },
                {"nd|nodeps", "Suppresses the automatic loading and translation of assembly dependencies.",
                    (bool b) => result.IncludeDependencies = !b},
                {"nodefaults", "Suppresses the default list of stubbed assemblies.",
                    (bool b) => result.ApplyDefaults = !b},
                {"fv=|frameworkVersion=", "Specifies the version of the .NET framework proxies to use. This ensures that correct type information is provided (as 3.5 and 4.0 use different standard libraries). Accepted values are '3.5' and '4.0'. Default: '4.0'",
                    (string fv) => result.FrameworkVersion = double.Parse(fv)},
                "Optimizer options",
                {"os", "Suppresses struct copy elimination.",
                    (bool b) => result.Optimizer.EliminateStructCopies = !b},
                {"ot", "Suppresses temporary local variable elimination.",
                    (bool b) => result.Optimizer.EliminateTemporaries = !b},
                {"oo", "Suppresses simplification of operator expressions and special method calls.",
                    (bool b) => result.Optimizer.SimplifyOperators = !b},
                {"ol", "Suppresses simplification of loop blocks.",
                    (bool b) => result.Optimizer.SimplifyLoops = !b},
            };

            filenames = os.Parse(arguments);

            if (filenames.Count == 0) {
                var asmName = Assembly.GetExecutingAssembly().GetName();
                Console.WriteLine("==== JSILc v{0}.{1}.{2} ====", asmName.Version.Major, asmName.Version.Minor, asmName.Version.Revision);
                Console.WriteLine("Specify one or more compiled assemblies (dll/exe) to translate them. Symbols will be loaded if they exist in the same directory.");
                Console.WriteLine("You can also specify Visual Studio solution files (sln) to build them and automatically translate their output(s).");
                Console.WriteLine("Specify the path of a .jsilconfig file to load settings from it.");

                os.WriteOptionDescriptions(Console.Out);

                return null;
            }

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

            if (configuration == null)
                return;

            foreach (var filename in filenames.ToArray()) {
                var extension = Path.GetExtension(filename);
                switch (extension.ToLower()) {
                    case ".sln":
                        foreach (var resultFilename in SolutionBuilder.Build(
                            filename, 
                            configuration.SolutionBuilder.Configuration, 
                            configuration.SolutionBuilder.Platform)
                        ) {
                            filenames.Add(resultFilename);
                        }
                        break;
                }
            }

            if (configuration.ApplyDefaults)
                ApplyDefaults(configuration);

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
