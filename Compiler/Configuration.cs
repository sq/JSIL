using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Compiler {
    [Serializable]
    public class Configuration : Translator.Configuration {
        [Serializable]
        public sealed class SolutionBuildConfiguration {
            public bool? AutoLoadConfigFiles;

            public string Configuration;
            public string Platform;

            public void MergeInto (SolutionBuildConfiguration result) {
                if (AutoLoadConfigFiles.HasValue)
                    result.AutoLoadConfigFiles = AutoLoadConfigFiles;

                if (Configuration != null)
                    result.Configuration = Configuration;

                if (Platform != null)
                    result.Platform = Platform;
            }
        }

        public readonly SolutionBuildConfiguration SolutionBuilder = new SolutionBuildConfiguration();

        public bool? UseLocalProxies;
        public string OutputDirectory;

        public void MergeInto (Configuration result) {
            base.MergeInto(result);

            if (UseLocalProxies.HasValue)
                result.UseLocalProxies = UseLocalProxies;
            if (OutputDirectory != null)
                result.OutputDirectory = OutputDirectory;

            SolutionBuilder.MergeInto(result.SolutionBuilder);
        }

        public Configuration Clone () {
            var result = new Configuration();
            MergeInto(result);
            return result;
        }
    }
}
