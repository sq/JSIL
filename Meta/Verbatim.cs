using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JSIL.Meta;

namespace JSIL {
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
}
