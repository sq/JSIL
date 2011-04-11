using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JSIL {
    public static class Verbatim {
        /// <summary>
        /// When running as C#, this method does nothing. When running as JavaScript, the passed-in script code is run.
        /// </summary>
        /// <param name="javascript">The script code to run.</param>
        public static void Eval (string javascript) {
        }
    }
}
