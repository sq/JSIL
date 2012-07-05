"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

$private = $jsilcore;

if (!JSIL.GetAssembly("mscorlib", true)) {

  JSIL.DeclareNamespace("System");
  JSIL.DeclareNamespace("System.IO");

  JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.MarshalByRefObject", true, [], function ($) {
    $.Field({Static:false, Public:false}, "__identity", $.Object);

    $.Property({Static:false, Public:false}, "Identity");
  });

  JSIL.MakeClass($jsilcore.TypeRef("System.MarshalByRefObject"), "System.IO.Stream", true, [], function ($) {
    $.Constant({Static:true , Public:false}, "_DefaultBufferSize", 4096);

    $.Field({Static:true , Public:true }, "Null", $.Type);

    $.Property({Static:false, Public:true }, "CanRead");

    $.Property({Static:false, Public:true }, "CanSeek");

    $.Property({Static:false, Public:true }, "CanTimeout");

    $.Property({Static:false, Public:true }, "CanWrite");

    $.Property({Static:false, Public:true }, "Length");

    $.Property({Static:false, Public:true }, "Position");

    $.Property({Static:false, Public:true }, "ReadTimeout");

    $.Property({Static:false, Public:true }, "WriteTimeout");

    $.ImplementInterfaces($jsilcore.TypeRef("System.IDisposable"))
  });

  JSIL.MakeClass($jsilcore.TypeRef("System.IO.Stream"), "System.IO.MemoryStream", true, [], function ($) {
    $.Field({Static:false, Public:false}, "_buffer", $jsilcore.TypeRef("System.Array", [$.Byte]));

    $.Field({Static:false, Public:false}, "_capacity", $.Int32);

    $.Field({Static:false, Public:false}, "_expandable", $.Boolean);

    $.Field({Static:false, Public:false}, "_exposable", $.Boolean);

    $.Field({Static:false, Public:false}, "_isOpen", $.Boolean);

    $.Field({Static:false, Public:false}, "_length", $.Int32);

    $.Field({Static:false, Public:false}, "_origin", $.Int32);

    $.Field({Static:false, Public:false}, "_position", $.Int32);

    $.Field({Static:false, Public:false}, "_writable", $.Boolean);

    $.Constant({Static:true , Public:false}, "MemStreamMaxLength", 2147483647);

    $.Property({Static:false, Public:true }, "CanRead");

    $.Property({Static:false, Public:true }, "CanSeek");

    $.Property({Static:false, Public:true }, "CanWrite");

    $.Property({Static:false, Public:true }, "Capacity");

    $.Property({Static:false, Public:true }, "Length");

    $.Property({Static:false, Public:true }, "Position");
  });

}

var $jsilio = JSIL.DeclareAssembly("JSIL.IO");

JSIL.ImplementExternals("System.IO.File", function ($) {
  $.Method({Static:true , Public:true }, "Exists", 
    new JSIL.MethodSignature($.Boolean, [$.String], []),
    function (filename) {
      return JSIL.Host.doesFileExist(filename) || 
        JSIL.Host.doesAssetExist(filename, true);
    }
  );

  $.Method({Static:true , Public:true }, "OpenRead", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [$.String], [])), 
    function OpenRead (path) {
      return new System.IO.FileStream(path, System.IO.FileMode.Open);
    }
  );

  $.Method({Static:true , Public:true }, "OpenWrite", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [$.String], [])), 
    function OpenWrite (path) {
      return new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);
    }
  );

  $.Method({Static:true , Public:true }, "ReadAllText", 
    new JSIL.MethodSignature($.String, [$.String], []),
    function (filename) {
      var file = JSIL.Host.getFile(filename);
      return String.fromCharCode.apply(String, file);
    }
  );
});

