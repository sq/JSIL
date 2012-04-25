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

  if ((root === null) || (root.documentElement.localName == "parsererror")) {
    throw new Error("Failed to parse XML document");
  }

  var result = JSIL.CreateInstanceOfType(
    System.Xml.XmlReader.__Type__, "$fromDOMNode", [root]
  );
  return result;
};

JSIL.ImplementExternals("System.Xml.Serialization.XmlSerializer", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$xmlasms[5].TypeRef("System.Type")], [])), 
    function _ctor (type) {
      this.type = type;
    }
  );

  $.Method({Static:false, Public:true }, "Deserialize", 
    (new JSIL.MethodSignature($.Object, [$xmlasms[5].TypeRef("System.IO.Stream")], [])), 
    function Deserialize (stream) {
      var xmlReader = JSIL.XML.ReaderFromStream(stream);

      var getType = function (name) {
        var parsed = JSIL.ParseTypeName(name);
        return JSIL.GetTypeInternal(parsed, JSIL.GlobalNamespace, true);
      };

      var readerName = "Microsoft.Xml.Serialization.GeneratedAssembly.XmlSerializationReader" + this.type.Name;
      var readerType = getType(readerName);
      var reader = JSIL.CreateInstanceOfType(readerType);
      reader.Init(xmlReader, null, null, null); 

      var serializerName = "Microsoft.Xml.Serialization.GeneratedAssembly." + this.type.Name + "Serializer";
      var serializerType = getType(serializerName);
      var serializer = JSIL.CreateInstanceOfType(serializerType);

      var signature = new JSIL.MethodSignature($.Object, [$xmlasms[16].System.Xml.Serialization.XmlSerializationReader], []);

      return signature.CallVirtual("Deserialize", null, serializer, reader);
    }
  );
});

