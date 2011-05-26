"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");
  
JSIL.DeclareAssembly("JSIL.IO");

System.IO.BinaryReader.prototype.ReadChars = function (count) {
  return new Array(count);
};

System.IO.BinaryReader.prototype.ReadInt32 = function () {
  return 0;
};

System.IO.BinaryReader.prototype.ReadInt16 = function () {
  return 0;
};

System.IO.BinaryReader.prototype.ReadByte = function () {
  return 0;
};

System.IO.BinaryReader.prototype.Close = function () {
};

System.IO.FileStream.prototype.Close = function () {
};