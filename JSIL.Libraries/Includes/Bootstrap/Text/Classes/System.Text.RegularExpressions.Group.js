JSIL.ImplementExternals("System.Text.RegularExpressions.Group", function ($) {
  $.RawMethod(false, "$internalCtor", function (text, success) {
    this._text = text;
    this._success = success;
    this._length = text.length;
  });

  $.Method({ Static: false, Public: true }, "get_Success",
    (new JSIL.MethodSignature($.Boolean, [], [])),
    function get_Success() {
      return this._success;
    }
  );
});