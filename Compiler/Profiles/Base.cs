using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            AssemblyTranslator.GenerateManifest(translator.Manifest, assemblyPath, result);

            return result;
        }

        public virtual void WriteOutputs (VariableSet variables, TranslationResult result, string path, string manifestPrefix) {
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
    }
}
