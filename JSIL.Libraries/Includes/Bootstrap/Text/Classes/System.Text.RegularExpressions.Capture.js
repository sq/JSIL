JSIL.ImplementExternals("System.Text.RegularExpressions.Capture", function ($) {
  $.Method({ Static: false, Public: true }, "get_Length",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function get_Length() {
      return this._length;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Value",
    (new JSIL.MethodSignature($.String, [], [])),
    function get_Value() {
      return this._text;
    }
  );

  $.Method({ Static: false, Public: true }, "toString",
    (new JSIL.MethodSignature($.String, [], [])),
    function toString() {
      return this._text;
    }
  );
});