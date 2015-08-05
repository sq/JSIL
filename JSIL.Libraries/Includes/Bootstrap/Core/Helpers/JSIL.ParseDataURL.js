JSIL.ParseDataURL = function (dataUrl) {
  var colonIndex = dataUrl.indexOf(":");
  if ((colonIndex != 4) || (dataUrl.substr(0, 5) !== "data:"))
    JSIL.RuntimeError("Invalid Data URL header");

  var semicolonIndex = dataUrl.indexOf(";");
  var mimeType = dataUrl.substr(colonIndex + 1, semicolonIndex - colonIndex - 1);

  var commaIndex = dataUrl.indexOf(",");
  if (commaIndex <= semicolonIndex)
    JSIL.RuntimeError("Invalid Data URL header");

  var encodingType = dataUrl.substr(semicolonIndex + 1, commaIndex - semicolonIndex - 1);
  if (encodingType.toLowerCase() !== "base64")
    JSIL.RuntimeError("Invalid Data URL encoding type: " + encodingType);

  var base64 = dataUrl.substr(commaIndex + 1);
  var bytes = System.Convert.FromBase64String(base64);

  return [mimeType, bytes];
};