using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Internal {
    public class TypeInfo {
        public readonly Dictionary<long, string> ValueToEnumMemberName = new Dictionary<long, string>();
        public readonly Dictionary<string, EnumMemberInfo> EnumMembers = new Dictionary<string, EnumMemberInfo>();
    }

    public class EnumMemberInfo {
        public readonly string Name;
        public readonly long Value;
    }
}
