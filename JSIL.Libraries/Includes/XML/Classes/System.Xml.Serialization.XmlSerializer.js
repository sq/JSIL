//? include("../Utils/JSIL.XML.ReaderFromStream.js");
//? include("../Utils/JSIL.XML.WriterForStream.js");

JSIL.ImplementExternals("System.Xml.Serialization.XmlSerializer", function ($) {
    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$xmlasms[5].TypeRef("System.Type")], [])),
      function _ctor(type) {
          this.type = type;
      }
    );


    var getType = function (name) {
        var parsed = JSIL.ParseTypeName(name);
        return JSIL.GetTypeInternal(parsed, JSIL.GlobalNamespace, true);
    };

    $.RawMethod(false, "XmlReaderFromStream", function (stream) {
        return JSIL.XML.ReaderFromStream(stream);
    });

    $.RawMethod(false, "XmlWriterForStream", function (stream) {
        return JSIL.XML.WriterForStream(stream);
    });

    $.RawMethod(false, "GetContractClass", function () {
        var contractName = "Microsoft.Xml.Serialization.GeneratedAssembly.XmlSerializerContract";

        var contractAssembly = this.type.Assembly.FullName;
        if (this.type.IsArray) {
            contractAssembly = this.type.GetElementType().Assembly.FullName;
        }
        var indexOfFirstComaInAssemblyName = contractAssembly.search(",");
        if (indexOfFirstComaInAssemblyName >= 0) {
            contractAssembly = contractAssembly.substring(0, indexOfFirstComaInAssemblyName) + ".XmlSerializers" + contractAssembly.substring(indexOfFirstComaInAssemblyName, contractAssembly.length);
            contractName = contractName + ", " + contractAssembly;
        }

        var contractType = getType(contractName);

        if (!contractType)
            throw new Error("No XmlSerializer assembly loaded.");

        var contract = JSIL.CreateInstanceOfType(contractType);

        if (!contract)
            throw new Error("Could not create XmlSerializer contract class.");

        return contract;
    });

    $.RawMethod(false, "MakeSerializationReader", function (xmlReader, events, encodingStyle) {
        var contract = this.GetContractClass();
        var reader = contract.get_Reader();

        reader.Init(xmlReader, events, encodingStyle, null);

        return reader;
    });

    $.RawMethod(false, "MakeSerializationWriter", function (xmlWriter, namespaces, encodingStyle, id) {
        var contract = this.GetContractClass();
        var writer = contract.get_Writer();
        writer.Init(xmlWriter, namespaces, encodingStyle, id, null);

        return writer;
    });

    $.RawMethod(false, "MakeSerializer", function () {
        var contract = this.GetContractClass();
        var serializer = contract.GetSerializer(this.type);

        if (!serializer)
            throw new Error("XmlSerializer assembly does not contain a serializer for type '" + this.type.toString() + "'");

        return serializer;
    });

    $.RawMethod(false, "DeserializeInternal", function Deserialize(serializer, reader) {
        var signature = new JSIL.MethodSignature($.Object, [$xmlasms[16].System.Xml.Serialization.XmlSerializationReader], []);

        return signature.CallVirtual("Deserialize", null, serializer, reader);
    });

    $.RawMethod(false, "SerializeInternal", function Serialize(serializer, writer, value) {
        var signature = new JSIL.MethodSignature(null, [$.Object, $xmlasms[16].System.Xml.Serialization.XmlSerializationWriter], []);

        return signature.CallVirtual("Serialize", null, serializer, value, writer);
    });


    $.Method({ Static: false, Public: true }, "Deserialize",
      (new JSIL.MethodSignature($.Object, [$xmlasms[5].TypeRef("System.IO.Stream")], [])),
      function Deserialize(stream) {
          return this.Deserialize(
            this.XmlReaderFromStream(stream),
            null, null
          );
      }
    );

    $.Method({ Static: false, Public: true }, "Deserialize",
      (new JSIL.MethodSignature($.Object, [$xmlasms[5].TypeRef("System.IO.TextReader")], [])),
      function Deserialize(textReader) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Deserialize",
      (new JSIL.MethodSignature($.Object, [$xmlasms[16].TypeRef("System.Xml.XmlReader")], [])),
      function Deserialize(xmlReader) {
          return this.Deserialize(xmlReader, null, null);
      }
    );

    $.Method({ Static: false, Public: true }, "Deserialize",
      (new JSIL.MethodSignature($.Object, [$xmlasms[16].TypeRef("System.Xml.XmlReader"), $xmlasms[16].TypeRef("System.Xml.Serialization.XmlDeserializationEvents")], [])),
      function Deserialize(xmlReader, events) {
          return this.Deserialize(xmlReader, null, events);
      }
    );

    $.Method({ Static: false, Public: true }, "Deserialize",
      (new JSIL.MethodSignature($.Object, [$xmlasms[16].TypeRef("System.Xml.XmlReader"), $.String], [])),
      function Deserialize(xmlReader, encodingStyle) {
          return this.Deserialize(xmlReader, encodingStyle, null);
      }
    );

    $.Method({ Static: false, Public: true }, "Deserialize",
      (new JSIL.MethodSignature($.Object, [
            $xmlasms[16].TypeRef("System.Xml.XmlReader"), $.String,
            $xmlasms[16].TypeRef("System.Xml.Serialization.XmlDeserializationEvents")
      ], [])),
      function Deserialize(xmlReader, encodingStyle, events) {
          var reader = this.MakeSerializationReader(xmlReader, events, encodingStyle);
          var serializer = this.MakeSerializer();

          return this.DeserializeInternal(serializer, reader);
      }
    );


    $.Method({ Static: false, Public: true }, "Serialize",
      (new JSIL.MethodSignature(null, [$xmlasms[5].TypeRef("System.IO.TextWriter"), $.Object], [])),
      function Serialize(textWriter, o) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Serialize",
      (new JSIL.MethodSignature(null, [
            $xmlasms[5].TypeRef("System.IO.TextWriter"), $.Object,
            $xmlasms[16].TypeRef("System.Xml.Serialization.XmlSerializerNamespaces")
      ], [])),
      function Serialize(textWriter, o, namespaces) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Serialize",
      (new JSIL.MethodSignature(null, [$xmlasms[5].TypeRef("System.IO.Stream"), $.Object], [])),
      function Serialize(stream, o) {
          this.Serialize(
            this.XmlWriterForStream(stream), o,
            null, null, null
          );
      }
    );

    $.Method({ Static: false, Public: true }, "Serialize",
      (new JSIL.MethodSignature(null, [
            $xmlasms[5].TypeRef("System.IO.Stream"), $.Object,
            $xmlasms[16].TypeRef("System.Xml.Serialization.XmlSerializerNamespaces")
      ], [])),
      function Serialize(stream, o, namespaces) {
          this.Serialize(
            this.XmlWriterForStream(stream), o,
            namespaces, null, null
          );
      }
    );

    $.Method({ Static: false, Public: true }, "Serialize",
      (new JSIL.MethodSignature(null, [$xmlasms[16].TypeRef("System.Xml.XmlWriter"), $.Object], [])),
      function Serialize(xmlWriter, o) {
          this.Serialize(
            xmlWriter, o,
            null, null, null
          );
      }
    );

    $.Method({ Static: false, Public: true }, "Serialize",
      (new JSIL.MethodSignature(null, [
            $xmlasms[16].TypeRef("System.Xml.XmlWriter"), $.Object,
            $xmlasms[16].TypeRef("System.Xml.Serialization.XmlSerializerNamespaces")
      ], [])),
      function Serialize(xmlWriter, o, namespaces) {
          this.Serialize(
            xmlWriter, o,
            namespaces, null, null
          );
      }
    );

    $.Method({ Static: false, Public: true }, "Serialize",
      (new JSIL.MethodSignature(null, [
            $xmlasms[16].TypeRef("System.Xml.XmlWriter"), $.Object,
            $xmlasms[16].TypeRef("System.Xml.Serialization.XmlSerializerNamespaces"), $.String
      ], [])),
      function Serialize(xmlWriter, o, namespaces, encodingStyle) {
          this.Serialize(
            xmlWriter, o,
            namespaces, encodingStyle, null
          );
      }
    );

    $.Method({ Static: false, Public: true }, "Serialize",
      (new JSIL.MethodSignature(null, [
            $xmlasms[16].TypeRef("System.Xml.XmlWriter"), $.Object,
            $xmlasms[16].TypeRef("System.Xml.Serialization.XmlSerializerNamespaces"), $.String,
            $.String
      ], [])),
      function Serialize(xmlWriter, o, namespaces, encodingStyle, id) {
          var writer = this.MakeSerializationWriter(xmlWriter, namespaces, encodingStyle, id);
          var serializer = this.MakeSerializer();

          return this.SerializeInternal(serializer, writer, o);
      }
    );

});