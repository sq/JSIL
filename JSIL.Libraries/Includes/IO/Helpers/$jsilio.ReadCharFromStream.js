$jsilio.ReadCharFromStream = function ReadCharFromStream(stream, encoding) {
  encoding.fallbackCharacter = "\uFFFF";
  var oldPosition = stream.Position;
  var firstChar = null, actualLength;

  var minCharLength = encoding.minimumCharLength || 1;
  var maxCharLength = encoding.maximumCharLength || 4;

  var bytes = JSIL.Array.New(System.Byte, maxCharLength);

  for (var i = minCharLength; i <= maxCharLength; i++) {
    stream.Position = oldPosition;

    // A valid UTF-8 codepoint is 1-4 bytes
    var bytesRead = stream.Read(bytes, 0, i);

    var str = encoding.$decode(bytes, 0, bytesRead);
    if (str.length < 1)
      continue;

    firstChar = str[0];
    if (firstChar === encoding.fallbackCharacter)
      continue;

    return firstChar;
  }

  return null;
};