JSIL.ImplementExternals("System.IO.Path", function ($) {
  var combineImpl = function () {
    return Array.prototype.slice.call(arguments).join("\\");
  };

  $.Method({Static:true , Public:true }, "Combine", 
    new JSIL.MethodSignature($.String, [$.String, $.String], []),
    combineImpl
  );

  $.Method({Static:true , Public:true }, "Combine", 
    new JSIL.MethodSignature($.String, [
        $.String, $.String, 
        $.String
      ], []),
    combineImpl
  );

  $.Method({Static:true , Public:true }, "Combine", 
    new JSIL.MethodSignature($.String, [
        $.String, $.String, 
        $.String, $.String
      ], []),
    combineImpl
  );

  $.Method({Static:true , Public:true }, "GetExtension", 
    (new JSIL.MethodSignature($.String, [$.String], [])), 
    function GetExtension (path) {
      var index = path.lastIndexOf(".");
      if (index >= 0) {
        return path.substr(index + 1);
      }

      return "";
    }
  );

  $.Method({Static:true , Public:true }, "GetDirectoryName", 
    (new JSIL.MethodSignature($.String, [$.String], [])), 
    function GetDirectoryName (path) {
      var index = path.lastIndexOf("\\");
      if (index >= 0) {
        return path.substr(0, index);
      }

      return "";
    }
  );

  $.Method({Static:true , Public:true }, "GetFullPath", 
    (new JSIL.MethodSignature($.String, [$.String], [])), 
    function GetFullPath (path) {
      // FIXME
      return path;
    }
  );

  $.Method({Static:true , Public:true }, "GetFileName", 
    (new JSIL.MethodSignature($.String, [$.String], [])), 
    function GetFileName (path) {
      var index = path.lastIndexOf("\\");
      if (index >= 0) {
        return path.substr(index + 1);
      }

      return path;
    }
  );

  $.Method({Static:true , Public:true }, "GetFileNameWithoutExtension", 
    (new JSIL.MethodSignature($.String, [$.String], [])), 
    function GetFileNameWithoutExtension (path) {
      var index = path.lastIndexOf("\\");
      if (index >= 0) {
        path = path.substr(index + 1);
      }

      index = path.indexOf(".");
      if (index >= 0)
        path = path.substr(0, index);

      return path;
    }
  );
});

JSIL.ImplementExternals("System.IO.Stream", function ($) {
  $.Method({Static:false, Public:true }, "ReadByte", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function ReadByte () {
      var buffer = [];
      var count = this.Read(buffer, 0, 1);

      if (count >= 1)
        return buffer[0];
      else
        return -1;
    }
  );

  $.Method({Static:false, Public:true }, "Close", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Close () {
      if (this._onClose)
        this._onClose();
    }
  );

  $.Method({Static:false, Public:true }, "Dispose", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Dispose () {
      if (this._onClose)
        this._onClose();
    }
  );

  $.RawMethod(false, "$GetURI", function () {
    throw new Error("Only valid on streams created from files!");
  });
});

var $bytestream = function ($) {
  $.Method({Static:false, Public:true }, "Read", 
    (new JSIL.MethodSignature($.Int32, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, 
          $.Int32
        ], [])), 
    function Read (buffer, offset, count) {
      var startPos = this._pos;
      var endPos = this._pos + count;

      if (endPos >= this._length) {
        endPos = this._length - 1;
        count = endPos - startPos + 1;
      }

      if ((startPos < 0) || (startPos >= this._length))
        return 0;

      for (var i = 0; i < count; i++) {
        buffer[i] = this._buffer[startPos + i];
      }

      this._pos += count;

      return count;
    }
  );

  $.Method({Static:false, Public:true }, "$PeekByte", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function PeekByte () {
      if (this._pos >= this._length)
        return -1;

      return this._buffer[this._pos];
    }
  );

  $.Method({Static:false, Public:true }, "get_Position", 
    (new JSIL.MethodSignature($.Int64, [], [])), 
    function get_Position () {
      return this._pos;
    }
  );

  $.Method({Static:false, Public:true }, "set_Position", 
    (new JSIL.MethodSignature(null, [$.Int64], [])), 
    function set_Position (value) {
      this._pos = value;
    }
  );

  $.Method({Static:false, Public:true }, "get_Length", 
    (new JSIL.MethodSignature($.Int64, [], [])), 
    function get_Length () {
      return this._length;
    }
  );

  $.Method({Static:false, Public:true }, "Write", 
    (new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, 
          $.Int32
        ], [])), 
    function Write (buffer, offset, count) {
      var newPosition = this._pos + count;

      if (newPosition > this._length)
        this._length = newPosition;

      for (var i = 0; i < count; i++)
        this._buffer[this._pos + i] = buffer[offset + i];

      this._pos = newPosition;

      this._modified = true;
    }
  );

  $.Method({Static:false, Public:true }, "WriteByte", 
    (new JSIL.MethodSignature(null, [$.Byte], [])), 
    function WriteByte (value) {
      if (this._pos >= this._length)
        this._length += 1;

      this._buffer[this._pos] = value;
      this._pos += 1;

      this._modified = true;
    }
  );

};

