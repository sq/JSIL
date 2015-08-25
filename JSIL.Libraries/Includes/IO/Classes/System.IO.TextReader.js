JSIL.ImplementExternals("System.IO.TextReader", function ($) {
    $.Method({ Static: false, Public: true }, "Dispose",
      (JSIL.MethodSignature.Void),
      function Dispose() {
      }
    );

    $.Method({ Static: false, Public: false }, "Dispose",
      (new JSIL.MethodSignature(null, [$.Boolean], [])),
      function Dispose(disposing) {
      }
    );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass($jsilcore.TypeRef("System.MarshalByRefObject"), "System.IO.TextReader", true, [], function ($) {
  var $thisType = $.publicInterface;

  $.ImplementInterfaces($jsilcore.TypeRef("System.IDisposable"))
});
//? }