"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");
  
JSIL.DeclareAssembly("JSIL.XML");

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.XML");
JSIL.DeclareNamespace("System");
JSIL.DeclareNamespace("System.Xml");

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
      if (state === sClosing) {
        this._nodeType = ntEndElement;
      } else {
        this._nodeType = ntElement;
      }
    } else if (proto === docProto) {
      if (state !== sClosing) {
        // Skip directly to the root node
        return this.$setCurrentNode(node.firstChild, "node");
      } else {
        return this.$setCurrentNode(null, sAfterDocument);
      }
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

      this._state = sClosing;
    } else if (this._state === sAttribute) {
      this._state = sSiblings;
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

      var noChildren = (typeof (this._current.childNodes) === "undefined") ||
        (this._current.childNodes === null) || 
        (this._current.childNodes.length === 0);

      var noAttributes = (typeof (this._current.attributes) === "undefined") ||
        (this._current.attributes === null) || 
        (this._current.attributes.length === 0);

      var noText = ((this._current.textContent === null) || (this._current.textContent.length === 0));

      return noChildren && noAttributes && noText;
    }
  );

  $.Method({Static:false, Public:true }, "get_NodeType", 
    (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlNodeType"), [], [])), 
    function get_NodeType () {
      return this._nodeType;
    }
  );  

  $.Method({Static:false, Public:true }, "get_Name", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_Name () {
      if (this._current !== null)
        return this._current.tagName || null;

      return null;
    }
  );

  $.Method({Static:false, Public:true }, "get_Value", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_Value () {
      if (this._nodeType !== ntText)
        return null;

      if (this._current !== null)
        return this._current.textContent || null;

      return null;
    }
  );

});

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

JSIL.MakeClass("System.Object", "System.Xml.XmlReader", true, [], function ($) {
  $.ExternalMembers(false, 
    "Read", "get_NodeType", 
    "get_IsEmptyElement", "get_Name", 
    "get_Value"
  );

  $.Property({Static:false, Public:true }, "NodeType");
  $.Property({Static:false, Public:true }, "IsEmptyElement");
  $.Property({Static:false, Public:true }, "Name");
  $.Property({Static:false, Public:true }, "Value");
});