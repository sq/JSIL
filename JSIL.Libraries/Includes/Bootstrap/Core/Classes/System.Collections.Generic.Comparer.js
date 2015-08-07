JSIL.ImplementExternals("System.Collections.Generic.Comparer`1", function ($) {
  $.Method({ Static: true, Public: true }, "get_Default",
    new JSIL.MethodSignature($.Type, [], []),
    function get_Default() {
      // HACK
      return new (JSIL.DefaultComparer$b1.Of(this.T));
    }
  );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Collections.Generic.Comparer`1", true, ["T"], function ($) {
  var $thisType = $.publicInterface;

  $.ExternalMethod({ Static: true, Public: true }, "get_Default",
    new JSIL.MethodSignature($.Type, [], [])
  );

  $.GenericProperty({ Static: true, Public: true }, "Default", $.Type);

  $.ImplementInterfaces(
    $jsilcore.TypeRef("System.Collections.IComparer"),
    $jsilcore.TypeRef("System.Collections.Generic.IComparer`1", [
      new JSIL.GenericParameter("T", "System.Collections.Generic.Comparer`1")
    ])
  );
});
//? }