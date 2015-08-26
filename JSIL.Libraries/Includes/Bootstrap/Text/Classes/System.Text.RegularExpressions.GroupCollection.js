JSIL.ImplementExternals("System.Text.RegularExpressions.GroupCollection", function ($) {
  var system = JSIL.GetAssembly("System", true);
  var $thisType = $.publicInterface;
  var tEnumerator = JSIL.ArrayEnumerator.Of(system.System.Text.RegularExpressions.Group);

  $.RawMethod(false, "$internalCtor", function (groups) {
    this._groups = groups;
  });

  $.Method({ Static: false, Public: true }, "get_Count",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function get_Count() {
      return this._groups.length;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Item",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Text.RegularExpressions.Group"), [$.Int32], [])),
    function get_Item(groupnum) {
      return this._groups[groupnum];
    }
  );

  $.Method({ Static: false, Public: true }, "get_Item",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Text.RegularExpressions.Group"), [$.String], [])),
    function get_Item(groupname) {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "GetEnumerator",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.IEnumerator"), [], [])),
    function GetEnumerator() {
      return new tEnumerator(this._groups, -1);
    }
  );
});