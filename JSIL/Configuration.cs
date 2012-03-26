using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace JSIL.Translator {
    [Serializable]
    public class Configuration {
        [Serializable]
        public sealed class AssemblyConfiguration {
            public readonly List<string> Ignored = new List<string>();
            public readonly List<string> Stubbed = new List<string>();

            public readonly List<string> Proxies = new List<string>();

            public void MergeInto (AssemblyConfiguration result) {
                result.Ignored.AddRange(Ignored);
                result.Stubbed.AddRange(Stubbed);
                result.Proxies.AddRange(Proxies);
            }
        }

        [Serializable]
        public sealed class OptimizerConfiguration {
            public bool? EliminateStructCopies;
            public bool? SimplifyOperators;
            public bool? SimplifyLoops;
            public bool? EliminateTemporaries;
            public bool? EliminateDuplicateControlFlow;

            public void MergeInto (OptimizerConfiguration result) {
                if (EliminateStructCopies.HasValue)
                    result.EliminateStructCopies = EliminateStructCopies;
                if (EliminateTemporaries.HasValue)
                    result.EliminateTemporaries = EliminateTemporaries;
                if (SimplifyLoops.HasValue)
                    result.SimplifyLoops = SimplifyLoops;
                if (SimplifyOperators.HasValue)
                    result.SimplifyOperators = SimplifyOperators;
                if (EliminateDuplicateControlFlow.HasValue)
                    result.EliminateDuplicateControlFlow = EliminateDuplicateControlFlow;
            }
        }

        public bool? ApplyDefaults;
        public bool? IncludeDependencies;
        public bool? UseSymbols;
        public bool? UseThreads;

        public double? FrameworkVersion;

        public readonly AssemblyConfiguration Assemblies = new AssemblyConfiguration();
        public readonly OptimizerConfiguration Optimizer = new OptimizerConfiguration();

        protected void MergeInto (Configuration result) {
            if (ApplyDefaults.HasValue)
                result.ApplyDefaults = ApplyDefaults;
            if (IncludeDependencies.HasValue)
                result.IncludeDependencies = IncludeDependencies;
            if (UseSymbols.HasValue)
                result.UseSymbols = UseSymbols;
            if (UseThreads.HasValue)
                result.UseThreads = UseThreads;
            if (FrameworkVersion.HasValue)
                result.FrameworkVersion = FrameworkVersion;

            Assemblies.MergeInto(result.Assemblies);
            Optimizer.MergeInto(result.Optimizer);
        }
    }
}
