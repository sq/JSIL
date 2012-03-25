using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using JSIL.Compiler.Extensibility;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;

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

        private static string MakeXNAColors () {
            var result = new StringBuilder();
            var colorType = typeof(Color);
            var colors = colorType.GetProperties(BindingFlags.Static | BindingFlags.Public);

            result.AppendLine("(function ($jsilxna) {");
            result.AppendLine("  $jsilxna.colors = [");

            foreach (var color in colors) {
                var colorValue = (Color)color.GetValue(null, null);

                result.AppendFormat("    [\"{0}\", {1}, {2}, {3}, {4}],\r\n", color.Name, colorValue.R, colorValue.G, colorValue.B, colorValue.A);
            }

            result.AppendLine("  ];");
            result.AppendLine("} )( JSIL.GetAssembly(\"JSIL.XNA\") );");

            return result.ToString();
        }

        public override void WriteOutputs (TranslationResult result, string path, string manifestPrefix) {
            result.Files["XNA.Colors.js"] = new ArraySegment<byte>(Encoding.UTF8.GetBytes(
                MakeXNAColors()
            ));

            base.WriteOutputs(result, path, manifestPrefix);
        }

        public override SolutionBuilder.SolutionBuildResult ProcessBuildResult (Configuration configuration, SolutionBuilder.SolutionBuildResult buildResult) {
            var contentOutputDirectory = configuration.ProfileSettings.GetValueOrDefault("ContentOutputDirectory", null) as string;
            if (contentOutputDirectory == null)
                return buildResult;
            
            contentOutputDirectory = contentOutputDirectory
                .Replace("%configpath%", configuration.Path)
                .Replace("%outputpath%", configuration.OutputDirectory);

            var projectCollection = new ProjectCollection();
            var contentProjects = buildResult.ProjectsBuilt.Where(
                (project) => project.File.EndsWith(".contentproj")
            ).ToArray();

            foreach (var builtContentProject in contentProjects) {
                var contentProjectPath = builtContentProject.File;

                Console.Error.WriteLine("// Processing content project '{0}' ...", contentProjectPath);

                var project = projectCollection.LoadProject(contentProjectPath);
                var projectProperties = project.Properties.ToDictionary(
                    (p) => p.Name
                );

                Project parentProject = null;
                Dictionary<string, ProjectProperty> parentProjectProperties = null;
                if (builtContentProject.Parent != null) {
                    parentProject = projectCollection.LoadProject(builtContentProject.Parent.File);
                    parentProjectProperties = parentProject.Properties.ToDictionary(
                        (p) => p.Name
                    );
                }

                var contentProjectDirectory = Path.GetDirectoryName(contentProjectPath);
                var localOutputDirectory = contentOutputDirectory
                    .Replace("%contentprojectpath%", contentProjectDirectory)
                    .Replace("/", "\\");

                var contentManifest = new StringBuilder();
                contentManifest.AppendLine("if (typeof (contentManifest) !== \"object\") { contentManifest = {}; };");
                contentManifest.AppendLine("contentManifest[\"" + Path.GetFileNameWithoutExtension(contentProjectPath) + "\"] = [");

                Action<string, string> logOutput = (type, filename) => {
                    var localPath = filename.Replace(localOutputDirectory, "");
                    if (localPath.StartsWith("\\"))
                        localPath = localPath.Substring(1);

                    Console.WriteLine(localPath);

                    var propertiesObject = new StringBuilder();
                    propertiesObject.AppendFormat("{{ \"sizeBytes\": {0} }}", new FileInfo(filename).Length);

                    contentManifest.AppendFormat("  [\"{0}\", \"{1}\", {2}],{3}", type, localPath.Replace("\\", "/"), propertiesObject.ToString(), Environment.NewLine);
                };

                foreach (var item in project.Items) {
                    if (item.ItemType != "Compile")
                        continue;

                    var metadata = item.DirectMetadata.ToDictionary((md) => md.Name);
                    var importerName = metadata["Importer"].EvaluatedValue;
                    var processorName = metadata["Processor"].EvaluatedValue;
                    var sourcePath = Path.Combine(contentProjectDirectory, item.EvaluatedInclude);
                    string xnbPath = null;

                    if (parentProjectProperties != null) {
                        xnbPath = Path.Combine(
                            Path.Combine(
                                Path.Combine(
                                    Path.GetDirectoryName(builtContentProject.Parent.File),
                                    parentProjectProperties["OutputPath"].EvaluatedValue
                                ),
                                "Content"
                            ),
                            item.EvaluatedInclude.Replace(Path.GetExtension(item.EvaluatedInclude), ".xnb")
                        );
                    }

                    switch (processorName) {
                        case "TextureProcessor":
                            var outputPath = Path.Combine(localOutputDirectory, item.EvaluatedInclude);
                            EnsureDirectoryExists(outputPath);
                            File.Copy(sourcePath, outputPath, true);
                            logOutput("Image", outputPath);
                            continue;
                    }

                    switch (importerName) {
                        case "XmlImporter":
                            var outputPath = Path.Combine(
                                localOutputDirectory, 
                                item.EvaluatedInclude.Replace(Path.GetExtension(item.EvaluatedInclude), ".xnb")
                            );
                            EnsureDirectoryExists(outputPath);
                            File.Copy(xnbPath, outputPath, true);
                            logOutput("XNB", outputPath);
                            break;
                        default:
                            Console.Error.WriteLine("// Can't process '{0}': importer '{1}' and processor '{2}' both unsupported.", item.EvaluatedInclude, importerName, processorName);
                            break;
                    }
                }

                contentManifest.AppendLine("];");
                File.WriteAllText(
                    Path.Combine(configuration.OutputDirectory, Path.GetFileName(contentProjectPath) + ".manifest.js"),
                    contentManifest.ToString()
                );
            }

            if (contentProjects.Length > 0)
                Console.Error.WriteLine("// Done processing content.");

            return base.ProcessBuildResult(configuration, buildResult);
        }
    }
}
