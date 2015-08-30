//? include("../Utils/JSIL.XML.ReaderFromStream.js");

JSIL.ImplementExternals("System.Xml.XmlReader", function ($) {
    var ntNone = System.Xml.XmlNodeType.None;
    var ntElement = System.Xml.XmlNodeType.Element;
    var ntAttribute = System.Xml.XmlNodeType.Attribute;
    var ntText = System.Xml.XmlNodeType.Text;
    var ntWhitespace = System.Xml.XmlNodeType.Whitespace;
    var ntComment = System.Xml.XmlNodeType.Comment;
    var ntDocument = System.Xml.XmlNodeType.Document;
    var ntEndElement = System.Xml.XmlNodeType.EndElement;

    var docProto = (window.Document.prototype);
    var elementProto = (window.Element.prototype);
    var attrProto = (window.Attr.prototype);
    var textProto = (window.Text.prototype);

    var sNode = "node";
    var sChildren = "children";
    var sSiblings = "siblings";
    var sClosing = "closing";

    var sBeforeDocument = "before document";
    var sAfterDocument = "after document";

    $.RawMethod(false, "$fromDOMNode", function (domNode) {
        this._domNode = domNode;
        this._eof = false;
        this.$setCurrentNode(null, sBeforeDocument);
        this.nameTable = new System.Xml.XmlNameTable();
        this.advanceCount = 0;
    });

    $.RawMethod(false, "$setCurrentNode", function (node, state) {
        this._current = node;
        // Index of next attribute to be fetched using LocalName/Prefix/Value
        // when fetchMode is 'attrs'
        this.currentAttributeIndex = 0;
        // Fetch mode can be 'attrs' or 'element'. It is switched when calling funcs
        // MoveToNextAttribute and MoveToElement
        this.fetchMode = 'element';
        this._state = state;

        if ((typeof (node) === "undefined") || (node === null)) {
            this._nodeType = ntNone;
            return false;
        }

        if (typeof (node) !== "object") {
            throw new Error("Non-object node:" + String(node));
        }

        switch (node.nodeType) {
            case Node.ELEMENT_NODE:
                if (state === sClosing) {
                    this._nodeType = ntEndElement;
                } else {
                    this._nodeType = ntElement;
                }
                break;
            case Node.TEXT_NODE:
                if (System.String.IsNullOrWhiteSpace(node.nodeValue)) {
                    this._nodeType = ntWhitespace;
                } else {
                    this._nodeType = ntText;
                }
                break;
            case Node.COMMENT_NODE:
                this._nodeType = ntComment;
                break;
            case Node.DOCUMENT_NODE:
                if (state !== sClosing) {
                    // Skip directly to the root node
                    return this.$setCurrentNode(node.firstChild, "node");
                } else {
                    return this.$setCurrentNode(null, sAfterDocument);
                }
            default:
                JSIL.Host.warning("Unsupported node type: " + node.nodeType + " " + node);
                return false;
        }

        return true;
    });

    $.RawMethod(false, "$moveNext", function () {
        var cur = this._current;
        if (cur === null) {
            if (this._eof) {
                return this.$setCurrentNode(null, sAfterDocument);
            } else {
                return this.$setCurrentNode(this._domNode, sNode);
            }
        }

        if (this._state === sNode) {
            this._state = sChildren;
        }

        if (this._state === sChildren) {
            if (cur.firstChild !== null)
                return this.$setCurrentNode(cur.firstChild, sNode);

            this._state = sClosing;
        }

        if (this._state === sSiblings) {
            if (cur.nextSibling !== null)
                return this.$setCurrentNode(cur.nextSibling, sNode);

            this._state = sClosing;
        }

        if (this._state === sClosing) {
            if (cur.nextSibling !== null)
                return this.$setCurrentNode(cur.nextSibling, sNode);

            return this.$setCurrentNode(cur.parentNode, sClosing);
        }

        this._eof = true;
        return this.$setCurrentNode(null, sAfterDocument);
    });

    $.RawMethod(false, "$skip", function () {
        var cur = this._current;
        if (cur === null) {
            this._eof = true;
            return this.$setCurrentNode(null, sAfterDocument);
        }

        if (cur.nextSibling !== null) {
            return this.$setCurrentNode(cur.nextSibling, sNode);
        } else if (cur.parentNode !== null) {
            return this.$setCurrentNode(cur.parentNode, sClosing);
        } else {
            this._eof = true;
            return this.$setCurrentNode(null, sAfterDocument);
        }
    });

    $.Method({ Static: false, Public: true }, "Read",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function Read() {
          this.advanceCount += 1;
          return this.$moveNext();
      }
    );

    $.Method({ Static: false, Public: true }, "ReadToFollowing",
      (new JSIL.MethodSignature($.Boolean, [$.String], [])),
      function ReadToFollowing(localName) {
          while (this.Read()) {
              if ((this._nodeType === ntElement) && (this.get_LocalName() === localName))
                  return true;
          }

          return false;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadToNextSibling",
      (new JSIL.MethodSignature($.Boolean, [$.String], [])),
      function ReadToNextSibling(localName) {
          while (this.$skip()) {
              if ((this._nodeType === ntElement) && (this.get_LocalName() === localName))
                  return true;
              else if (this._nodeType === ntEndElement)
                  return false;
          }

          return false;
      }
    );

    $.Method({ Static: false, Public: true }, "Skip",
      (JSIL.MethodSignature.Void),
      function Skip() {
          this.$skip();
      }
    );

    $.Method({ Static: false, Public: true }, "MoveToElement",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function MoveToElement() {
          if (this.fetchMode == 'attrs') {
              this.fetchMode = 'element';
              this.currentAttributeIndex = 0;
              // true if position has been "changed"
              return true;
          }
          return false;
      }
    );

    $.Method({ Static: false, Public: true }, "MoveToFirstAttribute",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function MoveToFirstAttribute() {
          // FIXME
          return false;
      }
    );

    $.Method({ Static: false, Public: true }, "MoveToNextAttribute",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function MoveToNextAttribute() {
          if (this.fetchMode == 'element') {
              if (this._current.attributes.length != 0) {
                  this.fetchMode = 'attrs';
                  this.currentAttributeIndex = 0;
                  return true;
              } else
                  return false;
          } else {
              if (this.currentAttributeIndex + 1 < this._current.attributes.length) {
                  this.currentAttributeIndex++;
                  return true;
              } else
                  return false;
          }
      }
    );

    $.Method({ Static: false, Public: true }, "MoveToContent",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlNodeType"), [], [])),
      function MoveToContent() {
          while (true) {
              switch (this._nodeType) {
                  case ntText:
                  case ntElement:
                  case ntEndElement:
                      return this._nodeType;
              }

              if (!this.Read())
                  return this._nodeType;
          }
      }
    );

    $.RawMethod(false, "$isTextualNode", function (includingComments) {
        switch (this._nodeType) {
            case ntText:
            case ntWhitespace:
                return true;
            case ntComment:
                return includingComments;
        }

        return false;
    });

    $.Method({ Static: false, Public: true }, "get_IsEmptyElement",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function get_IsEmptyElement() {
          if (this._current === null)
              return true;

          if (this.$isTextualNode(true))
              return false;

          // The DOM makes it impossible to tell whether an element is actually an empty element.
          // Furthermore, all elements with no children become empty elements when being serialized
          //  in Mozilla.
          // So, that sucks. This is broken.

          var noChildren = (typeof (this._current.childNodes) === "undefined") ||
            (this._current.childNodes === null) ||
            (this._current.childNodes.length === 0);

          return noChildren;
      }
    );

    $.Method({ Static: false, Public: true }, "IsStartElement",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function IsStartElement() {
          return this.MoveToContent() == ntElement;
      }
    );

    $.Method({ Static: false, Public: true }, "IsStartElement",
      (new JSIL.MethodSignature($.Boolean, [$.String], [])),
      function IsStartElement(name) {
          return (this.MoveToContent() == ntElement) &&
            (this.Name == name);
      }
    );

    $.Method({ Static: false, Public: true }, "IsStartElement",
      (new JSIL.MethodSignature($.Boolean, [$.String, $.String], [])),
      function IsStartElement(localname, ns) {
          return (this.MoveToContent() == ntElement) &&
            (this.LocalName == localname) &&
            (this.NamespaceURI == ns);
      }
    );

    $.Method({ Static: false, Public: true }, "ReadStartElement",
      (JSIL.MethodSignature.Void),
      function ReadStartElement() {
          if (!this.IsStartElement())
              throw new Error("Start element not found");

          this.Read();
      }
    );

    $.Method({ Static: false, Public: true }, "ReadEndElement",
      (JSIL.MethodSignature.Void),
      function ReadEndElement() {
          if (this.MoveToContent() != ntEndElement)
              throw new Error("End element not found");

          this.Read();
      }
    );

    $.Method({ Static: false, Public: true }, "ReadStartElement",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function ReadStartElement(name) {
          if (!this.IsStartElement(name))
              throw new Error("Start element not found");

          this.Read();
      }
    );

    $.Method({ Static: false, Public: true }, "ReadStartElement",
      (new JSIL.MethodSignature(null, [$.String, $.String], [])),
      function ReadStartElement(localname, ns) {
          if (!this.IsStartElement(localname, ns))
              throw new Error("Start element not found");

          this.Read();
      }
    );

    $.Method({ Static: false, Public: true }, "ReadElementString",
      (new JSIL.MethodSignature($.String, [], [])),
      function ReadElementString() {
          this.ReadStartElement();

          var result = this.ReadString();
          this.ReadEndElement();
          return result;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadElementString",
      (new JSIL.MethodSignature($.String, [$.String], [])),
      function ReadElementString(name) {
          this.ReadStartElement(name);

          var result = this.ReadString();
          this.ReadEndElement();
          return result;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadElementString",
      (new JSIL.MethodSignature($.String, [$.String, $.String], [])),
      function ReadElementString(localname, ns) {
          this.ReadStartElement(localname, ns);

          var result = this.ReadString();
          this.ReadEndElement();
          return result;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadContentAsString",
      (new JSIL.MethodSignature($.String, [], [])),
      function ReadContentAsString() {
          return this.ReadString();
      }
    );

    $.Method({ Static: false, Public: true }, "ReadString",
      (new JSIL.MethodSignature($.String, [], [])),
      function ReadString() {
          var result = "";
          // this.MoveToElement();

          // If we're positioned on a start element, advance into the body to find the text
          if (this._nodeType == ntElement) {
              if (this.get_IsEmptyElement())
                  return result;

              if (!this.Read())
                  throw new Error("Failed to read string");

              if (this._nodeType == ntEndElement)
                  return result;
          }

          while (this.$isTextualNode(false)) {
              result += this._current.nodeValue;

              if (!this.Read())
                  break;
          }

          return result;
      }
    );

    $.Method({ Static: false, Public: true }, "ReadSubtree",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [], [])),
      function ReadSubtree() {
        var result = JSIL.CreateInstanceOfType(System.Xml.XmlReader.__Type__, "$fromDOMNode", [this._current.cloneNode(true)]);

        this._state = sSiblings;
        return result;
      }
    );

    $.RawMethod(false, "SetupReadElementContent", function () {
        if (this._nodeType != ntElement)
            throw new System.Exception("Invalid start node for ReadElementContent");

        var isEmpty = this.IsEmptyElement;

        this.Read();
        if (isEmpty)
            return false;

        if (this._nodeType == ntEndElement) {
            this.Read();
            return false;
        } else if (this._nodeType == ntElement) {
            throw new System.Exception("Element contains another element, not text content");
        }

        return true;
    });

    $.RawMethod(false, "FinishReadElementContent", function () {
        if (this._nodeType != ntEndElement)
            throw new System.Exception("Read element string content but didn't end on an EndElement");

        this.Read();
    });

    $.Method({ Static: false, Public: true }, "ReadElementContentAsString",
      (new JSIL.MethodSignature($.String, [], [])),
      function ReadElementContentAsString() {
          var result = "";

          if (this.SetupReadElementContent()) {
              result = this.ReadString();
              this.FinishReadElementContent();
          }

          return result;
      }
    );

    $.Method({ Static: false, Public: true }, "get_NodeType",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlNodeType"), [], [])),
      function get_NodeType() {
          return this._nodeType;
      }
    );

    $.Method({ Static: false, Public: true }, "get_NameTable",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlNameTable"), [], [])),
      function get_NameTable() {
          return this.nameTable;
      }
    );

    $.Method({ Static: false, Public: true }, "get_Name",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_Name() {
          if (this._current !== null) {
              if (this.fetchMode == 'element')
                  return this._current.tagName || null;
              else if (this.fetchMode == 'attrs')
                  return this._current.attributes.item(this.currentAttributeIndex).name;
          }
          return null;
      }
    );

    $.Method({ Static: false, Public: true }, "get_LocalName",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_LocalName() {
          if (this._current !== null) {
              if (this.fetchMode == 'element')
                  return this._current.localName || null;
              else if (this.fetchMode == 'attrs')
                  return this._current.attributes.item(this.currentAttributeIndex).localName;
          }
          return null;
      }
    );

    $.Method({ Static: false, Public: true }, "get_NamespaceURI",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_NamespaceURI() {
          if (this._current !== null) {
              if (this.fetchMode == 'element')
                  return this._current.namespaceURI || "";
              else if (this.fetchMode == 'attrs')
                  return this._current.attributes.item(this.currentAttributeIndex).namespaceURI;
          }
          return "";
      }
    );

    $.Method({ Static: false, Public: true }, "get_Value",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_Value() {
          if (this._current !== null) {
              if (this.fetchMode == 'element')
                  return this._current.nodeValue || null;
              else if (this.fetchMode == 'attrs')
                  return this._current.attributes.item(this.currentAttributeIndex).value;
          }
          return null;
      }
    );

    $.Method({ Static: false, Public: true }, "get_Prefix",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_Value() {
          if (this._current !== null) {
              if (this.fetchMode == 'element')
                  return this._current.prefix || '';
              else if (this.fetchMode == 'attrs')
                  return this._current.attributes.item(this.currentAttributeIndex).prefix || '';
          }
          return null;
      }
    );

    $.Method({ Static: false, Public: false }, "get_AdvanceCount",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function get_AdvanceCount() {
          return this.advanceCount;
      }
    );

    $.Method({ Static: false, Public: true }, "get_AttributeCount",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function get_AttributeCount() {
          if (this.$isTextualNode(true))
              return 0;

          if (this._current !== null)
              return this._current.attributes.length;

          return 0;
      }
    );

    $.Method({ Static: false, Public: true }, "get_HasAttributes",
            (new JSIL.MethodSignature($.Boolean, [], [])),
            function get_HasAttributes() {
                if (this._current == null) return false;
                return (this.currentAttributeIndex < this._current.attributes.length);
            }
    );

    var getAttributeByName = function GetAttribute(name) {
        if (this._current.hasAttribute && this._current.hasAttribute(name))
            return this._current.getAttribute(name);

        return null;
    };

    var getAttributeByNameNS = function GetAttribute(name, namespaceURI) {
        if (this._current.hasAttributeNS && this._current.hasAttributeNS(namespaceURI, name))
            return this._current.getAttributeNS(namespaceURI, name);

        return null;
    };

    var getAttributeByIndex = function GetAttribute(i) {
        return this._current.attributes[i].value;
    };

    $.Method({ Static: false, Public: true }, "GetAttribute",
      (new JSIL.MethodSignature($.String, [$.String], [])),
      getAttributeByName
    );

    $.Method({ Static: false, Public: true }, "GetAttribute",
      (new JSIL.MethodSignature($.String, [$.String, $.String], [])),
      getAttributeByNameNS
    );

    $.Method({ Static: false, Public: true }, "GetAttribute",
      (new JSIL.MethodSignature($.String, [$.Int32], [])),
      getAttributeByIndex
    );

    $.Method({ Static: false, Public: true }, "get_Item",
      (new JSIL.MethodSignature($.String, [$.Int32], [])),
      getAttributeByIndex
    );

    $.Method({ Static: false, Public: true }, "get_Item",
      (new JSIL.MethodSignature($.String, [$.String], [])),
      getAttributeByName
    );

    $.Method({ Static: false, Public: true }, "get_Item",
      (new JSIL.MethodSignature($.String, [$.String, $.String], [])),
      getAttributeByNameNS
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [$.String], [])),
      function Create(inputUri) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [$.String, $xmlasms[16].TypeRef("System.Xml.XmlReaderSettings")], [])),
      function Create(inputUri, settings) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [
            $.String, $xmlasms[16].TypeRef("System.Xml.XmlReaderSettings"),
            $xmlasms[16].TypeRef("System.Xml.XmlParserContext")
      ], [])),
      function Create(inputUri, settings, inputContext) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [$xmlasms[5].TypeRef("System.IO.Stream")], [])),
      function Create(input) {
          return JSIL.XML.ReaderFromStream(input);
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [$xmlasms[5].TypeRef("System.IO.Stream"), $xmlasms[16].TypeRef("System.Xml.XmlReaderSettings")], [])),
      function Create(input, settings) {
          // FIXME      
          return JSIL.XML.ReaderFromStream(input);
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [
            $xmlasms[5].TypeRef("System.IO.Stream"), $xmlasms[16].TypeRef("System.Xml.XmlReaderSettings"),
            $.String
      ], [])),
      function Create(input, settings, baseUri) {
          // FIXME      
          return JSIL.XML.ReaderFromStream(input);
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [
            $xmlasms[5].TypeRef("System.IO.Stream"), $xmlasms[16].TypeRef("System.Xml.XmlReaderSettings"),
            $xmlasms[16].TypeRef("System.Xml.XmlParserContext")
      ], [])),
      function Create(input, settings, inputContext) {
          // FIXME      
          return JSIL.XML.ReaderFromStream(input);
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [$xmlasms[5].TypeRef("System.IO.TextReader")], [])),
      function Create(input) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [$xmlasms[5].TypeRef("System.IO.TextReader"), $xmlasms[16].TypeRef("System.Xml.XmlReaderSettings")], [])),
      function Create(input, settings) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [
            $xmlasms[5].TypeRef("System.IO.TextReader"), $xmlasms[16].TypeRef("System.Xml.XmlReaderSettings"),
            $.String
      ], [])),
      function Create(input, settings, baseUri) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [
            $xmlasms[5].TypeRef("System.IO.TextReader"), $xmlasms[16].TypeRef("System.Xml.XmlReaderSettings"),
            $xmlasms[16].TypeRef("System.Xml.XmlParserContext")
      ], [])),
      function Create(input, settings, inputContext) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [$xmlasms[16].TypeRef("System.Xml.XmlReader"), $xmlasms[16].TypeRef("System.Xml.XmlReaderSettings")], [])),
      function Create(reader, settings) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Dispose",
      (JSIL.MethodSignature.Void),
      function Dispose() {
          // FIXME
      }
    );

    $.Method({ Static: false, Public: false }, "Dispose",
      (new JSIL.MethodSignature(null, [$.Boolean], [])),
      function Dispose(disposing) {
          // FIXME
      }
    );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Xml.XmlReader", true, [], function ($) {
  $.ExternalMembers(false,
    "Read", "MoveToContent",
    "get_AdvanceCount", "get_AttributeCount",
    "get_IsEmptyElement",
    "get_LocalName", "get_NameTable",
    "get_NodeType", "get_Name",
    "get_NamespaceURI", "get_Value"
  );

  $.Property({ Static: false, Public: false }, "AdvanceCount");
  $.Property({ Static: false, Public: true }, "AttributeCount");
  $.Property({ Static: false, Public: true }, "IsEmptyElement");
  $.Property({ Static: false, Public: true }, "LocalName");
  $.Property({ Static: false, Public: true }, "NodeType");
  $.Property({ Static: false, Public: true }, "Name");
  $.Property({ Static: false, Public: true }, "NameTable");
  $.Property({ Static: false, Public: true }, "NamespaceURI");
  $.Property({ Static: false, Public: true }, "Value");

  $.ImplementInterfaces($jsilcore.TypeRef("System.IDisposable"))
});
//? }