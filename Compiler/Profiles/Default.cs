using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Compiler.Extensibility;
using JSIL.Utilities;

namespace JSIL.Compiler.Profiles {
    public class Default : BaseProfile {
        public override bool IsAppropriateForSolution (SolutionBuilder.SolutionBuildResult buildResult) {
            // Normally we'd return true so that this profile is always selected, but this is our fallback profile.
            return false;
        }

        public virtual TranslationResult Translate (AssemblyTranslator translator, Configuration configuration, string assemblyPath, bool scanForProxies) {
            var result = translator.Translate(assemblyPath, scanForProxies);

            ResourceConverter.ConvertEmbeddedResources(configuration, assemblyPath, result);

            AssemblyTranslator.GenerateManifest(translator.Manifest, assemblyPath, result);

            return result;
        }
    }
}
