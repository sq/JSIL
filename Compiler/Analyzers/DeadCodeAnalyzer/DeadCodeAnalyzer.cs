using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace JSIL.Compiler.Extensibility.DeadCodeAnalyzer {
    public class DeadCodeAnalyzer : IAnalyzer {
        private readonly List<MethodDefinition> entrypoints;
        private DeadCodeInfoProvider deadCodeInfo;

        private Compiler.Configuration compilerConfiguration;
        private Configuration Configuration;

        public DeadCodeAnalyzer() {
            
            entrypoints = new List<MethodDefinition>();
        }

        public void SetConfiguration(Compiler.Configuration configuration) {
            compilerConfiguration = configuration;

            if (configuration.AnalyzerSettings != null && configuration.AnalyzerSettings.ContainsKey("DeadCodeAnalyzer")) {
                Configuration = new Configuration((Dictionary<string, object>) configuration.AnalyzerSettings["DeadCodeAnalyzer"]);
            }

            if (Configuration.DeadCodeElimination.GetValueOrDefault(false)) {
                Console.WriteLine("// Using dead code elimination (experimental). Turn " +
                                  "DeadCodeElimination off and report an issue if you encounter problems!");
            
                deadCodeInfo = new DeadCodeInfoProvider(Configuration);
            }
        }

        public void AddAssemblies(AssemblyDefinition[] assemblies) {
             if (!Configuration.DeadCodeElimination.GetValueOrDefault(false))
                return;

            var foundEntrypoints = from assembly in assemblies
                                   from modules in assembly.Modules
                                   where modules.EntryPoint != null
                                   select modules.EntryPoint;

            entrypoints.AddRange(foundEntrypoints);

            deadCodeInfo.AddAssemblies(assemblies);
        }

        public void Analyze() {
            if (!Configuration.DeadCodeElimination.GetValueOrDefault(false))
                return;

            foreach (MethodDefinition method in entrypoints) {
                deadCodeInfo.WalkMethod(method);
            }

            deadCodeInfo.ResolveVirtualMethods();
        }

        public bool MemberCanBeSkipped(MemberReference member) {
             if (!Configuration.DeadCodeElimination.GetValueOrDefault(false))
                return false;

            return !deadCodeInfo.IsUsed(member);
        }
    }
}