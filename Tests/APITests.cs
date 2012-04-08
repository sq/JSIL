using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Mono.Cecil;
using NUnit.Framework;

namespace JSIL.Tests {
    [TestFixture]
    public class APITests {
        public readonly ModuleDefinition TestModule;
        public readonly IMetadataScope MetadataScope;

        public APITests () {
            TestModule = ModuleDefinition.CreateModule("TestModule", ModuleKind.NetModule);
            MetadataScope = TestModule;
        }

        [Test]
        public void BasicTypeEquality () {
            var t1 = new TypeReference("namespace", "Type1", TestModule, MetadataScope);
            var t1_2 = new TypeReference("namespace", "Type1", TestModule, MetadataScope);
            var t2 = new TypeReference("namespace", "Type2", TestModule, MetadataScope);
            var t3 = new TypeReference("namespace2", "Type1", TestModule, MetadataScope);

            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(t1, t1));
            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(t1, t1_2));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(t1, t2));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(t2, t3));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(t1, t3));
        }

        [Test]
        public void ArrayTypeEquality () {
            var t1 = new TypeReference("namespace", "Type1", TestModule, MetadataScope);
            var t2 = new TypeReference("namespace", "Type2", TestModule, MetadataScope);
            var t3 = new TypeReference("namespace2", "Type1", TestModule, MetadataScope);

            var at1 = new ArrayType(t1);
            var at2 = new ArrayType(t2);
            var at3 = new ArrayType(t3);

            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(at1, t1));
            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(at1, at1));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(at1, at2));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(at1, at3));
        }
    }
}
