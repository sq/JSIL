"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core is required");

$private = $jsilcore;

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.XML");
JSIL.DeclareNamespace("System");
JSIL.DeclareNamespace("System.Xml");

//? if ('GENERATE_STUBS' in  __out) {
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
//? }

    //? include("Classes/System.Xml.XmlNameTable.js"); writeln();
    //? include("Classes/System.Xml.XmlReader.js"); writeln();
    //? include("Classes/System.Xml.XmlWriter.js"); writeln();

var $xmlasms = new JSIL.AssemblyCollection({
    5: "mscorlib",
    6: "System",
    16: "System.Xml",
});

//? include("Classes/System.Xml.Serialization.XmlSerializer.js"); writeln();
//? include("Classes/System.Xml.Serialization.XmlSerializationReader.js"); writeln();
//? include("Classes/System.Xml.XmlQualifiedName.js"); writeln();
//? include("Classes/System.Xml.XmlConvert.js"); writeln();










