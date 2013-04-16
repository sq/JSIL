using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using JSIL.Ast;
using JSIL.Ast.Enumerators;
using JSIL.Internal;
using Mono.Cecil;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class TypeInformationTests {
        public readonly TypeInfoProvider TypeInfo = new TypeInfoProvider();
        public readonly DefaultAssemblyResolver Resolver = new DefaultAssemblyResolver();

        public AssemblyDefinition Corlib;

        [TestFixtureSetUp]
        public void SetUp () {
            Corlib = Resolver.Resolve("mscorlib");
        }

        [TestFixtureTearDown]
        public void TearDown () {
            TypeInfo.Dispose();
            Corlib = null;
        }

        [Test]
        public void MethodInfo_Overrides_ContainsOverridesInformation () {
            var tObject = Corlib.MainModule.TypeSystem.Object;
            var trList = new TypeReference("System.Collections.Generic", "List`1", tObject.Module, tObject.Scope, false);
            var tiList = TypeInfo.GetTypeInformation(trList);

            var getEnumeratorMethods = (from member in tiList.Members.Values where member.Name.Contains("GetEnumerator") select member).ToArray();

            Assert.AreEqual(1, getEnumeratorMethods.Count(
                (m) => !m.Overrides.Any()
            ), "Expected one GetEnumerator method to have no overrides");

            Assert.AreEqual(1, getEnumeratorMethods.Count(
                (m) => m.Overrides.Any(
                        (o) => o.InterfaceType.FullName == "System.Collections.IEnumerable"
                    )
            ), "Expected one GetEnumerator method to override IEnumerable");

            Assert.AreEqual(1, getEnumeratorMethods.Count(
                (m) => m.Overrides.Any(
                        (o) => o.InterfaceType.FullName == "System.Collections.Generic.IEnumerable`1<T>"
                    )
            ), "Expected one GetEnumerator method to override IEnumerable`1");
        }
    }
}
