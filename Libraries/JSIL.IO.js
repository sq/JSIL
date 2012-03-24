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
    },
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
      return this._decodeFloat(bytes, 23, 8);
    },
    ReadDouble: function () {
      var bytes = this.ReadBytes(8);
      return this._decodeFloat(bytes, 52, 11);
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
      return String.fromCharCode(this.m_stream.Peek());
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
    // Derived from http://blog.vjeux.com/wp-content/uploads/2010/01/binaryReader.js
    _shl: function (a, b) {
      for (
        ++b; --b;
        a = ((a %= 0x7fffffff + 1) & 0x40000000) == 0x40000000 ? 
          a * 2 : (a - 0x40000000) * 2 + 0x7fffffff + 1
      );

      return a;
    },
    _readBits: function (bytes, start, length) {
      var offsetLeft = (start + length) % 8;
      var offsetRight = start % 8;
      var curByte = bytes.length - (start >> 3) - 1;
      var lastByte = bytes.length + (-(start + length) >> 3);
      var diff = curByte - lastByte;

      var sum = (bytes[bytes.length - curByte - 1] >> offsetRight) & 
        ((1 << (diff ? 8 - offsetRight : length)) - 1);

      if (diff && offsetLeft)
        sum += (bytes[bytes.length - lastByte++ - 1] & ((1 << offsetLeft) - 1)) << (diff-- << 3) - offsetRight; 

      while (diff)
        sum += this._shl(bytes[bytes.length - lastByte++ - 1], (diff-- << 3) - offsetRight);

      return sum;
    },
    _decodeFloat: function (bytes, precisionBits, exponentBits) {
      var length = precisionBits + exponentBits + 1;
      if ((length >> 3) > bytes.length)
        throw new Error("Buffer too small");

      var bias = Math.pow(2, exponentBits - 1) - 1;
      var signal = this._readBits(bytes, precisionBits + exponentBits, 1);
      var exponent = this._readBits(bytes, precisionBits, exponentBits);
      var significand = 0;
      var divisor = 2;
      var curByte = 0;
      do {
        var byteValue = bytes[++curByte - bytes.length - 1];
        var startBit = precisionBits % 8 || 8;
        var mask = 1 << startBit;
        while (mask >>= 1) {
          if (byteValue & mask) {
            significand += 1 / divisor;
          }
          divisor *= 2;
        }
      } while (precisionBits -= startBit);

      var result = exponent == (bias << 1) + 1 ? significand ? NaN : signal ? -Infinity : +Infinity
        : (1 + signal * -2) * (exponent || significand ? !exponent ? Math.pow(2, -bias + 1) * significand
        : Math.pow(2, exponent - bias) * (1 + significand) : 0);

      return result;
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
          var next = this.stream.Peek();
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
