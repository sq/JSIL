using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using JSIL.Compiler;
using JSIL.Utilities;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using EncoderParameter = System.Drawing.Imaging.EncoderParameter;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using ImageLockMode = System.Drawing.Imaging.ImageLockMode;

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

public static class Common {
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern IntPtr LoadImage (IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);
    [DllImport("gdi32.dll")]
    static extern bool DeleteObject (IntPtr hObject);
    [DllImport("gdi32.dll")]
    static extern int GetDIBits (IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, [Out] IntPtr lpvBits, ref BITMAPINFO lpbmi, uint uUsage);
    [DllImport("gdi32.dll", SetLastError = true)]
    static extern IntPtr CreateCompatibleDC (IntPtr hdc);
    [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
    static extern IntPtr SelectObject (IntPtr hdc, IntPtr hgdiobj);

    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFO {
      public Int32 biSize;
      public Int32 biWidth;
      public Int32 biHeight;
      public Int16 biPlanes;
      public Int16 biBitCount;
      public Int32 biCompression;
      public Int32 biSizeImage;
      public Int32 biXPelsPerMeter;
      public Int32 biYPelsPerMeter;
      public Int32 biClrUsed;
      public Int32 biClrImportant;
      public Int32 colors;
    }

    public const int IMAGE_BITMAP = 0;
    public const int LR_LOADFROMFILE = 0x0010;

    static bool WarnedAboutLAME = false, WarnedAboutOggENC = false, WarnedAboutFFMPEG = false;

    public static string MakeXNAColors () {
        var result = new StringBuilder();
        var colorType = typeof(Color);
        var colors = colorType.GetProperties(BindingFlags.Static | BindingFlags.Public);

        result.AppendFormat("// {0}\r\n", JSIL.AssemblyTranslator.GetHeaderText());
        result.AppendLine("(function ($jsilxna) {");
        result.AppendLine("  $jsilxna.colors = [");

        foreach (var color in colors) {
            var colorValue = (Color)color.GetValue(null, null);

            result.AppendFormat("    [\"{0}\", {1}, {2}, {3}, {4}],\r\n", color.Name, colorValue.R, colorValue.G, colorValue.B, colorValue.A);
        }

        result.AppendLine("  ];");
        result.AppendLine("} )( JSIL.GetAssembly(\"JSIL.XNA\") );");

        return result.ToString();
    }

    public static void EnsureDirectoryExists (string directoryName) {
        if (!Directory.Exists(directoryName))
            Directory.CreateDirectory(directoryName);
    }

    private static System.Drawing.Imaging.ImageCodecInfo GetEncoder (System.Drawing.Imaging.ImageFormat format) {
        foreach (System.Drawing.Imaging.ImageCodecInfo codec in System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders()) {
            if (codec.FormatID == format.Guid)
                return codec;
        }

        return null;
    }

    private static byte[] ReadEntireStream (Stream stream) {
        var result = new List<byte>();
        var buffer = new byte[32767];

        while (true) {
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead < buffer.Length) {

                if (bytesRead > 0) {
                    result.Capacity = result.Count + bytesRead;
                    result.AddRange(buffer.Take(bytesRead));
                }

                if (bytesRead <= 0)
                    break;
            } else {
                result.AddRange(buffer);
            }
        }

        return result.ToArray();
    }

    [Serializable]
    public struct CompressResult {
        public int Version;
        public string Key;

        public string SourceFilename;
        public long SourceSize;
        public DateTime SourceTimestamp;

        public string Filename;
        public long Size;
        public DateTime Timestamp;
    }

    public static double DeltaSeconds (DateTime lhs, DateTime rhs) {
        var delta = rhs - lhs;
        return Math.Abs(delta.TotalSeconds);
    }

    private static bool NeedsRebuild (CompressResult? oldResult, int version, string sourcePath, out FileInfo sourceInfo, out FileInfo resultInfo) {
        sourceInfo = new FileInfo(sourcePath);
        resultInfo = null;

        if (oldResult.HasValue) {
            if (
                (oldResult.Value.Version == version) &&
                (oldResult.Value.SourceFilename == sourcePath) &&
                (DeltaSeconds(oldResult.Value.SourceTimestamp, sourceInfo.LastWriteTimeUtc) < 1.0) &&
                (oldResult.Value.SourceSize == sourceInfo.Length)
            ) {

                if (File.Exists(oldResult.Value.Filename)) {
                    resultInfo = new FileInfo(oldResult.Value.Filename);

                    if (
                        (DeltaSeconds(oldResult.Value.Timestamp, resultInfo.LastWriteTimeUtc) < 1.0) &&
                        (oldResult.Value.Size == resultInfo.Length)
                    ) {

                        return false;
                    }
                }
            }
        }

        return true;
    }

    public static string FFMPEGDecode (string sourcePath, ProfileSettings settings) {
        var outputPath = Path.GetTempFileName();
        var ffmpegPath = Path.GetFullPath(settings.Variables.ExpandPath(@"%jsildirectory%\..\Upstream\FFMPEG\ffmpeg.exe", false));
        if (!File.Exists(ffmpegPath)) {
            if (!WarnedAboutFFMPEG) {
                WarnedAboutFFMPEG = true;
                Console.Error.WriteLine(@"// Warning: ffmpeg.exe was not found at ..\Upstream\FFMPEG\ffmpeg.exe. Can't decode '{0}'.", Path.GetFileName(sourcePath));
            }

            return null;
        }

        var arguments = String.Format("-v error -i \"{0}\" -f wav \"{1}\"", sourcePath, outputPath);
        string stderr;
        byte[] stdout;

        File.Delete(outputPath);
        RunProcess(
            ffmpegPath, arguments, null, out stderr, out stdout
        );

        if (!String.IsNullOrWhiteSpace(stderr))
            Console.Error.WriteLine("// Error output from ffmpeg: {0}", stderr);
        if (stdout.Length > 0)
            Console.Error.WriteLine("// Output from ffmpeg: {0}", Encoding.ASCII.GetString(stdout));

        if (File.Exists(outputPath))
            return outputPath;
        else
            return null;
    }

