using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace JSIL.Tests {
    public static class CSharpUtil {
        public static Assembly Compile (string sourceCode) {
            using (var csc = new CSharpCodeProvider(new Dictionary<string, string>() { 
                { "CompilerVersion", "v4.0" } 
            })) {
                var parameters = new CompilerParameters(new[] {
                    "mscorlib.dll", "System.Core.dll",
                    typeof(JSIL.Meta.JSIgnore).Assembly.Location
                }) {
                    GenerateExecutable = true,
                    GenerateInMemory = false,
                    IncludeDebugInformation = true
                };

                var results = csc.CompileAssemblyFromSource(parameters, sourceCode);

                if (results.Errors.Count > 0) {
                    throw new Exception(
                        String.Join(Environment.NewLine, results.Errors.Cast<CompilerError>().Select((ce) => ce.ErrorText).ToArray())
                    );
                }

                return results.CompiledAssembly;
            }
        }
    }
}
