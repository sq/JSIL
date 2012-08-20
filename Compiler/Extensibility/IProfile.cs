using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Compiler.Extensibility {
    public interface IProfile {
        bool IsAppropriateForSolution (SolutionBuilder.BuildResult buildResult);

        SolutionBuilder.BuildResult ProcessBuildResult (
            VariableSet variables, Configuration configuration, SolutionBuilder.BuildResult buildResult
        );
        Configuration GetConfiguration (
            Configuration defaultConfiguration
        );
        TranslationResult Translate (
            VariableSet variables, AssemblyTranslator translator, 
            Configuration configuration, string assemblyPath, bool scanForProxies
        );
        void WriteOutputs (
            VariableSet variables, TranslationResult result, string path, string manifestPrefix
        );
    }
}
