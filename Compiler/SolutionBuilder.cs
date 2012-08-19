using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace JSIL.SolutionBuilder {
    public static class SolutionBuilder {
        private static object GetField (object target, string fieldName, BindingFlags fieldFlags) {
            return target.GetType().GetField(fieldName, fieldFlags).GetValue(target);        
        }

        private static void SetField (object target, string fieldName, BindingFlags fieldFlags, object value) {
            target.GetType().GetField(fieldName, fieldFlags).SetValue(target, value);            
        }

        // The only way to actually specify a solution configuration/platform is by messing around with internal/private types!
        // Using the normal globalProperties method to set configuration/platform will break all the projects inside the
        //  solution by forcibly overriding their configuration/platform. MSBuild is garbage.
        public static ProjectInstance[] ParseSolutionFile (
            string solutionFile, string buildConfiguration, string buildPlatform,
            Dictionary<string, string> globalProperties, BuildManager manager
        ) {
            var asmBuild = manager.GetType().Assembly;

            // Find the types used internally by MSBuild to convert .sln files into MSBuild projects.
            var tSolutionParser = asmBuild.GetType("Microsoft.Build.Construction.SolutionParser", true);
            var tProjectGenerator = asmBuild.GetType("Microsoft.Build.Construction.SolutionProjectGenerator", true);

            // Create an instance of the solution parser. The ctor is internal, hence the second arg.
            var solutionParser = Activator.CreateInstance(tSolutionParser, true);

            var fieldFlags = BindingFlags.Instance | 
                BindingFlags.FlattenHierarchy | 
                BindingFlags.NonPublic | 
                BindingFlags.Public;

            Func<object, string, object> getField = (target, fieldName) =>
                GetField(target, fieldName, fieldFlags);

            Action<object, string, object> setField = (target, fieldName, value) =>
                SetField(target, fieldName, fieldFlags, value);

            // Point the solution parser instance to the solution file.
            setField(solutionParser, "solutionFile", solutionFile);
            // Parse the solution file. The generator will use the parsed information later.
            solutionParser.GetType().InvokeMember(
                "ParseSolutionFile",
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                null, solutionParser, new object[0]
            );

            // Override the configuration and platform that may have been selected when parsing the solution
            //  file.
            if (buildConfiguration != null)
                setField(solutionParser, "defaultConfigurationName", buildConfiguration);
            if (buildPlatform != null)
                setField(solutionParser, "defaultPlatformName", buildPlatform);

            // Forces the solution parser to scan project dependencies and select the configuration/platform
            //  that we provided above.
            if ((buildConfiguration != null) || (buildPlatform != null))
                setField(solutionParser, "solutionContainsWebDeploymentProjects", true);

            // The generator needs a logging service and build context.
            var loggingService = manager.GetType().InvokeMember(
                "Microsoft.Build.BackEnd.IBuildComponentHost.get_LoggingService",
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                null, manager, new object[0]
            );
            var context = new BuildEventContext(0, 0, 0, 0);

            // Convert the parsed solution into one or more project instances that we can build.
            var result = tProjectGenerator.InvokeMember(
                "Generate", 
                BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                null, null, new object[] {
                    solutionParser, 
                    globalProperties,
                    null,
                    context,
                    loggingService
                }
            );

            return (ProjectInstance[])result;
        }

        public static void HandleCommandLine (int connectTimeoutMs = 2500) {
            var commandLineArgs = Environment.GetCommandLineArgs();
            if ((commandLineArgs.Length == 3) && (commandLineArgs[1] == "--buildSolution")) {
                try {
                    var jss = new JavaScriptSerializer();
                    jss.MaxJsonLength = 1024 * 1024 * 64;

                    var pipeId = commandLineArgs[2];

                    using (var pipe = new NamedPipeClientStream(pipeId)) {
                        pipe.Connect(connectTimeoutMs);

                        using (var sr = new StreamReader(pipe))
                        using (var sw = new StreamWriter(pipe)) {
                            var argsJson = sr.ReadLine();
                            var argsDict = jss.Deserialize<Dictionary<string, object>>(argsJson);

                            var buildResult = Build(
                                (string)argsDict["solutionFile"],
                                (string)argsDict["buildConfiguration"],
                                (string)argsDict["buildPlatform"],
                                (string)argsDict["buildTarget"],
                                (string)argsDict["logVerbosity"],
                                true
                            );

                            var resultJson = jss.Serialize(buildResult);

                            sw.WriteLine(resultJson);
                            sw.Flush();
                            pipe.Flush();
                            pipe.WaitForPipeDrain();
                        }
                    }
                } catch (Exception exc) {
                    Console.Error.WriteLine(exc.ToString());
                    Environment.Exit(1);
                }

                Environment.Exit(0);
            }
        }

        private static BuildResult OutOfProcessBuild (Dictionary<string, object> arguments, int startupTimeoutMs = 5000) {
            var jss = new JavaScriptSerializer();
            jss.MaxJsonLength = 1024 * 1024 * 64;

            var argsJson = jss.Serialize(arguments);
            var pipeId = String.Format("JSIL.Build{0:X4}", (new Random()).Next());

            Console.Error.WriteLine("// Starting out-of-process solution build with ID '{0}'...", pipeId);

            using (var pipe = new NamedPipeServerStream(
                pipeId, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous
            )) {
                var psi = new ProcessStartInfo {
                    FileName = JSIL.Internal.Util.GetPathOfAssembly(Assembly.GetExecutingAssembly()),
                    Arguments = String.Format("--buildSolution {0}", pipeId),
                    WorkingDirectory = Environment.CurrentDirectory,
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    ErrorDialog = false                    
                };
                var childProcess = Process.Start(psi);

                var connectedEvent = new ManualResetEventSlim(false);
                var exitedEvent = new ManualResetEventSlim(false);

                try {
                    var connectAR = pipe.BeginWaitForConnection((_) => connectedEvent.Set(), null);

                    try {
                        childProcess.Exited += (s, e) => exitedEvent.Set();
                        if (childProcess.HasExited)
                            exitedEvent.Set();
                    } catch {
                    }

                    WaitHandle.WaitAny(
                        new[] { connectedEvent.WaitHandle, exitedEvent.WaitHandle }, startupTimeoutMs
                    );

                    if (connectedEvent.IsSet) {
                        pipe.EndWaitForConnection(connectAR);
                    } else if (exitedEvent.IsSet) {
                        Console.Error.WriteLine("// Out-of-process solution build terminated unexpectedly with code {0}!", childProcess.ExitCode);
                        Environment.Exit(1);
                    } else {
                        Console.Error.WriteLine("// Out-of-process solution build timed out!");
                        Environment.Exit(2);
                    }

                    using (var sr = new StreamReader(pipe))
                    using (var sw = new StreamWriter(pipe)) {
                        sw.WriteLine(argsJson);
                        sw.Flush();
                        pipe.Flush();
                        pipe.WaitForPipeDrain();

                        var resultJson = sr.ReadLine();
                        var buildResult = jss.Deserialize<BuildResult>(resultJson);

                        Console.Error.WriteLine("// Out-of-process solution build completed successfully.");

                        return buildResult;
                    }
                } finally {
                    try {
                        if (!childProcess.HasExited)
                            childProcess.Kill();
                    } catch {
                    }

                    childProcess.Dispose();
                }
            }
        }

        public static BuildResult Build (
            string solutionFile, string buildConfiguration = null, 
            string buildPlatform = null, string buildTarget = "Build", 
            string logVerbosity = null, bool inProcess = false
        ) {
            if (!inProcess) {
                var argsDict = new Dictionary<string, object> {
                    {"solutionFile", solutionFile},
                    {"buildConfiguration", buildConfiguration},
                    {"buildPlatform", buildPlatform},
                    {"buildTarget", buildTarget},
                    {"logVerbosity", logVerbosity}
                };
                return OutOfProcessBuild(argsDict);
            }

            string configString = String.Format("{0}|{1}", buildConfiguration ?? "<default>", buildPlatform ?? "<default>");

            if ((buildConfiguration ?? buildPlatform) != null)
                Console.Error.WriteLine("// Running target '{2}' of '{0}' ({1}) ...", JSIL.Compiler.Program.ShortenPath(solutionFile), configString, buildTarget);
            else
                Console.Error.WriteLine("// Running target '{1}' of '{0}' ...", JSIL.Compiler.Program.ShortenPath(solutionFile), buildTarget);

            var pc = new ProjectCollection();
            var parms = new BuildParameters(pc);
            var globalProperties = new Dictionary<string, string>();

            var hostServices = new HostServices();
            var eventRecorder = new BuildEventRecorder();
            LoggerVerbosity _logVerbosity;

            if ((logVerbosity == null) || !Enum.TryParse(logVerbosity, out _logVerbosity))
                _logVerbosity = LoggerVerbosity.Quiet;

            parms.Loggers = new ILogger[] { 
                new ConsoleLogger(_logVerbosity), eventRecorder
            };

            var manager = BuildManager.DefaultBuildManager;

            Console.Error.Write("// Generating MSBuild projects for solution '{0}'...", Path.GetFileName(solutionFile));
            // Begin a fake build so the manager has a logger available.
            manager.BeginBuild(parms);

            var projects = ParseSolutionFile(
                solutionFile, buildConfiguration, buildPlatform,
                globalProperties, manager
            );

            manager.EndBuild();
            Console.Error.WriteLine(" {0} project(s) generated.", projects.Length);

            if (File.ReadAllText(solutionFile).Contains("ProjectSection(ProjectDependencies)")) {
                Console.Error.WriteLine("// WARNING: Your solution file contains project dependencies. MSBuild ignores these, so your build may fail. If it does, try building it in Visual Studio first to resolve the dependencies.");
            }

            var allItemsBuilt = new List<BuiltItem>();
            var resultFiles = new HashSet<string>();

            foreach (var project in projects) {
                // Save out the generated msbuild project for each solution, to aid debugging.
                try {
                    project.ToProjectRootElement().Save(project.FullPath, Encoding.UTF8);
                } catch (Exception exc) {
                    Console.Error.WriteLine("// Failed to save generated project '{0}': {1}", Path.GetFileName(project.FullPath), exc.Message);
                }
            }

            foreach (var project in projects) {
                Console.Error.WriteLine("// Building project '{0}'...", project.FullPath);

                var request = new BuildRequestData(
                    project, new string[] { buildTarget }, 
                    hostServices, BuildRequestDataFlags.None
                );

                Microsoft.Build.Execution.BuildResult result = null;
                try {
                    result = manager.Build(parms, request);
                } catch (Exception exc) {
                    Console.Error.WriteLine("// Compilation failed: {0}", exc.Message);
                    continue;
                }

                allItemsBuilt.AddRange(ExtractChildProjectResults(manager));

                foreach (var kvp in result.ResultsByTarget) {
                    var targetResult = kvp.Value;

                    if ((targetResult.Exception != null) || (targetResult.ResultCode == TargetResultCode.Failure)) {
                        string errorMessage = "Unknown error";
                        if (targetResult.Exception != null)
                            errorMessage = targetResult.Exception.Message;
                        Console.Error.WriteLine("// Compilation failed for target '{0}': {1}", kvp.Key, errorMessage);
                    }
                }
            }

            // ResultsByTarget doesn't reliably produce all the output executables, so we must
            //  extract them by hand.
            foreach (var builtItem in allItemsBuilt) {
                if (builtItem.TargetName != "Build")
                    continue;

                if (!File.Exists(builtItem.OutputPath)) {
                    Console.Error.WriteLine("// Ignoring nonexistent build output '" + Path.GetFileName(builtItem.OutputPath) + "'.");
                    continue;
                }

                var extension = Path.GetExtension(builtItem.OutputPath).ToLowerInvariant();

                switch (extension) {
                    case ".exe":
                    case ".dll":
                        resultFiles.Add(builtItem.OutputPath);
                        break;

                    default:
                        Console.Error.WriteLine("// Ignoring build output '" + Path.GetFileName(builtItem.OutputPath) + "' due to unknown file type.");
                        break;
                }
            }

            return new BuildResult(
                Path.GetFullPath(solutionFile),
                resultFiles.ToArray(),
                eventRecorder.ProjectsById.Values.ToArray(),
                eventRecorder.TargetFiles.ToArray(),
                allItemsBuilt.ToArray()
            );
        }
        
        // Enumerate all the projects the BuildManager built while building the projects we asked it to build.
        // This will allow us to identify any secondary outputs (like XNB files).
        private static BuiltItem[] ExtractChildProjectResults (BuildManager manager) {
            var resultsCache = GetField(manager, "resultsCache", BindingFlags.Instance | BindingFlags.NonPublic);
            var tResultsCache = resultsCache.GetType();

            var pResultsDictionary = tResultsCache.GetProperty("ResultsDictionary", BindingFlags.NonPublic | BindingFlags.Instance);
            var oResultsDictionary = pResultsDictionary.GetValue(resultsCache, null);

            var resultsDictionary = (Dictionary<int, Microsoft.Build.Execution.BuildResult>) oResultsDictionary;

            var result = new List<BuiltItem>();

            foreach (var projectResult in resultsDictionary.Values) {
                foreach (var kvp in projectResult.ResultsByTarget) {
                    result.AddRange(
                        from taskItem in kvp.Value.Items
                        select new BuiltItem(kvp.Key, taskItem)
                    );
                }
            }

            return result.ToArray();
        }
    }

    public class BuiltProject {
        public BuiltProject Parent;
        public int Id;
        public string File;

        public override string ToString () {
            return String.Format("{0} '{1}'", Id, File);
        }
    }

    public class BuildEventRecorder : ILogger {
        public readonly Dictionary<int, BuiltProject> ProjectsById = new Dictionary<int, BuiltProject>();
        public readonly HashSet<string> TargetFiles = new HashSet<string>(); 

        public void Initialize (IEventSource eventSource) {
            eventSource.ProjectStarted += (sender, args) => {
                var parentId = args.ParentProjectBuildEventContext.ProjectInstanceId;

                BuiltProject parentProject;
                ProjectsById.TryGetValue(parentId, out parentProject);

                var obj = new BuiltProject {
                    Parent = parentProject,
                    Id = args.ProjectId,
                    File = args.ProjectFile
                };

                ProjectsById[args.ProjectId] = obj;
            };
            eventSource.TargetStarted += (sender, args) =>
                TargetFiles.Add(args.TargetFile);
        }

        public string Parameters {
            get;
            set;
        }

        public void Shutdown () {
        }

        public LoggerVerbosity Verbosity {
            get;
            set;
        }
    }

    public class BuiltItem {
        public readonly string TargetName;
        public readonly string OutputPath;
        public readonly Dictionary<string, string> Metadata = new Dictionary<string, string>();

        // XMLSerializer sucks.
        public BuiltItem () {
        }

        internal BuiltItem (string targetName, ITaskItem item) {
            TargetName = targetName;
            OutputPath = item.ItemSpec;

            foreach (var name in item.MetadataNames)
                Metadata.Add((string)name, item.GetMetadata((string)name));
        }

        public override string ToString () {
            return String.Format("{0}: {1} ({2} metadata)", TargetName, OutputPath, Metadata.Count);
        }
    }

    public class BuildResult {
        public readonly string[] OutputFiles;
        public readonly BuiltProject[] ProjectsBuilt;
        public readonly string[] TargetFilesUsed;
        public readonly BuiltItem[] AllItemsBuilt;
        public readonly string SolutionPath;

        // XMLSerializer sucks.
        public BuildResult () {
        }

        internal BuildResult (
            string solutionPath,
            string[] outputFiles, BuiltProject[] projectsBuilt,
            string[] targetFiles, BuiltItem[] allItemsBuilt
        ) {
            SolutionPath = solutionPath;
            OutputFiles = outputFiles;
            ProjectsBuilt = projectsBuilt;
            TargetFilesUsed = targetFiles;
            AllItemsBuilt = allItemsBuilt;
        }
    }
}
