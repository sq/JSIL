using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Compiler.Extensibility.DeadCodeAnalyzer {
    public class Configuration {
        public bool? DeadCodeElimination;
        public IList<string> WhiteList;
    
        public Configuration(Dictionary<string, object> configuration) {
            DeadCodeElimination = configuration.ContainsKey("DeadCodeElimination") &&
                                  configuration["DeadCodeElimination"] is bool &&
                                  ((bool) configuration["DeadCodeElimination"]);
    
            if (configuration.ContainsKey("WhiteList") &&
                configuration["WhiteList"] is IList) {
                WhiteList = ((IList) configuration["WhiteList"]).Cast<string>().ToList();
            }
        }
    }
}
