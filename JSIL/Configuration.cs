using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace JSIL.Translator {
    [Serializable]
    public class Configuration {
        [Serializable]
        public class AssemblyConfiguration {
            public readonly List<string> Ignored = new List<string>();
            public readonly List<string> Stubbed = new List<string>();

            public readonly List<string> Proxies = new List<string>();
        }

        [Serializable]
        public class OptimizerConfiguration {
            public bool EliminateStructCopies = true;
            public bool SimplifyOperators = true;
            public bool SimplifyLoops = true;
            public bool EliminateTemporaries = true;
        }

        public bool ApplyDefaults = true;
        public bool IncludeDependencies = true;
        public bool UseSymbols = true;

        public readonly AssemblyConfiguration Assemblies = new AssemblyConfiguration();
        public readonly OptimizerConfiguration Optimizer = new OptimizerConfiguration();

        public double FrameworkVersion = 4.0;
    }
}
