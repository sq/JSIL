using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSIL.Compiler.Profiles {
    public static class Util {
        public static V GetValueOrDefault<K, V> (this Dictionary<K, V> dict, K key, V defaultValue) {
            V result;
            if (!dict.TryGetValue(key, out result))
                result = defaultValue;

            return result;
        }

        public static void SetDefault<K, V> (this Dictionary<K, V> dict, K key, V defaultValue) {
            if (dict.ContainsKey(key))
                return;

            dict[key] = defaultValue;
        }
    }
}
