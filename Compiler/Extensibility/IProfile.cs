using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Compiler.Extensibility {
    public interface IProfile {
        bool IsAppropriateForSolution (SolutionBuilder.SolutionBuildResult buildResult);

        SolutionBuilder.SolutionBuildResult ProcessBuildResult (SolutionBuilder.SolutionBuildResult buildResult);
        Configuration GetConfiguration (Configuration defaultConfiguration);
        TranslationResult Translate (AssemblyTranslator translator, string assemblyPath, bool scanForProxies);
        void WriteOutputs (TranslationResult result, string path, string manifestPrefix);
    }
}
