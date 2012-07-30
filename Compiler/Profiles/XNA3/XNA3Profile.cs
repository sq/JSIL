using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Compiler.Extensibility;
using JSIL.Utilities;

namespace JSIL.Compiler.Profiles {
    public class XNA3 : BaseProfile {
        public HashSet<string> ContentProjectsProcessed = new HashSet<string>();

        public override bool IsAppropriateForSolution (SolutionBuilder.SolutionBuildResult buildResult) {
            return buildResult.TargetFilesUsed.Any(
                (targetFile) => targetFile.Contains(@"XNA Game Studio\v3.0") || targetFile.Contains(@"XNA Game Studio\v3.1")
            );
        }

        public override TranslationResult Translate (AssemblyTranslator translator, Configuration configuration, string assemblyPath, bool scanForProxies) {
            var result = translator.Translate(assemblyPath, scanForProxies);

            ResourceConverter.ConvertEmbeddedResources(configuration, assemblyPath, result);

            result.AddFile("Script", "XNA.Colors.js", new ArraySegment<byte>(Encoding.UTF8.GetBytes(
                Common.MakeXNAColors()
            )), 0);

            AssemblyTranslator.GenerateManifest(translator.Manifest, assemblyPath, result);

            return result;
        }

        public override Configuration GetConfiguration (Configuration defaultConfiguration) {
            var result = defaultConfiguration.Clone();

            result.FrameworkVersion = 3.5;
            result.Assemblies.Proxies.Add("JSIL.Proxies.XNA3.dll");

            Common.InitConfiguration(result);

            return result;
        }

        public override SolutionBuilder.SolutionBuildResult ProcessBuildResult (Configuration configuration, SolutionBuilder.SolutionBuildResult buildResult) {
            Common.ProcessContentProjects(configuration, buildResult, ContentProjectsProcessed);

            return base.ProcessBuildResult(configuration, buildResult);
        }
    }
}
