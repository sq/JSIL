using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSIL.Compiler.Extensibility;

namespace JSIL.Compiler {
    public class BuildGroup {
        public IProfile Profile;
        public Configuration BaseConfiguration;
        public string[] FilesToBuild; 
    }
}
