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
        public static string[] Build (string solutionFile) {
            Console.Error.Write("// Building '{0}'... ", Path.GetFileName(solutionFile));

            var pc = new ProjectCollection();
            var parms = new BuildParameters(pc);
            var globalProperties = new Dictionary<string, string>();
            var request = new BuildRequestData(solutionFile, globalProperties, null, new string[] { "Build" }, null);

            parms.Loggers = new[] { 
                new ConsoleLogger(LoggerVerbosity.Quiet)
            };

            var result = BuildManager.DefaultBuildManager.Build(parms, request);
            var resultFiles = new HashSet<string>();

            Console.Error.WriteLine("done.");

            foreach (var kvp in result.ResultsByTarget) {
                var targetResult = kvp.Value;

                if (targetResult.Exception != null)
                    Console.Error.WriteLine("// Compilation failed for target '{0}':\r\n{1}", kvp.Key, targetResult.Exception.Message);
                else {
                    foreach (var filename in targetResult.Items)
                        resultFiles.Add(filename.ItemSpec);
                }
            }

            return resultFiles.ToArray();
        }
    }
}
