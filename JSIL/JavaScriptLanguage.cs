using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cecil.Decompiler;
using Cecil.Decompiler.Languages;
using Cecil.Decompiler.Steps;
using JSIL.Internal;

namespace JSIL.Languages {
    // We have to derive from CSharp because a ton of Cecil.Decompiler's types are internal. >:|
    public class JavaScript : CSharp {
        public override DecompilationPipeline CreatePipeline () {
            var result = base.CreatePipeline();

            return result;
        }

        new public ILanguageWriter GetWriter (IFormatter formatter) {
            return new JavaScriptWriter(this, formatter);
        }

        public override string Name {
            get {
                return "JavaScript";
            }
        }
    }
}
