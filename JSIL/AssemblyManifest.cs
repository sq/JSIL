using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL {
    public class AssemblyManifest : IDisposable {
        public class Token {
            public readonly string Assembly;

            public Int32? ID {
                get;
                internal set;
            }

            public string IDString {
                get {
                    return String.Format("$asm{0:X2}", ID.Value);
                }
            }

            internal Token (string assembly) {
                Assembly = assembly;
            }
        }

        protected readonly Dictionary<string, long> TranslatedAssemblySizes = new Dictionary<string, long>();
        protected readonly ConcurrentCache<string, Token> Tokens = new ConcurrentCache<string, Token>();
        protected bool AssignedIdentifiers = false;

        public void Dispose () {
            Tokens.Dispose();
        }

        public void AssignIdentifiers () {
            if (AssignedIdentifiers)
                return;

            var names = (from kvp in Tokens select kvp.Key).OrderBy((k) => k);
            int max = (from kvp in Tokens select kvp.Value.ID.GetValueOrDefault(-1)).Max();

            int i = max + 1;

            foreach (var name in names) {
                Token token;
                if (Tokens.TryGet(name, out token)) {
                    if (!token.ID.HasValue)
                        token.ID = i++;
                }
            }

            AssignedIdentifiers = true;
        }

        public Token GetPrivateToken (AssemblyDefinition assembly) {
            return GetPrivateToken(assembly.FullName);
        }

        public Token GetPrivateToken (string assemblyFullName) {
            Token result = Tokens.GetOrCreate(
                assemblyFullName, () => {
                    AssignedIdentifiers = false;
                    return new Token(assemblyFullName);
                }
            );

            return result;
        }

        public IEnumerable<KeyValuePair<string, string>> Entries {
            get {
                return
                    (from kvp in Tokens orderby kvp.Value.ID
                     select new KeyValuePair<string, string>(
                         kvp.Value.IDString, kvp.Key
                    ));
            }
        }

        public bool GetExistingSize (AssemblyDefinition assembly, out long fileSize) {
            lock (TranslatedAssemblySizes)
                return TranslatedAssemblySizes.TryGetValue(assembly.FullName, out fileSize);
        }

        public void SetAlreadyTranslated (AssemblyDefinition assembly, long fileSize) {
            lock (TranslatedAssemblySizes)
                TranslatedAssemblySizes.Add(assembly.FullName, fileSize);
        }
    }
}
