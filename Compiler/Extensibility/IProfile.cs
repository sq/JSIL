using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Compiler.Extensibility {
    public interface IProfile : ICompilerExtension {
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
        void ProcessSkippedAssembly (
            Configuration configuration, string assemblyPath, TranslationResult result
        );
        void WriteOutputs (
            VariableSet variables, TranslationResult result, string path, string manifestPrefix
        );
    }
}
