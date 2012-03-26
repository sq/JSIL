using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
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

    public static CompressResult? CompressImage (string imageName, string sourceFolder, string outputFolder, Dictionary<string, object> settings, CompressResult? oldResult) {
        const int CompressVersion = 1;

        EnsureDirectoryExists(outputFolder);

        var sourcePath = Path.Combine(sourceFolder, imageName);
        var sourceInfo = new FileInfo(sourcePath);
        FileInfo resultInfo;

        if (oldResult.HasValue) {
            if (
                (oldResult.Value.Version == CompressVersion) &&
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

                        return oldResult.Value;
                    }
                }
            }
        }

        var outputPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(imageName));
        var justCopy = true;

        switch (Path.GetExtension(imageName).ToLower()) {
            case ".jpg":
            case ".jpeg":
                outputPath += ".jpg";
                break;

            case ".bmp":
                justCopy = false;

                var hImage = LoadImage(IntPtr.Zero, sourcePath, IMAGE_BITMAP, 0, 0, LR_LOADFROMFILE);
                try {
                    using (var texture = (System.Drawing.Bitmap)System.Drawing.Image.FromHbitmap(hImage)) {
                        switch (texture.PixelFormat) {
                            case PixelFormat.Gdi:
                            case PixelFormat.Extended:
                            case PixelFormat.Canonical:
                            case PixelFormat.Undefined:
                            case PixelFormat.Format16bppRgb555:
                            case PixelFormat.Format16bppRgb565:
                            case PixelFormat.Format24bppRgb: {
                                outputPath += ".jpg";
                                var encoder = GetEncoder(ImageFormat.Jpeg);
                                var encoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                                encoderParameters.Param[0] = new EncoderParameter(
                                    System.Drawing.Imaging.Encoder.Quality,
                                    Convert.ToInt64(settings["JPEGQuality"])
                                );
                                texture.Save(outputPath, encoder, encoderParameters);
                                break;
                            }

                            // Do an elaborate song and dance to extract the alpha channel from the PNG, because
                            //  GDI+ is too utterly shitty to do that itself
                            case PixelFormat.Format32bppArgb:
                            case PixelFormat.Format32bppPArgb:
                            case PixelFormat.Format32bppRgb: {
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

                                outputPath += ".png";
                                newImage.Save(outputPath, ImageFormat.Png);
                                newImage.Dispose();

                                break;
                            }

                            default:
                                Console.Error.WriteLine("// Unsupported bitmap format: '{0}' {1}", Path.GetFileNameWithoutExtension(imageName), texture.PixelFormat);
                                return null;
                        }
                    }
                } finally {
                    DeleteObject(hImage);
                }
                break;

            case ".png":
            default:
                outputPath += Path.GetExtension(imageName);
                break;
        }

        if (justCopy)
            File.Copy(sourcePath, outputPath, true);

        bool usePNGQuant = Convert.ToBoolean(settings["UsePNGQuant"]);
        var pngQuantBlacklist = (settings["PNGQuantBlacklist"] as ArrayList).Cast<string>().OrderBy((s) => s).ToArray();
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
            Array.BinarySearch(pngQuantBlacklist, imageName) == -1
        ) {
            var psi = new ProcessStartInfo(pngQuantPath, pngQuantParameters);
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            using (var process = Process.Start(psi)) {
                process.StandardInput.AutoFlush = true;

                ThreadPool.QueueUserWorkItem(
                    (_) => {
                        var bytes = File.ReadAllBytes(outputPath);
                        process.StandardInput.BaseStream.Write(
                            bytes, 0, bytes.Length
                        );
                        process.StandardInput.BaseStream.Flush();
                        process.StandardInput.BaseStream.Close();
                    }, null
                );

                var stderr = new string[1] { null };
                ThreadPool.QueueUserWorkItem(
                    (_) => {
                        var text = Encoding.ASCII.GetString(ReadEntireStream(process.StandardError.BaseStream));
                        stderr[0] = text;
                    }, null
                );

                var result = ReadEntireStream(process.StandardOutput.BaseStream);

                File.WriteAllBytes(outputPath, result);

                process.WaitForExit();
                if (!String.IsNullOrWhiteSpace(stderr[0]))
                    Console.Error.WriteLine("// Error output from PNGQuant: {0}", stderr[0]);

                process.Close();
            }
        }

        resultInfo = new FileInfo(outputPath);
        return new CompressResult {
            Version = CompressVersion,

            SourceFilename = sourcePath,
            SourceSize = sourceInfo.Length,
            SourceTimestamp = sourceInfo.LastWriteTimeUtc,

            Filename = outputPath,
            Size = resultInfo.Length,
            Timestamp = resultInfo.LastWriteTimeUtc
        };
    }
}