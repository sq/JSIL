using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace JSIL.Compiler {
    public static class SolutionBuilder {
        public class SolutionBuildResult {
            public readonly string[] OutputFiles;
            public readonly string[] ProjectsBuilt;
            public readonly string[] TargetFilesUsed;

            public SolutionBuildResult (string[] outputFiles, string[] projectsBuilt, string[] targetFiles) {
                OutputFiles = outputFiles;
                ProjectsBuilt = projectsBuilt;
                TargetFilesUsed = targetFiles;
            }
        }

        public static SolutionBuildResult Build (string solutionFile, string buildConfiguration = null, string buildPlatform = null) {
            string configString = String.Format("{0}|{1}", buildConfiguration ?? "<default>", buildPlatform ?? "<default>");

            if ((buildConfiguration ?? buildPlatform) != null)
                Console.Error.Write("// Building '{0}' ({1}) ...", Program.ShortenPath(solutionFile), configString);
            else
                Console.Error.Write("// Building '{0}' ...", Program.ShortenPath(solutionFile));

            var pc = new ProjectCollection();
            var parms = new BuildParameters(pc);
            var globalProperties = new Dictionary<string, string>();

            if (buildConfiguration != null)
                globalProperties["Configuration"] = buildConfiguration;
            if (buildPlatform != null)
                globalProperties["Platform"] = buildPlatform;

            var request = new BuildRequestData(
                solutionFile, globalProperties, 
                null, new string[] { "Build" }, null, 
                BuildRequestDataFlags.ReplaceExistingProjectInstance
            );

            var eventRecorder = new BuildEventRecorder();

            parms.Loggers = new ILogger[] { 
                new ConsoleLogger(LoggerVerbosity.Quiet), eventRecorder
            };

            var manager = BuildManager.DefaultBuildManager;
            var result = manager.Build(parms, request);
            var resultFiles = new HashSet<string>();

            Console.Error.WriteLine(" done.");

            foreach (var kvp in result.ResultsByTarget) {
                var targetResult = kvp.Value;

                if (targetResult.Exception != null)
                    Console.Error.WriteLine("// Compilation failed for target '{0}':\r\n{1}", kvp.Key, targetResult.Exception.Message);
                else {
                    foreach (var filename in targetResult.Items)
                        resultFiles.Add(filename.ItemSpec);
                }
            }

            return new SolutionBuildResult(
                resultFiles.ToArray(),
                eventRecorder.Projects.ToArray(),
                eventRecorder.TargetFiles.ToArray()
            );
        }
    }

    public class BuildEventRecorder : ILogger {
        public readonly HashSet<string> Projects = new HashSet<string>();
        public readonly HashSet<string> TargetFiles = new HashSet<string>(); 

        public void Initialize (IEventSource eventSource) {
            eventSource.ProjectStarted += (sender, args) =>
                Projects.Add(args.ProjectFile);
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
}
