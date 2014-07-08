using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;

namespace JSIL.Compiler.Extensibility.DeadCodeAnalyzer {
    public class DeadCodeAnalyzer : IAnalyzer {
        private readonly List<AssemblyDefinition> assemblyDefinitions;
        private DeadCodeInfoProvider deadCodeInfo;

        private Compiler.Configuration compilerConfiguration;
        private Configuration Configuration;

        private Stopwatch stopwatchElapsed;

        public DeadCodeAnalyzer() {
            assemblyDefinitions = new List<AssemblyDefinition>();
        }

        public void SetConfiguration(Compiler.Configuration configuration) {
            compilerConfiguration = configuration;

            if (configuration.AnalyzerSettings != null && configuration.AnalyzerSettings.ContainsKey("DeadCodeAnalyzer")) {
                Configuration = new Configuration((Dictionary<string, object>) configuration.AnalyzerSettings["DeadCodeAnalyzer"]);
            }
            else
            {
                Configuration = new Configuration(new Dictionary<string, object>());
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

            assemblyDefinitions.AddRange(assemblies);
        }

        public void Analyze(TypeInfoProvider typeInfoProvider) {
            if (!Configuration.DeadCodeElimination.GetValueOrDefault(false))
                return;

            deadCodeInfo.TypeInfoProvider = typeInfoProvider;

            stopwatchElapsed = new Stopwatch();
            stopwatchElapsed.Start();

            var foundEntrypoints = from assembly in assemblyDefinitions
                                   from modules in assembly.Modules
                                   where modules.EntryPoint != null
                                   select modules.EntryPoint;

            deadCodeInfo.AddAssemblies(assemblyDefinitions);


            foreach (MethodDefinition method in foundEntrypoints)
            {
                deadCodeInfo.WalkMethod(method);
            }

            deadCodeInfo.ResolveVirtualMethodsCycle();

            stopwatchElapsed.Stop();
            Console.WriteLine("// Dead code analysis took {0} ms", stopwatchElapsed.ElapsedMilliseconds);
        }

        public bool MemberCanBeSkipped(MemberReference member) {
             if (!Configuration.DeadCodeElimination.GetValueOrDefault(false))
                return false;

            return !deadCodeInfo.IsUsed(member);
        }
    }
}