using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JSIL.Compiler.Extensibility;

namespace JSIL.Compiler.Profiles {
    public abstract class BaseProfile : IProfile {
        public abstract bool IsAppropriateForSolution (SolutionBuilder.BuildResult buildResult);

        public virtual Configuration GetConfiguration (Configuration defaultConfiguration) {
            return defaultConfiguration;
        }

        public virtual TranslationResult Translate (
            VariableSet variables, AssemblyTranslator translator, Configuration configuration, 
            string assemblyPath, bool scanForProxies
        ) {
            var result = translator.Translate(assemblyPath, scanForProxies);

            return result;
        }

        public virtual void ProcessSkippedAssembly (
            Configuration configuration, string assemblyPath, TranslationResult result
        ) {
        }

        public virtual void WriteOutputs (VariableSet variables, TranslationResult result, string path, string manifestPrefix) {
            AssemblyTranslator.GenerateManifest(result.AssemblyManifest, result.AssemblyPath, result);

            Console.WriteLine(manifestPrefix + "manifest.js");

            foreach (var fe in result.OrderedFiles)
                Console.WriteLine(fe.Filename);

            result.WriteToDirectory(path, manifestPrefix);
        }

        public virtual SolutionBuilder.BuildResult ProcessBuildResult (
            VariableSet variables, Configuration configuration, SolutionBuilder.BuildResult buildResult
        ) {
            return buildResult;
        }

        protected IEnumerable<string> GetPathsForProcessedAssemblies(
            Configuration configuration, string assemblyPath, TranslationResult result)
        {
            string basePath = Path.GetDirectoryName(assemblyPath);
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

            return assemblyPaths;
        }
    }
}
