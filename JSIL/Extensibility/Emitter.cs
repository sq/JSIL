using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
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
        void EmitHeader (bool stubbed);
        void EmitFooter ();
        void EmitAssemblyEntryPoint (
            AssemblyDefinition assembly, MethodDefinition entryMethod, MethodSignature signature
        );
        IAstEmitter MakeAstEmitter (
            JSILIdentifier jsil, TypeSystem typeSystem, 
            TypeInfoProvider typeInfoProvider, Configuration configuration
        );
        void EmitTypeAlias (TypeDefinition typedef);
        // Returns false if the caller should abort
        bool EmitTypeDeclarationHeader (
            DecompilerContext context, IAstEmitter astEmitter, TypeDefinition typedef, TypeInfo typeInfo
        );
        void EmitCustomAttributes (
            DecompilerContext context, 
            TypeReference declaringType,
            ICustomAttributeProvider member, 
            IAstEmitter astEmitter, 
            bool standalone = true
        );
        void EmitMethodDefinition (DecompilerContext context, MethodReference methodRef, MethodDefinition method, IAstEmitter astEmitter, bool stubbed, JSRawOutputIdentifier dollar, MethodInfo methodInfo = null);
        void EmitSpacer ();
        void EmitSemicolon ();
        void EmitProxyComment (string fullName);
        void EmitEvent (DecompilerContext context, IAstEmitter astEmitter, EventDefinition @event, JSRawOutputIdentifier dollar);
        void EmitProperty (DecompilerContext context, IAstEmitter astEmitter, PropertyDefinition property, JSRawOutputIdentifier dollar);
        void EmitPrimitiveDefinition (DecompilerContext context, TypeDefinition typedef, bool stubbed, JSRawOutputIdentifier dollar);
        void BeginEmitTypeDeclaration (TypeDefinition typedef);
        void BeginEmitTypeDefinition (IAstEmitter astEmitter, TypeDefinition typedef, TypeInfo typeInfo, TypeReference baseClass);
        void EndEmitTypeDefinition (IAstEmitter astEmitter, DecompilerContext context, TypeDefinition typedef);
        void EmitInterfaceList (TypeInfo typeInfo, IAstEmitter astEmitter, JSRawOutputIdentifier dollar);
    }

    public interface IAstEmitter {
        TypeSystem TypeSystem { get; }
        TypeReferenceContext ReferenceContext { get; }

        // FIXME
        SignatureCacher SignatureCacher { get; set; }

        void Emit (JSNode node);
    }
}
