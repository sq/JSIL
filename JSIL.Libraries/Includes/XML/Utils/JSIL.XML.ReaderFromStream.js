/*? if (!('JSIL_XML_ReaderFromStream' in __out)) { __out.JSIL_XML_ReaderFromStream = true; */
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
/*? }*/