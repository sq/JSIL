JSIL.ImplementExternals("System.Xml.Serialization.XmlSerializationReader", function ($) {

    $.Method({ Static: false, Public: false }, "Init",
      (new JSIL.MethodSignature(null, [
            $xmlasms[16].TypeRef("System.Xml.XmlReader"), $xmlasms[16].TypeRef("System.Xml.Serialization.XmlDeserializationEvents"),
            $.String, $xmlasms[16].TypeRef("System.Xml.Serialization.TempAssembly")
      ], [])),
      function Init(r, events, encodingStyle, tempAssembly) {
          this.r = r;

          this.typeID = r.NameTable.Add("type");
          this.nullID = r.NameTable.Add("null");
          this.nilID = r.NameTable.Add("nil");

          this.instanceNsID = r.NameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
          this.instanceNs2000ID = r.NameTable.Add("http://www.w3.org/2000/10/XMLSchema-instance");
          this.instanceNs1999ID = r.NameTable.Add("http://www.w3.org/1999/XMLSchema-instance");

          this.InitIDs();
      }
    );

    $.Method({ Static: false, Public: false }, "CheckReaderCount",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", [$.Int32]), $jsilcore.TypeRef("JSIL.Reference", [$.Int32])], [])),
      function CheckReaderCount(/* ref */ whileIterations, /* ref */ readerCount) {
          if (true) {
              whileIterations.set(whileIterations.get() + 1);

              if ((whileIterations.get() & 128) == 128) {
                  if (readerCount.get() == this.ReaderCount) {
                      throw new InvalidOperationException("XmlReader is stuck");
                  }

                  readerCount.set(this.ReaderCount);
              }
          }

      }
    );

    $.Method({ Static: false, Public: true }, "get_Reader",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [], [])),
      function get_Reader() {
          return this.r;
      }
    );

    $.Method({ Static: false, Public: true }, "get_ReaderCount",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function get_ReaderCount() {
          // Can't use the property because of type system nonsense.
          return this.r.advanceCount;
      }
    );

    $.RawMethod(false, "$getNullAttribute", function () {
        var a = this.r.GetAttribute(this.nilID, this.instanceNsID);
        if (a !== null)
            return System.Xml.XmlConvert.ToBoolean(a);

        a = this.r.GetAttribute(this.nilID, this.instanceNs2000ID);
        if (a !== null)
            return System.Xml.XmlConvert.ToBoolean(a);

        a = this.r.GetAttribute(this.nilID, this.instanceNs1999ID);
        if (a !== null)
            return System.Xml.XmlConvert.ToBoolean(a);

        return false;
    });

    $.Method({ Static: false, Public: false }, "ReadNull",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function ReadNull() {
          if (!this.$getNullAttribute())
              return false;

          this.r.Skip();
          return true;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadEndElement",
      (JSIL.MethodSignature.Void),
      function ReadEndElement() {
          while (this.r.NodeType == System.Xml.XmlNodeType.Whitespace)
              this.r.Skip();

          if (this.r.NodeType == System.Xml.XmlNodeType.None) {
              this.r.Skip();
              return;
          }

          this.r.ReadEndElement();
      }
    );

    $.Method({ Static: false, Public: false }, "GetXsiType",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlQualifiedName"), [], [])),
      function GetXsiType() {
          var result = new System.Xml.XmlQualifiedName();

          var r = this.r;
          var a = r.GetAttribute(this.typeID, this.instanceNsID);
          if (a !== null) {
              result.name = a;
              return result;
          }

          a = r.GetAttribute(this.typeID, this.instanceNs2000ID);
          if (a !== null) {
              result.name = a;
              return result;
          }

          a = r.GetAttribute(this.typeID, this.instanceNs1999ID);
          if (a !== null) {
              result.name = a;
              return result;
          }

          return null;
      }
    );

    $.Method({ Static: false, Public: false }, "ReadSerializable",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.Serialization.IXmlSerializable"), [$xmlasms[16].TypeRef("System.Xml.Serialization.IXmlSerializable")], [])),
      function ReadSerializable(serializable) {
          return this.ReadSerializable(serializable, false);
      }
    );

    $.Method({ Static: false, Public: false }, "ReadSerializable",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.Serialization.IXmlSerializable"), [$xmlasms[16].TypeRef("System.Xml.Serialization.IXmlSerializable"), $.Boolean], [])),
      function ReadSerializable(serializable, wrappedAny) {
          var localName, namespace;

          if (wrappedAny) {
              localName = this.r.LocalName;
              namespace = this.r.NamespaceURI;
              this.r.Read();
              this.r.MoveToContent();
          }

          serializable.ReadXml(this.r);

          if (wrappedAny) {
              var ntNone = System.Xml.XmlNodeType.None;
              var ntWhitespace = System.Xml.XmlNodeType.Whitespace;
              var ntEndElement = System.Xml.XmlNodeType.EndElement;

              while (this.r.NodeType === ntWhitespace)
                  this.r.Skip();

              if (this.r.NodeType === ntNone)
                  this.r.Skip();

              if (
                this.r.NodeType === ntEndElement &&
                this.r.LocalName === localName &&
                this.r.NamespaceURI === namespace
              )
                  this.Reader.Read();
          }

          return serializable;
      }
    );

});