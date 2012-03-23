using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Compiler.Extensibility;

namespace JSIL.Compiler.Profiles {
    public class XNA3 : BaseProfile {
        public override bool IsAppropriateForSolution (SolutionBuilder.SolutionBuildResult buildResult) {
            return buildResult.TargetFilesUsed.Any(
                (targetFile) => targetFile.Contains(@"XNA Game Studio\v3.0") || targetFile.Contains(@"XNA Game Studio\v3.1")
            );
        }

        public override Configuration GetConfiguration (Configuration defaultConfiguration) {
            var result = defaultConfiguration.Clone();

            result.FrameworkVersion = 3.5;
            result.Assemblies.Proxies.Add("JSIL.Proxies.XNA3.dll");

            return result;
        }
    }
}
