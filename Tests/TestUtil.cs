using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace JSIL.Tests {
    public static class ProcessUtil {        
        public static int Run (string filename, string parameters, byte[] stdin, out string stderr, out string stdout, string cwd = null) {
            var psi = new ProcessStartInfo(filename, parameters);

            psi.WorkingDirectory = cwd ?? Path.GetDirectoryName(filename);
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;

            using (var process = Process.Start(psi)) {
                var stdinStream = process.StandardInput.BaseStream;
                var stderrStream = process.StandardError.BaseStream;
                var stdoutStream = process.StandardOutput.BaseStream;

                if (stdin != null) {
                    ThreadPool.QueueUserWorkItem(
                        (_) => {
                            if (stdin != null) {
                                stdinStream.Write(
                                    stdin, 0, stdin.Length
                                );
                                stdinStream.Flush();
                            }

                            stdinStream.Close();
                        }, null
                    );
                }

                var temp = new string[2] { null, null };
                ThreadPool.QueueUserWorkItem(
                    (_) => {
                        temp[1] = Encoding.ASCII.GetString(ReadEntireStream(stderrStream));
                    }, null
                );

                temp[0] = Encoding.ASCII.GetString(ReadEntireStream(stdoutStream));

                process.WaitForExit();

                stdout = temp[0];
                stderr = temp[1];

                var exitCode = process.ExitCode;

                process.Close();

                return exitCode;
            }
        }

        private static byte[] ReadEntireStream (Stream stream) {
            var result = new List<byte>();
            var buffer = new byte[32767];

            while (true) {
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead < buffer.Length) {

                    if (bytesRead > 0) {
                        result.Capacity = result.Count + bytesRead;
                        result.AddRange(buffer.Take(bytesRead));
                    }

                    if (bytesRead <= 0)
                        break;
                } else {
                    result.AddRange(buffer);
                }
            }

            return result.ToArray();
        }
    }

    public class JavaScriptEvaluatorException : Exception {
        public readonly int ExitCode;
        public readonly string ErrorText;
        public readonly string Output;
        public readonly JavaScriptException[] Exceptions;

        public JavaScriptEvaluatorException (int exitCode, string stdout, string stderr, JavaScriptException[] exceptions)
            : base(FormatMessage(exitCode, stderr, exceptions)) 
        {
            ExitCode = exitCode;
            ErrorText = stderr;
            Output = stdout;
            Exceptions = exceptions;
        }

        private static string FormatMessage (int exitCode, string stderr, JavaScriptException[] exceptions) {
            if (exceptions.Length == 0)
                return String.Format(
                    "JavaScript interpreter exited with code {0}:\r\n{1}",
                    exitCode, stderr
                );

            return String.Format(
                "JavaScript interpreter exited with code {0} after throwing {1} exception(s):\r\n{2}",
                exitCode, exceptions.Length,
                string.Join("\r\n", (from exc in exceptions select exc.ToString()))
            );
        }
    }

    public class JavaScriptException : Exception {
        new public readonly string Message;
        public readonly string Stack;

        private static string FormatMessage (string message, string stack) {
            if (stack != null)
                return String.Format("JavaScript Exception: {0}\r\n{1}", message, stack);
            else
                return String.Format("JavaScript Exception: {0}", message);
        }

        public JavaScriptException (string message, string stack) 
            : base (FormatMessage(message, stack)) {
            Message = message;
            Stack = stack;
        }
    }

    public static class TestExtensions {
        public static bool ContainsRegex (this string text, string regex) {
            var m = Regex.Matches(text, regex);
            return (m.Count > 0);
        }
    }

    [Serializable]
    public class Metacomment {
        public static Regex Regex = new Regex(
            @"//@(?'command'[A-Za-z_0-9]+)( (?'arguments'[^\n\r]*))?",
            RegexOptions.ExplicitCapture
        );

        public readonly string Command;
        public readonly string Arguments;

        public Metacomment (string command, string arguments) {
            Command = command;
            Arguments = arguments;
        }

        public override string ToString () {
            return String.Format("{0} {1}", Command, Arguments);
        }

        public static Metacomment[] FromText (string text) {
            var result = new List<Metacomment>();

            foreach (Match match in Regex.Matches(text))
                result.Add(new Metacomment(match.Groups["command"].Value, match.Groups["arguments"].Value));

            return result.ToArray();
        }
    }

    public static class Portability {
        public static string NormalizeNewLines (string text) {
            return text.Replace("\r\n", Environment.NewLine);
        }

        public static string NormalizeDirectorySeparators (string filename) {
            return filename.Replace('\\', Path.DirectorySeparatorChar);
        }

        public static IEnumerable<string> NormalizeDirectorySeparators (IEnumerable<string> filenames) {
            return filenames.Select(NormalizeDirectorySeparators);
        }

    }
}
