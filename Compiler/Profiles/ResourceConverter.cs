using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using JSIL.Compiler;

namespace JSIL.Utilities {
    public static class ResourceConverter {
        public static void ConvertEmbeddedResources (Configuration configuration, string assemblyPath, TranslationResult result) {
            var asm = Assembly.ReflectionOnlyLoadFrom(assemblyPath);

            var resourceFiles = (from fn in asm.GetManifestResourceNames() where fn.EndsWith(".resources") select fn).ToArray();
            var encoding = new UTF8Encoding(false);

            foreach (var resourceName in resourceFiles) {
                if (!resourceName.EndsWith(".resources"))
                    continue;

                var resourceJson = ConvertEmbeddedResourceFile(configuration, assemblyPath, asm, resourceName);
                var bytes = encoding.GetBytes(resourceJson);

                result.AddFile(
                    "Resources",
                    Path.GetFileNameWithoutExtension(resourceName) + ".resj",
                    new ArraySegment<byte>(bytes)
                );
            }
        }

        public static string ConvertEmbeddedResourceFile (Configuration configuration, string assemblyName, Assembly assembly, string resourceName) {
            var output = new StringBuilder();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new ResourceReader(stream)) {
                output.AppendLine("{");

                bool first = true;

                var e = reader.GetEnumerator();
                while (e.MoveNext()) {
                    if (!first)
                        output.AppendLine(",");
                    else
                        first = false;

                    var key = Convert.ToString(e.Key);
                    output.AppendFormat("    {0}: ", JSIL.Internal.Util.EscapeString(key));

                    var value = e.Value;

                    if (value == null) {
                        output.Append("null");
                    } else {
                        switch (value.GetType().FullName) {
                            case "System.String":
                                output.Append(JSIL.Internal.Util.EscapeString((string)value));
                                break;
                            case "System.Single":
                            case "System.Double":
                            case "System.UInt16":
                            case "System.UInt32":
                            case "System.UInt64":
                            case "System.Int16":
                            case "System.Int32":
                            case "System.Int64":
                                output.Append(Convert.ToString(value));
                                break;
                            default:
                                output.Append(JSIL.Internal.Util.EscapeString(Convert.ToString(value)));
                                break;
                        }
                    }
                }

                output.AppendLine();
                output.AppendLine("}");
            }

            return output.ToString();
        }
    }
}
