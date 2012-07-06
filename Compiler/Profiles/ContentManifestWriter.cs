using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace JSIL.Utilities {
    public class ContentManifestWriter : IDisposable {
        public readonly string OutputPath;
        public readonly string Name;

        private readonly JavaScriptSerializer Serializer;
        private readonly StreamWriter Output;
        private bool Disposed = false;

        public ContentManifestWriter (string outputPath, string name) {
            OutputPath = outputPath;
            Name = name;

            Serializer = new JavaScriptSerializer();
            Output = new StreamWriter(outputPath, false, new UTF8Encoding(false));

            WriteHeader();
        }

        private void WriteHeader () {
            Output.WriteLine("// {0}\r\n", JSIL.AssemblyTranslator.GetHeaderText());
            Output.WriteLine();
            Output.WriteLine("if (typeof (contentManifest) !== \"object\") { contentManifest = {}; };");
            Output.WriteLine("contentManifest[{0}] = [", JSIL.Internal.Util.EscapeString(Name));
        }

        private void WriteFooter () {
            Output.WriteLine("];");
        }

        public void Add (string type, string path, object properties) {
            path = path.Replace("\\", "/");

            Output.WriteLine(
                "    [{0}, {1}, {2}],",
                JSIL.Internal.Util.EscapeString(type),
                JSIL.Internal.Util.EscapeString(path),
                Serializer.Serialize(properties)
            );
        }

        public void Dispose () {
            if (Disposed)
                return;

            Disposed = true;
            WriteFooter();
            Output.Dispose();
        }
    }
}
