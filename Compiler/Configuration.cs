using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Compiler {
    [Serializable]
    public class Configuration : Translator.Configuration {
        [Serializable]
        public class SolutionBuildConfiguration {
            public string Configuration = null;
            public string Platform = null;
        }

        public readonly SolutionBuildConfiguration SolutionBuilder = new SolutionBuildConfiguration();

        public string OutputDirectory = ".";
    }
}
