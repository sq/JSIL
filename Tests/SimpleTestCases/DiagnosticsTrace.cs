#define TRACE

using System;
using System.Diagnostics;

public class Program {
    public static void Main () {
        Trace.WriteLine("Traced");
        Trace.TraceError("Error");
        Trace.TraceInformation("Information");
        Trace.TraceWarning("Warning");
    }
}
