"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");
  
JSIL.DeclareAssembly("JSIL.XML");

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.XML");

var $xmlasms = new JSIL.AssemblyCollection({
    5: "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
    6: "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
    16: "System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
  });

JSIL.XML.ReaderFromStream = function (stream) {
  var bytes = new Array(stream.Length);
  stream.Read(bytes, 0, stream.Length);

  var xml = String.fromCharCode.apply(String, bytes);

  return JSIL.XML.ReaderFromString(xml);
};

JSIL.XML.ReaderFromString = function (xml) {
  var parser = new DOMParser();
  var root = parser.parseFromString(xml, "application/xml");

  var result = JSIL.CreateInstanceOfType(
    System.Xml.XmlReader.__Type__, "$fromDOMNode", [root]
  );
  return result;
};

JSIL.ImplementExternals("System.Xml.Serialization.XmlSerializationReader", function ($) {

  $.Method({Static:false, Public:false}, "Init", 
    (new JSIL.MethodSignature(null, [
          $xmlasms[16].TypeRef("System.Xml.XmlReader"), $xmlasms[16].TypeRef("System.Xml.Serialization.XmlDeserializationEvents"), 
          $.String, $xmlasms[16].TypeRef("System.Xml.Serialization.TempAssembly")
        ], [])), 
    function Init (r, events, encodingStyle, tempAssembly) {
      this.r = r;
    }
  );

});

JSIL.ImplementExternals("System.Xml.XmlReader", function ($) {
  var ntNone = System.Xml.XmlNodeType.None;
  var ntElement = System.Xml.XmlNodeType.Element;
  var ntAttribute = System.Xml.XmlNodeType.Attribute;
  var ntText = System.Xml.XmlNodeType.Text;
  var ntComment = System.Xml.XmlNodeType.Comment;
  var ntDocument = System.Xml.XmlNodeType.Document;
  var ntEndElement = System.Xml.XmlNodeType.EndElement;

  var docProto = (window.Document.prototype);
  var elementProto = (window.Element.prototype);
  var attrProto = (window.Attr.prototype);
  var textProto = (window.Text.prototype);

  var sAttribute = "attribute";
  var sAttributes = "attributes";

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
  });

  $.RawMethod(false, "$setCurrentNode", function (node, state) {
    this._current = node;
    this._state = state;

    if ((typeof (node) === "undefined") || (node === null)) {
      this._nodeType = ntNone;
      return false;
    }

    if (typeof (node) !== "object") {
      throw new Error("Non-object node:" + String(node));
    }

    var proto = Object.getPrototypeOf(node);

    if (proto === attrProto) {
      this._nodeType = ntAttribute;
    } else if (proto === textProto) {
      this._nodeType = ntText;
    } else if (proto === elementProto) {
      this._nodeType = ntElement;
    } else if (proto === docProto) {
      if (closing) {
        this._current = null;
        this._closing = false;
        return false;
      }

      this._nodeType = ntDocument;
    } else {
      JSIL.Host.warning("Unknown node type: ", node);
      this._nodeType = ntNone;
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
      this._state = sAttributes;
    }

    if (this._state === sAttributes) {
      if ((cur.attributes !== null) && (cur.attributes.length > 0))
        return this.$setCurrentNode(cur.attributes[0], sAttribute);

      this._state = sChildren;
    }

    if (this._state === sChildren) {
      if (cur.firstChild !== null)
        return this.$setCurrentNode(cur.firstChild, sNode);

      this._state = sSiblings;
    } else if (this._state === sAttribute) {
      this._state = sSiblings;
    }

    if (this._state === sSiblings) {
      if (cur.nextSibling !== null)
        return this.$setCurrentNode(cur.nextSibling, sNode);

      return this.$setCurrentNode(cur.parentNode, sClosing);
    }

    if (this._state === sClosing) {
      if (cur.nextSibling !== null)
        return this.$setCurrentNode(cur.nextSibling, sNode);

      return this.$setCurrentNode(cur.parentNode, sClosing);
    }

    this._eof = true;
    return this.$setCurrentNode(null, sAfterDocument);
  });

  $.Method({Static:false, Public:true }, "Read", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function Read () {
      return this.$moveNext();
    }
  );

  $.Method({Static:false, Public:true }, "get_IsEmptyElement", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsEmptyElement () {
      if (this._current === null)
        return true;      
      else if (this._nodeType === ntNone)
        return true;

      return (this._current.children.length === 0) && 
        (this._current.attributes.length === 0) && 
        ((this._current.textContent === null) || (this._current.textContent === ""));
    }
  );

  $.Method({Static:false, Public:true }, "get_NodeType", 
    (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlNodeType"), [], [])), 
    function get_NodeType () {
      return this._nodeType;
    }
  );  

});
