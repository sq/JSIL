using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class DependencyTests {
        [Test]
        public void EnumeratesAssemblyDependencies () {
            var cr = CompilerUtil.Compile(new[] { 
                Path.Combine(
                    ComparisonTest.TestSourceFolder,
                    @"SpecialTestCases",
                    "EnumeratesAssemblyDependencies.cs"
                )
            }, Path.Combine("DependencyTests", "EnumeratesAssemblyDependencies"));
            var assembly = cr.Assembly;

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
