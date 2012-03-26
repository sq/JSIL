using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using JSIL.Compiler.Extensibility;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;

namespace JSIL.Compiler.Profiles {
    public class XNA4 : BaseProfile {
        public HashSet<string> ContentProjectsProcessed = new HashSet<string>();

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
            result.ProfileSettings.SetDefault("JPEGQuality", 90);
            result.ProfileSettings.SetDefault("UsePNGQuant", false);
            result.ProfileSettings.SetDefault("PNGQuantColorCount", 256);
            result.ProfileSettings.SetDefault("PNGQuantOptions", "");
            result.ProfileSettings.SetDefault("PNGQuantBlacklist", new string[0]);

            return result;
        }

        public override TranslationResult Translate (AssemblyTranslator translator, string assemblyPath, bool scanForProxies) {
            var result = translator.Translate(assemblyPath, scanForProxies);

            result.Files["XNA.Colors.js"] = new ArraySegment<byte>(Encoding.UTF8.GetBytes(
                Common.MakeXNAColors()
            ));

            AssemblyTranslator.GenerateManifest(translator.Manifest, assemblyPath, result);

            return result;
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

            Dictionary<string, Common.CompressResult> existingJournal;
            Common.CompressResult? existingJournalEntry;
            var jss = new JavaScriptSerializer();

            foreach (var builtContentProject in contentProjects) {
                var contentProjectPath = builtContentProject.File;

                if (ContentProjectsProcessed.Contains(contentProjectPath))
                    continue;

                var journal = new List<Common.CompressResult>();
                var journalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JSIL", contentProjectPath.Replace("\\", "_").Replace(":", ""));

                Common.EnsureDirectoryExists(Path.GetDirectoryName(journalPath));
                if (File.Exists(journalPath)) {
                    var journalEntries = jss.Deserialize<Common.CompressResult[]>(File.ReadAllText(journalPath));
                    existingJournal = journalEntries.ToDictionary((je) => je.SourceFilename);
                } else {
                    existingJournal = null;
                }

                ContentProjectsProcessed.Add(contentProjectPath);
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
                contentManifest.AppendFormat("// {0}\r\n", JSIL.AssemblyTranslator.GetHeaderText());
                contentManifest.AppendLine();
                contentManifest.AppendLine("if (typeof (contentManifest) !== \"object\") { contentManifest = {}; };");
                contentManifest.AppendLine("contentManifest[\"" + Path.GetFileNameWithoutExtension(contentProjectPath) + "\"] = [");

                Action<string, string> logOutput = (type, filename) => {
                    var localPath = filename.Replace(localOutputDirectory, "");
                    if (localPath.StartsWith("\\"))
                        localPath = localPath.Substring(1);

                    Console.WriteLine(localPath);

                    var propertiesObject = String.Format("{{ \"sizeBytes\": {0} }}", new FileInfo(filename).Length);

                    contentManifest.AppendFormat("  [\"{0}\", \"{1}\", {2}],{3}", type, localPath.Replace("\\", "/"), propertiesObject, Environment.NewLine);
                };

                Action<ProjectItem, string, string> copyRawXnb = (item, xnbPath, type) => {
                    var outputPath = Path.Combine(
                        localOutputDirectory,
                        item.EvaluatedInclude.Replace(Path.GetExtension(item.EvaluatedInclude), ".xnb")
                    );

                    Common.EnsureDirectoryExists(Path.GetDirectoryName(outputPath));

                    File.Copy(xnbPath, outputPath, true);
                    logOutput(type, outputPath);
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
                        case "FontTextureProcessor":
                        case "FontDescriptionProcessor":
                            copyRawXnb(item, xnbPath, "SpriteFont");
                            continue;
                        case "TextureProcessor":
                            if (existingJournal == null)
                                existingJournalEntry = null;
                            else {
                                Common.CompressResult temp;
                                if (existingJournal.TryGetValue(sourcePath, out temp))
                                    existingJournalEntry = temp;
                                else
                                    existingJournalEntry = null;
                            }

                            var itemOutputDirectory = Path.Combine(localOutputDirectory, Path.GetDirectoryName(item.EvaluatedInclude));
                            var result = Common.CompressImage(
                                item.EvaluatedInclude, contentProjectDirectory, itemOutputDirectory, 
                                configuration.ProfileSettings, existingJournalEntry
                            );

                            if (result.HasValue) {
                                journal.Add(result.Value);
                                logOutput("Image", result.Value.Filename);
                            }

                            continue;
                    }

                    switch (importerName) {
                        case "XmlImporter":
                            copyRawXnb(item, xnbPath, "XNB");
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

                File.WriteAllText(
                    journalPath, jss.Serialize(journal).Replace("{", "\r\n{")
                );
            }

            if (contentProjects.Length > 0)
                Console.Error.WriteLine("// Done processing content.");

            return base.ProcessBuildResult(configuration, buildResult);
        }
    }
}
