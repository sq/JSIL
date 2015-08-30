//? include("../Utils/JSIL.XML.WriterForStream.js");

JSIL.ImplementExternals("System.Xml.XmlWriter", function ($) {

    $.RawMethod(false, "$forStream", function (stream) {
        this._disposed = false;
        this._stream = stream;
        this._stack = [];
        this._needPrologue = true;
    });

    $.RawMethod(false, "$pushElement", function (elementName) {
        var elt = {
            name: elementName,
            closePending: true,
            endElementPending: true,
            empty: true
        };

        this._stack.push(elt);

        return elt;
    });

    $.RawMethod(false, "$flush", function (forClose) {
        while (this._stack.length > 0) {
            this.$flushOne(forClose);
            this._stack.pop();
        }
    });

    $.RawMethod(false, "$flushOne", function (includeEndElement) {
        var item = this._stack[this._stack.length - 1];
        if (!item)
            return;

        if (item.empty && item.closePending && item.endElementPending && includeEndElement) {
            item.closePending = item.endElementPending = false;
            this.$write(" />");
        } else {
            if (item.closePending) {
                item.closePending = false;
                this.$write(">");
            }

            if (item.endElementPending && includeEndElement) {
                item.endElementPending = false;
                this.$write("</");
                this.$write(item.name);
                this.$write(">");
            }
        }
    });

    $.RawMethod(false, "$writeAttr", function (name, value) {
        var item = this._stack[this._stack.length - 1];
        if (!item)
            throw new Error("No element open");

        if (!item.closePending)
            throw new Error("Element start tag already closed");

        this.$write(" ");
        this.$write(name);
        this.$write("=\"");
        this.$writeEscaped(value);
        this.$write("\"");
    });

    $.RawMethod(false, "$writeEscaped", function (str) {
        this.$write(str);
    });

    $.RawMethod(false, "$write", function (str) {
        if (this._needPrologue)
            this.WriteStartDocument();

        for (var i = 0, l = str.length; i < l; i++) {
            var ch = str[i];
            var byte = ch.charCodeAt(0);
            this._stream.WriteByte(byte);
        }
    });

    $.RawMethod(false, "$dispose", function () {
        if (this._disposed)
            return;

        this._disposed = true;
        this.$flush(true);
        this._stream.Close();
    });

    $.Method({ Static: false, Public: true }, "Close",
      (JSIL.MethodSignature.Void),
      function Close() {
          this.$dispose();
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlWriter"), [$.String], [])),
      function Create(outputFileName) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlWriter"), [$.String, $xmlasms[16].TypeRef("System.Xml.XmlWriterSettings")], [])),
      function Create(outputFileName, settings) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlWriter"), [$xmlasms[5].TypeRef("System.IO.Stream")], [])),
      function Create(output) {
          return JSIL.XML.WriterForStream(output);
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlWriter"), [$xmlasms[5].TypeRef("System.IO.Stream"), $xmlasms[16].TypeRef("System.Xml.XmlWriterSettings")], [])),
      function Create(output, settings) {
          // FIXME
          return JSIL.XML.WriterForStream(output);
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlWriter"), [$xmlasms[5].TypeRef("System.IO.TextWriter")], [])),
      function Create(output) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlWriter"), [$xmlasms[5].TypeRef("System.IO.TextWriter"), $xmlasms[16].TypeRef("System.Xml.XmlWriterSettings")], [])),
      function Create(output, settings) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlWriter"), [$xmlasms[5].TypeRef("System.Text.StringBuilder")], [])),
      function Create(output) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlWriter"), [$xmlasms[5].TypeRef("System.Text.StringBuilder"), $xmlasms[16].TypeRef("System.Xml.XmlWriterSettings")], [])),
      function Create(output, settings) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlWriter"), [$xmlasms[16].TypeRef("System.Xml.XmlWriter")], [])),
      function Create(output) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlWriter"), [$xmlasms[16].TypeRef("System.Xml.XmlWriter"), $xmlasms[16].TypeRef("System.Xml.XmlWriterSettings")], [])),
      function Create(output, settings) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Flush",
      (JSIL.MethodSignature.Void),
      function Flush() {
          this.$flush(false);
      }
    );

    $.Method({ Static: false, Public: true }, "get_Settings",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlWriterSettings"), [], [])),
      function get_Settings() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "get_WriteState",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.WriteState"), [], [])),
      function get_WriteState() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "get_XmlLang",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_XmlLang() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "get_XmlSpace",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlSpace"), [], [])),
      function get_XmlSpace() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "LookupPrefix",
      (new JSIL.MethodSignature($.String, [$.String], [])),
      function LookupPrefix(ns) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteAttributes",
      (new JSIL.MethodSignature(null, [$xmlasms[16].TypeRef("System.Xml.XmlReader"), $.Boolean], [])),
      function WriteAttributes(reader, defattr) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteAttributeString",
      (new JSIL.MethodSignature(null, [
            $.String, $.String,
            $.String
      ], [])),
      function WriteAttributeString(localName, ns, value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteAttributeString",
      (new JSIL.MethodSignature(null, [$.String, $.String], [])),
      function WriteAttributeString(localName, value) {
          this.$writeAttr(localName, value);
      }
    );

    $.Method({ Static: false, Public: true }, "WriteAttributeString",
      (new JSIL.MethodSignature(null, [
            $.String, $.String,
            $.String, $.String
      ], [])),
      function WriteAttributeString(prefix, localName, ns, value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteBase64",
      (new JSIL.MethodSignature(null, [
            $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
            $.Int32
      ], [])),
      function WriteBase64(buffer, index, count) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteBinHex",
      (new JSIL.MethodSignature(null, [
            $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
            $.Int32
      ], [])),
      function WriteBinHex(buffer, index, count) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteCData",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteCData(text) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteCharEntity",
      (new JSIL.MethodSignature(null, [$.Char], [])),
      function WriteCharEntity(ch) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteChars",
      (new JSIL.MethodSignature(null, [
            $jsilcore.TypeRef("System.Array", [$.Char]), $.Int32,
            $.Int32
      ], [])),
      function WriteChars(buffer, index, count) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteComment",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteComment(text) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteDocType",
      (new JSIL.MethodSignature(null, [
            $.String, $.String,
            $.String, $.String
      ], [])),
      function WriteDocType(name, pubid, sysid, subset) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteElementString",
      (new JSIL.MethodSignature(null, [$.String, $.String], [])),
      function WriteElementString(localName, value) {
          this.WriteStartElement(localName);

          if (!System.String.IsNullOrWhiteSpace(value))
              this.WriteString(value);

          this.WriteEndElement();
      }
    );

    $.Method({ Static: false, Public: true }, "WriteElementString",
      (new JSIL.MethodSignature(null, [
            $.String, $.String,
            $.String
      ], [])),
      function WriteElementString(localName, ns, value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteElementString",
      (new JSIL.MethodSignature(null, [
            $.String, $.String,
            $.String, $.String
      ], [])),
      function WriteElementString(prefix, localName, ns, value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteEndAttribute",
      (JSIL.MethodSignature.Void),
      function WriteEndAttribute() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteEndDocument",
      (JSIL.MethodSignature.Void),
      function WriteEndDocument() {
          this.$flush(true);
      }
    );

    $.Method({ Static: false, Public: true }, "WriteEndElement",
      (JSIL.MethodSignature.Void),
      function WriteEndElement() {
          this.$flushOne(true);
          this._stack.pop();
      }
    );

    $.Method({ Static: false, Public: true }, "WriteEntityRef",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteEntityRef(name) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteFullEndElement",
      (JSIL.MethodSignature.Void),
      function WriteFullEndElement() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: false }, "WriteLocalNamespaces",
      (new JSIL.MethodSignature(null, [$xmlasms[16].TypeRef("System.Xml.XPath.XPathNavigator")], [])),
      function WriteLocalNamespaces(nsNav) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteName",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteName(name) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteNmToken",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteNmToken(name) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteNode",
      (new JSIL.MethodSignature(null, [$xmlasms[16].TypeRef("System.Xml.XmlReader"), $.Boolean], [])),
      function WriteNode(reader, defattr) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteNode",
      (new JSIL.MethodSignature(null, [$xmlasms[16].TypeRef("System.Xml.XPath.XPathNavigator"), $.Boolean], [])),
      function WriteNode(navigator, defattr) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteProcessingInstruction",
      (new JSIL.MethodSignature(null, [$.String, $.String], [])),
      function WriteProcessingInstruction(name, text) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteQualifiedName",
      (new JSIL.MethodSignature(null, [$.String, $.String], [])),
      function WriteQualifiedName(localName, ns) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteRaw",
      (new JSIL.MethodSignature(null, [
            $jsilcore.TypeRef("System.Array", [$.Char]), $.Int32,
            $.Int32
      ], [])),
      function WriteRaw(buffer, index, count) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteRaw",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteRaw(data) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteStartAttribute",
      (new JSIL.MethodSignature(null, [$.String, $.String], [])),
      function WriteStartAttribute(localName, ns) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteStartAttribute",
      (new JSIL.MethodSignature(null, [
            $.String, $.String,
            $.String
      ], [])),
      function WriteStartAttribute(prefix, localName, ns) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteStartAttribute",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteStartAttribute(localName) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteStartDocument",
      (JSIL.MethodSignature.Void),
      function WriteStartDocument() {
          this._needPrologue = false;
          this.$write('<?xml version="1.0" encoding="');
          this.$write("utf-8");
          this.$write('"?>');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteStartDocument",
      (new JSIL.MethodSignature(null, [$.Boolean], [])),
      function WriteStartDocument(standalone) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteStartElement",
      (new JSIL.MethodSignature(null, [$.String, $.String], [])),
      function WriteStartElement(localName, ns) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteStartElement",
      (new JSIL.MethodSignature(null, [
            $.String, $.String,
            $.String
      ], [])),
      function WriteStartElement(prefix, localName, ns) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteStartElement",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteStartElement(localName) {
          this.$flushOne(false);

          this.$write("<");
          this.$write(localName);

          var elt = this.$pushElement(localName);
      }
    );

    $.Method({ Static: false, Public: true }, "WriteString",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteString(text) {
          this.$flushOne(false);

          this.$writeEscaped(text);
      }
    );

    $.Method({ Static: false, Public: true }, "WriteSurrogateCharEntity",
      (new JSIL.MethodSignature(null, [$.Char, $.Char], [])),
      function WriteSurrogateCharEntity(lowChar, highChar) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteValue",
      (new JSIL.MethodSignature(null, [$.Object], [])),
      function WriteValue(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteValue",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteValue(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteValue",
      (new JSIL.MethodSignature(null, [$.Boolean], [])),
      function WriteValue(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteValue",
      (new JSIL.MethodSignature(null, [$xmlasms[5].TypeRef("System.DateTime")], [])),
      function WriteValue(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteValue",
      (new JSIL.MethodSignature(null, [$.Double], [])),
      function WriteValue(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteValue",
      (new JSIL.MethodSignature(null, [$.Single], [])),
      function WriteValue(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteValue",
      (new JSIL.MethodSignature(null, [$xmlasms[5].TypeRef("System.Decimal")], [])),
      function WriteValue(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteValue",
      (new JSIL.MethodSignature(null, [$.Int32], [])),
      function WriteValue(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteValue",
      (new JSIL.MethodSignature(null, [$.Int64], [])),
      function WriteValue(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "WriteWhitespace",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteWhitespace(ws) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Dispose",
      (JSIL.MethodSignature.Void),
      function Dispose() {
          // FIXME
          this.$dispose();
      }
    );

    $.Method({ Static: false, Public: false }, "Dispose",
      (new JSIL.MethodSignature(null, [$.Boolean], [])),
      function Dispose(disposing) {
          // FIXME
          this.$dispose();
      }
    );

});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Xml.XmlWriter", true, [], function ($) {
  $.Field({ Static: false, Public: false }, "writeNodeBuffer", $jsilcore.TypeRef("System.Array", [$.Char]));

  $.Constant({ Static: true, Public: false }, "WriteNodeBufferSize", 1024);

  $.Property({ Static: false, Public: true }, "Settings");
  $.Property({ Static: false, Public: true }, "WriteState");
  $.Property({ Static: false, Public: true }, "XmlLang");
  $.Property({ Static: false, Public: true }, "XmlSpace");

  $.ImplementInterfaces($jsilcore.TypeRef("System.IDisposable"))
});
//? }