//? include("../Utils/$jsilcore.$ParseInt.js");
//? include("../Utils/$jsilcore.$ParseFloat.js");
//? include("../Utils/$jsilcore.$ParseBoolean.js");
JSIL.ImplementExternals("System.Convert", function ($) {
  var base64IgnoredCodepoints = [
    9, 10, 13, 32
  ];

  var base64Table = [
    'A', 'B', 'C', 'D',
    'E', 'F', 'G', 'H',
    'I', 'J', 'K', 'L',
    'M', 'N', 'O', 'P',
    'Q', 'R', 'S', 'T',
    'U', 'V', 'W', 'X',
    'Y', 'Z',
    'a', 'b', 'c', 'd',
    'e', 'f', 'g', 'h',
    'i', 'j', 'k', 'l',
    'm', 'n', 'o', 'p',
    'q', 'r', 's', 't',
    'u', 'v', 'w', 'x',
    'y', 'z',
    '0', '1', '2', '3',
    '4', '5', '6', '7',
    '8', '9',
    '+', '/'
  ];

  var base64CodeTable = new Array(base64Table.length);
  for (var i = 0; i < base64Table.length; i++)
    base64CodeTable[i] = base64Table[i].charCodeAt(0);

  var toBase64StringImpl = function ToBase64String(inArray, offset, length, options) {
    if (options)
      JSIL.RuntimeError("Base64FormattingOptions not implemented");

    var reader = $jsilcore.makeByteReader(inArray, offset, length);
    var result = "";
    var ch1 = 0, ch2 = 0, ch3 = 0, bits = 0, equalsCount = 0, sum = 0;
    var mask1 = (1 << 24) - 1, mask2 = (1 << 18) - 1, mask3 = (1 << 12) - 1, mask4 = (1 << 6) - 1;
    var shift1 = 18, shift2 = 12, shift3 = 6, shift4 = 0;

    while (true) {
      ch1 = reader.read();
      ch2 = reader.read();
      ch3 = reader.read();

      if (ch1 === false)
        break;
      if (ch2 === false) {
        ch2 = 0;
        equalsCount += 1;
      }
      if (ch3 === false) {
        ch3 = 0;
        equalsCount += 1;
      }

      // Seems backwards, but is right!
      sum = (ch1 << 16) | (ch2 << 8) | (ch3 << 0);

      bits = (sum & mask1) >> shift1;
      result += base64Table[bits];
      bits = (sum & mask2) >> shift2;
      result += base64Table[bits];

      if (equalsCount < 2) {
        bits = (sum & mask3) >> shift3;
        result += base64Table[bits];
      }

      if (equalsCount === 2) {
        result += "==";
      } else if (equalsCount === 1) {
        result += "=";
      } else {
        bits = (sum & mask4) >> shift4;
        result += base64Table[bits];
      }
    }

    return result;
  };

  $.Method({ Static: true, Public: true }, "ToBase64String",
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])),
    function ToBase64String(inArray) {
      return toBase64StringImpl(inArray, 0, inArray.length, 0);
    }
  );

  $.Method({ Static: true, Public: true }, "ToBase64String",
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte]), $jsilcore.TypeRef("System.Base64FormattingOptions")], [])),
    function ToBase64String(inArray, options) {
      return toBase64StringImpl(inArray, 0, inArray.length, options);
    }
  );

  $.Method({ Static: true, Public: true }, "ToBase64String",
    (new JSIL.MethodSignature($.String, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
          $.Int32
    ], [])),
    function ToBase64String(inArray, offset, length) {
      return toBase64StringImpl(inArray, offset, length, 0);
    }
  );

  $.Method({ Static: true, Public: true }, "ToBase64String",
    (new JSIL.MethodSignature($.String, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
          $.Int32, $jsilcore.TypeRef("System.Base64FormattingOptions")
    ], [])),
    toBase64StringImpl
  );

  $.Method({ Static: true, Public: true }, "FromBase64String",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.String], [])),
    function FromBase64String(s) {
      var lengthErrorMessage = "Invalid length for a Base-64 char array.";
      var contentErrorMessage = "The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or a non-white space character among the padding characters.";

      var result = [];
      var reader = $jsilcore.makeCharacterReader(s);
      var sum = 0;
      var ch0 = 0, ch1 = 0, ch2 = 0, ch3 = 0;
      var index0 = -1, index1 = -1, index2 = -1, index3 = -1;
      var equals = "=".charCodeAt(0);

      while (true) {
        ch0 = reader.read();
        if (ch0 === false)
          break;
        if (base64IgnoredCodepoints.indexOf(ch0) >= 0)
          continue;

        ch1 = reader.read();
        ch2 = reader.read();
        ch3 = reader.read();

        if ((ch1 === false) || (ch2 === false) || (ch3 === false))
          throw new System.FormatException(lengthErrorMessage);

        index0 = base64CodeTable.indexOf(ch0);
        index1 = base64CodeTable.indexOf(ch1);
        index2 = base64CodeTable.indexOf(ch2);
        index3 = base64CodeTable.indexOf(ch3);

        if (
          (index0 < 0) || (index0 > 63) ||
          (index1 < 0) || (index1 > 63)
        )
          throw new System.FormatException(contentErrorMessage);

        sum = (index0 << 18) | (index1 << 12);

        if (index2 >= 0)
          sum |= (index2 << 6);
        else if (ch2 !== equals)
          throw new System.FormatException(contentErrorMessage);

        if (index3 >= 0)
          sum |= (index3 << 0);
        else if (ch3 !== equals)
          throw new System.FormatException(contentErrorMessage);

        result.push((sum >> 16) & 0xFF);
        if (index2 >= 0)
          result.push((sum >> 8) & 0xFF);
        if (index3 >= 0)
          result.push(sum & 0xFF);
      }

      return JSIL.Array.New($jsilcore.System.Byte, result);
    }
  );
});