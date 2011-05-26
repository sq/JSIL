using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace JSIL.Compiler {
    class Program {
        static void ParseOption (AssemblyTranslator translator, string option) {
            var m = Regex.Match(option, "-(-?)(?'key'[a-zA-Z]*)([=:](?'value'.*))?", RegexOptions.ExplicitCapture);
            if (m.Success) {
                switch (m.Groups["key"].Value) {
                    case "out":
                    case "o":
                        translator.OutputDirectory = Path.GetFullPath(m.Groups["value"].Value);
                        break;
                    case "nodeps":
                    case "nd":
                        translator.IncludeDependencies = false;
                        break;
                    case "ignore":
                    case "i":
                        translator.IgnoredAssemblies.Add(new Regex(m.Groups["value"].Value, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture));
                        break;
                    case "stub":
                    case "s":
                        translator.StubbedAssemblies.Add(new Regex(m.Groups["value"].Value, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture));
                        break;
                    case "proxy":
                    case "p":
                        translator.AddProxyAssembly(Path.GetFullPath(m.Groups["value"].Value), false);
                        break;
                }
            }
        }

        static void Main (string[] arguments) {
            var translator = new AssemblyTranslator();
            translator.StartedLoadingAssembly += (fn) => {
                Console.Error.WriteLine("Loading {0}...", fn);
            };
            translator.StartedDecompilingAssembly += (fn) => {
                Console.Error.WriteLine("Decompiling {0}...", fn);
            };
            translator.StartedTranslatingAssembly += (fn) => {
                Console.Error.WriteLine("Translating {0}...", fn);
            };
            translator.CouldNotLoadSymbols += (fn, ex) => {
                Console.Error.WriteLine("Could not load symbols for module {0}: {1}", fn, ex.Message);
            };
            translator.CouldNotResolveAssembly += (fn, ex) => {
                Console.Error.WriteLine("Could not load module {0}: {1}", fn, ex.Message);
            };
            translator.CouldNotDecompileMethod += (fn, ex) => {
                Console.Error.WriteLine("Could not decompile method {0}: {1}", fn, ex.Message);
            };
            translator.StartedDecompilingMethod += (fn) => {
                Console.Error.Write("Decompiling {0}... ", fn);
            };
            translator.FinishedDecompilingMethod += (fn) => {
                Console.Error.WriteLine("done.");
            };

            var filenames = new HashSet<string>(arguments);

            foreach (var filename in arguments) {
                if (filename.StartsWith("-")) {
                    filenames.Remove(filename);
                    ParseOption(translator, filename);
                }
            }

            if (filenames.Count == 0) {
                var asmName = Assembly.GetExecutingAssembly().GetName();
                Console.WriteLine("==== JSILc v{0}.{1}.{2} ====", asmName.Version.Major, asmName.Version.Minor, asmName.Version.Revision);
                Console.WriteLine("Usage: JSILc [options] assembly [assembly]");
                Console.WriteLine("Options:");
                Console.WriteLine("--out:<folder>");
                Console.WriteLine("  Specifies the directory into which the generated javascript should be written.");
                Console.WriteLine("--proxy:<assembly>");
                Console.WriteLine("  Specifies the location of a proxy assembly that contains type information for other assemblies.");
                Console.WriteLine("--nodeps");
                Console.WriteLine("  Disables translating dependencies.");
                Console.WriteLine("--ignore:<regex>");
                Console.WriteLine("  Specifies a regular expression filter used to ignore certain dependencies.");
                Console.WriteLine("--stub:<regex>");
                Console.WriteLine("  Specifies a regular expression filter used to specify that certain dependencies should only be generated as stubs.");
                return;
            }

            foreach (var filename in filenames) {
                translator.Translate(filename);
            }
        }
    }
}
