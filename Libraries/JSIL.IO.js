"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");
  
JSIL.DeclareAssembly("JSIL.IO");

JSIL.ImplementExternals(
  "System.IO.File", false, {
    Exists: function (filename) {
      return JSIL.Host.doesFileExist(filename);
    }
  }
);

JSIL.ImplementExternals(
  "System.IO.Path", false, {
    Combine: function () {
      return Array.prototype.slice.call(arguments).join("/");
    }
  }
);

JSIL.ImplementExternals(
  "System.IO.Stream", true, {
    ReadByte: function () {
      var buffer = [];
      var count = this.Read(buffer, 0, 1);

      if (count >= 1)
        return buffer[0];
      else
        return -1;
    }
  }
);

var $bytestream = {
  Read: function (buffer, offset, count) {
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
  },
  $PeekByte: function () {
    if (this._pos >= this._length)
      return -1;

    return this._buffer[this._pos];
  }
};

JSIL.ImplementExternals(
  "System.IO.FileStream", true, {
    _ctor$0: function () {
      System.IO.Stream.prototype._ctor.call(this);

      this._pos = 0;
      this._length = 0;
    },
    _ctor$1: function (filename, mode) {
      System.IO.Stream.prototype._ctor.call(this);

      this._fileName = filename;
      this._buffer = JSIL.Host.getFile(filename);
      if (
        (typeof (this._buffer) === "undefined") ||
        (typeof (this._buffer.length) !== "number")
      )
        throw new System.Exception("Unable to get an array for the file '" + filename + "'");

      this._pos = 0;
      this._length = this._buffer.length;
    },
    Close: function () {
    }
  }
);

JSIL.ImplementExternals(
  "System.IO.FileStream", true, $bytestream
);

JSIL.ImplementExternals(
  "System.IO.MemoryStream", true, {
    _ctor$2: function (bytes) {
      System.IO.MemoryStream.prototype._ctor$3.call(this, bytes, true);
    },
    _ctor$3: function (bytes, writable) {
      System.IO.Stream.prototype._ctor.call(this);

      this._buffer = bytes;
      this._writable = writable;
      this._length = this._capacity = bytes.length;
      this._pos = 0;
    }
  }
);

JSIL.ImplementExternals(
  "System.IO.MemoryStream", true, $bytestream
);

JSIL.ImplementExternals(
  "System.IO.BinaryReader", true, {
    _ctor$0: function (stream) {
      System.Object.prototype._ctor.call(this);

      if (typeof (stream) !== "object")
        throw new Error();

      this.m_stream = stream;
      this.m_encoding = System.Text.Encoding.ASCII;
    },
    _ctor$1: function (stream, encoding) {
      System.Object.prototype._ctor.call(this);

      if (typeof (stream) !== "object")
        throw new Error();

      this.m_stream = stream;
      this.m_encoding = encoding;
    },
    ReadBytes: function (count) {
      var result = new Array(count);
      var bytesRead = this.m_stream.Read(result, 0, count);
      return result.slice(0, bytesRead);
    },
    ReadChars: function (count) {
      var result = new Array(count);
      for (var i = 0; i < count; i++) {
        var b = this.m_stream.ReadByte();
        if (b === -1)
          return result.slice(0, i - 1);

        result[i] = String.fromCharCode(b);
      };

      return result;
    },
    ReadInt32: function () {
      var value = this.ReadUInt32();
      if (value > System.Int32.MaxValue)
        return value - 4294967296;
      else
        return value;
    },
    ReadUInt32: function () {
      var low1 = this.m_stream.ReadByte();
      var low2 = this.m_stream.ReadByte();
      var low3 = this.m_stream.ReadByte();
      return low1 | (low2 << 8) | (low3 << 16) | (this.m_stream.ReadByte() << 24);
    },
    ReadInt16: function () {
      var value = this.ReadUInt16();
      if (value > System.Int16.MaxValue)
        return value - 65536;
      else
        return value;
    },
    ReadUInt16: function () {
      var low = this.m_stream.ReadByte();
      return low | (this.m_stream.ReadByte() << 8);
    },
    ReadSByte: function () {
      var byt = this.m_stream.ReadByte();
      if (byt > 127)
        return byt - 256;
      else
        return byt;
    },
    ReadSingle: function () {
      var bytes = this.ReadBytes(4);
      return this.$decodeFloat(bytes, 1, 8, 23, -126, 127, true);
    },
    ReadDouble: function () {
      var bytes = this.ReadBytes(8);
      return this.$decodeFloat(bytes, 1, 11, 52, -1022, 1023, true);
    },
    ReadBoolean: function () {
      return this.m_stream.ReadByte() != 0;
    },
    ReadByte: function () {
      return this.m_stream.ReadByte();
    },
    ReadChar: function () {
      return String.fromCharCode(this.m_stream.ReadByte());
    },
    PeekChar: function () {
      return String.fromCharCode(this.m_stream.$PeekByte());
    },
    Read7BitEncodedInt: function () {
	    var result = 0, bits = 0;

	    while (bits < 35) {
		    var b = this.ReadByte();
		    result |= (b & 127) << bits;
		    bits += 7;

		    if ((b & 128) == 0)
			    return result;
	    }

	    throw new System.FormatException("Bad 7-bit int format");
    },
    Close: function () {
    },
    // Derived from http://stackoverflow.com/a/8545403/106786
    $decodeFloat: function (bytes, signBits, exponentBits, fractionBits, eMin, eMax, littleEndian) {
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
    },
    Dispose: function () {
      this.m_stream = null;
    },
    get_BaseStream: function () {
      return this.m_stream;
    }
  }
);

JSIL.ImplementExternals(
  "System.IO.StreamReader", true, {
    _ctor$7: function (filename) {
      this.stream = new System.IO.FileStream(filename, System.IO.FileMode.Open);
    },
    ReadLine: function () {
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
    },
    Dispose: function () {
      this.stream = null;
    }
  }
);

JSIL.ImplementExternals(
  "System.IO.TextReader", true, {
    Dispose$0: function () {
    },
    Dispose$1: function (b) {
    }
  }
);
