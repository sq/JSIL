using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace JSIL {
    public class TranslationResult {
        public readonly List<AssemblyDefinition> Assemblies = new List<AssemblyDefinition>();
        public readonly Dictionary<string, ArraySegment<byte>> Files = new Dictionary<string, ArraySegment<byte>>();
        public ArraySegment<byte> Manifest;

        public void WriteToStream (Stream output) {
            if (Manifest.Array == null)
                throw new Exception("AssemblyTranslator.GenerateManifest must be called first");

            var newline = Encoding.ASCII.GetBytes(Environment.NewLine);

            output.Write(Manifest.Array, Manifest.Offset, Manifest.Count);
            output.Write(newline, 0, newline.Length);

            foreach (var file in Files.Values) {
                output.Write(file.Array, file.Offset, file.Count);
                output.Write(newline, 0, newline.Length);
            }

            output.Flush();
        }

        public string WriteToString () {
            if (Manifest.Array == null)
                throw new Exception("AssemblyTranslator.GenerateManifest must be called first");

            using (var ms = new MemoryStream()) {
                WriteToStream(ms);
                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }

        private static void WriteBytesToFile (string folder, string name, ArraySegment<byte> bytes) {
            var filePath = Path.Combine(folder, name);
            var fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.CreateNew;

            using (var fs = File.Open(filePath, fileMode, FileAccess.Write, FileShare.Read)) {
                fs.Write(bytes.Array, bytes.Offset, bytes.Count);
                fs.Flush();
            }
        }

        public void WriteToDirectory (string path, string manifestPrefix = "") {
            if (Manifest.Array == null)
                throw new Exception("AssemblyTranslator.GenerateManifest must be called first");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            WriteBytesToFile(path, manifestPrefix + "manifest.js", Manifest);

            foreach (var kvp in Files)
                WriteBytesToFile(path, kvp.Key, kvp.Value);
        }
    }
}
