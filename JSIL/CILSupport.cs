using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using JSIL.Translator;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;

namespace JSIL.Internal {
    public class AssemblyCache : ConcurrentCache<string, AssemblyDefinition> {
        public bool RegisterAssembly (AssemblyDefinition assembly) {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var name = assembly.Name.FullName;

            return TryCreate(name, (fullName) => assembly);
        }
    }

    public class AssemblyResolver : BaseAssemblyResolver, IDisposable {
        private static readonly byte[] PCLPublicKeyToken = {
            124, 236, 133, 215,
            190, 167, 121, 142
        };

        private static readonly byte[] BCLPublicKeyToken = {
            183, 122, 92, 86,
            25, 52, 224, 137
        };

        protected readonly Configuration Configuration;
        protected readonly AssemblyCache Cache = new AssemblyCache();
        protected readonly bool OwnsCache;

        public AssemblyResolver (IEnumerable<string> dirs, Configuration configuration, AssemblyCache cache = null) {
            Configuration = configuration;

            OwnsCache = (cache == null);
            Cache = cache ?? new AssemblyCache();

            foreach (var dir in dirs)
                AddSearchDirectory(dir);
        }

        public void Dispose () {
            if (OwnsCache)
                Cache.Dispose();
        }

        public AssemblyNameReference FilterPortableClassLibraryReferences (AssemblyNameReference name) {
            // Portable class libraries are pretty shoddily constructed. Who came up with this nonsense?

            if (!name.PublicKeyToken.SequenceEqual(PCLPublicKeyToken))
                return name;

            var bclName = new AssemblyNameReference(
                name.Name,
                // FIXME: Is this right? It was 4.0 before, but that's definitely wrong.
                //  This should pick the highest appropriate version from the GAC.
                null
            ) {
                Culture = name.Culture,
                PublicKeyToken = BCLPublicKeyToken
            };

            return bclName;
        }

        public AssemblyNameReference FilterRedirectedReferences (AssemblyNameReference name, out string redirectedFrom) {
            redirectedFrom = null;

            foreach (var kvp in Configuration.Assemblies.Redirects) {
                if (Regex.IsMatch(name.FullName, kvp.Key, RegexOptions.IgnoreCase)) {
                    redirectedFrom = name.FullName;
                    return AssemblyNameReference.Parse(kvp.Value);
                }
            }

            return name;
        }

        public override AssemblyDefinition Resolve (AssemblyNameReference name, ReaderParameters parameters) {
            if (name == null)
                throw new ArgumentNullException("name");

            var actualName = FilterPortableClassLibraryReferences(name);
            string redirectedFrom;
            actualName = FilterRedirectedReferences(name, out redirectedFrom);

            var result = Cache.GetOrCreate(actualName.FullName, (fullName) => {
                if (redirectedFrom != null)
                    Console.Error.WriteLine("// Redirected '{0}' to '{1}'", redirectedFrom, actualName.FullName);

                return base.Resolve(actualName, parameters);
            });

            return result;
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

    public class CachingMetadataResolver : MetadataResolver {
        public struct Key {
            public readonly int HashCode;

            public readonly string Namespace;
			public readonly string Module;
            public readonly string Name;
            public readonly string DeclaringTypeName;

            public Key (TypeReference tr) {
                Namespace = tr.Namespace;
                Module = tr.Module.Name;
				Name = tr.Name;

				HashCode = Namespace.GetHashCode() ^ Name.GetHashCode() ^ Module.GetHashCode();

                if (tr.DeclaringType != null) {
                    DeclaringTypeName = tr.DeclaringType.FullName;
                    HashCode = HashCode ^ DeclaringTypeName.GetHashCode();
                } else {
                    DeclaringTypeName = null;
                }
            }

            public bool Equals (Key rhs) {
                return (Namespace == rhs.Namespace) && (Module == rhs.Module) &&
					(DeclaringTypeName == rhs.DeclaringTypeName) &&
					(Name == rhs.Name);
            }

            public override bool Equals (object obj) {
                if (obj is Key) {
                    return Equals((Key)obj);
                }

                return base.Equals(obj);
            }

            public override int GetHashCode() {
                return HashCode;
            }
        }

        public class KeyComparer : IEqualityComparer<Key> {
            public bool Equals (Key x, Key y) {
                return x.Equals(y);
            }

            public int GetHashCode (Key obj) {
                return obj.HashCode;
            }
        }

        public readonly ConcurrentCache<Key, TypeDefinition> Cache;
        protected readonly ConcurrentCache<Key, TypeDefinition>.CreatorFunction<TypeReference> DoResolve;

        public CachingMetadataResolver (IAssemblyResolver assemblyResolver) :
            base(assemblyResolver) {

            Cache = new ConcurrentCache<Key, TypeDefinition>(
                Environment.ProcessorCount, 4096, new KeyComparer()
            );

            DoResolve = (key, type) => {
                var result = base.Resolve(type);
                return result;
            };
        }

        public override TypeDefinition Resolve (TypeReference type) {
            var key = new Key(type);

            var result = Cache.GetOrCreate(
                key, type, DoResolve
            );

            return result;
        }
    }

    public class FullNameAssemblyComparer : IEqualityComparer<AssemblyDefinition> {
        public bool Equals (AssemblyDefinition x, AssemblyDefinition y) {
            return x.FullName.Equals(y.FullName);
        }

        public int GetHashCode (AssemblyDefinition obj) {
            return obj.FullName.GetHashCode();
        }
    }

    public static class CecilUtil {
        // WHY IS CECIL SO DUMB?
        public static MethodReference RebindMethod (MethodReference method, TypeReference newDeclaringType, TypeReference newReturnType = null) {
            var result = new MethodReference(
                method.Name, newReturnType ?? method.ReturnType, newDeclaringType
            ) {
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis
            };

            // TODO: Copy more attributes?

            return result;
        }
    }
}
