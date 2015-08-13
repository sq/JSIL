"use strict";

if (typeof (JSIL) == "undefined")
    throw new Error("JSIL.Core is required");

var $jsilcompression = JSIL.DeclareAssembly("JSIL.IO.Compression");
var $systemio = JSIL.GetAssembly("System.IO");

JSIL.ImplementExternals("System.IO.Compression.ZipArchive", function ($) {
    $.Method({ Static: false, Public: true }, ".ctor",
		new JSIL.MethodSignature(null, [$systemio.TypeRef("System.IO.Stream")], []),
		function (stream) {
		    this._stream = stream;
		    this._zip = new JSZip(this._stream._buffer);
		}
	);

    $.Method({ Static: false, Public: true }, ".ctor",
		new JSIL.MethodSignature(null, [$systemio.TypeRef("System.IO.Stream"), $jsilcompression.TypeRef("System.IO.Compression.ZipArchiveMode")]),
		function (stream, zipArchiveMode, leaveOpen) {
		    this._stream = stream;
		    this._zip = new JSZip(this._stream._buffer);
		}
	);

    $.Method({ Static: false, Public: true }, ".ctor",
		new JSIL.MethodSignature(null, [$systemio.TypeRef("System.IO.Stream"), $jsilcompression.TypeRef("System.IO.Compression.ZipArchiveMode"), $.Boolean]),
		function (stream, zipArchiveMode, leaveOpen) {
		    this._stream = stream;
		    this._zip = new JSZip(this._stream._buffer);
		}
	);

    $.Method({ Static: false, Public: true }, "CreateEntry",
		new JSIL.MethodSignature($jsilcompression.TypeRef("System.IO.Compression.ZipArchiveEntry"), [$.String], []),
		function (entryName) {
		    // FIXME: ZipArchive CreateEntry
		}
	);

    $.Method({ Static: false, Public: true }, "Dispose",
		new JSIL.MethodSignature(null, [], []),
		function Dispose() {
            // FIXME: ZipArchive Dispose
		}
	);

    $.Method({ Static: false, Public: true }, "get_Entries",
		new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.ObjectModel.ReadOnlyCollection`1", $jsilcompression.TypeRef("System.IO.Compression.ZipArchiveEntry")), [], []),
		function get_Entries() {
		    if (this._entries === null) {
		        var tList = System.Collections.Generic.List$b1.Of(System.IO.Compression.ZipArchiveEntry.__Type__).__Type__;
		        var list = JSIL.CreateInstanceOfType(tList);

		        for (var entryName in this._zip.files) {
		            var entry = this._zip.file(entryName);

		            // Only add files
		            if (entry !== null && entry.dir === false) {
		                var tZipArchiveEntry = System.IO.Compression.ZipArchiveEntry.__Type__;
		                var zipArchiveEntry = JSIL.CreateInstanceOfType(tZipArchiveEntry, "$internalCtor", [entry]);

		                list.Add(zipArchiveEntry);
		            }
		        }

		        var tReadOnlyCollection = System.Collections.ObjectModel.ReadOnlyCollection$b1.Of(System.IO.Compression.ZipArchiveEntry.__Type__).__Type__;
		        this._entries = JSIL.CreateInstanceOfType(tReadOnlyCollection, "$listCtor", [list]);
		    }

		    return this._entries;
		}
	);

    $.Method({ Static: false, Public: true }, "GetEntry",
		new JSIL.MethodSignature($jsilcompression.TypeRef("System.IO.Compression.ZipArchiveEntry"), [$.String], []),
		function GetEntry(entryName) {
		    var entry = this._zip.file(entryName);

		    // Only return files
		    if (entry !== null && entry.dir === false) {
		        var tZipArchiveEntry = System.IO.Compression.ZipArchiveEntry.__Type__;
		        var zipArchiveEntry = JSIL.CreateInstanceOfType(tZipArchiveEntry, "$internalCtor", [entry]);

		        return zipArchiveEntry;
		    } else {
		        // FIXME: Check the exception that's returned if an entry is not found
		        return null;
		    }
		}
	);
});

JSIL.ImplementExternals("System.IO.Compression.ZipArchiveEntry", function ($) {
    $.RawMethod(false, "$internalCtor",
		function internalCtor(entry) {
		    this._entry = entry;

		    // Cache this - data is overwritten when decompressed
		    this._compressedSize = entry._data._compressedSize;
		    this._uncompressedSize = entry._data._uncompressedSize;
		}
	);

    $.Method({ Static: false, Public: true }, ".ctor",
		new JSIL.MethodSignature(null, [], []),
		function () {
		    // FIXME: Initialize ZipArchiveEntry?		
		}
	);

    $.Method({ Static: false, Public: true }, "Open",
		new JSIL.MethodSignature($jsilcompression.TypeRef("System.IO.Stream"), [], []),
		function () {
		    var array = this._entry.asUint8Array();

		    var stream = new System.IO.MemoryStream(array);

		    return stream;
		}
	);

    $.Method({ Static: false, Public: true }, "Delete",
		new JSIL.MethodSignature(null, [], []),
		function () {
		    // FIXME: Delete ZipArchiveEntry
		}
	);

    $.Method({ Static: false, Public: true }, "get_Name",
		new JSIL.MethodSignature($.String, [], []),
		function get_Name() {
		    var entryName = System.IO.Path.GetFileName(this._entry.name);
		    return entryName;
		}
	);

    $.Method({ Static: false, Public: true }, "get_FullName",
		new JSIL.MethodSignature($.String, [], []),
		function get_FullName() {
		    return this._entry.name;
		}
	);

    $.Method({ Static: false, Public: true }, "get_CompressedLength",
		new JSIL.MethodSignature($.Int64, [], []),
		function get_CompressedLength() {
		    return this._compressedSize;
		}
	);

    $.Method({ Static: false, Public: true }, "get_Length",
		new JSIL.MethodSignature($.Int64, [], []),
		function get_Length() {
		    return this._uncompressedSize;
		}
	);
});