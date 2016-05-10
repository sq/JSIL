using System;
using System.Collections.Generic;

namespace JSIL.Translator {
    [Serializable]
    public class Configuration {
        [Serializable]
        public sealed class AssemblyConfiguration {
            public readonly List<string> TranslateAdditional = new List<string>();
            public readonly List<string> Ignored = new List<string>();
            public readonly List<string> Stubbed = new List<string>();

            public readonly List<string> Proxies = new List<string>();

            public readonly Dictionary<string, string> Redirects = new Dictionary<string, string>();

            public void MergeInto (AssemblyConfiguration result) {
                result.TranslateAdditional.AddRange(TranslateAdditional);
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
            public bool? DisableGenericSignaturesLocalCache;
            public bool? PreferLocalCacheForGenericMethodSignatures;
            public bool? PreferLocalCacheForGenericInterfaceMethodSignatures;
            public bool? CacheOneMethodSignaturePerMethod;
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
            public bool? StripUnusedLoopNames;
            public bool? IntroduceCharCasts;
            public bool? IntroduceEnumCasts;
            public bool? EmulateInt64;
            public bool? DecomposeAllMutationOperators;

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
                if (DisableGenericSignaturesLocalCache.HasValue)
                    result.DisableGenericSignaturesLocalCache = DisableGenericSignaturesLocalCache;
                if (PreferLocalCacheForGenericMethodSignatures.HasValue)
                    result.PreferLocalCacheForGenericMethodSignatures = PreferLocalCacheForGenericMethodSignatures;
                if (PreferLocalCacheForGenericInterfaceMethodSignatures.HasValue)
                    result.PreferLocalCacheForGenericInterfaceMethodSignatures = PreferLocalCacheForGenericInterfaceMethodSignatures;
                if (CacheOneMethodSignaturePerMethod.HasValue)
                    result.CacheOneMethodSignaturePerMethod = CacheOneMethodSignaturePerMethod;
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
                if (StripUnusedLoopNames.HasValue)
                    result.StripUnusedLoopNames = StripUnusedLoopNames;
                if (IntroduceCharCasts.HasValue)
                    result.IntroduceCharCasts = IntroduceCharCasts;
                if (IntroduceEnumCasts.HasValue)
                    result.IntroduceEnumCasts = IntroduceEnumCasts;
                if (EmulateInt64.HasValue)
                    result.EmulateInt64 = EmulateInt64;
                if (DecomposeAllMutationOperators.HasValue)
                    result.DecomposeAllMutationOperators = DecomposeAllMutationOperators;
            }
        }

        public bool? ApplyDefaults;
        public bool? IncludeDependencies;
        public bool? UseSymbols;
        public bool? UseThreads;
        public bool? UseDefaultProxies;
        public bool? GenerateSkeletonsForStubbedAssemblies;
        public bool? SkipManifestCreation;
        public bool? GenerateContentManifest;
        public bool? RunBugChecks;
        public bool? TuneGarbageCollection;
        public string FilenameEscapeRegex;
        public Dictionary<string, string> FilenameReplaceRegexes = new Dictionary<string, string>();
        public string AssemblyCollectionName;
        public List<string> EmitterFactories = new List<string>();
        public bool? BuildSourceMap;
        public bool? InlineAssemblyReferences;

        public double? FrameworkVersion;

        public readonly AssemblyConfiguration Assemblies = new AssemblyConfiguration();
        public readonly CodeGeneratorConfiguration CodeGenerator = new CodeGeneratorConfiguration();

        public virtual Configuration Clone () {
            var result = new Configuration();
            MergeInto(result);
            return result;
        }

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
            if (SkipManifestCreation.HasValue)
                result.SkipManifestCreation = SkipManifestCreation;
            if (RunBugChecks.HasValue)
                result.RunBugChecks = RunBugChecks;
            if (TuneGarbageCollection.HasValue)
                result.TuneGarbageCollection = TuneGarbageCollection;

            if (FilenameEscapeRegex != null)
                result.FilenameEscapeRegex = FilenameEscapeRegex;
            if (AssemblyCollectionName != null)
                result.AssemblyCollectionName = AssemblyCollectionName;

            if (BuildSourceMap != null)
                result.BuildSourceMap = BuildSourceMap;

            if (InlineAssemblyReferences != null)
                result.InlineAssemblyReferences = InlineAssemblyReferences;

            foreach (var kvp in FilenameReplaceRegexes)
                result.FilenameReplaceRegexes[kvp.Key] = kvp.Value;

            foreach (var emitterFactory in EmitterFactories) {
                result.EmitterFactories.Add(emitterFactory);
            }

            Assemblies.MergeInto(result.Assemblies);
            CodeGenerator.MergeInto(result.CodeGenerator);
        }
    }
}
