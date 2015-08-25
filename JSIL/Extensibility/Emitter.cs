using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JSIL.Ast;
using JSIL.Internal;
using JSIL.Transforms;
using JSIL.Translator;
using Mono.Cecil;

namespace JSIL.Compiler.Extensibility {
    public interface IEmitterFactory {
        string FileExtension { get; }

        IAssemblyEmitter MakeAssemblyEmitter (
            AssemblyTranslator assemblyTranslator,
            JavascriptFormatter formatter
        );
    }

    public interface IAssemblyEmitter {
        void EmitHeader (
            bool stubbed, bool skeletons
        );
        void EmitAssemblyEntryPoint (
            AssemblyDefinition assembly, MethodDefinition entryMethod, MethodSignature signature
        );
        IAstEmitter MakeAstEmitter (
            JSILIdentifier jsil, TypeSystem typeSystem, 
            TypeInfoProvider typeInfoProvider, Configuration configuration
        );
        void DeclareTypeAlias (TypeDefinition typedef);
    }

    public interface IAstEmitter {
        TypeSystem TypeSystem { get; }
        TypeReferenceContext ReferenceContext { get; }

        // FIXME
        SignatureCacher SignatureCacher { get; set; }

        void Emit (JSNode node);
    }
}
