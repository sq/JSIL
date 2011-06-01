"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");
  
JSIL.DeclareAssembly("JSIL.IO");

System.IO.File.Exists = function (filename) {
  return JSIL.Host.doesFileExist(filename);
}

System.IO.Path.Combine = function () {
  return Array.prototype.slice.call(arguments).join("/");
}

System.IO.FileStream.prototype._ctor$0 = function () {
  System.IO.Stream.prototype._ctor.call(this);
}

System.IO.FileStream.prototype._ctor$1 = function (filename, mode) {
  System.IO.Stream.prototype._ctor.call(this);

  this._fileName = filename;
  this._buffer = JSIL.Host.getFile(filename);
  if (
    (typeof (this._buffer) === "undefined") ||
    (typeof (this._buffer.length) !== "number")
  )
    throw new System.Exception("Unable to get an array for the file '" + filename + "'");

  this._pos = 0;
};

System.IO.FileStream.prototype.Peek = function () {
  if ((this._pos < 0) || (this._pos >= this._buffer.length))
    return -1;

  return this._buffer[this._pos];
};

System.IO.FileStream.prototype.ReadByte = function () {
  if ((this._pos < 0) || (this._pos >= this._buffer.length))
    return -1;

  return this._buffer[this._pos++];
};

System.IO.FileStream.prototype.Close = function () {
};

System.IO.BinaryReader.prototype._ctor$0 = function (stream) {
  System.Object.prototype._ctor.call(this);

  if (typeof (stream) !== "object")
    throw new Error();

  this.m_stream = stream;
  this.m_encoding = System.Text.Encoding.ASCII;
};

System.IO.BinaryReader.prototype._ctor$1 = function (stream, encoding) {
  System.Object.prototype._ctor.call(this);

  if (typeof (stream) !== "object")
    throw new Error();

  this.m_stream = stream;
  this.m_encoding = encoding;
};

System.IO.BinaryReader.prototype.ReadBytes = function (count) {
  var result = new Array(count);
  for (var i = 0; i < count; i++) {
    var b = this.m_stream.ReadByte();
    if (b === -1)
      return result.slice(0, i - 1);

    result[i] = b;
  };

  return result;
};

System.IO.BinaryReader.prototype.ReadChars = function (count) {
  var result = new Array(count);
  for (var i = 0; i < count; i++) {
    var b = this.m_stream.ReadByte();
    if (b === -1)
      return result.slice(0, i - 1);

    result[i] = String.fromCharCode(b);
  };

  return result;
}  

System.IO.BinaryReader.prototype.ReadInt32 = function () {
  var value = this.ReadUInt32();
  if (value > System.Int32.MaxValue)
    return value - 4294967296;
  else
    return value;
};

System.IO.BinaryReader.prototype.ReadUInt32 = function () {
  var low1 = this.m_stream.ReadByte();
  var low2 = this.m_stream.ReadByte();
  var low3 = this.m_stream.ReadByte();
  return low1 | (low2 << 8) | (low3 << 16) | (this.m_stream.ReadByte() << 24);
};

System.IO.BinaryReader.prototype.ReadInt16 = function () {
  var value = this.ReadUInt16();
  if (value > System.Int16.MaxValue)
    return value - 65536;
  else
    return value;
};

System.IO.BinaryReader.prototype.ReadUInt16 = function () {
  var low = this.m_stream.ReadByte();
  return low | (this.m_stream.ReadByte() << 8);
};

System.IO.BinaryReader.prototype.ReadSByte = function () {
  var byt = this.m_stream.ReadByte();
  if (byt > 127)
    return byt - 256;
  else
    return byt;
};

System.IO.BinaryReader.prototype.ReadSingle = function () {
  var bytes = this.ReadBytes(4);
  return this._decodeFloat(bytes, 23, 8);
};

System.IO.BinaryReader.prototype.ReadDouble = function () {
  var bytes = this.ReadBytes(8);
  return this._decodeFloat(bytes, 52, 11);
};

System.IO.BinaryReader.prototype.ReadByte = function () {
  return this.m_stream.ReadByte();
};

System.IO.BinaryReader.prototype.Close = function () {
};

// Derived from http://blog.vjeux.com/wp-content/uploads/2010/01/binaryReader.js

System.IO.BinaryReader.prototype._shl = function (a, b) {
  for (
    ++b; --b;
    a = ((a %= 0x7fffffff + 1) & 0x40000000) == 0x40000000 ? 
      a * 2 : (a - 0x40000000) * 2 + 0x7fffffff + 1
  );

  return a;
};

System.IO.BinaryReader.prototype._readBits = function (bytes, start, length) {
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
};

System.IO.BinaryReader.prototype._decodeFloat = function (bytes, precisionBits, exponentBits) {
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
};

System.IO.BinaryReader.prototype.IDisposable_Dispose = function () {
  this.m_stream = null;
};

System.IO.StreamReader.prototype._ctor$7 = function (filename) {
  this.stream = new System.IO.FileStream(filename, System.IO.FileMode.Open);
};

System.IO.StreamReader.prototype.ReadLine = function () {
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
};

System.IO.StreamReader.prototype.IDisposable_Dispose = function () {
  this.stream = null;
};
