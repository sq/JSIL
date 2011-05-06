using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JSIL {
    public static class Builtins {
        public static readonly JSGlobal Global = new JSGlobal();

        /// <summary>
        /// When running as C#, this method does nothing and returns null.
        /// When running as JavaScript, this method call is replaced with an invocation of the builtin javascript eval function.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        public static dynamic Eval (string expression) {
            return null;
        }
    }

    public static class Verbatim {
        /// <summary>
        /// When running as C#, this method does nothing and returns null.
        /// When running as JavaScript, the passed-in script code replaces this method call.
        /// </summary>
        /// <param name="javascript">The script expression.</param>
        public static dynamic Expression (string javascript) {
            return null;
        }
    }

    public class JSGlobal {
        public dynamic this[string name] {
            get {
                return null;
            }
        }
    }
}
