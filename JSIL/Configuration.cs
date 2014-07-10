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

            public readonly Dictionary<string, string> Redirects = new Dictionary<string, string>();

            public void MergeInto (AssemblyConfiguration result) {
                result.Ignored.AddRange(Ignored);
                result.Stubbed.AddRange(Stubbed);
                result.Proxies.AddRange(Proxies);

                foreach (var kvp in Redirects)
                    result.Redirects[kvp.Key] = kvp.Value;
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
            public bool? CacheBaseMethodHandles;
            public bool? EliminatePointlessFinallyBlocks;
            public bool? PreferAccessorMethods;
            public bool? HintIntegerArithmetic;
            public bool? EnableThreadedTransforms;
            public bool? FreezeImmutableObjects;
            public bool? EnableUnsafeCode;
            public bool? HoistAllocations;
            public bool? HintDoubleArithmetic;
            public bool? AutoGenerateEventAccessorsInSkeletons;
            public bool? AggressivelyUseElementProxies;
            public bool? EmitAllParameterNames;

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
                if (CacheBaseMethodHandles.HasValue)
                    result.CacheBaseMethodHandles = CacheBaseMethodHandles;
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
                if (HoistAllocations.HasValue)
                    result.HoistAllocations = HoistAllocations;
                if (HintDoubleArithmetic.HasValue)
                    result.HintDoubleArithmetic = HintDoubleArithmetic;
                if (AutoGenerateEventAccessorsInSkeletons.HasValue)
                    result.AutoGenerateEventAccessorsInSkeletons = AutoGenerateEventAccessorsInSkeletons;
                if (AggressivelyUseElementProxies.HasValue)
                    result.AggressivelyUseElementProxies = AggressivelyUseElementProxies;
                if (EmitAllParameterNames.HasValue)
                    result.EmitAllParameterNames = EmitAllParameterNames;
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
        public bool? TuneGarbageCollection;
        public string FilenameEscapeRegex;
        public string AssemblyCollectionName;

        public double? FrameworkVersion;

        public readonly AssemblyConfiguration Assemblies = new AssemblyConfiguration();
        public readonly CodeGeneratorConfiguration CodeGenerator = new CodeGeneratorConfiguration();

        public virtual void MergeInto (Configuration result) {
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
            if (TuneGarbageCollection.HasValue)
                result.TuneGarbageCollection = TuneGarbageCollection;

            if (FilenameEscapeRegex != null)
                result.FilenameEscapeRegex = FilenameEscapeRegex;
            if (AssemblyCollectionName != null)
                result.AssemblyCollectionName = AssemblyCollectionName;

            Assemblies.MergeInto(result.Assemblies);
            CodeGenerator.MergeInto(result.CodeGenerator);
        }
    }
}
