using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JSIL.Compiler.Extensibility;
using Microsoft.Build.Evaluation;

namespace JSIL.Compiler.Profiles {
    public class XNA4 : BaseProfile {
        public override bool IsAppropriateForSolution (SolutionBuilder.SolutionBuildResult buildResult) {
            return buildResult.TargetFilesUsed.Any(
                (targetFile) => targetFile.Contains(@"XNA Game Studio\v4.0")
            );
        }

        public override Configuration GetConfiguration (Configuration defaultConfiguration) {
            var result = defaultConfiguration.Clone();

            result.FrameworkVersion = 4.0;
            result.Assemblies.Proxies.Add("JSIL.Proxies.XNA4.dll");

            result.ProfileSettings.SetDefault("ContentOutputDirectory", null);

            return result;
        }

        private static void EnsureDirectoryExists (string path) {
            var directoryName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
        }

        public override SolutionBuilder.SolutionBuildResult ProcessBuildResult (Configuration configuration, SolutionBuilder.SolutionBuildResult buildResult) {
            var contentOutputDirectory = configuration.ProfileSettings.GetValueOrDefault("ContentOutputDirectory", null) as string;
            if (contentOutputDirectory == null)
                return buildResult;
            
            contentOutputDirectory = contentOutputDirectory
                .Replace("%configpath%", configuration.Path)
                .Replace("%outputpath%", configuration.OutputDirectory);

            var contentProjects = buildResult.ProjectsBuilt.Where(
                (projectFile) => projectFile.EndsWith(".contentproj")
            ).ToArray();

            foreach (var contentProjectPath in contentProjects) {
                Console.Error.WriteLine("// Processing content project '{0}' ...", contentProjectPath);

                var project = new Project(contentProjectPath);

                var contentProjectDirectory = Path.GetDirectoryName(contentProjectPath);
                var localOutputDirectory = contentOutputDirectory
                    .Replace("%contentprojectpath%", contentProjectDirectory)
                    .Replace("/", "\\");

                foreach (var item in project.Items) {
                    if (item.ItemType != "Compile")
                        continue;

                    var metadata = item.DirectMetadata.ToDictionary((md) => md.Name);
                    var importerName = metadata["Importer"].EvaluatedValue;
                    var processorName = metadata["Processor"].EvaluatedValue;
                    var sourcePath = Path.Combine(contentProjectDirectory, item.EvaluatedInclude);
                    var outputPath = Path.Combine(localOutputDirectory, item.EvaluatedInclude);

                    switch (processorName) {
                        case "PassThroughProcessor":
                            EnsureDirectoryExists(outputPath);
                            File.Copy(sourcePath, outputPath, true);
                            Console.WriteLine(outputPath.Replace(configuration.OutputDirectory, ""));
                            break;
                        case "TextureProcessor":
                            EnsureDirectoryExists(outputPath);
                            File.Copy(sourcePath, outputPath, true);
                            Console.WriteLine(outputPath.Replace(configuration.OutputDirectory, ""));
                            break;
                        default:
                            Console.Error.WriteLine("// Can't process '{0}': processor '{1}' unsupported.", item.EvaluatedInclude, processorName);
                            break;
                    }
                }

                Console.Error.WriteLine("// Done processing content.");
            }

            return base.ProcessBuildResult(configuration, buildResult);
        }
    }
}
