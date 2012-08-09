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
            using (var s = new Session("Mannux", false)) {
                s.LoadPage("Mannux/Mannux.html");

                s.PassOrFail(
                    () => 
                        s.WaitFor(
                            "return window.$jsilbrowserstate.hasMainRun",
                            timeoutMs: 45000
                        ),
                    "Waiting for game to start", "started."
                );

                s.PassOrFail(
                    () => {
                        // Turn right
                        s.Evaluate("test.pressKeysFor([39], 2000);");

                        // Fire the gun until the monster dies
                        s.WaitFor(
                            "test.pressKeysFor([162, 163], 250); " +
                            "return test.logText.indexOf('HP: 0') >= 0",
                            timeoutMs: 10000,
                            tickRateMs: 200
                        );
                    },
                    "Killing the monster"
                );

                Console.WriteLine("// Game log follows:");
                Console.WriteLine(s.GetLogText());

                var exceptions = s.GetExceptions();
                var unexpectedExceptions = exceptions.ToList();

                // Ignore errors about unimplemented WinForms methods.
                unexpectedExceptions.RemoveAll((exc) => 
                    exc.Text.Contains("The external method") && exc.Text.Contains("of type 'System.Windows.Forms")
                );
                // Ignore set_SynchronizeWithVerticalRetrace.
                unexpectedExceptions.RemoveAll((exc) =>
                    exc.Text.Contains("set_SynchronizeWithVerticalRetrace")
                );

                Console.WriteLine("// Game threw {0} exception(s), {1} of which were unexpected:", exceptions.Length, unexpectedExceptions.Count);

                foreach (var exc in unexpectedExceptions)
                    Console.WriteLine(exc);

                Assert.AreEqual(unexpectedExceptions.Count, 0);
            }
        }
    }
}
