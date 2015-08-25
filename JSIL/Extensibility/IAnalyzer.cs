using System.Collections.Generic;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Compiler.Extensibility {
    public interface IAnalyzer {
        void SetConfiguration (IDictionary<string, object> analyzerSettings);

        void Analyze (AssemblyTranslator translator, AssemblyDefinition[] assemblies, TypeInfoProvider typeInfoProvider);

        void InitializeTransformPipeline (FunctionTransformPipeline transformPipeline);
        bool ShouldSkipMember (MemberReference member);

        string SettingsKey { get; }
    }
}