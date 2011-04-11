using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JSIL {
    public static class Verbatim {
        /// <summary>
        /// When running as C#, this method does nothing and returns null.
        /// When running as JavaScript, the passed-in script code is evaluated as an expression.
        /// </summary>
        /// <param name="javascript">The script expression to evaluate.</param>
        public static dynamic Eval (string javascript) {
            return null;
        }
    }
}
