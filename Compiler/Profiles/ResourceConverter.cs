using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Policy;
using System.Text;
using JSIL.Compiler;

namespace JSIL.Utilities {
    public static class ResourceConverter {
        public static void ConvertResources (Configuration configuration, string assemblyPath, TranslationResult result)
        {
            assemblyPath = Path.GetFullPath(assemblyPath);
            ConvertEmbeddedResources(configuration, assemblyPath, result);
            ConvertSatelliteResources(configuration, assemblyPath, result);
        }

        public static void ConvertEmbeddedResources (Configuration configuration, string assemblyPath, TranslationResult result) {
            using (var domain = new TemporaryAppDomain("ConvertEmbeddedResources")) {
                var resourceExtractor = domain.CreateInstanceAndUnwrap<
                    ManifestResourceExtractor, IManifestResourceExtractor
                >();

                var manifestResources = resourceExtractor.GetManifestResources(
                    assemblyPath, (fn) => fn.EndsWith(".resources")
                );

                var encoding = new UTF8Encoding(false);

                foreach (var kvp in manifestResources) {
                    var outputPath = Path.GetFileNameWithoutExtension(kvp.Key) + ".resj";
                    // FIXME: We're converting embedded resources multiple times per run :(
                    if (result.Files.ContainsKey(outputPath))
                        continue;

                    Console.WriteLine(kvp.Key);

                    string resourceJson;
                    using (var memoryStream = new MemoryStream(kvp.Value, false))
                        resourceJson = ConvertResources(memoryStream);

                    var bytes = encoding.GetBytes(resourceJson);

                    result.AddFile(
                        "Resources",
                        outputPath,
                        new ArraySegment<byte>(bytes)
                    );
                }
            }
        }

        public static void ConvertSatelliteResources (Configuration configuration, string assemblyPath, TranslationResult result) {
            var filterKey = Path.GetFileNameWithoutExtension(assemblyPath) + ".resources.dll";

            var satelliteResourceAssemblies = Directory.GetFiles(
                Path.GetDirectoryName(assemblyPath), filterKey, SearchOption.AllDirectories
            );

            foreach (var satelliteResourceAssembly in satelliteResourceAssemblies) {
                ConvertEmbeddedResources(configuration, satelliteResourceAssembly, result);
            }
        }

        public static string ConvertResources (Stream resourceStream) {
            var output = new StringBuilder();

            using (var reader = new ResourceReader(resourceStream)) {
                output.AppendLine("{");

                bool first = true;

                var e = reader.GetEnumerator();
                while (e.MoveNext()) {
                    if (!first)
                        output.AppendLine(",");
                    else
                        first = false;

                    var key = Convert.ToString(e.Key);
                    output.AppendFormat("    {0}: ", JSIL.Internal.Util.EscapeString(key, forJson: true));

                    var value = e.Value;

                    if (value == null) {
                        output.Append("null");
                    } else {
                        switch (value.GetType().FullName) {
                            case "System.String":
                                output.Append(JSIL.Internal.Util.EscapeString((string)value, forJson:true));
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
                                output.Append(JSIL.Internal.Util.EscapeString(Convert.ToString(value), forJson: true));
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

    public class TemporaryAppDomain : IDisposable {
        public readonly AppDomain Domain;

        public TemporaryAppDomain (string name) {
            var currentDomain = AppDomain.CurrentDomain;
            var currentSetup = currentDomain.SetupInformation;
            var domainSetup = new AppDomainSetup {
                ApplicationBase = currentSetup.ApplicationBase,
                ApplicationName = currentSetup.ApplicationName,
                ApplicationTrust = currentSetup.ApplicationTrust,
                CachePath = currentSetup.CachePath,
                ConfigurationFile = currentSetup.ConfigurationFile,
                DisallowCodeDownload = true,
                PrivateBinPath = currentSetup.PrivateBinPath,
                PrivateBinPathProbe = currentSetup.PrivateBinPathProbe,
                ShadowCopyDirectories = currentSetup.ShadowCopyDirectories,
                ShadowCopyFiles = currentSetup.ShadowCopyFiles,
                LoaderOptimization = LoaderOptimization.MultiDomain
            };

            try
            {
                domainSetup.DynamicBase = currentSetup.DynamicBase;
            }
            catch (System.MemberAccessException)
            {
                // Mono bugs! Yay!
            }

            Domain = AppDomain.CreateDomain(name, null, domainSetup);
        }

        public TResult CreateInstanceAndUnwrap<TInstance, TResult> ()
            where TInstance : MarshalByRefObject
        {
            var tInstance = typeof(TInstance);
            return (TResult)Domain.CreateInstanceFromAndUnwrap(tInstance.Assembly.Location, tInstance.FullName);
        }

        public void Dispose () {
            AppDomain.Unload(Domain);
        }
    }

    public interface IManifestResourceExtractor {
        Dictionary<string, byte[]> GetManifestResources (string assemblyPath, Func<string, bool> filenamePredicate);
    }

    public class ManifestResourceExtractor : MarshalByRefObject, IManifestResourceExtractor {
        public Dictionary<string, byte[]> GetManifestResources (string assemblyPath, Func<string, bool> filenamePredicate) {
            var asm = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
            var resourceFiles = (from fn in asm.GetManifestResourceNames() where filenamePredicate(fn) select fn).ToArray();
            var result = new Dictionary<string, byte[]>();

            foreach (var resourceFile in resourceFiles) {
                var stream = asm.GetManifestResourceStream(resourceFile);
                if (stream == null)
                    throw new FileNotFoundException("No manifest resource stream named " + resourceFile);

                using (stream) {
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    result[resourceFile] = buffer;
                }
            }

            return result;
        }
    
        public static void ExtractFromAssembly (Configuration configuration, string assemblyPath, TranslationResult result) {
            using (var domain = new TemporaryAppDomain("ExtractFromAssembly")) {
                var resourceExtractor = domain.CreateInstanceAndUnwrap<
                    ManifestResourceExtractor, IManifestResourceExtractor
                >();

                var manifestResources = resourceExtractor.GetManifestResources(
                    assemblyPath, (fn) => !fn.EndsWith(".resources")
                );

                foreach (var kvp in manifestResources) {
                    var key = kvp.Key;

                    if (result.Files.ContainsKey(key)) {
                        if (result.Files[key].Size != kvp.Value.Length)
                            throw new InvalidOperationException("Found two conflicting manifest resources named '" + key + "'");
                    } else {
                        Console.WriteLine(key);

                        result.AddFile(
                            "ManifestResource", key,
                            new ArraySegment<byte>(kvp.Value),
                            properties: new Dictionary<string, object> {
                                {"assembly", Path.GetFileNameWithoutExtension(assemblyPath)}
                            }
                        );
                    }
                }
            }
        }
    }
}
