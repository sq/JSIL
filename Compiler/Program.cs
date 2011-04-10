using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Compiler {
    class Program {
        static void Main (string[] args) {
            var translator = new AssemblyTranslator();
            foreach (var filename in args)
                translator.Translate(filename);
        }
    }
}
