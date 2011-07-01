using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL {
    public delegate void ProgressEventHandler (object sender, int current, int maximum);

    public class ProgressReporter {
        public event ProgressEventHandler ProgressChanged;
        public event EventHandler Finished;

        internal void OnProgressChanged (int current, int maximum) {
            if (ProgressChanged != null)
                ProgressChanged(this, current, maximum);
        }

        internal void OnFinished () {
            if (Finished != null)
                Finished(this, EventArgs.Empty);
        }
    }
}
