using System.Collections.Generic;
using Mono.Cecil;

namespace JSIL.Compiler.Extensibility {
    public interface IAnalyzer {
        void SetConfiguration (IDictionary<string, object> analyzerSettings);

        void AddAssemblies(AssemblyDefinition[] assemblies);

        void Analyze(TypeInfoProvider typeInfoProvider);

        bool MemberCanBeSkipped(MemberReference member);

        string SettingsKey { get; }
    }
}