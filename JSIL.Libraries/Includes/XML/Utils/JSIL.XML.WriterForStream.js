/*? if (!('JSIL_XML_WriterForStream' in __out)) { __out.JSIL_XML_WriterForStream = true; */
JSIL.XML.WriterForStream = function (stream) {
  var result = JSIL.CreateInstanceOfType(
    System.Xml.XmlWriter.__Type__, "$forStream", [stream]
  );

  return result;
};
/*? }*/