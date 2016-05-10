using System;
using System.Collections.Generic;
using System.IO;
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

        public virtual void WriteOutputs (VariableSet variables, TranslationResultCollection result, string path,bool quiet) {
            foreach (var translationResult in result.TranslationResults) {
                if (!quiet)
                {
                    foreach (var fe in translationResult.OrderedFiles)
                        Console.WriteLine(fe.Filename);
                }

                translationResult.WriteToDirectory(path);
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
