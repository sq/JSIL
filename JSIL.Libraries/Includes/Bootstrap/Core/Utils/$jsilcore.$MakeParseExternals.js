/*? if (!('$jsilcore_$MakeParseExternals' in __out)) { __out.$jsilcore_$MakeParseExternals = true; */
$jsilcore.$MakeParseExternals = function ($, type, parse, tryParse) {
  $.Method({ Static: true, Public: true }, "Parse",
    (new JSIL.MethodSignature(type, [$.String], [])),
    parse
  );

  $.Method({ Static: true, Public: true }, "Parse",
    (new JSIL.MethodSignature(type, [$.String, $jsilcore.TypeRef("System.Globalization.NumberStyles")], [])),
    parse
  );

  $.Method({ Static: true, Public: true }, "Parse",
    (new JSIL.MethodSignature(type, [$.String, $jsilcore.TypeRef("System.IFormatProvider")], [])),
    function (input, formatProvider) {
      // TODO: Really use fromat provider
      return parse(input, null);
    }
  );

  $.Method({ Static: true, Public: true }, "TryParse",
    (new JSIL.MethodSignature($.Boolean, [$.String, $jsilcore.TypeRef("JSIL.Reference", [type])], [])),
    tryParse
  );
};
/*? }*/