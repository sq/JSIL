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
        public void TetrisRunsReplayWithoutErrors () {
            using (var s = new Session("Tetris", true)) {
                s.LoadPage(
                    "Tetris/Tetris.html", 
                    "replayURI=test.replay&fastReplay&testFixture&profile&forceCanvas&autoPlay&viewportScale=0.25&disableFiltering"
                );

                s.PassOrFail(
                    () => 
                        s.WaitFor(
                            "return window.$jsilbrowserstate.hasMainRun",
                            timeoutMs: 5 * 60 * 1000
                        ),
                    "Waiting for game to start", "started."
                );

                s.PassOrFail(
                    () =>
                        s.WaitFor(
                            "return JSIL.Host.getService('replayPlayer').playbackEnded >= 0",
                            timeoutMs: 3 * 60 * 1000
                        ),
                    "Waiting for replay to finish", "finished."
                );

                int score = Convert.ToInt32(s.Evaluate("return window.test.game.score"));
                int totalLines = Convert.ToInt32(s.Evaluate("return window.test.game.totalLines"));

                Console.WriteLine("// Game log follows:");
                Console.WriteLine(s.GetLogText());

                var exceptions = s.GetExceptions();
                var unexpectedExceptions = exceptions.ToList();

                Console.WriteLine("// Game threw {0} exception(s), {1} of which were unexpected:", exceptions.Length, unexpectedExceptions.Count);

                foreach (var exc in unexpectedExceptions)
                    Console.WriteLine(exc);

                Assert.AreEqual(0, unexpectedExceptions.Count, "Unexpected exceptions were thrown");

                Assert.AreEqual(600, score, "Score did not match expectation");
                Assert.AreEqual(8, totalLines, "Total lines did not match expectation");
            }
        }
    }
}
