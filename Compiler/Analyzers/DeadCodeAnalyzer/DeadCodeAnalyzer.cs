using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL.Compiler.Extensibility.DeadCodeAnalyzer {
    public class DeadCodeAnalyzer : IAnalyzer {
        private readonly List<AssemblyDefinition> assemblyDefinitions;
        private DeadCodeInfoProvider deadCodeInfo;

        private Configuration Configuration;

        private Stopwatch stopwatchElapsed;

        public DeadCodeAnalyzer () {
            assemblyDefinitions = new List<AssemblyDefinition>();
        }

        public string SettingsKey { get { return "DeadCodeAnalyzer"; } }

        public void SetConfiguration (IDictionary<string, object> analyzerSettings) {
            Configuration = new Configuration(analyzerSettings ?? new Dictionary<string, object>());

            if (Configuration.DeadCodeElimination) {
                Console.WriteLine("// Using dead code elimination (experimental). Turn " +
                                  "DeadCodeElimination off and report an issue if you encounter problems!");
            
                deadCodeInfo = new DeadCodeInfoProvider(Configuration);
            }
        }

        public void Analyze (AssemblyTranslator translator, AssemblyDefinition[] assemblies, TypeInfoProvider typeInfoProvider) {
            if (!Configuration.DeadCodeElimination)
                return;

            assemblyDefinitions.Clear();
            assemblyDefinitions.AddRange(assemblies);

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

            deadCodeInfo.FinishProcessing();

            stopwatchElapsed.Stop();
            Console.WriteLine("// Dead code analysis took {0} ms", stopwatchElapsed.ElapsedMilliseconds);
        }

        public bool ShouldSkipMember (AssemblyTranslator translator, MemberReference member) {
             if (!Configuration.DeadCodeElimination)
                return false;

            return !deadCodeInfo.IsUsed(member);
        }

        public IEnumerable<IFunctionTransformer> FunctionTransformers {
            get {
                yield break;
            }
        }
    }
}