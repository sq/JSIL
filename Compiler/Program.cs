using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using JSIL.Compiler.Extensibility;
using JSIL.Internal;
using JSIL.Translator;
using Mono.Cecil;

namespace JSIL.Compiler {
    class Program {
        static TypeInfoProvider CachedTypeInfoProvider = null;
        static Configuration CachedTypeInfoProviderConfiguration = null;

        public static string ShortenPath (string path) {
            var cwd = new Uri(Environment.CurrentDirectory);

            Uri pathUri;
            if (Uri.TryCreate(path, UriKind.Absolute, out pathUri)) {
                var relativeUri = cwd.MakeRelativeUri(pathUri);
                var shortened = Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
                if (shortened.Length < path.Length)
                    return shortened;
            }

            return path;
        }

        static Configuration LoadConfiguration (string filename) {
            var jss = new JavaScriptSerializer();
            try {
                var json = File.ReadAllText(filename);
                var result = jss.Deserialize<Configuration>(json);

                var variables = result.ApplyTo(new VariableSet());

                result.Path = Path.GetDirectoryName(Path.GetFullPath(filename));
                result.ContributingPaths = new[] { Path.GetFullPath(filename) };

                Console.Error.WriteLine("// Applied settings from '{0}'.", ShortenPath(filename));

                return result;
            } catch (Exception ex) {
                Console.Error.WriteLine("// Error reading '{0}': {1}", filename, ex);
                throw;
            }
        }