JSIL.ImplementExternals("System.IO.FileStream", function ($) {
  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      System.IO.Stream.prototype._ctor.call(this);

      this._pos = 0;
      this._length = 0;
    }
  );
  
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.String, $jsilcore.TypeRef("System.IO.FileMode"), $jsilcore.TypeRef("System.IO.FileAccess")], [])), 
    function _ctor (path, mode, access) {
      System.IO.FileStream.prototype._ctor.call(this, path, mode);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.String, $jsilcore.TypeRef("System.IO.FileMode")], [])), 
    function _ctor (path, mode) {
      System.IO.Stream.prototype._ctor.call(this);

      this._fileName = path;
      this._buffer = JSIL.Host.getFile(path);
      if (
        (typeof (this._buffer) === "undefined") ||
        (typeof (this._buffer.length) !== "number")
      )
        throw new System.Exception("Unable to get an array for the file '" + path + "'");

      this._pos = 0;
      this._length = this._buffer.length;

      this.$applyMode(mode);
    }
  );
  
  $.Method({Static:false, Public:true }, "get_CanSeek", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_CanSeek () {
      return true;
    }
  );
  
  $.Method({Static:false, Public:true }, "Seek", 
    (new JSIL.MethodSignature($.Int64, [$.Int64, $jsilcore.TypeRef("System.IO.SeekOrigin")], [])), 
    function Seek (offset, origin) {
      switch (origin)
      {
      case System.IO.SeekOrigin.Begin:
        this._pos = offset;
        break;
      case System.IO.SeekOrigin.Current:
        this._pos += offset;
        break;
      case System.IO.SeekOrigin.End:
        this._pos = this._buffer.length - offset;
        break;
      }
      return this._pos;
    }
  );

  $.RawMethod(false, "$applyMode", function (fileMode) {
    var fm = System.IO.FileMode;

    if (
      (fileMode == fm.Create) ||
      (fileMode == fm.Truncate) ||
      (fileMode == fm.CreateNew)
    ) {
      this._buffer = [];
      this._length = 0;
    } else if (fileMode == fm.Append) {
      this._pos = this._length;
    }
  });

  $.RawMethod(false, "$GetURI", function () {
    var slashRe = /\\/g;
    var uri = ("./" + this._fileName).replace(slashRe, "/");

    return uri;
  });
});

JSIL.ImplementExternals(
  "System.IO.FileStream", $bytestream
);

JSIL.ImplementExternals("System.IO.MemoryStream", function ($) {
  var ctorBytesImpl = function (self, bytes, writable) {
    System.IO.Stream.prototype._ctor.call(self);

    self._buffer = bytes;
    self._writable = writable;
    self._length = self._capacity = bytes.length;
    self._pos = 0;
  };

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      ctorBytesImpl(this, [], true);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])), 
    function _ctor (buffer) {
      ctorBytesImpl(this, buffer, true);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Boolean], [])), 
    function _ctor (buffer, writable) {
      ctorBytesImpl(this, buffer, writable);
    }
  );

  $.Method({Static:false, Public:true }, "GetBuffer", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [], [])), 
    function GetBuffer () {
      return this._buffer;
    }
  );
});

JSIL.ImplementExternals(
  "System.IO.MemoryStream", $bytestream
);

JSIL.ImplementExternals("System.IO.BinaryWriter", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream")], [])), 
    function _ctor (output) {
      this.m_stream = output;
    }
  );

  $.Method({Static:false, Public:true }, "get_BaseStream", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.Stream"), [], [])), 
    function get_BaseStream () {
      return this.m_stream;
    }
  );
});

