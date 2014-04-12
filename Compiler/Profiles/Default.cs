using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Compiler.Extensibility;
using JSIL.Utilities;

namespace JSIL.Compiler.Profiles {
    public class Default : BaseProfile {
        public override bool IsAppropriateForSolution (SolutionBuilder.BuildResult buildResult) {
            // Normally we'd return true so that this profile is always selected, but this is our fallback profile.
            return false;
        }

        public override TranslationResult Translate (
            VariableSet variables, 
            AssemblyTranslator translator, 
            Configuration configuration, 
            string assemblyPath, 
            bool scanForProxies
        ) {
            var result = translator.Translate(assemblyPath, scanForProxies);

            foreach (var path in GetPathsForProcessedAssemblies(configuration, assemblyPath, result))
            {
                ProcessSkippedAssembly(configuration, path, result);
            }

            return result;
        }

        public override void ProcessSkippedAssembly (
            Configuration configuration, string assemblyPath, TranslationResult result
        ) {
            ResourceConverter.ConvertResources(configuration, assemblyPath, result);
            ManifestResourceExtractor.ExtractFromAssembly(configuration, assemblyPath, result);
        }

        public override SolutionBuilder.BuildResult ProcessBuildResult (VariableSet variables, Configuration configuration, SolutionBuilder.BuildResult buildResult) {
            CopiedOutputGatherer.GatherFromProjectFiles(
                variables, configuration, buildResult
            );

            return base.ProcessBuildResult(variables, configuration, buildResult);
        }
    }
}
