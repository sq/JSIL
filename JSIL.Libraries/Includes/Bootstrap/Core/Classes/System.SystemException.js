JSIL.ImplementExternals(
  "System.SystemException", function ($) {
    $.Method({ Static: false, Public: true }, ".ctor",
      JSIL.MethodSignature.Void,
      function () {
        System.Exception.prototype._ctor.call(this);
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, [$.String], []),
      function (message) {
        System.Exception.prototype._ctor.call(this, message);
      }
    );
  }
);

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Exception", "System.SystemException", true);
//? }