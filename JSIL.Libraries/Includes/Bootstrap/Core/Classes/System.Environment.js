JSIL.ImplementExternals("System.Environment", function ($) {
  $.Method({ Static: true, Public: true }, "GetFolderPath",
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Environment+SpecialFolder")], [])),
    function GetFolderPath(folder) {
      // FIXME
      return folder.name;
    }
  );

  $.Method({ Static: true, Public: true }, "get_NewLine",
    (new JSIL.MethodSignature($.String, [], [])),
    function get_NewLine() {
      // FIXME: Maybe this should just be \n?
      return "\r\n";
    }
  );

  $.Method({ Static: true, Public: true }, "get_TickCount",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function get_TickCount() {
      return JSIL.Host.getTickCount() | 0;
    }
  );

});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Environment", false, [], function ($) {
  $.Property({ Static: true, Public: true }, "CurrentManagedThreadId", $.Int32);

  $.Property({ Static: true, Public: true }, "TickCount");
});
//? }