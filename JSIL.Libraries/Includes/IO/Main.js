"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core is required");

$private = $jsilcore;

//? if ('GENERATE_STUBS' in  __out) {
    JSIL.DeclareNamespace("System");
    JSIL.DeclareNamespace("System.IO");

    JSIL.MakeEnum(
      "System.IO.SeekOrigin", true, {
          Begin: 0,
          Current: 1,
          End: 2
      }, false
    );
//? }

//? include("Helpers/$bytestream.js");

//? include("Classes/System.MarshalByRefObject.js"); writeln();
//? include("Classes/System.IO.Stream.js"); writeln();
//? include("Classes/System.IO.MemoryStream.js"); writeln();
//? include("Classes/System.IO.BinaryWriter.js"); writeln();
//? include("Classes/System.IO.BinaryReader.js"); writeln();
//? include("Classes/System.IO.TextReader.js"); writeln();
//? include("Classes/System.IO.StreamReader.js"); writeln();
//? include("Classes/System.IO.TextWriter.js"); writeln();

var $jsilio = JSIL.DeclareAssembly("JSIL.IO");

//? include("Helpers/$jsilio.ReadCharFromStream.js");

//? include("Classes/System.Environment.js"); writeln();
//? include("Classes/System.IO.File.js"); writeln();
//? include("Classes/System.IO.Path.js"); writeln();

//? include("Classes/System.IO.FileStream.js"); writeln();

//? include("Classes/System.IO.FileSystemInfo.js"); writeln();
//? include("Classes/System.IO.DirectoryInfo.js"); writeln();
//? include("Classes/System.IO.FileInfo.js"); writeln();
//? include("Classes/System.IO.Directory.js"); writeln();




