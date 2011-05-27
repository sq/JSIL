"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");
  
JSIL.DeclareAssembly("JSIL.IO");

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

System.IO.FileStream.prototype.ReadByte = function () {
  if ((this._pos < 0) || (this._pos >= this._buffer.length))
    throw new System.ArgumentOutOfRangeException();

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

System.IO.BinaryReader.prototype.ReadChars = function (count) {
  var result = new Array(count);
  for (var i = 0; i < count; i++) {
    result[i] = this.m_stream.ReadByte();
  };

  return result;
};

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
  var byte = this.m_stream.ReadByte();
  if (byte > 127)
    return byte - 256;
  else
    return byte;
};

System.IO.BinaryReader.prototype.ReadSingle = function () {
  var bytes = this.ReadChars(4);
  return 0.0;
};

System.IO.BinaryReader.prototype.ReadDouble = function () {
  var bytes = this.ReadChars(8);
  return 0.0;
};

System.IO.BinaryReader.prototype.ReadByte = function () {
  return this.m_stream.ReadByte();
};

System.IO.BinaryReader.prototype.Close = function () {
};