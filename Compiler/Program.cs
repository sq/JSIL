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
        public static string ShortenPath (string path) {
            var cwd = new Uri(Environment.CurrentDirectory);

            Uri pathUri;
            if (Uri.TryCreate(path, UriKind.Absolute, out pathUri)) {
                var relativeUri = cwd.MakeRelativeUri(pathUri);
                return Uri.UnescapeDataString(relativeUri.ToString()).Replace("/", "\\");
            }

            return path;
        }

        static Configuration LoadConfiguration (string filename) {
            var jss = new JavaScriptSerializer();
            try {
                var json = File.ReadAllText(filename);
                var result = jss.Deserialize<Configuration>(json);

                result.OutputDirectory = result.OutputDirectory
                    .Replace("%configpath%", Path.GetDirectoryName(Path.GetFullPath(filename)))
                    .Replace("/", "\\");

                Console.Error.WriteLine("// Applied settings from '{0}'.", ShortenPath(filename));

                return result;
            } catch (Exception ex) {
                Console.Error.WriteLine("// Error reading '{0}': {1}", filename, ex);
                throw;
            }
        }

        static Configuration MergeConfigurations (Configuration baseConfiguration, params Configuration[] toMerge) {
            var result = baseConfiguration.Clone();

            foreach (var m in toMerge)
                m.MergeInto(result);

            return result;
        }

        static void ParseCommandLine (IEnumerable<string> arguments, List<KeyValuePair<Configuration, IEnumerable<string>>> buildGroups) {
            var baseConfig = new Configuration();
            List<string> filenames;

            {
                var os = new Mono.Options.OptionSet {
                    {"o=|out=", 
                        "Specifies the output directory for generated javascript and manifests. " +
                        "You can use '%configpath%' in jsilconfig files to refer to the directory containing the configuration file, and '%assemblypath%' to refer to the directory containing the assembly being translated.",
                        (string path) => baseConfig.OutputDirectory = Path.GetFullPath(path) },
                    {"nac|noautoconfig", 
                        "Suppresses automatic loading of same-named .jsilconfig files located next to solutions and/or assemblies.",
                        (bool b) => baseConfig.AutoLoadConfigFiles = !b },

                    "Solution Builder options",
                    {"configuration=", 
                        "When building one or more solution files, specifies the build configuration to use (like 'Debug').",
                        (string v) => baseConfig.SolutionBuilder.Configuration = v },
                    {"platform=", 
                        "When building one or more solution files, specifies the build platform to use (like 'x86').",
                        (string v) => baseConfig.SolutionBuilder.Platform = v },

                    "Assembly options",
                    {"p=|proxy=", 
                        "Loads a type proxy assembly to provide type information for the translator.",
                        (string name) => baseConfig.Assemblies.Proxies.Add(Path.GetFullPath(name)) },
                    {"i=|ignore=", 
                        "Specifies a regular expression pattern for assembly names that should be ignored during the translation process.",
                        (string regex) => baseConfig.Assemblies.Ignored.Add(regex) },
                    {"s=|stub=", 
                        "Specifies a regular expression pattern for assembly names that should be stubbed during the translation process. " +
                        "Stubbing forces all methods to be externals.",
                        (string regex) => baseConfig.Assemblies.Stubbed.Add(regex) },
                    {"nd|nodeps", 
                        "Suppresses the automatic loading and translation of assembly dependencies.",
                        (bool b) => baseConfig.IncludeDependencies = !b},
                    {"nodefaults", 
                        "Suppresses the default list of stubbed assemblies.",
                        (bool b) => baseConfig.ApplyDefaults = !b},
                    {"nolocal", 
                        "Disables using local proxy types from translated assemblies.",
                        (bool b) => baseConfig.UseLocalProxies = !b},
                    {"fv=|frameworkVersion=", 
                        "Specifies the version of the .NET framework proxies to use. " +
                        "This ensures that correct type information is provided (as 3.5 and 4.0 use different standard libraries). " +
                        "Accepted values are '3.5' and '4.0'. Default: '4.0'",
                        (string fv) => baseConfig.FrameworkVersion = double.Parse(fv)},

                    "Optimizer options",
                    {"os", 
                        "Suppresses struct copy elimination.",
                        (bool b) => baseConfig.Optimizer.EliminateStructCopies = !b},
                    {"ot", 
                        "Suppresses temporary local variable elimination.",
                        (bool b) => baseConfig.Optimizer.EliminateTemporaries = !b},
                    {"oo", 
                        "Suppresses simplification of operator expressions and special method calls.",
                        (bool b) => baseConfig.Optimizer.SimplifyOperators = !b},
                    {"ol", 
                        "Suppresses simplification of loop blocks.",
                        (bool b) => baseConfig.Optimizer.SimplifyLoops = !b},
                };

                filenames = os.Parse(arguments);

                if (filenames.Count == 0) {
                    var asmName = Assembly.GetExecutingAssembly().GetName();
                    Console.WriteLine("==== JSILc v{0}.{1}.{2} ====", asmName.Version.Major, asmName.Version.Minor, asmName.Version.Revision);
                    Console.WriteLine("Specify one or more compiled assemblies (dll/exe) to translate them. Symbols will be loaded if they exist in the same directory.");
                    Console.WriteLine("You can also specify Visual Studio solution files (sln) to build them and automatically translate their output(s).");
                    Console.WriteLine("Specify the path of a .jsilconfig file to load settings from it.");

                    os.WriteOptionDescriptions(Console.Out);

                    return;
                }
            }

            baseConfig = MergeConfigurations(
                baseConfig,
                (from fn in filenames
                 where Path.GetExtension(fn) == ".jsilconfig"
                 select LoadConfiguration(fn)).ToArray()
            );

            foreach (var solution in
                     (from fn in filenames where Path.GetExtension(fn) == ".sln" select fn)
                    ) {

                var solutionConfigPath = Path.Combine(
                    Path.GetDirectoryName(solution),
                    String.Format("{0}.jsilconfig", Path.GetFileName(solution))
                );
                var solutionConfig = File.Exists(solutionConfigPath) 
                    ? new Configuration[] { LoadConfiguration(solutionConfigPath) } 
                    : new Configuration[] {};

                var config = MergeConfigurations(baseConfig, solutionConfig);
                var outputs = SolutionBuilder.Build(
                    solution,
                    config.SolutionBuilder.Configuration,
                    config.SolutionBuilder.Platform
                );

                buildGroups.Add(new KeyValuePair<Configuration, IEnumerable<string>>(
                    config, outputs
                ));
            }

            var mainGroup = (from fn in filenames
                             where
                                 (new[] { ".exe", ".dll" }.Contains(Path.GetExtension(fn)))
                             select fn).ToArray();

            if (mainGroup.Length > 0)
                buildGroups.Add(new KeyValuePair<Configuration, IEnumerable<string>>(
                    baseConfig, mainGroup
                ));
        }

        static Action<ProgressReporter> MakeProgressHandler (string description) {
            const int scale = 20;

            return (progress) => {
                Console.Error.Write("// {0} ", description);

                var previous = new int[1] { 0 };

                progress.ProgressChanged += (s, p, max) => {
                    var current = p * scale / max;
                    var delta = current - previous[0];
                    if (delta > 0) {
                        previous[0] = current;

                        for (var i = 0; i < delta; i++)
                            Console.Error.Write(".");
                    }
                };

                progress.Finished += (s, e) => {
                    var delta = scale - previous[0];
                    for (var i = 0; i < delta; i++)
                        Console.Error.Write(".");

                    Console.Error.WriteLine(" done.");
                };
            };
        }

        static AssemblyTranslator CreateTranslator (Configuration configuration) {
            var translator = new AssemblyTranslator(configuration);

            translator.Decompiling += MakeProgressHandler("Decompiling   ");
            translator.Optimizing += MakeProgressHandler ("Optimizing    ");
            translator.Writing += MakeProgressHandler    ("Generating JS ");

            var indentLevel = new int[1] { 0 };

            translator.AssemblyLoaded += (fn) => {
                Console.Error.WriteLine("// Loaded {0}", ShortenPath(fn));
            };
            translator.CouldNotLoadSymbols += (fn, ex) => {
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
            var buildGroups = new List<KeyValuePair<Configuration, IEnumerable<string>>>();

            ParseCommandLine(arguments, buildGroups);

            if (buildGroups.Count < 1)
                return;

            foreach (var kvp in buildGroups) {
                var config = kvp.Key;
                if (config.ApplyDefaults.GetValueOrDefault(true))
                    config = MergeConfigurations(LoadConfiguration("defaults.jsilconfig"), config);

                foreach (var filename in kvp.Value) {
                    var fileConfigPath = Path.Combine(
                        Path.GetDirectoryName(filename),
                        String.Format("{0}.jsilconfig", Path.GetFileName(filename))
                    );
                    var fileConfig = File.Exists(fileConfigPath)
                        ? new Configuration[] { LoadConfiguration(fileConfigPath) }
                        : new Configuration[] { };
                    var localConfig = MergeConfigurations(config, fileConfig);

                    var translator = CreateTranslator(localConfig);
                    var outputs = translator.Translate(filename, localConfig.UseLocalProxies.GetValueOrDefault(true));
                    var outputDir = localConfig.OutputDirectory
                        .Replace("%assemblypath%", Path.GetDirectoryName(Path.GetFullPath(filename)));

                    Console.Error.Write("// Saving to '{0}' ...", ShortenPath(outputDir) + "\\");

                    EmitLog(outputDir, localConfig, filename, outputs);

                    outputs.WriteToDirectory(outputDir);

                    Console.Error.WriteLine(" done.");
                }
            }
        }

        static void EmitLog (string logPath, Configuration configuration, string inputFile, TranslationResult outputs) {
            var logText = new StringBuilder();
            var asmName = Assembly.GetExecutingAssembly().GetName();
            logText.AppendLine(String.Format("// JSILc v{0}.{1}.{2}", asmName.Version.Major, asmName.Version.Minor, asmName.Version.Revision));
            logText.AppendLine(String.Format("// The following settings were used when translating '{0}':", inputFile));
            logText.AppendLine((new JavaScriptSerializer()).Serialize(configuration));
            logText.AppendLine("// The following outputs were produced:");
            foreach (var kvp2 in outputs.Files)
                logText.AppendLine(kvp2.Key);

            File.WriteAllText(
                Path.Combine(logPath, String.Format("{0}.jsillog", Path.GetFileName(inputFile))),
                logText.ToString()
            );
        }
    }
}
