using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace JSIL {
    public class TranslationResult {
        public struct ResultFile {
            public string Filename;
            public long Size;
            public ArraySegment<byte> Contents;
        }

        public readonly List<AssemblyDefinition> Assemblies = new List<AssemblyDefinition>();
        public readonly List<string> FileOrder = new List<string>();
        public readonly Dictionary<string, ResultFile> Files = new Dictionary<string, ResultFile>();
        public ArraySegment<byte> Manifest;

        public IEnumerable<ResultFile> OrderedFiles {
            get {
                foreach (var filename in FileOrder)
                    yield return Files[filename];
            }
        }

        public void AddFile (string filename, ArraySegment<byte> bytes, int? position = null) {
            lock (Files) {
                if (position.HasValue)
                    FileOrder.Insert(position.Value, filename);
                else
                    FileOrder.Add(filename);

                Files.Add(filename, new ResultFile {
                    Filename = filename,
                    Contents = bytes,
                    Size = bytes.Count
                });
            }
        }

        public void WriteToStream (Stream output) {
            if (Manifest.Array == null)
                throw new Exception("AssemblyTranslator.GenerateManifest must be called first");

            var newline = Encoding.ASCII.GetBytes(Environment.NewLine);

            output.Write(Manifest.Array, Manifest.Offset, Manifest.Count);
            output.Write(newline, 0, newline.Length);

            foreach (var file in Files.Values) {
                if (file.Contents.Count > 0) {
                    output.Write(file.Contents.Array, file.Contents.Offset, file.Contents.Count);
                }
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

            foreach (var kvp in Files) {
                if (kvp.Value.Contents.Count > 0)
                    WriteBytesToFile(path, kvp.Key, kvp.Value.Contents);
            }
        }

        internal void AddExistingFile (string filename, long fileSize, int? position = null) {
            lock (Files) {
                if (position.HasValue)
                    FileOrder.Insert(position.Value, filename);
                else
                    FileOrder.Add(filename);

                Files.Add(filename, new ResultFile {
                    Filename = filename,
                    Size = fileSize
                });
            }
        }
    }
}
