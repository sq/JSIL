using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace JSIL.Tests {
    public class JavaScriptEvaluatorException : Exception {
        public readonly int ExitCode;
        public readonly string ErrorText;
        public readonly string Output;
        public readonly JavaScriptException[] Exceptions;

        public JavaScriptEvaluatorException (int exitCode, string stdout, string stderr, JavaScriptException[] exceptions)
            : base(String.Format("JavaScript interpreter exited with code {0} after throwing {3} exception(s):\r\n{1}\r\n{2}", exitCode, stdout, stderr, exceptions.Length)) 
        {
            ExitCode = exitCode;
            ErrorText = stderr;
            Output = stdout;
            Exceptions = exceptions;
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
