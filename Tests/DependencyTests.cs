using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class DependencyTests {
        [Test]
        public void EnumeratesAssemblyDependencies () {
            TempFileCollection temporaryFiles;

            var assembly = CompilerUtil.CompileCS(new[] { @"
using System;
using System.Text.RegularExpressions;

public static class Program {
    public static void Main () {
        var regex = new Regex(""[A-Za-z]*"");
        var text = ""Hello, World!"";
        var match = regex.Match(text);
        Console.WriteLine(""{0} {1}"", match.Success, match.Groups[0].Value);
    }
}" }, out temporaryFiles);

            var translator = new AssemblyTranslator(
                new Translator.Configuration {
                    IncludeDependencies = false
                }
            );

            var assemblyDefinition = translator.LoadAssembly(assembly.Location);

            translator = new AssemblyTranslator(
                new Translator.Configuration {
                    IncludeDependencies = true
                }
            );

            var assemblyPlusDependencies = translator.LoadAssembly(assembly.Location);

            Assert.AreNotEqual(
                (from ad in assemblyDefinition select ad.FullName).ToArray(), 
                (from ad in assemblyPlusDependencies select ad.FullName).ToArray()
            );
        }
    }
}