    public static bool CompressMP3 (string sourcePath, string outputPath, ProfileSettings settings) {
        var lamePath = Path.GetFullPath(settings.Variables.ExpandPath(@"%jsildirectory%\..\Upstream\LAME\lame.exe", false));
        if (!File.Exists(lamePath)) {
            if (!WarnedAboutLAME) {
                WarnedAboutLAME = true;
                Console.Error.WriteLine(@"// Warning: lame.exe was not found at ..\Upstream\LAME\lame.exe. Can't encode MP3.");
            }

            return false;
        }

        var arguments = String.Format("--quiet {0} \"{1}\" \"{2}\"", settings["MP3Quality"], sourcePath, outputPath);
        string stderr;
        byte[] stdout;

        if (File.Exists(outputPath))
            File.Delete(outputPath);

        RunProcess(
            lamePath, arguments, null, out stderr, out stdout
        );

        if (!String.IsNullOrWhiteSpace(stderr))
            Console.Error.WriteLine("// Error output from lame: {0}", stderr);
        if (stdout.Length > 0)
            Console.Error.WriteLine("// Output from lame: {0}", Encoding.ASCII.GetString(stdout));

        return File.Exists(outputPath);
    }

    public static bool CompressOGG (string sourcePath, string outputPath, ProfileSettings settings) {
        var oggencPath = Path.GetFullPath(settings.Variables.ExpandPath(@"%jsildirectory%\..\Upstream\OggEnc\oggenc2.exe", false));
        if (!File.Exists(oggencPath)) {
            if (!WarnedAboutOggENC) {
                WarnedAboutOggENC = true;
                Console.Error.WriteLine(@"// Warning: oggenc2.exe was not found at ..\Upstream\OggEnc\oggenc2.exe. Can't encode OGG.");
            }

            return false;
        }

        var arguments = String.Format("-Q {0} \"{1}\" -o \"{2}\"", settings["OGGQuality"], sourcePath, outputPath);
        string stderr;
        byte[] stdout;

        if (File.Exists(outputPath))
            File.Delete(outputPath);

        RunProcess(
            oggencPath, arguments, null, out stderr, out stdout
        );

        if (!String.IsNullOrWhiteSpace(stderr))
            Console.Error.WriteLine("// Error output from oggenc2: {0}", stderr);
        if (stdout.Length > 0)
            Console.Error.WriteLine("// Output from oggenc2: {0}", Encoding.ASCII.GetString(stdout));

        return File.Exists(outputPath);
    }

