JSIL.ImplementExternals(
  "System.InvalidCastException", function ($) {
    $.Method({ Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, [$.String], []),
      function (message) {
        System.Exception.prototype._ctor.call(this, message);
      }
    );
  }
);

JSIL.MakeClass("System.SystemException", "System.InvalidCastException", true);