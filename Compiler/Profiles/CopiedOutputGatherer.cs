using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JSIL.SolutionBuilder;
using System.Threading;

#if WINDOWS
using Microsoft.Build.Evaluation;
#endif

namespace JSIL.Utilities {
    public static class CopiedOutputGatherer {
        public static void CopyFile (string sourcePath, string destinationPath, bool overwrite) {
            const int maxRetries = 5;
            const int retryDelayMs = 500;

            bool wroteFailureMessage = false;

            for (int retries = 0; retries < maxRetries; retries++) {
                try {
                    File.Copy(sourcePath, destinationPath, overwrite);
                    if (wroteFailureMessage)
                        Console.Error.WriteLine();

                    break;
                } catch (IOException ioe) {
                    if (!wroteFailureMessage) {
                        Console.Error.Write("// Copy failed for '{0}' -> '{1}'! Retrying ", sourcePath, destinationPath);
                        wroteFailureMessage = true;
                    }

                    if (retries < (maxRetries - 1)) {
                        Console.Error.Write(".");
                    } else {
                        if (wroteFailureMessage)
                            Console.Error.WriteLine();
                        throw;
                    }
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                Thread.Sleep(retryDelayMs);
            }
        }

        public static void EnsureDirectoryExists (string directory) {
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public static void GatherFromProjectFiles (
            Compiler.VariableSet variables, Compiler.Configuration configuration, BuildResult buildResult
        ) {
#if WINDOWS
            var outputDir = variables.ExpandPath(configuration.OutputDirectory, false);

            var fileOutputDir = configuration.FileOutputDirectory;
            if (String.IsNullOrWhiteSpace(fileOutputDir))
                fileOutputDir = String.Format(@"{0}\Files", outputDir);

            fileOutputDir = variables.ExpandPath(fileOutputDir, false);

            ContentManifestWriter manifestWriter = null;

            var outputDirectorySourcePaths = new HashSet<string>(
                from bi in buildResult.AllItemsBuilt 
                where bi.TargetName == "GetCopyToOutputDirectoryItems" 
                select bi.OutputPath.ToLower()
            );

            using (var projectCollection = new ProjectCollection()) {
                foreach (var projectFilePath in (from p in buildResult.ProjectsBuilt select p.File)) {
                    var projectFileName = Path.GetFileName(projectFilePath);

                    // Skip XNA content projects because the XNA profile will automatically copy the files over
                    //  and put them in its content manifest.
                    if (projectFileName.Contains(".contentproj"))
                        continue;

                    var projectFileDirectory = Path.GetDirectoryName(projectFilePath);
                    var manifestPath = Path.Combine(fileOutputDir, projectFileName + ".manifest.js");

                    bool copiedAny = false;
                    var project = projectCollection.LoadProject(projectFilePath);

                    foreach (var item in project.AllEvaluatedItems) {
                        var ctod = item.GetMetadata("CopyToOutputDirectory");
                        if (ctod == null)
                            continue;

                        switch (ctod.EvaluatedValue) {
                            case "Always":
                            case "PreserveNewest":
                                break;

                            default:
                                continue;
                        }

                        var outputPath = Path.Combine(fileOutputDir, item.EvaluatedInclude);
                        EnsureDirectoryExists(Path.GetDirectoryName(outputPath));

                        if (!copiedAny) {
                            manifestWriter = new ContentManifestWriter(manifestPath, "Files/" + projectFileName);
                            copiedAny = true;
                        }

                        var sourcePath = Path.Combine(projectFileDirectory, item.EvaluatedInclude);
                        if (outputDirectorySourcePaths.Contains(sourcePath.ToLower())) {
                            var fileInfo = new FileInfo(sourcePath);

                            CopyFile(sourcePath, outputPath, true);

                            manifestWriter.Add("File", item.EvaluatedInclude, new Dictionary<string, object>() {
                                {"sizeBytes", fileInfo.Length}
                            });
                        }
                    }

                    if (manifestWriter != null) {
                        manifestWriter.Dispose();
                        manifestWriter = null;

                        var localPath = manifestPath.Replace(outputDir, "");
                        if (localPath.StartsWith("\\"))
                            localPath = localPath.Substring(1);
                        Console.WriteLine(localPath);
                    }
                }
            }
#else // !WINDOWS
            Console.Error.WriteLine("// CopiedOutputGatherer not running because JSIL was compiled on a non-Windows platform.");
#endif
        }
    }
}
