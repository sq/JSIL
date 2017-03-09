using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JSIL.Compiler.Extensibility;
using JSIL.Utilities;

namespace JSIL.Compiler.Profiles {
    public abstract class BaseProfile : IProfile {
        public abstract bool IsAppropriateForSolution (SolutionBuilder.BuildResult buildResult);

        public virtual Configuration GetConfiguration (Configuration defaultConfiguration) {
            return defaultConfiguration;
        }

        public virtual TranslationResultCollection Translate (
            VariableSet variables, AssemblyTranslator translator, Configuration configuration, 
            string assemblyPath, bool scanForProxies
        ) {
            var result = translator.Translate(assemblyPath, scanForProxies);

            return result;
        }

        public virtual void RegisterPostprocessors (IEnumerable<IEmitterGroupFactory> emitters, Configuration configuration, string assemblyPath, string[] skippedAssemblies) {
        }

        public virtual void WriteOutputs (VariableSet variables, TranslationResultCollection result, Configuration configuration, bool quiet) {
            foreach (var translationResult in result.TranslationResults) {
                if (!quiet)
                {
                    foreach (var fe in translationResult.OrderedFiles)
                        Console.WriteLine(fe.Filename);
                }

                translationResult.WriteToDirectory(configuration.OutputDirectory);
            }

            var jsilPath = Path.GetDirectoryName(JSIL.Internal.Util.GetPathOfAssembly(Assembly.GetExecutingAssembly()));
            var searchPath = Path.Combine(jsilPath, "JS Libraries\\JsLibraries\\");
            if (!string.IsNullOrEmpty(configuration.JsLibrariesOutputDirectory) && Directory.Exists(searchPath))
            {
                foreach (var file in Directory.GetFiles(searchPath, "*", SearchOption.AllDirectories)) {
                    var target = Uri.UnescapeDataString(Path.Combine(configuration.JsLibrariesOutputDirectory, new Uri(searchPath).MakeRelativeUri(new Uri(file)).ToString()))
                        .Replace('/', Path.DirectorySeparatorChar);
                    var directory = Path.GetDirectoryName(target);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    File.Copy(file, target, true);

                    if (!quiet)
                    {
                            Console.WriteLine(target);
                    }
                }
            }
        }

        public virtual SolutionBuilder.BuildResult ProcessBuildResult (
            VariableSet variables, Configuration configuration, SolutionBuilder.BuildResult buildResult
        ) {
            return buildResult;
        }
    }

    public abstract class BaseJavaScriptProfile : BaseProfile {
        public override void RegisterPostprocessors(IEnumerable<IEmitterGroupFactory> emitters, Configuration configuration, string assemblyPath, string[] skippedAssemblies)
        {
            foreach (var emitter in emitters)
            {
                if (emitter is JavascriptEmitterGroupFactory)
                {
                    emitter.RegisterPostprocessor(result => {
                        PostProcessAllTranslatedAssemblies(configuration, assemblyPath, result);

                        if (skippedAssemblies != null)
                        {
                            var processedAssemblies = new HashSet<string>();
                            foreach (var sa in skippedAssemblies)
                            {
                                if (processedAssemblies.Contains(sa))
                                    continue;

                                processedAssemblies.Add(sa);

                                PostProcessAssembly(configuration, sa, result);
                            }
                        }
                    });
                }
            }
        }

        protected void PostProcessAssembly(Configuration configuration, string assemblyPath, TranslationResult result)
        {
            ResourceConverter.ConvertResources(configuration, assemblyPath, result);
            ManifestResourceExtractor.ExtractFromAssembly(configuration, assemblyPath, result);
        }

        protected virtual void PostProcessAllTranslatedAssemblies(
            Configuration configuration, string assemblyPath, TranslationResult result)
        {
            string basePath = Path.GetDirectoryName(Path.GetFullPath(assemblyPath));
            List<string> assemblyPaths = new List<string>();

            foreach (var item in result.Assemblies)
            {
                var path = Path.Combine(basePath, item.Name.Name + ".dll");
                if (File.Exists(path))
                {
                    assemblyPaths.Add(path);
                }
                else
                {
                    path = Path.Combine(basePath, item.Name.Name + ".exe");
                    if (File.Exists(path))
                    {
                        assemblyPaths.Add(path);
                    }
                }
            }

            foreach (var path in assemblyPaths)
            {
                PostProcessAssembly(configuration, path, result);
            }
        }
    }
}
