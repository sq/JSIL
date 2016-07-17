JSIL.ImplementExternals("System.Char", function ($) {
  $.Method({ Static: true, Public: true }, "IsControl",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsControl(c) {
      // FIXME: Unicode
      var charCode = c.charCodeAt(0);
      return (charCode <= 0x1F) || (charCode === 0x7F);
    }
  );

  $.Method({ Static: true, Public: true }, "IsDigit",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsDigit(c) {
      // FIXME: Unicode
      var charCode = c.charCodeAt(0);
      return (charCode >= 48) && (charCode <= 57);
    }
  );

  $.Method({ Static: true, Public: true }, "IsLetter",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsLetter(c) {
      // FIXME: Unicode
      var charCode = c.charCodeAt(0);
      return (
        ((charCode >= 65) && (charCode <= 90)) ||
        ((charCode >= 97) && (charCode <= 122)));
    }
  );

  $.Method({ Static: true, Public: true }, "IsNumber",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsNumeric(c) {
      // FIXME: Unicode
      var charCode = c.charCodeAt(0);
      return (charCode >= 48) && (charCode <= 57);
    }
  );

  $.Method({ Static: true, Public: true }, "IsLetterOrDigit",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsLetterOrDigit(c) {
      return $jsilcore.System.Char.IsLetter(c) || $jsilcore.System.Char.IsDigit(c);
    }
  );

  $.Method({ Static: true, Public: true }, "IsSurrogate",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsSurrogate(c) {
      var charCode = c.charCodeAt(0);
      return (charCode >= 0xD800) && (charCode <= 0xDFFF);
    }
  );

  $.Method({ Static: true, Public: true }, "IsHighSurrogate",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsSurrogate(c) {
      var charCode = c.charCodeAt(0);
      return (charCode >= 0xD800) && (charCode <= 0xDBFF);
    }
  );

  $.Method({ Static: true, Public: true }, "IsWhiteSpace",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsWhiteSpace(c) {
      // FIXME: Unicode
      var charCode = c.charCodeAt(0);
      return (
        ((charCode >= 0x09) && (charCode <= 0x13)) ||
        (charCode === 0x20) ||
        (charCode === 0xA0) ||
        (charCode === 0x85));
    }
  );

  $.Method({ Static: true, Public: true }, "ToLowerInvariant",
    new JSIL.MethodSignature($.Char, [$.Char], []),
    function ToLowerInvariant(c) {
      return c.toLowerCase();
    }
  );

  $.Method({ Static: true, Public: true }, "ToUpperInvariant",
    new JSIL.MethodSignature($.Char, [$.Char], []),
    function ToLowerInvariant(c) {
      return c.toUpperCase();
    }
  );

  $.Method({ Static: true, Public: true }, "ConvertToUtf32",
    new JSIL.MethodSignature($.Int32, [$.String, $.Int32], []),
    function ConvertToUtf32(s, i) {
      return $jsilcore.charCodeAt(s, i);
    }
  );

  $.Method({ Static: true, Public: true }, "ConvertFromUtf32",
    new JSIL.MethodSignature($.String, [$.Int32], []),
    function ConvertFromUtf32(i) {
        return $jsilcore.fromCharCode(i);
    }
  );

  $.Method({ Static: true, Public: true }, "ToString",
    new JSIL.MethodSignature($.String, [$.Char], []),
    function ToString(c) {
      return c;
    }
  );
});