JSIL.ImplementExternals("System.IO.BinaryReader", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream")], [])), 
    function _ctor (input) {
      System.Object.prototype._ctor.call(this);

      if (typeof (input) !== "object")
        throw new Error("Invalid stream");

      this.m_stream = input;
      this.m_encoding = System.Text.Encoding.ASCII;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.IO.Stream"), $jsilcore.TypeRef("System.Text.Encoding")], [])), 
    function _ctor (input, encoding) {
      System.Object.prototype._ctor.call(this);

      if (typeof (input) !== "object")
        throw new Error("Invalid stream");

      this.m_stream = input;
      this.m_encoding = encoding;
    }
  );

  $.Method({Static:false, Public:true }, "Close", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Close () {
      this.m_stream = null;
      this.m_encoding = null;
    }
  );

  $.Method({Static:false, Public:false}, "Read7BitEncodedInt", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function Read7BitEncodedInt () {
      var result = 0, bits = 0;

      while (bits < 35) {
        var b = this.ReadByte();
        result |= (b & 127) << bits;
        bits += 7;

        if ((b & 128) == 0)
          return result;
      }

      throw new System.FormatException("Bad 7-bit int format");
    }
  );

  $.Method({Static:false, Public:true }, "ReadBoolean", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function ReadBoolean () {
      return this.m_stream.ReadByte() != 0;
    }
  );

  $.Method({Static:false, Public:true }, "ReadByte", 
    (new JSIL.MethodSignature($.Byte, [], [])), 
    function ReadByte () {
      return this.m_stream.ReadByte();
    }
  );

  $.Method({Static:false, Public:true }, "ReadBytes", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Int32], [])), 
    function ReadBytes (count) {
      var result = new Array(count);
      var bytesRead = this.m_stream.Read(result, 0, count);
      return result.slice(0, bytesRead);
    }
  );

  $.Method({Static:false, Public:true }, "Read", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function Read () {
      return this.ReadChar().charCodeAt(0);
    }
  );

  $.Method({Static:false, Public:true }, "ReadChar", 
    (new JSIL.MethodSignature($.Char, [], [])), 
    function ReadChar () {
      var oldPosition = this.m_stream.Position;
      var utf8Encoding = new System.Text.UTF8Encoding();
      utf8Encoding.fallbackCharacter = "\uFFFF";
      var firstChar, actualLength;

      for (var i = 1; i < 5; i++) {
        this.m_stream.Position = oldPosition;
        
        // A valid UTF-8 codepoint is 1-4 bytes
        var bytes = [false, false, false, false];
        this.m_stream.Read(bytes, 0, bytes.length);

        var str = utf8Encoding.$decode(bytes);
        if (str.length < 1)
          continue;

        firstChar = str[0];

        if (firstChar === utf8Encoding.fallbackCharacter)
          continue;

        // We need to ensure that if we grabbed extra bytes, we rewind
        // FIXME: This is wrong if the chars aren't normalized. Augh.
        var actualCharEncoded = utf8Encoding.$encode(str.substr(0, 1));
        actualLength = actualCharEncoded.length;

        break;
      }

      this.m_stream.Position = oldPosition + actualLength;
      return firstChar;
    }
  );

  $.Method({Static:false, Public:true }, "ReadChars", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Char]), [$.Int32], [])), 
    function ReadChars (count) {
      var result = new Array(count);
      for (var i = 0; i < count; i++) {
        // FIXME: This should probably be ReadChar?
        var b = this.m_stream.ReadByte();
        if (b === -1)
          return result.slice(0, i - 1);

        result[i] = String.fromCharCode(b);
      };

      return result;
    }
  );

  $.Method({Static:false, Public:true }, "ReadDouble", 
    (new JSIL.MethodSignature($.Double, [], [])), 
    function ReadDouble () {
      var bytes = this.ReadBytes(8);
      return this.$decodeFloat(bytes, 1, 11, 52, -1022, 1023, true);
    }
  );

  $.Method({Static:false, Public:true }, "ReadInt16", 
    (new JSIL.MethodSignature($.Int16, [], [])), 
    function ReadInt16 () {
      var value = this.ReadUInt16();
      if (value > 32767)
        return value - 65536;
      else
        return value;
    }
  );

  $.Method({Static:false, Public:true }, "ReadInt32", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function ReadInt32 () {
      var value = this.ReadUInt32();
      if (value > 2147483647)
        return value - 4294967296;
      else
        return value;
    }
  );

  $.Method({Static:false, Public:true }, "ReadInt64", 
    (new JSIL.MethodSignature($.Int64, [], [])), 
    function ReadInt64 () {
      // FIXME: Does this work right for negative numbers or does 53-bit rounding kill it?
      // FIXME: Generate warnings for values out of 53-bit range.
      var value = this.ReadUInt64();
      if (value > System.Int64.MaxValue)
        return value - 18446744073709551616;
      else
        return value;
    }
  );

  $.Method({Static:false, Public:true }, "ReadSByte", 
    (new JSIL.MethodSignature($.SByte, [], [])), 
    function ReadSByte () {
      var byt = this.m_stream.ReadByte();
      if (byt > 127)
        return byt - 256;
      else
        return byt;
    }
  );

  $.Method({Static:false, Public:true }, "ReadSingle", 
    (new JSIL.MethodSignature($.Single, [], [])), 
    function ReadSingle () {
      var bytes = this.ReadBytes(4);
      return this.$decodeFloat(bytes, 1, 8, 23, -126, 127, true);
    }
  );

  $.Method({Static:false, Public:true }, "ReadString", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function ReadString () {
      var size = this.Read7BitEncodedInt();
      if (size <= 0)
        return "";

      var bytes = this.ReadBytes(size);
      return String.fromCharCode.apply(String, bytes);
    }
  );

  $.Method({Static:false, Public:true }, "ReadUInt16", 
    (new JSIL.MethodSignature($.UInt16, [], [])), 
    function ReadUInt16 () {
      var low = this.m_stream.ReadByte();
      return low + (this.m_stream.ReadByte() * 256);
    }
  );

  $.Method({Static:false, Public:true }, "ReadUInt32", 
    (new JSIL.MethodSignature($.UInt32, [], [])), 
    function ReadUInt32 () {
      var low1 = this.m_stream.ReadByte();
      var low2 = this.m_stream.ReadByte();
      var low3 = this.m_stream.ReadByte();
      var low4 = this.m_stream.ReadByte();
      return low1 + (low2 * 256) + (low3 * 65536) + (low4 * 16777216);
    }
  );

  $.Method({Static:false, Public:true }, "ReadUInt64", 
    (new JSIL.MethodSignature($.UInt64, [], [])), 
    function ReadUInt64 () {
      // FIXME: Generate warnings for values out of 53-bit range.
      var low1 = this.m_stream.ReadByte();
      var low2 = this.m_stream.ReadByte();
      var low3 = this.m_stream.ReadByte();
      var low4 = this.m_stream.ReadByte();
      var low5 = this.m_stream.ReadByte();
      var low6 = this.m_stream.ReadByte();
      var low7 = this.m_stream.ReadByte();
      var low8 = this.m_stream.ReadByte();
      return low1 | (low2 << 8) | (low3 << 16) | (low4 << 24) | (low5 << 32) | (low6 << 40) | (low7 << 48) | (low8 << 56);
    }
  );

  $.Method({Static:false, Public:true }, "PeekChar", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function PeekChar () {
      return String.fromCharCode(this.m_stream.$PeekByte());
    }
  );

  $.RawMethod(false, "$decodeFloat", 
    // Derived from http://stackoverflow.com/a/8545403/106786
    function decodeFloat (bytes, signBits, exponentBits, fractionBits, eMin, eMax, littleEndian) {
      var totalBits = (signBits + exponentBits + fractionBits);

      var binary = "";
      for (var i = 0, l = bytes.length; i < l; i++) {
        var bits = bytes[i].toString(2);
        while (bits.length < 8) 
          bits = "0" + bits;

        if (littleEndian)
          binary = bits + binary;
        else
          binary += bits;
      }

      var sign = (binary.charAt(0) == '1')?-1:1;
      var exponent = parseInt(binary.substr(signBits, exponentBits), 2) - eMax;
      var significandBase = binary.substr(signBits + exponentBits, fractionBits);
      var significandBin = '1'+significandBase;
      var i = 0;
      var val = 1;
      var significand = 0;

      if (exponent == -eMax) {
          if (significandBase.indexOf('1') == -1)
              return 0;
          else {
              exponent = eMin;
              significandBin = '0'+significandBase;
          }
      }

      while (i < significandBin.length) {
          significand += val * parseInt(significandBin.charAt(i));
          val = val / 2;
          i++;
      }

      return sign * significand * Math.pow(2, exponent);
    }
  );

  $.Method({Static:false, Public:true }, "Dispose", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Dispose () {
      this.m_stream = null;
    }
  );

  $.Method({Static:false, Public:true }, "get_BaseStream", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.Stream"), [], [])), 
    function get_BaseStream () {
      return this.m_stream;
    }
  );

});

JSIL.ImplementExternals("System.IO.StreamReader", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function _ctor (path) {
      this.stream = new System.IO.FileStream(path, System.IO.FileMode.Open);
    }
  );

  $.Method({Static:false, Public:false}, "Dispose", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function Dispose (disposing) {
      this.stream = null;
    }
  );

  $.Method({Static:false, Public:true }, "ReadLine", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function ReadLine () {
      var line = [];

      while (true) {
        var ch = this.stream.ReadByte();
        if (ch === -1) {
          if (line.length === 0)
            return null;
          else
            break;
        } else if (ch === 13) {
          var next = this.stream.$PeekByte();
          if (next === 10)
            continue;
          else if (next === -1) {
            if (line.length === 0)
              return null;
            else
              break;
          } else
            break;
        } else if (ch === 10) {
          break;
        }

        line.push(ch);
      };

      return String.fromCharCode.apply(null, line);
    }
  );
});

JSIL.ImplementExternals("System.IO.TextReader", function ($) {
  $.Method({Static:false, Public:true }, "Dispose", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Dispose () {
    }
  );

  $.Method({Static:false, Public:false}, "Dispose", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function Dispose (disposing) {
    }
  );
});