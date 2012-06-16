using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using JSIL.Compiler.Extensibility;
using JSIL.Internal;
using JSIL.Translator;
using Mono.Cecil;

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

                result.Path = Path.GetDirectoryName(Path.GetFullPath(filename));

                if (result.OutputDirectory != null)
                    result.OutputDirectory = MapConfigPath(result.OutputDirectory, result.Path);

                var newProxies = (from p in result.Assemblies.Proxies
                                 let newP = MapConfigPath(p, result.Path)
                                 select newP).ToArray();

                result.Assemblies.Proxies.Clear();
                result.Assemblies.Proxies.AddRange(newProxies);

                Console.Error.WriteLine("// Applied settings from '{0}'.", ShortenPath(filename));

                return result;
            } catch (Exception ex) {
                Console.Error.WriteLine("// Error reading '{0}': {1}", filename, ex);
                throw;
            }
        }

        static string MapConfigPath (string reference, string configPath) {
            return reference
                .Replace("%configpath%", configPath)
                .Replace("/", "\\");
        }

        static string MapAssemblyPath (string reference, string assemblyPath, bool ensureExists, bool reportErrors = false) {
            var result = reference
                .Replace("%assemblypath%", assemblyPath)
                .Replace("/", "\\");

            if (ensureExists) {
                if (!File.Exists(result)) {
                    if (reportErrors)
                        Console.Error.WriteLine("// Could not find proxy assembly '{0}'!", reference);

                    return null;
                }
            }

            return result;
        }

        static Configuration MergeConfigurations (Configuration baseConfiguration, params Configuration[] toMerge) {
            var result = baseConfiguration.Clone();

            foreach (var m in toMerge)
                m.MergeInto(result);

            return result;
        }

        static void ParseCommandLine (IEnumerable<string> arguments, List<BuildGroup> buildGroups, Dictionary<string, IProfile> profiles) {
            var baseConfig = new Configuration();
            IProfile defaultProfile = new Profiles.Default();
            var profileAssemblies = new List<string>();
            bool[] autoloadProfiles = new bool[] { true };
            string[] newDefaultProfile = new string[] { null };
            List<string> filenames;

            {
                var os = new Mono.Options.OptionSet {
                    {"o=|out=", 
                        "Specifies the output directory for generated javascript and manifests. " +
                        "You can use '%configpath%' in jsilconfig files to refer to the directory containing the configuration file, and '%assemblypath%' to refer to the directory containing the assembly being translated.",
                        (path) => baseConfig.OutputDirectory = Path.GetFullPath(path) },
                    {"nac|noautoconfig", 
                        "Suppresses automatic loading of same-named .jsilconfig files located next to solutions and/or assemblies.",
                        (b) => baseConfig.AutoLoadConfigFiles = b == null },
                    {"nt|nothreads",
                        "Suppresses use of multiple threads to speed up the translation process.",
                        (b) => baseConfig.UseThreads = b == null },

                    "Solution Builder options",
                    {"configuration=", 
                        "When building one or more solution files, specifies the build configuration to use (like 'Debug').",
                        (v) => baseConfig.SolutionBuilder.Configuration = v },
                    {"platform=", 
                        "When building one or more solution files, specifies the build platform to use (like 'x86').",
                        (v) => baseConfig.SolutionBuilder.Platform = v },
                    {"target=", 
                        "When building one or more solution files, specifies the build target to use (like 'Build'). The default is 'Build'.",
                        (v) => baseConfig.SolutionBuilder.Target = v },
                    {"logVerbosity=", 
                        "When building one or more solution files, specifies the level of log verbosity. Valid options are 'Quiet', 'Minimal', 'Normal', 'Detailed', and 'Diagnostic'.",
                        (v) => baseConfig.SolutionBuilder.LogVerbosity = v },

                    "Assembly options",
                    {"p=|proxy=", 
                        "Loads a type proxy assembly to provide type information for the translator.",
                        (name) => baseConfig.Assemblies.Proxies.Add(Path.GetFullPath(name)) },
                    {"i=|ignore=", 
                        "Specifies a regular expression pattern for assembly names that should be ignored during the translation process.",
                        (regex) => baseConfig.Assemblies.Ignored.Add(regex) },
                    {"s=|stub=", 
                        "Specifies a regular expression pattern for assembly names that should be stubbed during the translation process. " +
                        "Stubbing forces all methods to be externals.",
                        (regex) => baseConfig.Assemblies.Stubbed.Add(regex) },
                    {"nd|nodeps", 
                        "Suppresses the automatic loading and translation of assembly dependencies.",
                        (b) => baseConfig.IncludeDependencies = b == null},
                    {"nodefaults", 
                        "Suppresses the default list of stubbed assemblies.",
                        (b) => baseConfig.ApplyDefaults = b == null},
                    {"nolocal", 
                        "Disables using local proxy types from translated assemblies.",
                        (b) => baseConfig.UseLocalProxies = b == null},
                    {"fv=|frameworkVersion=", 
                        "Specifies the version of the .NET framework proxies to use. " +
                        "This ensures that correct type information is provided (as 3.5 and 4.0 use different standard libraries). " +
                        "Accepted values are '3.5' and '4.0'. Default: '4.0'",
                        (fv) => baseConfig.FrameworkVersion = double.Parse(fv)},

                    "Profile options",
                    {"nap|noautoloadprofiles",
                        "Disables automatic loading of profile assemblies from the compiler directory.",
                        (b) => autoloadProfiles[0] = (b == null)},
                    {"pa=|profileAssembly=",
                        "Loads one or more project profiles from the specified profile assembly. Note that this does not force the profiles to be used.",
                        (filename) => profileAssemblies.Add(filename)},
                    {"dp=|defaultProfile=",
                        "Overrides the default profile to use for projects by specifying the name of the new default profile..",
                        (profileName) => newDefaultProfile[0] = profileName},

                    "Optimizer options",
                    {"os", 
                        "Suppresses struct copy elimination.",
                        (b) => baseConfig.Optimizer.EliminateStructCopies = b == null},
                    {"ot", 
                        "Suppresses temporary local variable elimination.",
                        (b) => baseConfig.Optimizer.EliminateTemporaries = b == null},
                    {"oo", 
                        "Suppresses simplification of operator expressions and special method calls.",
                        (b) => baseConfig.Optimizer.SimplifyOperators = b == null},
                    {"ol", 
                        "Suppresses simplification of loop blocks.",
                        (b) => baseConfig.Optimizer.SimplifyLoops = b == null},
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

            {
                if (autoloadProfiles[0])
                    profileAssemblies.AddRange(Directory.GetFiles(".", "JSIL.Profiles.*.dll"));

                foreach (var filename in profileAssemblies) {
                    var fullPath = Path.GetFullPath(filename);

                    try {
                        var assembly = Assembly.LoadFile(fullPath);

                        foreach (var type in assembly.GetTypes()) {
                            if (
                                type.FindInterfaces(
                                    (interfaceType, o) => interfaceType == (Type)o, typeof(IProfile)
                                ).Length != 1
                            )
                                continue;

                            var ctor = type.GetConstructor(
                                BindingFlags.Public | BindingFlags.Instance,
                                null, System.Type.EmptyTypes, null
                            );
                            var profileInstance = (IProfile)ctor.Invoke(new object[0]);

                            profiles.Add(type.Name, profileInstance);
                        }
                    } catch (Exception exc) {
                        Console.Error.WriteLine("Warning: Failed to load profile '{0}': {1}", filename, exc);
                    }
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
                    : new Configuration[] { };

                var config = MergeConfigurations(baseConfig, solutionConfig);
                var buildResult = SolutionBuilder.Build(
                    solution,
                    config.SolutionBuilder.Configuration,
                    config.SolutionBuilder.Platform,
                    config.SolutionBuilder.Target ?? "Build",
                    config.SolutionBuilder.LogVerbosity
                );

                IProfile profile = defaultProfile;

                foreach (var candidateProfile in profiles.Values) {
                    if (!candidateProfile.IsAppropriateForSolution(buildResult))
                        continue;

                    Console.Error.WriteLine("// Auto-selected the profile '{0}' for this project.", candidateProfile.GetType().Name);
                    profile = candidateProfile;
                    break;
                }

                profile.ProcessBuildResult(
                    profile.GetConfiguration(config), 
                    buildResult
                );

                if (buildResult.OutputFiles.Length > 0) {
                    buildGroups.Add(new BuildGroup {
                        BaseConfiguration = config,
                        FilesToBuild = buildResult.OutputFiles,
                        Profile = profile
                    });
                }
            }

            var assemblyNames = (from fn in filenames
                                 where Path.GetExtension(fn).Contains(",") ||
                                    Path.GetExtension(fn).Contains(" ") ||
                                    Path.GetExtension(fn).Contains("=")
                                 select fn).ToArray();

            var resolver = new Mono.Cecil.DefaultAssemblyResolver();
            var metaResolver = new CachingMetadataResolver(resolver);
            var resolverParameters = new ReaderParameters {
                AssemblyResolver = resolver,
                MetadataResolver = metaResolver,
                ReadSymbols = false,
                ReadingMode = ReadingMode.Deferred,
            };
            var resolvedAssemblyPaths = (from an in assemblyNames
                                      let asm = resolver.Resolve(an, resolverParameters)
                                      where asm != null
                                      select asm.MainModule.FullyQualifiedName).ToArray();

            var mainGroup = (from fn in filenames
                             where
                                 (new[] { ".exe", ".dll" }.Contains(Path.GetExtension(fn)))
                             select fn)
                             .Concat(resolvedAssemblyPaths)
                             .ToArray();

            if (mainGroup.Length > 0) {
                buildGroups.Add(new BuildGroup {
                    BaseConfiguration = baseConfig,
                    FilesToBuild = mainGroup,
                    Profile = defaultProfile
                });
            }
        }

        static Action<ProgressReporter> MakeProgressHandler (string description) {
            const int scale = 40;

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

        static AssemblyTranslator CreateTranslator (Configuration configuration, AssemblyManifest manifest, AssemblyCache assemblyCache) {
            var translator = new AssemblyTranslator(configuration, null, manifest, assemblyCache);

            translator.Decompiling += MakeProgressHandler("Decompiling   ");
            translator.Optimizing += MakeProgressHandler ("Optimizing    ");
            translator.Writing += MakeProgressHandler    ("Generating JS ");

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
            var buildGroups = new List<BuildGroup>();
            var profiles = new Dictionary<string, IProfile>();
            var manifest = new AssemblyManifest();
            var assemblyCache = new AssemblyCache();

            ParseCommandLine(arguments, buildGroups, profiles);

            if (buildGroups.Count < 1) {
                Console.Error.WriteLine("// No assemblies specified to translate. Exiting.");
                return;
            }

            foreach (var buildGroup in buildGroups) {
                var config = buildGroup.BaseConfiguration;
                if (config.ApplyDefaults.GetValueOrDefault(true))
                    config = MergeConfigurations(LoadConfiguration("defaults.jsilconfig"), config);

                foreach (var filename in buildGroup.FilesToBuild) {
                    GC.Collect();

                    if (config.Assemblies.Ignored.Any(
                        (ignoreRegex) => Regex.IsMatch(filename, ignoreRegex, RegexOptions.IgnoreCase))
                    ) {
                        Console.Error.WriteLine("// Ignoring build result '{0}' based on configuration.", Path.GetFileName(filename));
                        continue;
                    }

                    var fileConfigPath = Path.Combine(
                        Path.GetDirectoryName(filename),
                        String.Format("{0}.jsilconfig", Path.GetFileName(filename))
                    );
                    var fileConfig = File.Exists(fileConfigPath)
                        ? new Configuration[] { LoadConfiguration(fileConfigPath) }
                        : new Configuration[] { };

                    var localConfig = MergeConfigurations(config, fileConfig);

                    var localProfile = buildGroup.Profile;
                    if (localConfig.Profile != null) {
                        if (profiles.ContainsKey(localConfig.Profile))
                            localProfile = profiles[localConfig.Profile];
                        else
                            throw new Exception(String.Format(
                                "No profile named '{0}' was found. Did you load the correct profile assembly?", localConfig.Profile
                            ));
                    }

                    localConfig = localProfile.GetConfiguration(localConfig);

                    var assemblyPath = Path.GetDirectoryName(Path.GetFullPath(filename));

                    var newProxies = (from p in localConfig.Assemblies.Proxies
                                      let newP = MapAssemblyPath(p, assemblyPath, true, true)
                                      where newP != null
                                      select newP).ToArray();

                    localConfig.Assemblies.Proxies.Clear();
                    localConfig.Assemblies.Proxies.AddRange(newProxies);

                    var translator = CreateTranslator(localConfig, manifest, assemblyCache);
                    var outputs = buildGroup.Profile.Translate(translator, filename, localConfig.UseLocalProxies.GetValueOrDefault(true));
                    if (localConfig.OutputDirectory == null)
                        throw new Exception("No output directory was specified!");

                    var outputDir = MapAssemblyPath(localConfig.OutputDirectory, assemblyPath, false);

                    Console.Error.WriteLine("// Saving output to '{0}'.", ShortenPath(outputDir) + "\\");

                    // Ensures that the log file contains the name of the profile that was actually used.
                    localConfig.Profile = localProfile.GetType().Name;

                    EmitLog(outputDir, localConfig, filename, outputs);

                    buildGroup.Profile.WriteOutputs(outputs, outputDir, Path.GetFileName(filename) + ".");
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

            foreach (var fe in outputs.OrderedFiles)
                logText.AppendLine(fe.Filename);

            File.WriteAllText(
                Path.Combine(logPath, String.Format("{0}.jsillog", Path.GetFileName(inputFile))),
                logText.ToString()
            );
        }
    }
}
