using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Compiler.Extensibility;
using JSIL.Utilities;

namespace JSIL.Compiler.Profiles {
    public class Default : BaseJavaScriptProfile
    {
        public override bool IsAppropriateForSolution (SolutionBuilder.BuildResult buildResult) {
            // Normally we'd return true so that this profile is always selected, but this is our fallback profile.
            return false;
        }
    }
}
