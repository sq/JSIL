//
// Annotations.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;
using Mono.Cecil;

namespace Mono.Linker {
    public class AnnotationStore {
        private readonly Dictionary<MethodDefinition, HashSet<MethodDefinition>> base_methods = new Dictionary<MethodDefinition, HashSet<MethodDefinition>>();
        private readonly Dictionary<MethodDefinition, HashSet<MethodDefinition>> override_methods = new Dictionary<MethodDefinition, HashSet<MethodDefinition>>();
        private readonly Dictionary<IMemberDefinition, HashSet<MethodDefinition>> preserved_methods = new Dictionary<IMemberDefinition, HashSet<MethodDefinition>>();

        public void AddOverride(MethodDefinition @base, MethodDefinition @override) {
            HashSet<MethodDefinition> methods = GetOverrides(@base);
            if (methods == null) {
                methods = new HashSet<MethodDefinition>();
                override_methods[@base] = methods;
            }

            methods.Add(@override);
        }

        public HashSet<MethodDefinition> GetOverrides(MethodDefinition method) {
            HashSet<MethodDefinition> overrides;
            if (override_methods.TryGetValue(method, out overrides)) {
                return overrides;
            }

            return null;
        }

        public void AddBaseMethod(MethodDefinition method, MethodDefinition @base) {
            HashSet<MethodDefinition> methods = GetBaseMethods(method);
            if (methods == null) {
                methods = new HashSet<MethodDefinition>();
                base_methods[method] = methods;
            }

            methods.Add(@base);
        }

        public HashSet<MethodDefinition> GetBaseMethods(MethodDefinition method) {
            HashSet<MethodDefinition> bases;
            if (base_methods.TryGetValue(method, out bases)) {
                return bases;
            }

            return null;
        }

        public HashSet<MethodDefinition> GetPreservedMethods(TypeDefinition type) {
            return GetPreservedMethods(type as IMemberDefinition);
        }

        public void AddPreservedMethod(TypeDefinition type, MethodDefinition method) {
            AddPreservedMethod(type as IMemberDefinition, method);
        }

        public HashSet<MethodDefinition> GetPreservedMethods(MethodDefinition method) {
            return GetPreservedMethods(method as IMemberDefinition);
        }

        public void AddPreservedMethod(MethodDefinition key, MethodDefinition method) {
            AddPreservedMethod(key as IMemberDefinition, method);
        }

        private HashSet<MethodDefinition> GetPreservedMethods(IMemberDefinition definition) {
            HashSet<MethodDefinition> preserved;
            if (preserved_methods.TryGetValue(definition, out preserved)) {
                return preserved;
            }

            return null;
        }

        private void AddPreservedMethod(IMemberDefinition definition, MethodDefinition method) {
            HashSet<MethodDefinition> methods = GetPreservedMethods(definition);
            if (methods == null) {
                methods = new HashSet<MethodDefinition>();
                preserved_methods[definition] = methods;
            }

            methods.Add(method);
        }
    }
}