    public static IEnumerable<CompressResult> CompressAudio (
        string fileName, string sourceFolder, string outputFolder, ProfileSettings settings, Dictionary<string, CompressResult> existingJournal
    ) {
        const int CompressVersion = 4;

        EnsureDirectoryExists(outputFolder);

        var sourcePath = Path.Combine(sourceFolder, fileName);
        FileInfo sourceInfo, resultInfo;
        CompressResult existingJournalEntry;

        var outputPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileName));

        string sourceWave = null;
        bool sourceWaveIsTemporary = false;

        var originalExtension = Path.GetExtension(fileName).ToLower();

        try {
            switch (originalExtension) {
                case ".wma":
                case ".mp3":
                case ".aac":
                    sourceWave = FFMPEGDecode(sourcePath, settings);
                    if (sourceWave == null)
                        yield break;

                    sourceWaveIsTemporary = true;

                    break;

                case ".wav":
                    sourceWave = sourcePath;
                    break;
            }

            var mp3Path = outputPath + ".mp3";
            var oggPath = outputPath + ".ogg";
            bool makeMp3 = false, makeOgg = false;

            if (
                existingJournal.TryGetValue(sourcePath + ":mp3", out existingJournalEntry)
            ) {
                if (NeedsRebuild(existingJournalEntry, CompressVersion, sourcePath, out sourceInfo, out resultInfo))
                    makeMp3 = true;
                else
                    yield return existingJournalEntry;
            } else {
                sourceInfo = new FileInfo(sourcePath);
                makeMp3 = true;
            }

            if (makeMp3) {
                if (CompressMP3(sourceWave, mp3Path, settings))
                    yield return MakeCompressResult(CompressVersion, "mp3", mp3Path, sourcePath, sourceInfo);
            }

            if (
                existingJournal.TryGetValue(sourcePath + ":ogg", out existingJournalEntry)
            ) {
                if (NeedsRebuild(existingJournalEntry, CompressVersion, sourcePath, out sourceInfo, out resultInfo))
                    makeOgg = true;
                else
                    yield return existingJournalEntry;
            } else {
                sourceInfo = new FileInfo(sourcePath);
                makeOgg = true;
            }

            if (makeOgg) {
                if (CompressOGG(sourceWave, oggPath, settings))
                    yield return MakeCompressResult(CompressVersion, "ogg", oggPath, sourcePath, sourceInfo);
            }
        } finally {
            if (sourceWaveIsTemporary)
                File.Delete(sourceWave);
        }
    }

    private static System.Drawing.Bitmap LoadBitmap (string filename, out bool hasAlphaChannel, out System.Drawing.Color? existingColorKeyColor) {
        existingColorKeyColor = null;

        switch (Path.GetExtension(filename).ToLower()) {
            case ".bmp":
                int bitsPerPixel;

                using (var bitmapHandle = File.OpenRead(filename)) {
                    var buffer = new byte[4];
                    if (bitmapHandle.Read(buffer, 0, 2) != 2)
                        throw new InvalidOperationException("Could not read bitmap header");

                    if ((buffer[0] != 'B') || (buffer[1] != 'M'))
                        throw new InvalidOperationException("Invalid bitmap header");

                    bitmapHandle.Seek(28, SeekOrigin.Begin);
                    if (bitmapHandle.Read(buffer, 0, 4) != 4)
                        throw new InvalidOperationException("Could not read bitmap bits per pixel");

                    bitsPerPixel = BitConverter.ToInt32(buffer, 0);

                    if ((bitsPerPixel < 0) || (bitsPerPixel > 32))
                        throw new InvalidOperationException("Unsupported bitmap format");
                }

                if (bitsPerPixel == 32) {
                    var hImage = LoadImage(IntPtr.Zero, filename, IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);
                    try {
                        var texture = System.Drawing.Image.FromHbitmap(hImage);
                        // Do an elaborate song and dance to extract the alpha channel from the PNG, because
                        //  GDI+ is too utterly shitty to do that itself
                        var dc = CreateCompatibleDC(IntPtr.Zero);

                        var newImage = new System.Drawing.Bitmap(
                            texture.Width, texture.Height, PixelFormat.Format32bppArgb
                        );
                        var bits = newImage.LockBits(
                            new System.Drawing.Rectangle(0, 0, texture.Width, texture.Height),
                            ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb
                        );

                        var info = new BITMAPINFO {
                            biBitCount = 32,
                            biClrImportant = 0,
                            biClrUsed = 0,
                            biCompression = 0,
                            biHeight = -texture.Height,
                            biPlanes = 1,
                            biSizeImage = bits.Stride * bits.Height,
                            biWidth = bits.Width,
                        };
                        info.biSize = Marshal.SizeOf(info);

                        var rv = GetDIBits(dc, hImage, 0, (uint)texture.Height, bits.Scan0, ref info, 0);

                        newImage.UnlockBits(bits);

                        DeleteObject(dc);

                        texture.Dispose();
                        hasAlphaChannel = true;
                        return newImage;
                    } finally {
                        DeleteObject(hImage);
                    }
                } else {
                    hasAlphaChannel = false;
                    return (System.Drawing.Bitmap)System.Drawing.Image.FromFile(filename);
                }

            default: {
                var result = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(filename, true);

                switch (result.PixelFormat) {
                    case PixelFormat.Format8bppIndexed:
                    case PixelFormat.Format4bppIndexed:
                    case PixelFormat.Format1bppIndexed:
                    case PixelFormat.Indexed:
                        var palette = result.Palette;

                        for (var i = 0; i < palette.Entries.Length; i++) {
                            if (palette.Entries[i].A < 255) {
                                existingColorKeyColor = palette.Entries[i];
                                break;
                            }
                        }

                        hasAlphaChannel = (existingColorKeyColor.HasValue);
                        break;

                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                        hasAlphaChannel = true;
                        break;

                    default:
                        hasAlphaChannel = false;
                        break;
                }

                return result;
            }
        }
    }

    public static unsafe void MakeChannelImage (System.Drawing.Bitmap bitmap, int colorChannel) {
        var lockRect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var lockedBits = bitmap.LockBits(lockRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

        for (int y = 0; y < bitmap.Height; y++) {
            var scanStart = lockedBits.Scan0 + (lockedBits.Stride * y);

            for (int x = 0; x < bitmap.Width; x++) {
                var ptr = (Color *)(scanStart + (x * 4));
                var current = *ptr;

                switch (colorChannel) {
                    case 0:
                        *ptr = new Color(0, 0, current.B, current.A);
                        break;
                    case 1:
                        *ptr = new Color(0, current.G, 0, current.A);
                        break;
                    case 2:
                        *ptr = new Color(current.R, 0, 0, current.A);
                        break;
                    case 3:
                        *ptr = new Color(0, 0, 0, current.A);
                        break;
                }
            }
        }

        bitmap.UnlockBits(lockedBits);
    }

    public static void CompressImage (
        string imageName, string sourceFolder,
        string outputFolder, ProfileSettings settings, 
        Dictionary<string, ProjectMetadata> itemMetadata,
        CompressResult? oldResult, Action<CompressResult?> writeResult,
        int? colorChannel = null
    ) {
        const int CompressVersion = 5;

        EnsureDirectoryExists(outputFolder);

        var sourcePath = Path.Combine(sourceFolder, imageName);
        FileInfo sourceInfo, resultInfo;

        bool forceJpeg = settings.Files[imageName].Contains("forcejpeg");
        bool generateChannels = settings.Files[imageName].Contains("generatechannels");

        if (!NeedsRebuild(oldResult, CompressVersion, sourcePath, out sourceInfo, out resultInfo)) {
            writeResult(oldResult);
            return;
        }

        if (generateChannels && !colorChannel.HasValue) {
            for (int i = 0; i < 4; i++) {
                CompressImage(
                    imageName, sourceFolder, outputFolder,
                    settings, itemMetadata, null,
                    writeResult, i
                );
            }
        }

        var outputPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(imageName));
        var justCopy = true;

        Action<System.Drawing.Bitmap, String> saveJpeg = (bitmap, path) => {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(
                System.Drawing.Imaging.Encoder.Quality,
                Convert.ToInt64(settings["JPEGQuality"])
            );
            bitmap.Save(path, encoder, encoderParameters);
        };

        bool colorKey = true;
        var colorKeyColor = System.Drawing.Color.FromArgb(255, 255, 0, 255);

        if (itemMetadata.ContainsKey("ProcessorParameters_ColorKeyEnabled")) {
            colorKey = Convert.ToBoolean(itemMetadata["ProcessorParameters_ColorKeyEnabled"].EvaluatedValue);
        }

        if (itemMetadata.ContainsKey("ProcessorParameters_ColorKeyColor")) {
            var parts = itemMetadata["ProcessorParameters_ColorKeyColor"].EvaluatedValue.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            colorKeyColor = System.Drawing.Color.FromArgb(
                Convert.ToInt32(parts[3]),
                Convert.ToInt32(parts[0]),
                Convert.ToInt32(parts[1]),
                Convert.ToInt32(parts[2])
            );
        }

        justCopy &= !colorKey;

        justCopy &= !colorChannel.HasValue;

        if (forceJpeg) {
            outputPath += ".jpg";

            bool temp;
            System.Drawing.Color? temp2;
            using (var img = LoadBitmap(sourcePath, out temp, out temp2))
                saveJpeg(img, outputPath);
        } else if (justCopy) {
            outputPath += Path.GetExtension(sourcePath);

            CopiedOutputGatherer.CopyFile(sourcePath, outputPath, true);
        } else {
            bool hasAlphaChannel;
            System.Drawing.Color? existingColorKey;

            using (var img = LoadBitmap(sourcePath, out hasAlphaChannel, out existingColorKey)) {
                if (colorChannel.HasValue) {
                    var channelNames = new string[] { "r", "g", "b", "a" };
                    outputPath += "_" + channelNames[colorChannel.Value];
                }

                if (hasAlphaChannel || colorKey || existingColorKey.HasValue)
                    outputPath += ".png";
                else
                    outputPath += ".jpg";

                if (existingColorKey.HasValue)
                    img.MakeTransparent(existingColorKey.Value);

                if (colorKey)
                    img.MakeTransparent(colorKeyColor);

                if (colorChannel.HasValue)
                    MakeChannelImage(img, colorChannel.Value);

                if (hasAlphaChannel || colorKey || existingColorKey.HasValue)
                    img.Save(outputPath, ImageFormat.Png);
                else
                    saveJpeg(img, outputPath);
            }
        }

        bool usePNGQuant = Convert.ToBoolean(settings["UsePNGQuant"]);
        var pngQuantParameters = String.Format(
            "{0} {1}", 
            settings["PNGQuantColorCount"],
            settings["PNGQuantOptions"]
        );
        var pngQuantPath = Path.Combine(
            Path.GetDirectoryName(JSIL.Internal.Util.GetPathOfAssembly(
                typeof(JSIL.Compiler.Configuration).Assembly
            )),
            "PNGQuant.exe"
        );

        if (
            usePNGQuant && 
            (Path.GetExtension(outputPath).ToLower() == ".png") &&
            !settings.Files[imageName].Contains("nopngquant")
        ) {
            byte[] result;
            string stderr;

            var exitCode = RunProcess(
                pngQuantPath, pngQuantParameters,
                File.ReadAllBytes(outputPath),
                out stderr, out result
            );

            var outputLength = new FileInfo(outputPath).Length;

            if (!String.IsNullOrWhiteSpace(stderr) || (outputLength <= 0) || (exitCode != 0)) {
                Console.Error.WriteLine("// PNGquant failed with error output: {0}", stderr);
                Console.Error.WriteLine("// Using uncompressed PNG.");
            } else {
                File.WriteAllBytes(outputPath, result);
            }
        }

        string key = null;
        if (colorChannel.HasValue)
            key = colorChannel.Value.ToString();

        writeResult(MakeCompressResult(CompressVersion, key, outputPath, sourcePath, sourceInfo));
    }

    private static int RunProcess (string filename, string parameters, byte[] stdin, out string stderr, out byte[] stdout) {
        var psi = new ProcessStartInfo(filename, parameters);

        psi.WorkingDirectory = Path.GetDirectoryName(filename);
        psi.UseShellExecute = false;
        psi.RedirectStandardInput = true;
        psi.RedirectStandardError = true;
        psi.RedirectStandardOutput = true;

        using (var process = Process.Start(psi)) {
            var stdinStream = process.StandardInput.BaseStream;
            var stderrStream = process.StandardError.BaseStream;

            if (stdin != null) {
                ThreadPool.QueueUserWorkItem(
                    (_) => {
                        if (stdin != null) {
                            stdinStream.Write(
                                stdin, 0, stdin.Length
                            );
                            stdinStream.Flush();
                        }

                        stdinStream.Close();
                    }, null
                );
            }

            var temp = new string[1] { null };
            ThreadPool.QueueUserWorkItem(
                (_) => {
                    var text = Encoding.ASCII.GetString(ReadEntireStream(stderrStream));
                    temp[0] = text;
                }, null
            );

            stdout = ReadEntireStream(process.StandardOutput.BaseStream);

            process.WaitForExit();
            stderr = temp[0];

            var exitCode = process.ExitCode;

            process.Close();

            return exitCode;
        }
    }

    private static CompressResult MakeCompressResult (int version, string key, string outputPath, string sourcePath, FileInfo sourceInfo) {
        var resultInfo = new FileInfo(outputPath);
        return new CompressResult {
            Version = version,
            Key = key,

            SourceFilename = sourcePath,
            SourceSize = sourceInfo.Length,
            SourceTimestamp = sourceInfo.LastWriteTimeUtc,

            Filename = outputPath,
            Size = resultInfo.Length,
            Timestamp = resultInfo.LastWriteTimeUtc
        };
    }

    public static string GetActualCase (string pathname) {
        var directoryName = Path.GetDirectoryName(pathname);
        var fileName = Path.GetFileName(pathname);

        string result = null;

        if (Directory.Exists(directoryName)) {
            result = Directory.GetFiles(directoryName, fileName).FirstOrDefault();

            if (result == null)
                result = Directory.GetDirectories(directoryName, fileName).FirstOrDefault();
        }

        if (result == null) {
            var directoryParent = Path.GetDirectoryName(directoryName);
            var directorySubName = Path.GetFileName(directoryName);

            result = Path.Combine(
                Directory.GetDirectories(directoryParent, directorySubName).FirstOrDefault() ?? directoryName,
                fileName
            );
        }

        return result;
    }

    public static void ProcessContentProjects (
        VariableSet variables,
        Configuration configuration, 
        JSIL.SolutionBuilder.BuildResult buildResult, 
        HashSet<string> contentProjectsProcessed
    ) {
        var settings = new ProfileSettings(variables, configuration);

        var contentOutputDirectory =
            configuration.ProfileSettings.GetValueOrDefault("ContentOutputDirectory", null) as string;

        if (contentOutputDirectory == null) {
            Console.Error.WriteLine("// ContentOutputDirectory is not set. Skipping XNA content processing.");
            return;
        }

        contentOutputDirectory = variables.ExpandPath(contentOutputDirectory, false);

        using (var projectCollection = new ProjectCollection()) {
            var contentProjects = buildResult.ProjectsBuilt.Where(
                (project) => project.File.EndsWith(".contentproj")
                ).ToArray();

            var builtXNBs =
                (from bi in buildResult.AllItemsBuilt
                 where bi.OutputPath.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase)
                 select bi).Distinct().ToArray();

            var forceCopyImporters = new HashSet<string>(
                ((IEnumerable)configuration.ProfileSettings["ForceCopyXNBImporters"]).Cast<string>()
            );

            var forceCopyProcessors = new HashSet<string>(
                ((IEnumerable)configuration.ProfileSettings["ForceCopyXNBProcessors"]).Cast<string>()
            );

            Dictionary<string, Common.CompressResult> existingJournal = new Dictionary<string, CompressResult>();
            Common.CompressResult? existingJournalEntry;
            var jss = new JavaScriptSerializer();

            foreach (var builtContentProject in contentProjects) {
                var contentProjectPath = builtContentProject.File;

                if (contentProjectsProcessed.Contains(contentProjectPath))
                    continue;

                var journal = new List<Common.CompressResult>();
                var journalPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JSIL",
                                               contentProjectPath.Replace("\\", "_").Replace(":", ""));

                Common.EnsureDirectoryExists(Path.GetDirectoryName(journalPath));
                if (File.Exists(journalPath)) {
                    var journalEntries = jss.Deserialize<Common.CompressResult[]>(File.ReadAllText(journalPath));

                    if (journalEntries != null) {
                        existingJournal = new Dictionary<string, CompressResult>();

                        foreach (var je in journalEntries) {
                            string uniqueKey;

                            if (je.Key != null)
                                uniqueKey = je.SourceFilename + ":" + je.Key;
                            else
                                uniqueKey = je.SourceFilename;

                            if (existingJournal.ContainsKey(uniqueKey))
                                Console.Error.WriteLine("// Duplicate content build journal entry for '{0}'! Ignoring...", uniqueKey);
                            else
                                existingJournal.Add(uniqueKey, je);
                        }
                    }
                }

                contentProjectsProcessed.Add(contentProjectPath);
                Console.Error.WriteLine("// Processing content project '{0}' ...", contentProjectPath);

                var project = projectCollection.LoadProject(contentProjectPath);
                var projectProperties = project.Properties.ToDictionary(
                    (p) => p.Name
                    );

                Project parentProject = null, rootProject = null;
                Dictionary<string, ProjectProperty> parentProjectProperties = null, rootProjectProperties = null;

                if (builtContentProject.Parent != null) {
                    parentProject = projectCollection.LoadProject(builtContentProject.Parent.File);
                    parentProjectProperties = parentProject.Properties.ToDictionary(
                        (p) => p.Name
                        );
                }

                {
                    var parent = builtContentProject.Parent;

                    while (parent != null) {
                        var nextParent = parent.Parent;
                        if (nextParent != null) {
                            parent = nextParent;
                            continue;
                        }

                        rootProject = projectCollection.LoadProject(parent.File);
                        rootProjectProperties = rootProject.Properties.ToDictionary(
                            (p) => p.Name
                            );
                        break;
                    }
                }

                var contentProjectDirectory = Path.GetDirectoryName(contentProjectPath);
                var myvars = variables.Clone();
                myvars.Add("ContentProjectDirectory", contentProjectDirectory);

                var localOutputDirectory =
                    myvars.ExpandPath(contentOutputDirectory, false);
                var caseFixedLocalOutputDirectory = GetActualCase(localOutputDirectory);

                EnsureDirectoryExists(localOutputDirectory);

                var contentManifestPath = Path.Combine(
                    localOutputDirectory, Path.GetFileName(contentProjectPath) + ".manifest.js"
                );
                var contentManifest = new ContentManifestWriter(
                    contentManifestPath, Path.GetFileName(contentProjectPath)
                );

                Action<string, string, Dictionary<string, object>> logOutput =
                (type, filename, properties) => {
                    var fileInfo = new FileInfo(filename);

                    var caseFixedPath = GetActualCase(filename);

                    var localPath = caseFixedPath.Replace(caseFixedLocalOutputDirectory, "");
                    if (localPath.StartsWith("\\"))
                        localPath = localPath.Substring(1);

                    Console.WriteLine(localPath);

                    string propertiesObject;

                    if (properties == null) {
                        properties = new Dictionary<string, object> {
                            { "sizeBytes", fileInfo.Length }
                        };
                    }

                    contentManifest.Add(
                        type, localPath, properties
                    );
                };

                Action<ProjectItem> copyRawFile =
                (item) =>
                    CopyRawFile(localOutputDirectory, logOutput, item, contentProjectDirectory);

                Action<ProjectItem, string, string> copyRawXnb =
                (item, xnbPath, type) =>
                    CopyXNB(logOutput, type, localOutputDirectory, xnbPath, item);

                foreach (var item in project.Items) {
                    switch (item.ItemType) {
                        case "Reference":
                        case "ProjectReference":
                            continue;
                        case "Compile":
                        case "None":
                            break;
                        default:
                            continue;
                    }

                    var itemOutputDirectory = FixupOutputDirectory(
                        localOutputDirectory,
                        Path.GetDirectoryName(item.EvaluatedInclude)
                    );

                    var sourcePath = Path.Combine(contentProjectDirectory, item.EvaluatedInclude);
                    var metadata = item.DirectMetadata.ToDictionary((md) => md.Name);

                    if (item.ItemType == "None") {
                        string copyToOutputDirectory = "Always";
                        ProjectMetadata temp2;

                        if (metadata.TryGetValue("CopyToOutputDirectory", out temp2)) {
                            copyToOutputDirectory = temp2.EvaluatedValue;
                        }

                        switch (copyToOutputDirectory) {
                            case "Always":
                            case "PreserveNewest":
                                copyRawFile(item);
                                break;
                            default:
                                break;
                        }

                        continue;
                    }

                    var importerName = metadata["Importer"].EvaluatedValue;
                    var processorName = metadata["Processor"].EvaluatedValue;
                    string xnbPath = null;

                    Common.CompressResult temp;
                    if (existingJournal.TryGetValue(sourcePath, out temp))
                        existingJournalEntry = temp;
                    else
                        existingJournalEntry = null;

                    var evaluatedXnbPath = item.EvaluatedInclude.Replace(Path.GetExtension(item.EvaluatedInclude), ".xnb");
                    var matchingBuiltPaths = (from bi in builtXNBs
                                              where
                                                  bi.OutputPath.Contains(":\\") &&
                                                  bi.OutputPath.EndsWith("\\" + evaluatedXnbPath)
                                              select bi.Metadata["FullPath"]).Distinct().ToArray();

                    if (matchingBuiltPaths.Length == 0) {
                    } else if (matchingBuiltPaths.Length > 1) {
                        throw new AmbiguousMatchException("Found multiple outputs for asset " + evaluatedXnbPath);
                    } else {
                        xnbPath = matchingBuiltPaths[0];
                        if (!File.Exists(xnbPath))
                            throw new FileNotFoundException("Asset " + xnbPath + " not found.");
                    }

                    if (settings.Files[item.EvaluatedInclude].Contains("usexnb")) {
                        copyRawXnb(item, xnbPath, "XNB");
                    } else if (
                        forceCopyProcessors.Contains(processorName) ||
                        forceCopyImporters.Contains(importerName)
                    ) {
                        copyRawXnb(item, xnbPath, "XNB");
                    } else {
                        switch (processorName) {
                            case "XactProcessor":
                                Common.ConvertXactProject(
                                    item.EvaluatedInclude, contentProjectDirectory, itemOutputDirectory,
                                    settings, existingJournal,
                                    journal, logOutput
                                );
                                continue;

                            case "FontTextureProcessor":
                            case "FontDescriptionProcessor":
                                copyRawXnb(item, xnbPath, "SpriteFont");
                                continue;

                            case "SoundEffectProcessor":
                            case "SongProcessor":
                                journal.AddRange(CompressAudioGroup(
                                    item.EvaluatedInclude, contentProjectDirectory, itemOutputDirectory,
                                    settings, existingJournal, logOutput
                                ));
                                continue;

                            case "TextureProcessor":
                                if (Path.GetExtension(sourcePath).ToLower() == ".tga") {
                                    copyRawXnb(item, xnbPath, "Texture2D");
                                    continue;
                                }

                                Common.CompressImage(
                                    item.EvaluatedInclude, contentProjectDirectory, itemOutputDirectory,
                                    settings, metadata, existingJournalEntry,
                                    (result) => {
                                        if (result.HasValue) {
                                            journal.Add(result.Value);
                                            logOutput("Image", result.Value.Filename, null);
                                        }
                                    }
                                );

                                continue;

                            case "EffectProcessor":
                                copyRawXnb(item, xnbPath, "XNB");
                                continue;
                        }

                        switch (importerName) {
                            case "XmlImporter":
                                copyRawXnb(item, xnbPath, "XNB");
                                break;

                            default:
                                Console.Error.WriteLine(
                                    "// Can't process '{0}': importer '{1}' and processor '{2}' both unsupported.",
                                    item.EvaluatedInclude, importerName, processorName);
                                break;
                        }
                    }
                }

                contentManifest.Dispose();

                File.WriteAllText(
                    journalPath, jss.Serialize(journal).Replace("{", "\r\n{")
                );
            }

            if (contentProjects.Length > 0)
                Console.Error.WriteLine("// Done processing content.");
        }
    }

    private static void CopyRawFile (
        string localOutputDirectory, Action<string, string, Dictionary<string, object>> logOutput, ProjectItem item,                                    
        string contentProjectDirectory
    ) {
        var sourcePath = Path.Combine(
            contentProjectDirectory,
            item.EvaluatedInclude
            );
        var outputPath = FixupOutputDirectory(
            localOutputDirectory,
            item.EvaluatedInclude
            );

        Common.EnsureDirectoryExists(
            Path.GetDirectoryName(outputPath));

        try {
            CopiedOutputGatherer.CopyFile(sourcePath, outputPath, true);
            logOutput("File", outputPath, null);
        } catch (Exception exc) {
            Console.Error.WriteLine("// Could not copy '{0}'! Error: {1}", item.EvaluatedInclude, exc.Message);
        }
    }

    private static void DecompressXNB (string sourcePath, string destinationPath) {
        Console.Error.Write("// Decompressing {0}... ", Path.GetFileName(sourcePath));

        var assetBytes = File.ReadAllBytes(sourcePath);

        // Decompress the contents of the asset.
        var decompressedSize = BitConverter.ToInt32(assetBytes, 10);
        var tDecompressStream = typeof(ContentReader).Assembly.GetType("Microsoft.Xna.Framework.Content.DecompressStream", true);

        var baseStream = new MemoryStream(assetBytes, 14, assetBytes.Length - 14, false);
        var decompressor = (Stream)Activator.CreateInstance(
            tDecompressStream, (Stream)baseStream, (int)baseStream.Length, (int)decompressedSize
        );

        var resultBytesStream = new MemoryStream();
        var resultBytesWriter = new BinaryWriter(resultBytesStream);
        resultBytesWriter.Write(assetBytes, 0, 5);
        resultBytesWriter.Write((byte)(assetBytes[5] & ~0x80));
        resultBytesWriter.Write(decompressedSize + 10);
        var buf = new byte[decompressedSize];

        int bytesToDecompress = buf.Length, bytesDecompressed = 0, decompressOffset = 0;

        while ((bytesDecompressed = decompressor.Read(buf, decompressOffset, bytesToDecompress)) > 0) {
            decompressOffset += bytesDecompressed;
            bytesToDecompress -= bytesDecompressed;
        }

        resultBytesWriter.Write(buf, 0, buf.Length);
        resultBytesWriter.Flush();
        using (var stream = File.Open(destinationPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) {
            stream.SetLength(resultBytesStream.Length);
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(resultBytesStream.GetBuffer(), 0, (int)resultBytesStream.Length);
        }

        Console.Error.WriteLine("done.");
    }

    private static void CopyXNB (Action<string, string, Dictionary<string, object>> logOutput, string type, string localOutputDirectory, string xnbPath, ProjectItem item) {
        if (xnbPath == null)
            throw new FileNotFoundException("Asset " + item.EvaluatedInclude + " was not built.");

        bool isCompressed = false;
        using (var stream = File.OpenRead(xnbPath)) {
            var bytes = new byte[6];
            stream.Read(bytes, 0, bytes.Length);
            isCompressed = (bytes[5] & 0x80) == 0x80;
        }

        var outputPath = FixupOutputDirectory(
            localOutputDirectory,
            item.EvaluatedInclude.Replace(
                Path.GetExtension(item.EvaluatedInclude),
                ".xnb")
            );

        Common.EnsureDirectoryExists(
            Path.GetDirectoryName(outputPath));

        try {
            if (isCompressed)
                DecompressXNB(xnbPath, outputPath);
            else
                CopiedOutputGatherer.CopyFile(xnbPath, outputPath, true);
            logOutput(type, outputPath, null);
        } catch (Exception exc) {
            Console.Error.WriteLine("// Could not copy '{0}'! Error: {1}", item.EvaluatedInclude, exc.Message);
        }
    }

    private static IEnumerable<CompressResult> CompressAudioGroup(
        string fileName, string contentProjectDirectory, string itemOutputDirectory,
        ProfileSettings settings, Dictionary<string, CompressResult> existingJournal, 
        Action<string, string, Dictionary<string, object>> logOutput
    ) {
        var fileSettings = settings.Files[fileName];

        var results = Common.CompressAudio(
            fileName, contentProjectDirectory, itemOutputDirectory,
            settings, existingJournal
        ).ToArray();

        if (results.Length == 0) {
            Console.Error.WriteLine("// No audio files generated for '{0}'. Skipping.", fileName);
            yield break;
        }

        var formats = new List<string>();
        var properties = new Dictionary<string, object> {
            {"formats", formats},
            {"sizeBytes", results.Max((r) => r.Size)}
        };

        if (fileSettings.Contains("stream"))
            properties.Add("stream", true);

        foreach (var result in results)
            formats.Add(Path.GetExtension(result.Filename));

        var prefixName = FixupOutputDirectory(
            Path.GetDirectoryName(results.First().Filename),
            Path.GetFileNameWithoutExtension(results.First().Filename)
        );

        logOutput("Sound", prefixName, properties);

        foreach (var result in results)
            yield return result;
    }

    [Serializable]
    public class CueEntry {
        public string Name;
        public string[] Sounds;
    }

    [Serializable]
    public class TrackEntry {        
        public string Name;
        public Dictionary<string, object>[] Events;
    }

    [Serializable]
    public class SoundEntry {
        public string Name;
        public string Category;
        public TrackEntry[] Tracks;
    }

    private static Dictionary<string, object> TranslateEvent (
        Xap.EventBase evt, HashSet<string> waveNames
    ) {
        return new Dictionary<string, object>() {
            {"Type", evt.GetType().Name}
        };
    }

    private static Dictionary<string, object> TranslateEvent (
        Xap.PlayWaveEvent evt, HashSet<string> waveNames 
    ) {
        var waveEntry = evt.m_waveEntries.First();

        waveNames.Add(waveEntry.m_entryName);

        return new Dictionary<string, object>() {
            {"Type", evt.GetType().Name},
            {"WaveBank", waveEntry.m_bankName},
            {"Wave", waveEntry.m_entryName},
            {"LoopCount", evt.m_loopCount}
        };
    }

    public static string FixupOutputDirectory (string parentDirectory, string subDirectory) {
        if (String.IsNullOrWhiteSpace(parentDirectory))
            return subDirectory.Replace("..\\", "").Replace("\\..", "");

        bool retried = false;

    retry:
        var outputDirectory = Path.Combine(parentDirectory, subDirectory);

        var parentNormalized = Path.GetFullPath(parentDirectory).ToLowerInvariant();
        var outputNormalized = Path.GetFullPath(outputDirectory).ToLowerInvariant();

        if (outputNormalized.IndexOf(parentNormalized) != 0) {
            if (retried)
                throw new Exception("Invalid output directory: " + subDirectory);

            subDirectory = subDirectory.Replace("..\\", "").Replace("\\..", "");
            retried = true;
            goto retry;
        }

        return outputDirectory;
    }

    private static void ConvertXactProject (
        string projectFile, string sourceFolder,
        string outputFolder, ProfileSettings settings, 
        Dictionary<string, CompressResult> existingJournal, List<CompressResult> journal, 
        Action<string, string, Dictionary<string, object>> logOutput
    ) {
        const int CompressVersion = 2;

        Console.Error.WriteLine("// Processing {0}...", projectFile);

        var projectSubdir = Path.GetDirectoryName(projectFile);
        var projectPath = Path.Combine(sourceFolder, projectFile);
        var projectDirectory = Path.GetDirectoryName(projectPath);
        var projectInfo = new FileInfo(projectPath);

        var project = new Xap.Project();
        project.Parse(File.ReadAllLines(projectPath));

        var waveManifest = new Dictionary<string, string>();
        var jss = new JavaScriptSerializer();

        foreach (var waveBank in project.m_waveBanks) {

            foreach (var wave in waveBank.m_waves) {
                var waveFileName = wave.m_fileName;

                waveFileName = waveFileName.ToLower().Replace(projectDirectory.ToLower(), "");
                if (waveFileName.StartsWith("\\"))
                    waveFileName = waveFileName.Substring(1);

                var waveFolder = Path.GetDirectoryName(waveFileName);

                var waveOutputFolder = FixupOutputDirectory(outputFolder, waveFolder);

                waveManifest[wave.m_name] = FixupOutputDirectory(
                    projectSubdir, 
                    waveFileName
                        .Replace(Path.GetExtension(waveFileName), "")
                );

                journal.AddRange(CompressAudioGroup(
                    Path.Combine(projectSubdir, waveFileName), sourceFolder, 
                    waveOutputFolder, settings, existingJournal, logOutput
                ));
            }
        }

        foreach (var soundBank in project.m_soundBanks) {
            var waveNames = new HashSet<string>();

            var soundEntries =
                (from sound in soundBank.m_sounds
                 select new SoundEntry {
                     Name = sound.m_name,
                     Category = (sound.m_category == null) ? null : sound.m_category.Name,
                     Tracks = (
                        from track in sound.m_tracks
                        select new TrackEntry {
                            Name = track.m_name,
                            Events = (
                                from evt in track.m_events
                                select (Dictionary<string, object>)TranslateEvent(
                                    evt as dynamic, waveNames
                                )
                            ).ToArray()
                        }
                     ).ToArray()
                 }).ToArray();

            var cueEntries = 
                (from cue in soundBank.m_cues
                select new CueEntry {
                    Name = cue.m_name,
                    Sounds = (
                        from se in cue.m_soundEntries select se.m_name
                    ).ToArray()
                }).ToArray();
                
            var soundBankJson = jss.Serialize(
                new Dictionary<string, object> {
                    {"Name", soundBank.m_name},
                    {"Cues", cueEntries},
                    {"Sounds", soundEntries},
                    {"Waves", waveManifest}
                }
            );

            var soundBankPath = Path.Combine(outputFolder, soundBank.m_name + ".soundBank");
            File.WriteAllText(
                soundBankPath, soundBankJson
            );

            journal.Add(MakeCompressResult(
                CompressVersion, null, soundBankPath, projectPath, projectInfo
            ));
            logOutput("SoundBank", soundBankPath, null);
        }

        Console.Error.WriteLine("// Done processing {0}.", projectFile);
    }

    public class ProfileSettings {
        public readonly VariableSet Variables;
        public readonly Configuration Configuration;
        public readonly FileSettings Files;

        private readonly Dictionary<string, object> Dict;

        public ProfileSettings (VariableSet variables, Configuration configuration) {
            Variables = variables;
            Configuration = configuration;
            Dict = Configuration.ProfileSettings;
            Files = new FileSettings(this);

            Dict.SetDefault("ContentOutputDirectory", null);
            Dict.SetDefault("JPEGQuality", 90);
            Dict.SetDefault("MP3Quality", "-V 3");
            Dict.SetDefault("OGGQuality", "-q 6");

            Dict.SetDefault("UsePNGQuant", false);
            Dict.SetDefault("PNGQuantColorCount", 256);
            Dict.SetDefault("PNGQuantOptions", "");

            Dict.SetDefault("FileSettings", new Dictionary<string, object>());

            Dict.SetDefault("ForceCopyXNBImporters", new string[0]);
            Dict.SetDefault("ForceCopyXNBProcessors", new string[0]);
        }

        public object this[string key] {
            get {
                return Dict.GetValueOrDefault(key, null);
            }
        }
    }

    public class FileSettings {
        private readonly ProfileSettings Profile;
        private readonly Dictionary<string, object> SettingsDict = new Dictionary<string, object>();
        private readonly Dictionary<string, Regex> Regexes = new Dictionary<string, Regex>();

        public FileSettings (ProfileSettings profile) {
            Profile = profile;

            SettingsDict = profile["FileSettings"] as Dictionary<string, object>;
            if (SettingsDict == null)
                SettingsDict = new Dictionary<string, object>();

            var globChars = new char[] { '?', '*' };

            foreach (var key in SettingsDict.Keys) {
                if (key.IndexOfAny(globChars) < 0)
                    continue;

                var regex = new Regex(
                    Regex.Escape(key)
                        .Replace("/", "\\\\")
                        .Replace("\\*", "(.*)")
                        .Replace("\\?", "(.)"),
                    RegexOptions.Compiled | RegexOptions.IgnoreCase
                );

                Regexes[key] = regex;
            }
        }

        public HashSet<string> this[string filename] {
            get {
                string result = null;
                object settings;

                if (SettingsDict.TryGetValue(filename, out settings)) {
                    result = settings.ToString();
                } else if (SettingsDict.TryGetValue(filename.Replace("\\", "/"), out settings)) {
                    result = settings.ToString();
                } else {
                    foreach (var kvp in Regexes) {
                        if (kvp.Value.IsMatch(filename)) {
                            result = SettingsDict[kvp.Key].ToString();
                            break;
                        }
                    }
                }

                if (result != null) {
                    return new HashSet<string>(
                        Profile.Variables.Expand(result).ToLower().Split(' ')
                    );
                } else {
                    return new HashSet<string>();
                }
            }
        }
    }
}