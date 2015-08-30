JSIL.ImplementExternals("System.Text.RegularExpressions.Match", function ($) {
  $.RawMethod(false, "$internalCtor", function (text, groups) {
    this._text = text;
    this._groupcoll = groups;
    this._length = text.length;
  });

  $.Method({ Static: false, Public: true }, "get_Groups",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Text.RegularExpressions.GroupCollection"), [], [])),
    function get_Groups() {
      return this._groupcoll;
    }
  );
});