        static string MapPath (string path, VariableSet variables, bool ensureExists, bool reportErrors = false) {
            var result = variables.ExpandPath(path, false);

            if (ensureExists) {
                if (!Directory.Exists(result) && !File.Exists(result)) {
                    if (reportErrors)
                        Console.Error.WriteLine("// Could not find file '{0}' -> '{1}'!", path, result);

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

        static Configuration ParseCommandLine (
            IEnumerable<string> arguments, List<BuildGroup> buildGroups, 
            Dictionary<string, IProfile> profiles
        ) {
            var baseConfig = new Configuration();
            var commandLineConfig = new Configuration();
            IProfile defaultProfile = new Profiles.Default();
            var profileAssemblies = new List<string>();
            bool[] autoloadProfiles = new bool[] { true };
            string[] newDefaultProfile = new string[] { null };
            List<string> filenames;

            {
                var os = new Mono.Options.OptionSet {
                    {"o=|out=", 
                        "Specifies the output directory for generated javascript and manifests.",
                        (path) => commandLineConfig.OutputDirectory = Path.GetFullPath(path) },
                    {"nac|noautoconfig", 
                        "Suppresses automatic loading of same-named .jsilconfig files located next to solutions and/or assemblies.",
                        (b) => commandLineConfig.AutoLoadConfigFiles = b == null },
                    {"nt|nothreads",
                        "Suppresses use of multiple threads to speed up the translation process.",
                        (b) => commandLineConfig.UseThreads = b == null },
                    {"sbc|suppressbugcheck",
                        "Suppresses JSIL bug checks that detect bugs in .NET runtimes and standard libraries.",
                        (b) => commandLineConfig.RunBugChecks = b == null },

                    "Solution Builder options",
                    {"configuration=", 
                        "When building one or more solution files, specifies the build configuration to use (like 'Debug').",
                        (v) => commandLineConfig.SolutionBuilder.Configuration = v },
                    {"platform=", 
                        "When building one or more solution files, specifies the build platform to use (like 'x86').",
                        (v) => commandLineConfig.SolutionBuilder.Platform = v },
                    {"target=", 
                        "When building one or more solution files, specifies the build target to use (like 'Build'). The default is 'Build'.",
                        (v) => commandLineConfig.SolutionBuilder.Target = v },
                    {"logVerbosity=", 
                        "When building one or more solution files, specifies the level of log verbosity. Valid options are 'Quiet', 'Minimal', 'Normal', 'Detailed', and 'Diagnostic'.",
                        (v) => commandLineConfig.SolutionBuilder.LogVerbosity = v },

                    "Assembly options",
                    {"p=|proxy=", 
                        "Loads a type proxy assembly to provide type information for the translator.",
                        (name) => commandLineConfig.Assemblies.Proxies.Add(Path.GetFullPath(name)) },
                    {"i=|ignore=", 
                        "Specifies a regular expression pattern for assembly names that should be ignored during the translation process.",
                        (regex) => commandLineConfig.Assemblies.Ignored.Add(regex) },
                    {"s=|stub=", 
                        "Specifies a regular expression pattern for assembly names that should be stubbed during the translation process. " +
                        "Stubbing forces all methods to be externals.",
                        (regex) => commandLineConfig.Assemblies.Stubbed.Add(regex) },
                    {"nd|nodeps", 
                        "Suppresses the automatic loading and translation of assembly dependencies.",
                        (b) => commandLineConfig.IncludeDependencies = b == null},
                    {"nodefaults", 
                        "Suppresses the default list of stubbed assemblies.",
                        (b) => commandLineConfig.ApplyDefaults = b == null},
                    {"nolocal", 
                        "Disables using local proxy types from translated assemblies.",
                        (b) => commandLineConfig.UseLocalProxies = b == null},
                    {"fv=|frameworkVersion=", 
                        "Specifies the version of the .NET framework proxies to use. " +
                        "This ensures that correct type information is provided (as 3.5 and 4.0 use different standard libraries). " +
                        "Accepted values are '3.5' and '4.0'. Default: '4.0'",
                        (fv) => commandLineConfig.FrameworkVersion = double.Parse(fv)},

                    "Profile options",
                    {"nap|noautoloadprofiles",
                        "Disables automatic loading of profile assemblies from the compiler directory.",
                        (b) => autoloadProfiles[0] = (b == null)},
                    {"pa=|profileAssembly=",
                        "Loads one or more project profiles from the specified profile assembly. Note that this does not force the profiles to be used.",
                        (filename) => profileAssemblies.Add(filename)},
                    {"dp=|defaultProfile=",
                        "Overrides the default profile to use for projects by specifying the name of the new default profile.",
                        (profileName) => newDefaultProfile[0] = profileName},

                    "Optimizer options",
                    {"os", 
                        "Suppresses struct copy elimination.",
                        (b) => commandLineConfig.Optimizer.EliminateStructCopies = b == null},
                    {"ot", 
                        "Suppresses temporary local variable elimination.",
                        (b) => commandLineConfig.Optimizer.EliminateTemporaries = b == null},
                    {"oo", 
                        "Suppresses simplification of operator expressions and special method calls.",
                        (b) => commandLineConfig.Optimizer.SimplifyOperators = b == null},
                    {"ol", 
                        "Suppresses simplification of loop blocks.",
                        (b) => commandLineConfig.Optimizer.SimplifyLoops = b == null},
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
            }

            if (commandLineConfig.ApplyDefaults.GetValueOrDefault(true)) {
                baseConfig = MergeConfigurations(
                    LoadConfiguration(Path.Combine(
                        GetJSILDirectory(), 
                        "defaults.jsilconfig"
                    )), 
                    baseConfig
                );
            }

            {
                if (autoloadProfiles[0])
                    profileAssemblies.AddRange(Directory.GetFiles(
                        GetJSILDirectory(), 
                        "JSIL.Profiles.*.dll"
                    ));

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

            commandLineConfig = MergeConfigurations(
                commandLineConfig,
                (from fn in filenames
                 where Path.GetExtension(fn) == ".jsilconfig"
                 select LoadConfiguration(fn)).ToArray()
            );

            foreach (var solution in
                     (from fn in filenames where Path.GetExtension(fn) == ".sln" select fn)
                    ) {

                var solutionFullPath = Path.GetFullPath(solution);
                var solutionDir = Path.GetDirectoryName(solutionFullPath);

                if ((solutionDir == null) || (solutionFullPath == null)) {
                    Console.Error.WriteLine("// Can't process solution '{0}' - path seems malformed", solution);
                    continue;
                }

                var solutionConfigPath = Path.Combine(
                    solutionDir,
                    String.Format("{0}.jsilconfig", Path.GetFileName(solutionFullPath))
                );
                var solutionConfig = File.Exists(solutionConfigPath)
                    ? new Configuration[] { LoadConfiguration(solutionConfigPath) }
                    : new Configuration[] {  };

                var mergedSolutionConfig = MergeConfigurations(baseConfig, solutionConfig);
                var config = MergeConfigurations(mergedSolutionConfig, commandLineConfig);
                var buildStarted = DateTime.UtcNow.Ticks;
                
                var buildResult = SolutionBuilder.SolutionBuilder.Build(
                    solutionFullPath,
                    config.SolutionBuilder.Configuration,
                    config.SolutionBuilder.Platform,
                    config.SolutionBuilder.Target ?? "Build",
                    config.SolutionBuilder.LogVerbosity
                );

                var jss = new JavaScriptSerializer();
                jss.MaxJsonLength = (1024 * 1024) * 64;
                var buildResultJson = jss.Serialize(buildResult);
                buildResult = jss.Deserialize<SolutionBuilder.BuildResult>(buildResultJson);

                var buildEnded = DateTime.UtcNow.Ticks;

                IProfile profile = defaultProfile;

                foreach (var candidateProfile in profiles.Values) {
                    if (!candidateProfile.IsAppropriateForSolution(buildResult))
                        continue;

                    Console.Error.WriteLine("// Auto-selected the profile '{0}' for this project.", candidateProfile.GetType().Name);
                    profile = candidateProfile;
                    break;
                }

                var localVariables = config.ApplyTo(new VariableSet());
                localVariables["SolutionDirectory"] = () => solutionDir;

                var processStarted = DateTime.UtcNow.Ticks;
                profile.ProcessBuildResult(
                    localVariables,
                    profile.GetConfiguration(config),
                    buildResult
                );
                var processEnded = DateTime.UtcNow.Ticks;

                {
                    var logPath = localVariables.ExpandPath(String.Format(
                        "%outputdirectory%/{0}.buildlog", Path.GetFileName(solution)
                    ), false);

                    if (!Directory.Exists(Path.GetDirectoryName(logPath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                    
                    using (var logWriter = new StreamWriter(logPath, false, Encoding.UTF8)) {
                        logWriter.WriteLine(
                            "Build of solution '{0}' processed {1} task(s) and produced {2} result file(s):",
                            solution, buildResult.AllItemsBuilt.Length, buildResult.OutputFiles.Length
                        );

                        foreach (var of in buildResult.OutputFiles)
                            logWriter.WriteLine(of);

                        logWriter.WriteLine("----");
                        logWriter.WriteLine("Elapsed build time: {0:0000.0} second(s).", TimeSpan.FromTicks(buildEnded - buildStarted).TotalSeconds);
                        logWriter.WriteLine("Selected profile '{0}' to process results of this build.", profile.GetType().Name);
                        logWriter.WriteLine("Elapsed processing time: {0:0000.0} second(s).", TimeSpan.FromTicks(processEnded - processStarted).TotalSeconds);
                    }
                }

                var outputFiles = buildResult.OutputFiles.Concat(
                    (from eo in config.SolutionBuilder.ExtraOutputs
                     let expanded = localVariables.ExpandPath(eo, true)
                     select expanded)
                ).ToArray();

                if (outputFiles.Length > 0) {
                    buildGroups.Add(new BuildGroup {
                        BaseConfiguration = mergedSolutionConfig,
                        BaseVariables = localVariables,
                        FilesToBuild = outputFiles,
                        Profile = profile,
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
                var variables = commandLineConfig.ApplyTo(new VariableSet());

                buildGroups.Add(new BuildGroup {
                    BaseConfiguration = baseConfig,
                    BaseVariables = variables,
                    FilesToBuild = mainGroup,
                    Profile = defaultProfile
                });
            }

            return commandLineConfig;
        }

        internal static string GetJSILDirectory () {
            return Path.GetDirectoryName(JSIL.Internal.Util.GetPathOfAssembly(Assembly.GetExecutingAssembly()));
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

        static AssemblyTranslator CreateTranslator (
            Configuration configuration, AssemblyManifest manifest, AssemblyCache assemblyCache
        ) {
            TypeInfoProvider typeInfoProvider = null;

            if (
                configuration.ReuseTypeInfoAcrossAssemblies.GetValueOrDefault(true) && 
                (CachedTypeInfoProvider != null)
            ) {
                if (CachedTypeInfoProviderConfiguration.Assemblies.Equals(configuration.Assemblies))
                    typeInfoProvider = CachedTypeInfoProvider;
            }

            var translator = new AssemblyTranslator(
                configuration, typeInfoProvider, manifest, assemblyCache, 
                onProxyAssemblyLoaded: (name) => {
                    Console.Error.WriteLine("// Loaded proxies from '{0}'", ShortenPath(name));
                }
            );

            translator.Decompiling += MakeProgressHandler       ("Decompiling ");
            translator.RunningTransforms += MakeProgressHandler ("Translating ");
            translator.Writing += MakeProgressHandler           ("Writing JS  ");

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

            if (typeInfoProvider == null) {
                if (CachedTypeInfoProvider != null)
                    CachedTypeInfoProvider.Dispose();

                CachedTypeInfoProvider = translator.GetTypeInfoProvider();
                CachedTypeInfoProviderConfiguration = configuration;
            }

            return translator;
        }

        static void Main (string[] arguments) {
            SolutionBuilder.SolutionBuilder.HandleCommandLine();

            var buildGroups = new List<BuildGroup>();
            var profiles = new Dictionary<string, IProfile>();
            var manifest = new AssemblyManifest();
            var assemblyCache = new AssemblyCache();

            var commandLineConfiguration = ParseCommandLine(arguments, buildGroups, profiles);

            if ((buildGroups.Count < 1) || (commandLineConfiguration == null)) {
                Console.Error.WriteLine("// No assemblies specified to translate. Exiting.");
            }

            foreach (var buildGroup in buildGroups) {
                var config = buildGroup.BaseConfiguration;
                var variables = buildGroup.BaseVariables;

                foreach (var filename in buildGroup.FilesToBuild) {
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
                        ? new Configuration[] { LoadConfiguration(fileConfigPath), commandLineConfiguration }
                        : new Configuration[] { commandLineConfiguration };

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
                    var localVariables = localConfig.ApplyTo(variables);

                    var assemblyPath = Path.GetDirectoryName(Path.GetFullPath(filename));
                    localVariables["AssemblyDirectory"] = () => assemblyPath;

                    var newProxies = (from p in localConfig.Assemblies.Proxies
                                      let newP = MapPath(p, localVariables, true, true)
                                      where newP != null
                                      select newP).ToArray();

                    localConfig.Assemblies.Proxies.Clear();
                    localConfig.Assemblies.Proxies.AddRange(newProxies);

                    using (var translator = CreateTranslator(localConfig, manifest, assemblyCache)) {
                        var outputs = buildGroup.Profile.Translate(localVariables, translator, localConfig, filename, localConfig.UseLocalProxies.GetValueOrDefault(true));
                        if (localConfig.OutputDirectory == null)
                            throw new Exception("No output directory was specified!");

                        var outputDir = MapPath(localConfig.OutputDirectory, localVariables, false);

                        Console.Error.WriteLine("// Saving output to '{0}'.", ShortenPath(outputDir) + Path.DirectorySeparatorChar);

                        // Ensures that the log file contains the name of the profile that was actually used.
                        localConfig.Profile = localProfile.GetType().Name;

                        EmitLog(outputDir, localConfig, filename, outputs);

                        buildGroup.Profile.WriteOutputs(localVariables, outputs, outputDir, Path.GetFileName(filename) + ".");
                    }
                }
            }
            
            if (Environment.UserInteractive && Debugger.IsAttached) {
                Console.Error.WriteLine("// Press the any key to continue.");
                Console.ReadKey();
            }
        }

        static void EmitLog (
            string logPath, Configuration configuration, 
            string inputFile, TranslationResult outputs
        ) {
            var logText = new StringBuilder();
            var asmName = Assembly.GetExecutingAssembly().GetName();
            logText.AppendLine(String.Format("// JSILc v{0}.{1}.{2}", asmName.Version.Major, asmName.Version.Minor, asmName.Version.Revision));
            logText.AppendLine(String.Format("// The following configuration was used when translating '{0}':", inputFile));
            logText.AppendLine((new JavaScriptSerializer()).Serialize(configuration));
            logText.AppendLine("// The configuration was generated from the following configuration files:");

            foreach (var cf in configuration.ContributingPaths)
                logText.AppendLine(cf);

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
