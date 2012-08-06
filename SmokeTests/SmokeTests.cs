using System;   
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace SmokeTests {
    [TestFixture]
    public class SmokeTests {
        [Test]
        [Ignore]
        public void MannuxRuns () {
            using (var s = new Session("Mannux")) {
                s.LoadPage("Mannux/Mannux.html");

                Console.Write("Waiting for game to start... ");
                try {
                    s.WaitFor("window.$jsilbrowserstate.hasMainRun", null, 45000);
                    Console.WriteLine("started.");
                } catch (Exception exc) {
                    Console.WriteLine("failed.");
                    throw;
                }

                Console.WriteLine(s.GetLogText());

                var exceptions = s.GetExceptions();
                Console.WriteLine("Game threw {0} exception(s):", exceptions.Length);

                foreach (var exc in exceptions) {
                    Console.WriteLine(exc);
                }
            }
        }
    }
}
