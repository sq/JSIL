JSIL.ImplementExternals("System.Text.ASCIIEncoding", function ($) {
  $.RawMethod(false, "$encode", function ASCIIEncoding_Encode(string, outputBytes, outputIndex) {
    var writer = this.$makeWriter(outputBytes, outputIndex);

    var fallbackCharacter = this.fallbackCharacter.charCodeAt(0);
    var reader = this.$makeCharacterReader(string), ch;

    while (!reader.eof) {
      ch = reader.read();

      if (ch === false)
        continue;
      else if (ch <= 127)
        writer.write(ch);
      else
        writer.write(fallbackCharacter);
    }

    return writer.getResult();
  });

  $.RawMethod(false, "$decode", function ASCIIEncoding_Decode(bytes, index, count) {
    var reader = this.$makeByteReader(bytes, index, count), byte;
    var result = "";

    while (!reader.eof) {
      byte = reader.read();

      if (byte === false)
        continue;
      else if (byte > 127)
        result += this.fallbackCharacter;
      else
        result += String.fromCharCode(byte);
    }

    return result;
  });
});

JSIL.MakeClass("System.Text.Encoding", "System.Text.ASCIIEncoding", true, [], function ($) {
});