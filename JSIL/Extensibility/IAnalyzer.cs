using System.Collections.Generic;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Compiler.Extensibility {
    public interface IAnalyzer {
        void SetConfiguration (IDictionary<string, object> analyzerSettings);

        void Analyze (AssemblyTranslator translator, AssemblyDefinition[] assemblies, TypeInfoProvider typeInfoProvider);

        void InitializeTransformPipeline (AssemblyTranslator translator, FunctionTransformPipeline transformPipeline);
        bool ShouldSkipMember (AssemblyTranslator translator, MemberReference member);

        string SettingsKey { get; }
    }
}