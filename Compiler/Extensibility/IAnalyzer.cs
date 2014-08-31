using Mono.Cecil;

namespace JSIL.Compiler.Extensibility {
    public interface IAnalyzer : ICompilerExtension {
        void SetConfiguration (Configuration configuration);

        void AddAssemblies(AssemblyDefinition[] assemblies);

        void Analyze(TypeInfoProvider typeInfoProvider);

        bool MemberCanBeSkipped(MemberReference member);
    }
}