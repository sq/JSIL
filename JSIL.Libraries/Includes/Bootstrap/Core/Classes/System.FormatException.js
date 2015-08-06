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

//? if (typeof GENERATE_STUBS !== 'undefined') {
JSIL.MakeClass("System.SystemException", "System.FormatException", true);
//? }