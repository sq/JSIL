JSIL.ImplementExternals(
  "System.FormatException", function ($) {
    $.Method({ Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, [$.String], []),
      function (message) {
        System.Exception.prototype._ctor.call(this, message);
      }
    );
  }
);

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.SystemException", "System.FormatException", true);
//? }