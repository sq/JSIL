using Mono.Cecil;

namespace JSIL.Compiler.Extensibility {
    using System.Collections.Generic;

    public interface IAnalyzer {
        void SetConfiguration (IDictionary<string, object> analyzerSettings);

        void AddAssemblies(AssemblyDefinition[] assemblies);

        void Analyze(TypeInfoProvider typeInfoProvider);

        bool MemberCanBeSkipped(MemberReference member);
    }
}