using System;
using System.Collections.Generic;

namespace JSIL.Compiler {
    [Serializable]
    public class Configuration {
        [Serializable]
        public class AssemblyConfiguration {
            public readonly List<string> IgnoredRegexes = new List<string>();
            public readonly List<string> StubbedRegexes = new List<string>();

            public readonly List<string> Proxies = new List<string>();
        }

        [Serializable]
        public class OptimizerConfiguration {
            public bool OptimizeStructCopies = true;
            public bool SimplifyOperators = true;
            public bool SimplifyLoops = true;
            public bool EliminateTemporaries = true;
        }

        public bool ApplyDefaults = true;
        public bool IncludeDependencies = true;

        public readonly AssemblyConfiguration Assemblies = new AssemblyConfiguration();
        public readonly OptimizerConfiguration Optimizer = new OptimizerConfiguration();

        public string OutputDirectory = ".";
        public string FrameworkVersion = "4.0";
    }
}
