using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Compiler.Extensibility;

namespace JSIL.Compiler.Profiles {
    public abstract class BaseProfile : IProfile {
        public abstract bool IsAppropriateForSolution (SolutionBuilder.SolutionBuildResult buildResult);

        public virtual Configuration GetConfiguration (Configuration defaultConfiguration) {
            return defaultConfiguration;
        }

        public virtual TranslationResult Translate (AssemblyTranslator translator, string assemblyPath, bool scanForProxies) {
            return translator.Translate(assemblyPath, scanForProxies);
        }

        public virtual void WriteOutputs (TranslationResult result, string path, string manifestPrefix) {
            result.WriteToDirectory(path, manifestPrefix);
        }

        public virtual SolutionBuilder.SolutionBuildResult ProcessBuildResult (SolutionBuilder.SolutionBuildResult buildResult) {
            return buildResult;
        }
    }
}
