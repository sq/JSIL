JSIL.ImplementExternals("System.Text.RegularExpressions.MatchCollection", function ($) {
  var system = JSIL.GetAssembly("System", true);
  var mscorlib = JSIL.GetCorlib();
  var tEnumerator = JSIL.ArrayEnumerator.Of(system.System.Text.RegularExpressions.Match);

  $.RawMethod(false, "$internalCtor", function (matches) {
    this._matches = matches;
  });

  $.Method({ Static: false, Public: true }, "get_Count",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function get_Count() {
      return this._matches.length;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Item",
    (new JSIL.MethodSignature(system.TypeRef("System.Text.RegularExpressions.Match"), [$.Int32], [])),
    function get_Item(i) {
      return this._matches[i];
    }
  );

  $.Method({ Static: false, Public: false }, "GetMatch",
    (new JSIL.MethodSignature(system.TypeRef("System.Text.RegularExpressions.Match"), [$.Int32], [])),
    function GetMatch(i) {
      return this._matches[i];
    }
  );

  $.Method({ Static: false, Public: true }, "GetEnumerator",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Collections.IEnumerator"), [], [])),
    function GetEnumerator() {
      return new tEnumerator(this._matches, -1);
    }
  );
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Text.RegularExpressions.MatchCollection", true, [], function ($) {
});