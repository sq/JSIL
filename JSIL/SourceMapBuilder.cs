/*
 * Based on the Source Map library:
 * https://github.com/mozilla/source-map

 * Copyright 2011 Mozilla Foundation and contributors
 * Licensed under the New BSD license. See LICENSE or:
 * http://opensource.org/licenses/BSD-3-Clause
 *
 * Based on the Base 64 VLQ implementation in Closure Compiler:
 * https://code.google.com/p/closure-compiler/source/browse/trunk/src/com/google/debugging/sourcemap/Base64VLQ.java
 *
 * Copyright 2011 The Closure Compiler Authors. All rights reserved.
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *  * Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 *  * Redistributions in binary form must reproduce the above
 *    copyright notice, this list of conditions and the following
 *    disclaimer in the documentation and/or other materials provided
 *    with the distribution.
 *  * Neither the name of Google Inc. nor the names of its
 *    contributors may be used to endorse or promote products derived
 *    from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace JSIL.Internal
{
    public class SourceMapBuilder
    {
        private readonly List<Mapping> _mappings = new List<Mapping>(); 

        public void AddInfo(int genaratedLine, int generatedColumn, IEnumerable<SequencePoint> info)
        {
            var point = info != null ? info.FirstOrDefault() : null;
            if (point == null)
                return;
            _mappings.Add(new Mapping(
                genaratedLine + 1,
                generatedColumn,
                point.StartLine,
                point.StartColumn - 1,
                point.Document.Url,
                null));
        }

        public void AddInfoEnd(int genaratedLine, int generatedColumn, IEnumerable<SequencePoint> info)
        {
            var point = info != null ? info.FirstOrDefault() : null;
            if (point == null)
                return;
            _mappings.Add(new Mapping(
                genaratedLine + 1,
                generatedColumn,
                point.EndLine,
                point.EndColumn,
                point.Document.Url,
                null));
        }

        public bool Build(string path, string sourceName)
        {
            if (!_mappings.Any())
            {
                return false;
            }

            var mappings = _mappings.OrderBy(item => item.GeneratedLine).ThenBy(item => item.GeneratedColumn).ToList();

            var sources = new List<string>();
            var names = new List<string>();
            var previousGeneratedColumn = 0;
            var previousGeneratedLine = 1;
            var previousOriginalColumn = 0;
            var previousOriginalLine = 0;
            var previousName = 0;
            var previousSource = 0;
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                if (mapping.Source != null && !sources.Contains(mapping.Source))
                {
                    sources.Add(mapping.Source);
                }
                if (mapping.Name != null && !names.Contains(mapping.Name))
                {
                    names.Add(mapping.Name);
                }

                if (mapping.GeneratedLine != previousGeneratedLine)
                {
                    previousGeneratedColumn = 0;
                    while (mapping.GeneratedLine != previousGeneratedLine)
                    {
                        result.Append(';');
                        previousGeneratedLine++;
                    }
                }
                else
                {
                    if (i > 0)
                    {
                        if (mapping == mappings[i - 1])
                        {
                            continue;
                        }
                        result.Append(',');
                    }
                }

                result.Append(Base64Vlq.Encode(mapping.GeneratedColumn - previousGeneratedColumn));
                previousGeneratedColumn = mapping.GeneratedColumn;

                if (mapping.Source != null)
                {
                    var sourceIdx = sources.IndexOf(mapping.Source);
                    result.Append(Base64Vlq.Encode(sourceIdx - previousSource));
                    previousSource = sourceIdx;

                    // lines are stored 0-based in SourceMap spec version 3
                    result.Append(Base64Vlq.Encode(mapping.OriginalLine  - 1 - previousOriginalLine));
                    previousOriginalLine = mapping.OriginalLine - 1;

                    result.Append(Base64Vlq.Encode(mapping.OriginalColumn - previousOriginalColumn));
                    previousOriginalColumn = mapping.OriginalColumn;

                    if (mapping.Name != null)
                    {
                        var nameIdx = names.IndexOf(mapping.Name);
                        result.Append(Base64Vlq.Encode(nameIdx - previousName));
                        previousName = nameIdx;
                    }
                }
                //else
                //{
                ////IK: I'm not sure if we should do it? 
                //    previousOriginalLine = 0;
                //    previousOriginalColumn = 0;
                //}
            }

            var sourceMap = new StringBuilder();
            sourceMap.AppendLine("{");
            sourceMap.AppendLine("\t\"version\" : 3,");
            sourceMap.AppendLine(string.Format("\t\"file\" : \"{0}\",", sourceName));
            sourceMap.AppendLine(string.Format("\t\"sourceRoot\" : \"{0}\",", new Uri(Path.GetFullPath(path))));
            sourceMap.AppendLine(string.Format("\t\"sources\" : [{0}],", string.Join(", ", sources.Select(item => "\"" + MakeRelativePath(path, item) + "\""))));
            sourceMap.AppendLine(string.Format("\t\"names\" : [{0}],", string.Join(", ", names.Select(item => "\"" + item + "\""))));
            sourceMap.AppendLine(string.Format("\t\"mappings\" : \"{0}\"", result));
            sourceMap.AppendLine("}");

            using (var file = File.Create(GetFullMapPath(path, sourceName)))
            {
                using (var tw = new StreamWriter(file))
                {
                    tw.Write(sourceMap.ToString());
                    tw.Flush();

                }
            }
            return true;
        }

        public static Uri MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(Path.GetFullPath(fromPath));
            Uri toUri = new Uri(Path.GetFullPath(toPath));

            if (fromUri.Scheme != toUri.Scheme) { return toUri; } // path can't be made relative.

            return fromUri.MakeRelativeUri(toUri);
        }

        private sealed class Mapping
        {
            public int GeneratedLine { get; private set; }
            public int GeneratedColumn { get; private set; }

            public int OriginalLine { get; private set; }
            public int OriginalColumn { get; private set; }

            public string Source { get; private set; }
            public string Name { get; private set; }

            public Mapping(int generatedLine, int generatedColumn, int originalLine, int originalColumn, string source, string name)
            {
                GeneratedLine = generatedLine;
                GeneratedColumn = generatedColumn;
                OriginalLine = originalLine;
                OriginalColumn = originalColumn;
                Source = source;
                Name = name;
            }

            private bool Equals(Mapping other)
            {
                return GeneratedLine == other.GeneratedLine && GeneratedColumn == other.GeneratedColumn && OriginalLine == other.OriginalLine && OriginalColumn == other.OriginalColumn && string.Equals(Source, other.Source) && string.Equals(Name, other.Name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Mapping) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = GeneratedLine.GetHashCode();
                    hashCode = (hashCode*397) ^ GeneratedColumn.GetHashCode();
                    hashCode = (hashCode*397) ^ OriginalLine.GetHashCode();
                    hashCode = (hashCode*397) ^ OriginalColumn.GetHashCode();
                    hashCode = (hashCode*397) ^ (Source != null ? Source.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                    return hashCode;
                }
            }

            public static bool operator ==(Mapping left, Mapping right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Mapping left, Mapping right)
            {
                return !Equals(left, right);
            }
        }

        private static class Base64Vlq
        {
            private const string IntToCharMap = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

            // A single base 64 digit can contain 6 bits of data. For the base 64 variable
            // length quantities we use in the source map spec, the first bit is the sign,
            // the next four bits are the actual value, and the 6th bit is the
            // continuation bit. The continuation bit tells us whether there are more
            // digits in this value following this digit.
            //
            //   Continuation
            //   |    Sign
            //   |    |
            //   V    V
            //   101011

            private const int VLQ_BASE_SHIFT = 5;

            // binary: 100000
            private const int VLQ_BASE = 1 << VLQ_BASE_SHIFT;

            // binary: 011111
            private const int VLQ_BASE_MASK = VLQ_BASE - 1;

            // binary: 100000
            private const int VLQ_CONTINUATION_BIT = VLQ_BASE;

            /// <summary>
            /// Converts from a two-complement value to a value where the sign bit is
            /// placed in the least significant bit.For example, as decimals:
            ///   1 becomes 2 (10 binary), -1 becomes 3 (11 binary)
            ///   2 becomes 4 (100 binary), -2 becomes 5 (101 binary)
            /// </summary>
            private static uint ToVlqSigned(int aValue)
            {
                return (uint) (aValue < 0
                    ? ((-aValue) << 1) + 1
                    : (aValue << 1) + 0);
            }

            public static string Encode(int aValue)
            {
                var encoded = "";

                var vlq = ToVlqSigned(aValue);

                do
                {
                    var digit = vlq & VLQ_BASE_MASK;
                    vlq >>= VLQ_BASE_SHIFT;
                    if (vlq > 0)
                    {
                        // There are still more digits in this value, so we must make sure the
                        // continuation bit is marked.
                        digit |= VLQ_CONTINUATION_BIT;
                    }
                    encoded += EncodeDigit((int) digit);
                } while (vlq > 0);

                return encoded;
            }

            private static char EncodeDigit(int number)
            {
                if (number >= 0 && number < IntToCharMap.Length)
                {
                    return IntToCharMap[number];
                }
                throw new ArgumentException("Must be between 0 and 63: " + number);
            }
        }

        public void WriteSourceMapLink(Stream outputStream, string path, string sourceName)
        {
            var writer = new StreamWriter(outputStream);
            writer.Write("//# sourceMappingURL=" + new Uri(GetFullMapPath(path, sourceName)));
            writer.Flush();
        }

        private string GetFullMapPath(string path, string sourceName)
        {
            return Path.GetFullPath(Path.Combine(path, sourceName + ".map")).Replace(" ", "");
        }
    }
}