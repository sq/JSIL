"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core is required");

$private = $jsilcore;

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.XML");
JSIL.DeclareNamespace("System");
JSIL.DeclareNamespace("System.Xml");

if (!JSIL.GetAssembly("System.Xml", true)) {
    JSIL.MakeEnum(
      "System.Xml.XmlNodeType", true, {
          None: 0,
          Element: 1,
          Attribute: 2,
          Text: 3,
          CDATA: 4,
          EntityReference: 5,
          Entity: 6,
          ProcessingInstruction: 7,
          Comment: 8,
          Document: 9,
          DocumentType: 10,
          DocumentFragment: 11,
          Notation: 12,
          Whitespace: 13,
          SignificantWhitespace: 14,
          EndElement: 15,
          EndEntity: 16,
          XmlDeclaration: 17
      }, false
    );

    //? include("Classes/System.Xml.XmlNameTable.js"); writeln();
    //? include("Classes/System.Xml.XmlReader.js"); writeln();
    //? include("Classes/System.Xml.XmlWriter.js"); writeln();
    
}


var $xmlasms = new JSIL.AssemblyCollection({
    5: "mscorlib",
    6: "System",
    16: "System.Xml",
});


JSIL.XML.ReaderFromStream = function (stream) {
    // FIXME: Won't work if the stream is written to while being read from.

    var streamLength = stream.Length.ToInt32();
    var bytes = JSIL.Array.New(System.Byte, streamLength);
    stream.Read(bytes, 0, streamLength);

    var xml;

    // Detect UTF-8 BOM and remove it because browsers choke on it.
    if ((bytes[0] === 0xEF) && (bytes[1] === 0xBB) && (bytes[2] === 0xBF)) {
        xml = System.Text.Encoding.UTF8.$decode(bytes, 3, bytes.length - 3);
    } else {
        xml = JSIL.StringFromByteArray(bytes);
    }

    return JSIL.XML.ReaderFromString(xml);
};

JSIL.XML.ReaderFromString = function (xml) {
    var parser = new DOMParser();
    var root = parser.parseFromString(xml, "application/xml");

    if ((root === null) || (root.documentElement.localName == "parsererror")) {
        throw new Error("Failed to parse XML document");
    }

    var result = JSIL.CreateInstanceOfType(
      System.Xml.XmlReader.__Type__, "$fromDOMNode", [root]
    );
    return result;
};


JSIL.XML.WriterForStream = function (stream) {
    var result = JSIL.CreateInstanceOfType(
      System.Xml.XmlWriter.__Type__, "$forStream", [stream]
    );

    return result;
};

//? include("Classes/System.Xml.Serialization.XmlSerializer.js"); writeln();
//? include("Classes/System.Xml.Serialization.XmlSerializationReader.js"); writeln();
//? include("Classes/System.Xml.XmlQualifiedName.js"); writeln();
//? include("Classes/System.Xml.XmlConvert.js"); writeln();










