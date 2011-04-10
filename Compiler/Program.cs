using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Compiler {
    class Program {
        static void Main (string[] args) {
            var translator = new AssemblyTranslator();

            foreach (var filename in args) {
                Console.Error.WriteLine("// {0}", filename);
                Console.WriteLine(translator.Translate(filename));
            }

            Console.ReadLine();
        }
    }
}
