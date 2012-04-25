using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xap {
    public class InvalidContentException : Exception {
        public InvalidContentException (string message)
            : base(message) {
        }
    }
}
