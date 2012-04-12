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

        protected readonly TypeReference T1, T1_2, T2, T3;

        public APITests () {
            TestModule = ModuleDefinition.CreateModule("TestModule", ModuleKind.NetModule);
            MetadataScope = TestModule;

            T1 = new TypeReference("namespace", "Type1", TestModule, MetadataScope);
            T1_2 = new TypeReference("namespace", "Type1", TestModule, MetadataScope);
            T2 = new TypeReference("namespace", "Type2", TestModule, MetadataScope);
            T3 = new TypeReference("namespace2", "Type1", TestModule, MetadataScope);
        }

        [Test]
        public void BasicTypeEquality () {
            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(T1, T1));
            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(T1, T1_2));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(T1, T2));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(T2, T3));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(T1, T3));
        }

        [Test]
        public void ArrayTypeEquality () {
            var at1 = new ArrayType(T1);
            var at2 = new ArrayType(T2);
            var at3 = new ArrayType(T3);
            var at4 = new ArrayType(T1, 2);

            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(at1, T1));

            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(at1, T1));
            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(at1, at1));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(at1, at2));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(at1, at3));

            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(at1, at4, true));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(at1, at4, false));
        }

        [Test]
        public void RefTypeEquality () {
            var rt1 = new ByReferenceType(T1);
            var rt1_2 = new ByReferenceType(T1_2);
            var rt2 = new ByReferenceType(T2);
            var rt3 = new ByReferenceType(T3);

            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(rt1, T1));

            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(rt1, rt1));
            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(rt1, rt1_2));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(rt1, rt2));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(rt1, rt3));
        }

        [Test]
        public void GenericInstanceEquality () {
            var git1 = new GenericInstanceType(T1);
            var git1_2 = new GenericInstanceType(T1_2);
            var git2 = new GenericInstanceType(T2);

            // FIXME: Is this correct?
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(git1, T1, true));
            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(git1, T1, false));

            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(git1, git1));
            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(git1, git1_2));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(git1, git2));
        }

        [Test]
        public void GenericInstanceParameterEquality () {
            var git1 = new GenericInstanceType(T1);
            git1.GenericArguments.Add(T1);
            var git2 = new GenericInstanceType(T1);
            git2.GenericArguments.Add(T1_2);
            var git3 = new GenericInstanceType(T1);
            git3.GenericArguments.Add(T2);
            var git4 = new GenericInstanceType(T2);
            git4.GenericArguments.Add(T1);

            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(git1, git1));
            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(git1, git2));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(git1, git3, true));
            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(git1, git3, false));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(git3, git4));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(git1, git4));
        }

        [Test]
        public void GenericParameterEquality () {
            var gp1 = new GenericParameter("GP1", T1);
            var gp1_2 = new GenericParameter("GP1", T1_2);
            var gp2 = new GenericParameter("GP2", T1);
            var gp3 = new GenericParameter("GP1", T2);
            var gp4 = new GenericParameter("GP1", T3);

            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(gp1, T1));

            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(gp1, gp1));
            Assert.IsTrue(ILBlockTranslator.TypesAreEqual(gp1, gp1_2));

            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(gp1, gp2, true));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(gp1, gp2, false));

            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(gp1, gp3));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(gp1, gp4));

            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(gp2, gp3));
            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(gp2, gp4));

            Assert.IsFalse(ILBlockTranslator.TypesAreEqual(gp3, gp4));
        }
    }
}