JSIL.ImplementExternals("System.Xml.Serialization.XmlSerializationReader", function ($) {

  $.Method({Static:false, Public:false}, "Init", 
    (new JSIL.MethodSignature(null, [
          $xmlasms[16].TypeRef("System.Xml.XmlReader"), $xmlasms[16].TypeRef("System.Xml.Serialization.XmlDeserializationEvents"), 
          $.String, $xmlasms[16].TypeRef("System.Xml.Serialization.TempAssembly")
        ], [])), 
    function Init (r, events, encodingStyle, tempAssembly) {
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

  $.Method({Static:false, Public:false}, "CheckReaderCount", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", [$.Int32]), $jsilcore.TypeRef("JSIL.Reference", [$.Int32])], [])), 
    function CheckReaderCount (/* ref */ whileIterations, /* ref */ readerCount) {
      if (true) {
        whileIterations.value += 1;

        if ((whileIterations.value & 128) == 128) {
          if (readerCount.value == this.ReaderCount) {
            throw new InvalidOperationException("XmlReader is stuck");
          }

          readerCount.value = this.ReaderCount;
        }
      }

    }
  );

  $.Method({Static:false, Public:true }, "get_Reader", 
    (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlReader"), [], [])), 
    function get_Reader () {
      return this.r;
    }
  );

  $.Method({Static:false, Public:true }, "get_ReaderCount", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_ReaderCount () {
      // Can't use the property because of type system nonsense.
      return this.r.advanceCount;
    }
  );

  $.RawMethod(false, "$getNullAttribute", function () {
    var a = this.r.GetAttribute(this.nilID, this.instanceNsID);
    if (a !== null)
      return Microsoft.Xml.XmlConvert.ToBoolean(a);

    a = this.r.GetAttribute(this.nilID, this.instanceNs2000ID);
    if (a !== null)
      return Microsoft.Xml.XmlConvert.ToBoolean(a);

    a = this.r.GetAttribute(this.nilID, this.instanceNs1999ID);
    if (a !== null)
      return Microsoft.Xml.XmlConvert.ToBoolean(a);

    return false;
  });

  $.Method({Static:false, Public:false}, "ReadNull", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function ReadNull () {
      if (!this.$getNullAttribute())
        return false;

      this.r.Skip();
      return true;
    }
  );

  $.Method({Static:false, Public:true }, "ReadEndElement", 
    (new JSIL.MethodSignature(null, [], [])), 
    function ReadEndElement () {
      while (this.r.NodeType == System.Xml.XmlNodeType.Whitespace)
        this.r.Skip();

      if (this.r.NodeType == System.Xml.XmlNodeType.None) {
        this.r.Skip();
        return;
      }

      this.r.ReadEndElement();
    }
  );

  $.Method({Static:false, Public:false}, "GetXsiType", 
    (new JSIL.MethodSignature($xmlasms[16].TypeRef("System.Xml.XmlQualifiedName"), [], [])), 
    function GetXsiType () {
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

});

JSIL.ImplementExternals("System.Xml.XmlQualifiedName", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this.name = "";
      this.ns = "";
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function _ctor (name) {
      this.name = name;
      this.ns = "";
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.String, $.String], [])), 
    function _ctor (name, ns) {
      this.name = name;
      this.ns = ns;
    }
  );

  $.Method({Static:false, Public:true }, "get_Name", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_Name () {
      return this.name;
    }
  );

  $.Method({Static:false, Public:true }, "get_Namespace", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_Namespace () {
      return this.ns;
    }
  );

  var equalsImpl = function (lhs, rhs) {
    if (lhs === rhs)
      return true;

    if ((lhs === null) || (rhs === null))
      return lhs === rhs;

    return (lhs.name == rhs.name) && (lhs.ns == rhs.ns);
  }

  $.Method({Static:true , Public:true }, "op_Equality", 
    (new JSIL.MethodSignature($.Boolean, [$xmlasms[16].TypeRef("System.Xml.XmlQualifiedName"), $xmlasms[16].TypeRef("System.Xml.XmlQualifiedName")], [])), 
    function op_Equality (a, b) {
      return equalsImpl(a, b);
    }
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, [$xmlasms[16].TypeRef("System.Xml.XmlQualifiedName"), $xmlasms[16].TypeRef("System.Xml.XmlQualifiedName")], [])), 
    function op_Inequality (a, b) {
      return !equalsImpl(a, b);
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
    this.advanceCount = 0;
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

  $.Method({Static:false, Public:true }, "Read", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function Read () {
      this.advanceCount += 1;
      return this.$moveNext();
    }
  );

  $.Method({Static:false, Public:true }, "Skip", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Skip () {
      this.$skip();
    }
  );

  $.Method({Static:false, Public:true }, "MoveToElement", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function MoveToElement () {
      // FIXME
      return true;
    }
  );

  $.Method({Static:false, Public:true }, "MoveToFirstAttribute", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function MoveToFirstAttribute () {
      // FIXME
      return false;
    }
  );

  $.Method({Static:false, Public:true }, "MoveToNextAttribute", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function MoveToNextAttribute () {
      // FIXME
      return false;
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

  $.Method({Static:false, Public:true }, "get_IsEmptyElement", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsEmptyElement () {
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

  $.Method({Static:false, Public:true }, "IsStartElement", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function IsStartElement () {
      return this.MoveToContent() == ntElement;
    }
  );

  $.Method({Static:false, Public:true }, "IsStartElement", 
    (new JSIL.MethodSignature($.Boolean, [$.String], [])), 
    function IsStartElement (name) {
      return (this.MoveToContent() == ntElement) &&
        (this.Name == name);
    }
  );

  $.Method({Static:false, Public:true }, "IsStartElement", 
    (new JSIL.MethodSignature($.Boolean, [$.String, $.String], [])), 
    function IsStartElement (localname, ns) {
      return (this.MoveToContent() == ntElement) &&
        (this.LocalName == localname) &&
        (this.NamespaceURI == ns);
    }
  );

  $.Method({Static:false, Public:true }, "ReadStartElement", 
    (new JSIL.MethodSignature(null, [], [])), 
    function ReadStartElement () {
      if (!this.IsStartElement())
        throw new Error("Start element not found");

      this.Read();
    }
  );

  $.Method({Static:false, Public:true }, "ReadEndElement", 
    (new JSIL.MethodSignature(null, [], [])), 
    function ReadEndElement () {
      if (this.MoveToContent() != ntEndElement)
        throw new Error("End element not found");

      this.Read();
    }
  );

  $.Method({Static:false, Public:true }, "ReadStartElement", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function ReadStartElement (name) {
      if (!this.IsStartElement(name))
        throw new Error("Start element not found");

      this.Read();
    }
  );

  $.Method({Static:false, Public:true }, "ReadStartElement", 
    (new JSIL.MethodSignature(null, [$.String, $.String], [])), 
    function ReadStartElement (localname, ns) {
      if (!this.IsStartElement(localname, ns))
        throw new Error("Start element not found");

      this.Read();
    }
  );

  $.Method({Static:false, Public:true }, "ReadElementString", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function ReadElementString () {
      this.ReadStartElement();

      var result = this.ReadString();
      this.ReadEndElement();
      return result;
    }
  );

  $.Method({Static:false, Public:true }, "ReadElementString", 
    (new JSIL.MethodSignature($.String, [$.String], [])), 
    function ReadElementString (name) {
      this.ReadStartElement(name);

      var result = this.ReadString();
      this.ReadEndElement();
      return result;
    }
  );

  $.Method({Static:false, Public:true }, "ReadElementString", 
    (new JSIL.MethodSignature($.String, [$.String, $.String], [])), 
    function ReadElementString (localname, ns) {
      this.ReadStartElement(localname, ns);

      var result = this.ReadString();
      this.ReadEndElement();
      return result;
    }
  );

  $.Method({Static:false, Public:true }, "ReadString", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function ReadString () {
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

  $.Method({Static:false, Public:false}, "get_AdvanceCount", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_AdvanceCount () {
      return this.advanceCount;
    }
  );

  $.Method({Static:false, Public:true }, "get_AttributeCount", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_AttributeCount () {
      if (this.$isTextualNode(true))
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
    "get_AdvanceCount", "get_AttributeCount", 
    "get_IsEmptyElement", 
    "get_LocalName", "get_NameTable",
    "get_NodeType", "get_Name", 
    "get_NamespaceURI", "get_Value"
  );

  $.Property({Static:false, Public:false}, "AdvanceCount");
  $.Property({Static:false, Public:true }, "AttributeCount");
  $.Property({Static:false, Public:true }, "IsEmptyElement");
  $.Property({Static:false, Public:true }, "LocalName");
  $.Property({Static:false, Public:true }, "NodeType");
  $.Property({Static:false, Public:true }, "Name");
  $.Property({Static:false, Public:true }, "NameTable");
  $.Property({Static:false, Public:true }, "NamespaceURI");
  $.Property({Static:false, Public:true }, "Value");
});

JSIL.ImplementExternals("System.Xml.XmlConvert", function ($) {

  $.Method({Static:true , Public:true }, "ToDouble", 
    (new JSIL.MethodSignature($.Double, [$.String], [])), 
    function ToDouble (s) {
      return parseFloat(s);
    }
  );

  $.Method({Static:true , Public:true }, "ToSingle", 
    (new JSIL.MethodSignature($.Single, [$.String], [])), 
    function ToSingle (s) {
      return parseFloat(s);
    }
  );

  $.Method({Static:true , Public:true }, "ToInt16", 
    (new JSIL.MethodSignature($.Int16, [$.String], [])), 
    function ToInt16 (s) {
      var i = parseInt(s, 10);
      if (isNaN(i))
        throw new Error("Invalid integer");

      return i;
    }
  );

  $.Method({Static:true , Public:true }, "ToInt32", 
    (new JSIL.MethodSignature($.Int32, [$.String], [])), 
    function ToInt32 (s) {
      var i = parseInt(s, 10);
      if (isNaN(i))
        throw new Error("Invalid integer");

      return i;
    }
  );

  $.Method({Static:true , Public:true }, "ToInt64", 
    (new JSIL.MethodSignature($.Int64, [$.String], [])), 
    function ToInt64 (s) {
      var i = parseInt(s, 10);
      if (isNaN(i))
        throw new Error("Invalid integer");

      return i;
    }
  );

  $.Method({Static:true , Public:true }, "ToUInt16", 
    (new JSIL.MethodSignature($.UInt16, [$.String], [])), 
    function ToUInt16 (s) {
      var i = parseInt(s, 10);
      if (isNaN(i) || i < 0)
        throw new Error("Invalid unsigned integer");

      return i;
    }
  );

  $.Method({Static:true , Public:true }, "ToUInt32", 
    (new JSIL.MethodSignature($.UInt32, [$.String], [])), 
    function ToUInt32 (s) {
      var i = parseInt(s, 10);
      if (isNaN(i) || i < 0)
        throw new Error("Invalid unsigned integer");

      return i;
    }
  );

  $.Method({Static:true , Public:true }, "ToUInt64", 
    (new JSIL.MethodSignature($.UInt64, [$.String], [])), 
    function ToUInt64 (s) {
      var i = parseInt(s, 10);
      if (isNaN(i) || i < 0)
        throw new Error("Invalid unsigned integer");

      return i;
    }
  );

  $.Method({Static:true , Public:true }, "ToBoolean", 
    (new JSIL.MethodSignature($.Boolean, [$.String], [])), 
    function ToBoolean (s) {
      var text = s.toLowerCase().trim();
      if (text == "true")
        return true;
      else if (text == "false")
        return false;

      var i = parseInt(s, 10);
      if (isNaN(i))
        throw new Error("Invalid boolean");

      return (i != 0);
    }
  );

  $.Method({Static:true , Public:true }, "ToByte", 
    (new JSIL.MethodSignature($.Byte, [$.String], [])), 
    function ToByte (s) {
      var i = parseInt(s, 10);
      if (isNaN(i) || i < 0 || i > 255)
        throw new Error("Invalid byte");

      return i;
    }
  );

});