using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using JSIL.Compiler.Extensibility;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;

namespace JSIL.Compiler.Profiles {
    public class XNA4 : BaseProfile {
        public HashSet<string> ContentProjectsProcessed = new HashSet<string>();

        public override bool IsAppropriateForSolution (SolutionBuilder.SolutionBuildResult buildResult) {
            return buildResult.TargetFilesUsed.Any(
                (targetFile) => targetFile.Contains(@"XNA Game Studio\v4.0")
            );
        }

        public override Configuration GetConfiguration (Configuration defaultConfiguration) {
            var result = defaultConfiguration.Clone();

            result.FrameworkVersion = 4.0;
            result.Assemblies.Proxies.Add("JSIL.Proxies.XNA4.dll");

            Common.InitConfiguration(result);

            return result;
        }

        public override TranslationResult Translate (AssemblyTranslator translator, string assemblyPath, bool scanForProxies) {
            var result = translator.Translate(assemblyPath, scanForProxies);

            result.Files["XNA.Colors.js"] = new ArraySegment<byte>(Encoding.UTF8.GetBytes(
                Common.MakeXNAColors()
            ));

            AssemblyTranslator.GenerateManifest(translator.Manifest, assemblyPath, result);

            return result;
        }

        public override SolutionBuilder.SolutionBuildResult ProcessBuildResult (Configuration configuration, SolutionBuilder.SolutionBuildResult buildResult) {
            Common.ProcessContentProjects(configuration, buildResult, ContentProjectsProcessed);

            return base.ProcessBuildResult(configuration, buildResult);
        }
    }
}
