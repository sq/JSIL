using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using JSIL.Internal;
using Mono.Cecil;

namespace JSIL {
    public class AssemblyManifest {
        protected int NextID = 0;
        protected readonly ConcurrentCache<string, Int32> AssemblyIDs = new ConcurrentCache<string,int>();

        public string GetPrivateToken (AssemblyDefinition assembly) {
            return GetPrivateToken(assembly.FullName);
        }

        public string GetPrivateToken (string assemblyFullName) {
            Int32 result = AssemblyIDs.GetOrCreate(
                assemblyFullName, () => Interlocked.Increment(ref NextID)
            );

            return FormatID(result);
        }

        protected string FormatID (Int32 result) {
            return String.Format("$asm{0:X2}", result);
        }

        public IEnumerable<KeyValuePair<string, string>> Entries {
            get {
                return
                    (from kvp in AssemblyIDs orderby kvp.Value 
                     select new KeyValuePair<string, string>(
                         FormatID(kvp.Value), kvp.Key
                    ));
            }
        }
    }
}
