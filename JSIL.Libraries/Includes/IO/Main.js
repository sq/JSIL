"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core is required");

$private = $jsilcore;

if (!JSIL.GetAssembly("mscorlib", true)) {
    JSIL.DeclareNamespace("System");
    JSIL.DeclareNamespace("System.IO");

    //? include("Classes/System.MarshalByRefObject.js"); writeln();
    //? include("Classes/System.IO.Stream.js"); writeln();
    //? include("Classes/System.IO.MemoryStream.js"); writeln();
    

    JSIL.MakeEnum(
      "System.IO.SeekOrigin", true, {
          Begin: 0,
          Current: 1,
          End: 2
      }, false
    );

    //? include("Classes/System.IO.BinaryWriter.js"); writeln();
    //? include("Classes/System.IO.BinaryReader.js"); writeln();
    //? include("Classes/System.IO.TextReader.js"); writeln();
    //? include("Classes/System.IO.StreamReader.js"); writeln();
    //? include("Classes/System.IO.TextWriter.js"); writeln();

    

}

var $jsilio = JSIL.DeclareAssembly("JSIL.IO");

//? include("Classes/System.Environment.js"); writeln();
//? include("Classes/System.IO.File.js"); writeln();
//? include("Classes/System.IO.Path.js"); writeln();


var $bytestream = function ($) {
    $.Method({ Static: false, Public: true }, "Read",
      (new JSIL.MethodSignature($.Int32, [
            $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
            $.Int32
      ], [])),
      function Read(buffer, offset, count) {
          var startPos = this._pos | 0;
          var endPos = (this._pos + count) | 0;

          if (endPos >= this._length) {
              endPos = this._length - 1;
              count = endPos - startPos + 1;
          }

          if ((startPos < 0) || (startPos >= this._length))
              return 0;

          offset = (offset | 0);
          count = (count | 0);

          var sourceBuffer = this._buffer;
          for (var i = 0; i < count; i = (i + 1) | 0) {
              var destIndex = (offset + i) | 0;
              var sourceIndex = (startPos + i) | 0;

              /*
              if ((destIndex < 0) || (destIndex >= buffer.length))
                throw new Error("Destination index out of range: " + destIndex);
              if ((sourceIndex < 0) || (sourceIndex >= sourceBuffer.length))
                throw new Error("Source index out of range: " + sourceIndex);
              */

              buffer[destIndex] = sourceBuffer[sourceIndex];
          }

          this._pos += count;

          return count;
      }
    );

    $.Method({ Static: false, Public: true }, "$PeekByte",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function PeekByte() {
          if (this._pos >= this._length)
              return -1;

          return this._buffer[this._pos];
      }
    );

    $.Method({ Static: false, Public: true }, "get_Position",
      (new JSIL.MethodSignature($.Int64, [], [])),
      function get_Position() {
          return $jsilcore.System.Int64.FromInt32(this._pos);
      }
    );

    $.Method({ Static: false, Public: true }, "set_Position",
      (new JSIL.MethodSignature(null, [$.Int64], [])),
      function set_Position(value) {
          this._pos = value.ToInt32();
      }
    );

    $.Method({ Static: false, Public: true }, "get_Length",
      (new JSIL.MethodSignature($.Int64, [], [])),
      function get_Length() {
          return $jsilcore.System.Int64.FromInt32(this._length);
      }
    );

    $.Method({ Static: false, Public: true }, "get_CanSeek",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function get_CanSeek() {
          return true;
      }
    );

    $.Method({ Static: false, Public: true }, "get_CanRead",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function get_CanRead() {
          return true;
      }
    );

    $.Method({ Static: false, Public: true }, "Seek",
      (new JSIL.MethodSignature($.Int64, [$.Int64, $jsilcore.TypeRef("System.IO.SeekOrigin")], [])),
      function Seek(offset, origin) {
          switch (origin) {
              case System.IO.SeekOrigin.Begin:
                  this._pos = offset.ToInt32();
                  break;
              case System.IO.SeekOrigin.Current:
                  this._pos += offset.ToInt32();
                  break;
              case System.IO.SeekOrigin.End:
                  this._pos = this._buffer.length + offset.ToInt32();
                  break;
          }

          return $jsilcore.System.Int64.FromInt32(this._pos);
      }
    );

    $.Method({ Static: false, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [
            $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
            $.Int32
      ], [])),
      function Write(buffer, offset, count) {
          var newPosition = this._pos + count;

          if (newPosition > this._length)
              this._length = newPosition;

          for (var i = 0; i < count; i++)
              this._buffer[this._pos + i] = buffer[offset + i];

          this._pos = newPosition;

          this._modified = true;
      }
    );

    $.Method({ Static: false, Public: true }, "WriteByte",
      (new JSIL.MethodSignature(null, [$.Byte], [])),
      function WriteByte(value) {
          if (this._pos >= this._length)
              this._length += 1;

          this._buffer[this._pos] = value;
          this._pos += 1;

          this._modified = true;
      }
    );

};

//? include("Classes/System.IO.FileStream.js"); writeln();







$jsilio.ReadCharFromStream = function ReadCharFromStream(stream, encoding) {
    encoding.fallbackCharacter = "\uFFFF";
    var oldPosition = stream.Position;
    var firstChar = null, actualLength;

    var minCharLength = encoding.minimumCharLength || 1;
    var maxCharLength = encoding.maximumCharLength || 4;

    var bytes = JSIL.Array.New(System.Byte, maxCharLength);

    for (var i = minCharLength; i <= maxCharLength; i++) {
        stream.Position = oldPosition;

        // A valid UTF-8 codepoint is 1-4 bytes
        var bytesRead = stream.Read(bytes, 0, i);

        var str = encoding.$decode(bytes, 0, bytesRead);
        if (str.length < 1)
            continue;

        firstChar = str[0];
        if (firstChar === encoding.fallbackCharacter)
            continue;

        return firstChar;
    }

    return null;
};

//? include("Classes/System.IO.FileSystemInfo.js"); writeln();
//? include("Classes/System.IO.DirectoryInfo.js"); writeln();
//? include("Classes/System.IO.FileInfo.js"); writeln();
//? include("Classes/System.IO.Directory.js"); writeln();




