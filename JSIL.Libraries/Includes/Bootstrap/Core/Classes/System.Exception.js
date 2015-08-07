// HACK: Nasty compatibility shim for JS Error <-> C# Exception
Error.prototype.get_Message = function () {
  return String(this);
};

Error.prototype.get_StackTrace = function () {
  return this.stack || "";
};

JSIL.ImplementExternals(
  "System.Exception", function ($) {
    var mscorlib = JSIL.GetCorlib();

    function captureStackTrace() {
      var e = new Error();
      var stackText = e.stack || "";
      return stackText;
    };

    $.Method({ Static: false, Public: true }, ".ctor",
      (JSIL.MethodSignature.Void),
      function _ctor() {
        this._message = null;
        this._stackTrace = captureStackTrace();
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function _ctor(message) {
        this._message = message;
        this._stackTrace = captureStackTrace();
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String, mscorlib.TypeRef("System.Exception")], [])),
      function _ctor(message, innerException) {
        this._message = message;
        this._innerException = innerException;
        this._stackTrace = captureStackTrace();
      }
    );

    $.Method({ Static: false, Public: true }, "get_InnerException",
      (new JSIL.MethodSignature(mscorlib.TypeRef("System.Exception"), [], [])),
      function get_InnerException() {
        return this._innerException;
      }
    );

    $.Method({ Static: false, Public: true }, "get_Message",
      new JSIL.MethodSignature($.String, []),
      function () {
        if ((typeof (this._message) === "undefined") || (this._message === null))
          return System.String.Format("Exception of type '{0}' was thrown.", JSIL.GetTypeName(this));
        else
          return this._message;
      }
    );

    $.Method({ Static: false, Public: true }, "get_StackTrace",
      new JSIL.MethodSignature($.String, []),
      function () {
        return this._stackTrace || "";
      }
    );

    $.Method({ Static: false, Public: true }, "toString",
      new JSIL.MethodSignature($.String, []),
      function () {
        var message = this.Message;
        var result = System.String.Format("{0}: {1}", JSIL.GetTypeName(this), message);

        if (this._innerException) {
          result += "\n-- Inner exception follows --\n";
          result += this._innerException.toString();
        }

        return result;
      }
    );
  }
);

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Exception", true, [], function ($) {
  $.Property({ Public: true, Static: false, Virtual: true }, "Message");
  $.Property({ Public: true, Static: false }, "InnerException");
});
//? }