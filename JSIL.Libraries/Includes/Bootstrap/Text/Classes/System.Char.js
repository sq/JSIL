JSIL.ImplementExternals("System.Char", function ($) {
  $.Method({ Static: true, Public: true }, "IsControl",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsControl(c) {
      // FIXME: Unicode
      return (c <= 0x1F) || (c === 0x7F);
    }
  );

  $.Method({ Static: true, Public: true }, "IsDigit",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsDigit(c) {
      // FIXME: Unicode
      return (c >= 48) && (c <= 57);
    }
  );

  $.Method({ Static: true, Public: true }, "IsLetter",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsLetter(c) {
      // FIXME: Unicode
      return (
        ((c >= 65) && (c <= 90)) ||
        ((c >= 97) && (c <= 122)));
    }
  );

  $.Method({ Static: true, Public: true }, "IsNumber",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsNumeric(c) {
      // FIXME: Unicode
      return (c >= 48) && (c <= 57);
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
      return (c >= 0xD800) && (c <= 0xDFFF);
    }
  );

  $.Method({ Static: true, Public: true }, "IsWhiteSpace",
    new JSIL.MethodSignature($.Boolean, [$.Char], []),
    function IsWhiteSpace(c) {
      // FIXME: Unicode
      return (
        ((c >= 0x09) && (c <= 0x13)) ||
        (c === 0x20) ||
        (c === 0xA0) ||
        (c === 0x85));
    }
  );

  $.Method({ Static: true, Public: true }, "ToLowerInvariant",
    new JSIL.MethodSignature($.Char, [$.Char], []),
    function ToLowerInvariant(c) {
      return String.fromCharCode(c).toLowerCase().charCodeAt(0);
    }
  );

  $.Method({ Static: true, Public: true }, "ToUpperInvariant",
    new JSIL.MethodSignature($.Char, [$.Char], []),
    function ToLowerInvariant(c) {
      return String.fromCharCode(c).toUpperCase().charCodeAt(0);
    }
  );

  $.Method({ Static: true, Public: true }, "ToString",
    new JSIL.MethodSignature($.String, [$.Char], []),
    function ToString(c) {
      return String.fromCharCode(c);
    }
  );
});