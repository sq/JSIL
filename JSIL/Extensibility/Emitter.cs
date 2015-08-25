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
        IAssemblyEmitter MakeAssemblyEmitter ();
        IAstEmitter      MakeAstEmitter (
            JavascriptFormatter output, JSILIdentifier jsil, 
            TypeSystem typeSystem, ITypeInfoSource typeInfo,
            Configuration configuration
        );
    }

    public interface IAssemblyEmitter {
    }

    public interface IAstEmitter {
        TypeSystem TypeSystem { get; }
        TypeReferenceContext ReferenceContext { get; }

        // FIXME
        SignatureCacher SignatureCacher { get; set; }

        void Emit (JSNode node);
    }
}
