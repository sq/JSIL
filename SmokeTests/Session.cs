using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using JSIL.Internal;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace SmokeTests {
    public class Session : IDisposable {
        public readonly string TempPath, LogPath;
        public readonly Process Proxy;
        public readonly DesiredCapabilities DriverCapabilities;
        public readonly RemoteWebDriver WebDriver;
        public readonly bool RunningAgainstLocalServer;

        // Seconds
        public const int CommandTimeout = 30;
        // Seconds
        public const int MaximumTestDuration = 60 * 10;
        // Seconds
        public const int IdleTestTimeout = 30;

        public static readonly string SeleniumVersion = "2.28.0";
        public static readonly string LocalHost = "http://127.0.0.1:8080";
        public static readonly string RemoteHost = "http://hildr.luminance.org";
        public static readonly string DefaultPageOptions = "testFixture&profile&forceCanvas&autoPlay";

        private bool IsDisposed = false;

        public Session (string testName, bool runAgainstLocalServer = false) {
            try {
                RunningAgainstLocalServer = runAgainstLocalServer;

                TempPath = Path.GetTempPath();
                LogPath = Path.Combine(TempPath, "sauce_connect.log");

                DriverCapabilities = DesiredCapabilities.Chrome();
                DriverCapabilities.SetCapability(
                    CapabilityType.Platform, new Platform(PlatformType.XP)
                );
                DriverCapabilities.SetCapability(
                    "name", testName
                );
                DriverCapabilities.SetCapability(
                    "username", SauceLabs.Credentials.username
                );
                DriverCapabilities.SetCapability(
                    "accessKey", SauceLabs.Credentials.accessKey
                );
                DriverCapabilities.SetCapability(
                    "max-duration", MaximumTestDuration
                );
                DriverCapabilities.SetCapability(
                    "idle-timeout", IdleTestTimeout
                );
                DriverCapabilities.SetCapability(
                    "avoid-proxy", true
                );
                DriverCapabilities.SetCapability(
                    "sauce-advisor", false
                );
                DriverCapabilities.SetCapability(
                    "record-video", true
                );
                DriverCapabilities.SetCapability(
                    "video-upload-on-pass", false
                );
                DriverCapabilities.SetCapability(
                    "record-screenshots", false
                );

                /*
                DriverCapabilities.SetCapability(
                    "selenium-version", SeleniumVersion
                );
                 */

                if (runAgainstLocalServer)
                    Proxy = StartProxy();
                else
                    Proxy = null;

                WebDriver = PassOrFail(
                    () => 
                        new RemoteWebDriver(
                            new Uri("http://ondemand.saucelabs.com:80/wd/hub"),
                            DriverCapabilities,
                            TimeSpan.FromSeconds(CommandTimeout)
                        ),
                    "Starting browser", "started."
                );

                // Sauce doesn't support this :|
                /*
                WebDriver.Manage().Timeouts().SetPageLoadTimeout(
                    TimeSpan.FromSeconds(5)
                );
                WebDriver.Manage().Timeouts().SetScriptTimeout(
                    TimeSpan.FromSeconds(1)
                );
                 */

            } catch (Exception exc2) {
                Dispose();
                throw;
            }
        }

        private Process StartProxy () {
            Process proxy = null;

            var assemblyPath = Path.GetDirectoryName(
                Util.GetPathOfAssembly(Assembly.GetExecutingAssembly())
            );

            var psi = new ProcessStartInfo(
                Path.GetFullPath(Path.Combine(
                    assemblyPath, @"..\..\..\..\Upstream\SauceLabs\Sauce-Connect.jar"
                )),
                String.Format(
                    "{0} {1}",
                    SauceLabs.Credentials.username,
                    SauceLabs.Credentials.accessKey
                )
            ) {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal,
                ErrorDialog = false,
                WorkingDirectory = TempPath
            };

            if (File.Exists(LogPath))
                File.Delete(LogPath);

            PassOrFail(
                () => {
                    proxy = Process.Start(psi);
                    proxy.WaitForInputIdle(5000);

                    if (proxy.HasExited)
                        throw new Exception("Process terminated prematurely with exit code " + Proxy.ExitCode);
                },
                "Starting proxy", "started."
            );

            PassOrFail(
                () => WaitForProxyLogText("INFO - Connected! You may start your tests."), 
                "Waiting for proxy connection", "connected."
            );

            return proxy;
        }

        public void PassOrFail (Action action, string caption, string passText = "succeeded.", string failText = "failed.") {
            PassOrFail<object>(
                () => {
                    action();
                    return null;
                }, caption, passText, failText
            );
        }

        public T PassOrFail<T> (Func<T> fn, string caption, string passText = "succeeded.", string failText = "failed.") {
            T result;

            Console.Write(caption + "... ");
            try {
                result = fn();
                Console.WriteLine(passText);
            } catch (Exception exc) {
                Console.WriteLine(failText);
                throw;
            }

            return result;
        }

        public void WaitForProxyLogText (string searchText, int timeoutMs = 60000) {
            var started = DateTime.UtcNow.Ticks;
            var timeoutAt = started + TimeSpan.FromMilliseconds(timeoutMs).Ticks;

            while (DateTime.UtcNow.Ticks < timeoutAt) {
                if (File.Exists(LogPath))
                    break;
            }

            using (var stream = new FileStream(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            while (DateTime.UtcNow.Ticks < timeoutAt) {
                stream.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[stream.Length];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);

                var logText = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                if (logText.Contains(searchText))
                    return;

                Thread.Sleep(100);
            }

            throw new Exception("Timed out without seeing expected log text");
        }

        public void LoadPage (string path, string pageOptions = null) {
            var generatedUrl = String.Format("{0}/{1}?{2}",
                RunningAgainstLocalServer ? LocalHost : RemoteHost,
                path,
                pageOptions ?? DefaultPageOptions
            );

            PassOrFail(
                () => 
                    WebDriver.Navigate().GoToUrl(
                        generatedUrl
                    )
                ,
                String.Format("Loading {0}", generatedUrl),
                "loaded."
            );
        }

        public object Evaluate (string expression) {
            return WebDriver.ExecuteScript(expression);
        }

        private static bool DefaultResultPredicate (object value) {
            if (value == null)
                return false;

            try {
                if (Convert.ToInt64(value) != 0)
                    return true;
            } catch {
            }

            return false;
        }

        public void WaitFor (string expression, Func<object, bool> resultPredicate = null, int timeoutMs = 5000, int tickRateMs = 1000, string evaluateEachTick = null) {
            var started = DateTime.UtcNow.Ticks;
            var timeoutAt = started + TimeSpan.FromMilliseconds(timeoutMs).Ticks;

            object currentValue = null;
            if (resultPredicate == null)
                resultPredicate = DefaultResultPredicate;

            while (DateTime.UtcNow.Ticks < timeoutAt) {
                if (evaluateEachTick != null)
                    Evaluate(evaluateEachTick);

                currentValue = Evaluate(expression);

                if (resultPredicate(currentValue))
                    return;

                Thread.Sleep(tickRateMs);
            }

            throw new Exception("Timed out. Last value was: " + Convert.ToString(currentValue));
        }

        public string GetLogText () {
            var logText = WebDriver.ExecuteScript("return window.test.logText;");
            return (string)logText;
        }

        public ExceptionInfo[] GetExceptions () {
            var result = new List<ExceptionInfo>();

            var exceptions = WebDriver.ExecuteScript("return window.test.exceptions;");

            var exceptionList = (ReadOnlyCollection<object>)exceptions;

            foreach (var exc in exceptionList) {
                var excInfo = (ReadOnlyCollection<object>)exc;

                result.Add(new ExceptionInfo {
                    TimeStamp = Convert.ToDouble(excInfo[0]),
                    Text = Convert.ToString(excInfo[1])
                });
            }

            return result.ToArray();
        }

        public void Dispose () {
            if (!IsDisposed) {
                IsDisposed = true;

                Console.Write("Closing session... ");

                if (Proxy != null) {
                    Proxy.CloseMainWindow();
                    Proxy.WaitForExit(10000);
                    Proxy.Kill();
                    Proxy.Dispose();
                }

                if (WebDriver != null) {
                    WebDriver.Quit();
                    WebDriver.Dispose();
                }

                Console.WriteLine("closed.");
            }
        }
    }

    public struct ExceptionInfo {
        public double TimeStamp;
        public string Text;

        public override string ToString () {
            return String.Format("{0}: {1}", TimeStamp, Text);
        }
    }
}
