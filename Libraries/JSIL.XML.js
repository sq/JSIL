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

      this.InitIDs();
    }
  );

  $.Method({Static:false, Public:true }, "get_Reader", 
    (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [], [])), 
    function get_Reader () {
      return this.r;
    }
  );

});

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
        JSIL.Host.warning("Unsupported node type: ", node.nodeType, " ", node);
        break;
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

  $.Method({Static:false, Public:true }, "Read", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function Read () {
      return this.$moveNext();
    }
  );

  $.Method({Static:false, Public:true }, "MoveToContent", 
    (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlNodeType"), [], [])), 
    function MoveToContent () {
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

  $.RawMethod(false, "$isTextualNode", function () {
    switch (this._nodeType) {
      case ntText:
      case ntComment:
      case ntWhitespace:
        return true;
    }

    return false;
  });

  $.Method({Static:false, Public:true }, "get_IsEmptyElement", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsEmptyElement () {
      if (this._current === null)
        return true;      

      if (this.$isTextualNode())
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

  $.Method({Static:false, Public:true }, "get_NodeType", 
    (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlNodeType"), [], [])), 
    function get_NodeType () {
      return this._nodeType;
    }
  );  

  $.Method({Static:false, Public:true }, "get_NameTable", 
    (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlNameTable"), [], [])), 
    function get_NameTable () {
      return this.nameTable;
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

  $.Method({Static:false, Public:true }, "get_LocalName", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_LocalName () {
      if (this._current !== null)
        return this._current.localName || null;

      return null;
    }
  );

  $.Method({Static:false, Public:true }, "get_NamespaceURI", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_NamespaceURI () {
      if (this._current !== null)
        return this._current.namespaceURI || "";

      return "";
    }
  );

  $.Method({Static:false, Public:true }, "get_Value", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_Value () {
      if (this._current !== null)
        return this._current.nodeValue || null;

      return null;
    }
  );

  $.Method({Static:false, Public:true }, "get_AttributeCount", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_AttributeCount () {
      if (this.$isTextualNode())
        return 0;

      if (this._current !== null)
        return this._current.attributes.length;

      return 0;
    }
  );

  $.Method({Static:false, Public:true }, "GetAttribute", 
    (new JSIL.MethodSignature($.String, [$.String], [])), 
    function GetAttribute (name) {
      if (this._current.hasAttribute(name))
        return this._current.getAttribute(name);

      return null;
    }
  );

  $.Method({Static:false, Public:true }, "GetAttribute", 
    (new JSIL.MethodSignature($.String, [$.String, $.String], [])), 
    function GetAttribute (name, namespaceURI) {
      if (this._current.hasAttributeNS(namespaceURI, name))
        return this._current.getAttributeNS(namespaceURI, name);

      return null;
    }
  );

  $.Method({Static:false, Public:true }, "GetAttribute", 
    (new JSIL.MethodSignature($.String, [$.Int32], [])), 
    function GetAttribute (i) {      
      return this._current.attributes[i].value;
    }
  );

});

JSIL.ImplementExternals("System.Xml.XmlNameTable", function ($) {
  $.Method({Static:false, Public:false}, ".ctor", 
    new JSIL.MethodSignature(null, [], []),
    function () {
      this._names = {};
    }
  );

  $.Method({Static:false, Public:true }, "Add", 
    new JSIL.MethodSignature($.String, [$.String], []),
    function Add (str) {
      var result = this._names[str];
      if (typeof (result) === "string")
        return result;

      this._names[str] = str;
      return str;
    }
  );

  $.Method({Static:false, Public:true }, "Get", 
    new JSIL.MethodSignature($.String, [$.String], []),
    function Get (str) {
      var result = this._names[str];

      if (typeof (result) !== "string")
        return null;

      return result;
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

JSIL.MakeClass("System.Object", "System.Xml.XmlNameTable", true, [], function ($) {
  $.ExternalMembers(false,
    ".ctor", "Add", "Get"
  );
});

JSIL.MakeClass("System.Object", "System.Xml.XmlReader", true, [], function ($) {
  $.ExternalMembers(false, 
    "Read", "MoveToContent",
    "get_AttributeCount", "get_IsEmptyElement", 
    "get_LocalName", "get_NameTable",
    "get_NodeType", "get_Name", 
    "get_NamespaceURI", "get_Value"
  );

  $.Property({Static:false, Public:true }, "AttributeCount");
  $.Property({Static:false, Public:true }, "IsEmptyElement");
  $.Property({Static:false, Public:true }, "LocalName");
  $.Property({Static:false, Public:true }, "NodeType");
  $.Property({Static:false, Public:true }, "Name");
  $.Property({Static:false, Public:true }, "NameTable");
  $.Property({Static:false, Public:true }, "NamespaceURI");
  $.Property({Static:false, Public:true }, "Value");
});