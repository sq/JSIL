using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using JSIL.Ast;
using JSIL.Ast.Enumerators;
using JSIL.Internal;
using JSIL.Translator;
using Mono.Cecil;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class ConfigurationTests {
        public ConfigurationTests () {
        }

        public Configuration LoadDefaultConfiguration () {
            var testAssembly = typeof(ComparisonTest).Assembly;
            var assemblyPath = Path.GetDirectoryName(Util.GetPathOfAssembly(testAssembly));

            var configFolder = Path.GetFullPath(Path.Combine(assemblyPath, "..", "Compiler"));
            if (configFolder[configFolder.Length - 1] != Path.DirectorySeparatorChar)
                configFolder += Path.DirectorySeparatorChar;

            var filename = Path.Combine(configFolder, "defaults.jsilconfig");

            var jss = new JavaScriptSerializer();
            var json = File.ReadAllText(filename);
            var result = jss.Deserialize<Configuration>(json);
            return result;        
        }

        public AssemblyTranslator MakeTranslator (Configuration configuration) {
            return new AssemblyTranslator(configuration);
        }

        public AssemblyDefinition MakeFakeAssembly (string name, Version version) {
            return AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(name, version), name, ModuleKind.Dll);
        }

        [Test]
        public void MicrosoftAssembliesAreStubbed () {
            var translator = MakeTranslator(LoadDefaultConfiguration());

            Assert.AreEqual("stubbed", translator.ClassifyAssembly(MakeFakeAssembly("mscorlib", new Version(4, 0, 0, 0))));
            Assert.AreEqual("stubbed", translator.ClassifyAssembly(MakeFakeAssembly("System", new Version(4, 0, 0, 0))));
            Assert.AreEqual("stubbed", translator.ClassifyAssembly(MakeFakeAssembly("System.Core", new Version(4, 0, 0, 0))));
        }

        [Test]
        public void AssemblyNamedSolarSystemIsNotStubbed () {
            var translator = MakeTranslator(LoadDefaultConfiguration());

            Assert.AreEqual("translate", translator.ClassifyAssembly(MakeFakeAssembly("SolarSystem", new Version(1, 0, 0, 0))));
        }
    }
}
