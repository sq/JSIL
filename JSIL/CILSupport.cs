using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;

namespace JSIL.Internal {
    public class AssemblyCache : ConcurrentCache<string, AssemblyDefinition>, IDisposable {
        public void Dispose () {
            Clear();
        }
    }

    public class AssemblyResolver : BaseAssemblyResolver {
        protected readonly AssemblyCache Cache = new AssemblyCache();

        public AssemblyResolver (IEnumerable<string> dirs, AssemblyCache cache = null) {
            if (cache != null)
                Cache = cache;
            else
                Cache = new AssemblyCache();

            foreach (var dir in dirs)
                AddSearchDirectory(dir);
        }

        public override AssemblyDefinition Resolve (AssemblyNameReference name, ReaderParameters parameters) {
            if (name == null)
                throw new ArgumentNullException("name");

            return Cache.GetOrCreate(name.FullName, () => base.Resolve(name, parameters));
        }

        protected void RegisterAssembly (AssemblyDefinition assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var name = assembly.Name.FullName;

            Cache.TryCreate(name, () => assembly);
        }
    }

    public class NoSymbolsException : FileNotFoundException {
        public NoSymbolsException (ModuleDefinition module, string fileName)
        : base(
            String.Format("Module '{0}' has no symbols.", Path.GetFileName(module.FullyQualifiedName)), fileName
        ) {
        }
    }

    public class SymbolProvider : ISymbolReaderProvider {
        public ISymbolReader GetSymbolReader (ModuleDefinition module, string fileName) {
            if (String.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Invalid path", "fileName");

            var pdbPath = Path.Combine(
                Path.GetDirectoryName(fileName),
                Path.GetFileNameWithoutExtension(fileName) + ".pdb"
            );
            var mdbPath = Path.Combine(
                Path.GetDirectoryName(fileName),
                Path.GetFileName(fileName) + ".mdb"
            );

            if (File.Exists(pdbPath)) {
                return (new PdbReaderProvider()).GetSymbolReader(module, File.OpenRead(pdbPath));
            } else if (File.Exists(mdbPath)) {
                return (new MdbReaderProvider()).GetSymbolReader(module, fileName);
            } else {
                throw new NoSymbolsException(module, fileName);
            }
        }

        public ISymbolReader GetSymbolReader (ModuleDefinition module, Stream symbolStream) {
            // Internal constructor for no reason! Thanks!
            return (new PdbReaderProvider()).GetSymbolReader(module, symbolStream);
        }
    }
}
