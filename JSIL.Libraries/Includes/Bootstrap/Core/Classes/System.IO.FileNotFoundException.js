JSIL.ImplementExternals(
  "System.IO.FileNotFoundException", function ($) {
    $.Method({ Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, [$.String], []),
      function (message) {
        System.Exception.prototype._ctor.call(this, message);
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String, $.String], [])),
      function _ctor(message, fileName) {
        System.Exception.prototype._ctor.call(this, message);
        this._fileName = fileName;
      }
    );
  }
);

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.IOException", "System.IO.FileNotFoundException", true);
//? }