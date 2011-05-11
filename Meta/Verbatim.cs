using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JSIL {
    public static class Builtins {
        public static readonly JSGlobal Global = new JSGlobal();
        public static readonly JSLocal Local = new JSLocal();

        /// <summary>
        /// When running as C#, this method does nothing and returns null.
        /// When running as JavaScript, this method call is replaced with an invocation of the builtin javascript eval function.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        public static object Eval (string expression) {
            return null;
        }

        /// <summary>
        /// When running as javascript, this property evaluates to the current scope's this-reference.
        /// </summary>
        public static object This {
            get {
                return null;
            }
        }
    }

    public static class Verbatim {
        /// <summary>
        /// When running as C#, this method does nothing and returns null.
        /// When running as JavaScript, the passed-in script code replaces this method call.
        /// </summary>
        /// <param name="javascript">The script expression.</param>
        public static object Expression (string javascript) {
            return null;
        }
    }

    public class JSGlobal {
        /// <summary>
        /// Retrieves a name from the global namespace (note that this is the global namespace at the time that the JSIL runtime was loaded).
        /// </summary>
        /// <param name="name">The name to retrieve. This may be a literal, or a string-producing expression.</param>
        public object this[string name] {
            get {
                return null;
            }
        }
    }

    public class JSLocal {
        /// <summary>
        /// Retrieves a name from the local namespace.
        /// </summary>
        /// <param name="name">The name to retrieve. This must be a string literal!</param>
        public object this[string name] {
            get {
                return null;
            }
        }
    }
}
