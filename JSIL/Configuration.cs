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
        public sealed class CodeGeneratorConfiguration {
            public bool? EliminateStructCopies;
            public bool? SimplifyOperators;
            public bool? SimplifyLoops;
            public bool? EliminateTemporaries;
            public bool? EliminateRedundantControlFlow;
            public bool? CacheMethodSignatures;
            public bool? CacheGenericMethodSignatures;
            public bool? CacheTypeExpressions;
            public bool? EliminatePointlessFinallyBlocks;
            public bool? PreferAccessorMethods;
            public bool? HintIntegerArithmetic;
            public bool? EnableThreadedTransforms;
            public bool? FreezeImmutableObjects;
            public bool? EnableUnsafeCode;

            public void MergeInto (CodeGeneratorConfiguration result) {
                if (EliminateStructCopies.HasValue)
                    result.EliminateStructCopies = EliminateStructCopies;
                if (EliminateTemporaries.HasValue)
                    result.EliminateTemporaries = EliminateTemporaries;
                if (SimplifyLoops.HasValue)
                    result.SimplifyLoops = SimplifyLoops;
                if (SimplifyOperators.HasValue)
                    result.SimplifyOperators = SimplifyOperators;
                if (EliminateRedundantControlFlow.HasValue)
                    result.EliminateRedundantControlFlow = EliminateRedundantControlFlow;
                if (CacheMethodSignatures.HasValue)
                    result.CacheMethodSignatures = CacheMethodSignatures;
                if (CacheGenericMethodSignatures.HasValue)
                    result.CacheGenericMethodSignatures = CacheGenericMethodSignatures;
                if (CacheTypeExpressions.HasValue)
                    result.CacheTypeExpressions = CacheTypeExpressions;
                if (EliminatePointlessFinallyBlocks.HasValue)
                    result.EliminatePointlessFinallyBlocks = EliminatePointlessFinallyBlocks;
                if (PreferAccessorMethods.HasValue)
                    result.PreferAccessorMethods = PreferAccessorMethods;
                if (HintIntegerArithmetic.HasValue)
                    result.HintIntegerArithmetic = HintIntegerArithmetic;
                if (FreezeImmutableObjects.HasValue)
                    result.FreezeImmutableObjects = FreezeImmutableObjects;
                if (EnableUnsafeCode.HasValue)
                    result.EnableUnsafeCode = EnableUnsafeCode;
                if (EnableThreadedTransforms.HasValue)
                    result.EnableThreadedTransforms = EnableThreadedTransforms;
            }
        }

        public bool? ApplyDefaults;
        public bool? IncludeDependencies;
        public bool? UseSymbols;
        public bool? UseThreads;
        public bool? UseDefaultProxies;
        public bool? GenerateSkeletonsForStubbedAssemblies;
        public bool? GenerateContentManifest;
        public bool? RunBugChecks;
        public string FilenameEscapeRegex;

        public double? FrameworkVersion;

        public readonly AssemblyConfiguration Assemblies = new AssemblyConfiguration();
        public readonly CodeGeneratorConfiguration CodeGenerator = new CodeGeneratorConfiguration();

        protected void MergeInto (Configuration result) {
            if (ApplyDefaults.HasValue)
                result.ApplyDefaults = ApplyDefaults;
            if (IncludeDependencies.HasValue)
                result.IncludeDependencies = IncludeDependencies;
            if (UseSymbols.HasValue)
                result.UseSymbols = UseSymbols;
            if (UseThreads.HasValue)
                result.UseThreads = UseThreads;
            if (UseDefaultProxies.HasValue)
              result.UseDefaultProxies = UseDefaultProxies;

            if (FrameworkVersion.HasValue)
                result.FrameworkVersion = FrameworkVersion;
            if (GenerateSkeletonsForStubbedAssemblies.HasValue)
                result.GenerateSkeletonsForStubbedAssemblies = GenerateSkeletonsForStubbedAssemblies;
            if (GenerateContentManifest.HasValue)
                result.GenerateContentManifest = GenerateContentManifest;
            if (RunBugChecks.HasValue)
                result.RunBugChecks = RunBugChecks;

            if (FilenameEscapeRegex != null)
                result.FilenameEscapeRegex = FilenameEscapeRegex;

            Assemblies.MergeInto(result.Assemblies);
            CodeGenerator.MergeInto(result.CodeGenerator);
        }
    }
}
