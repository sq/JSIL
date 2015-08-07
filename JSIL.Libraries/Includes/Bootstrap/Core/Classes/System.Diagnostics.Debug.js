JSIL.ImplementExternals(
  "System.Diagnostics.Debug", function ($) {
    $.Method({ Static: true, Public: true }, "WriteLine",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function WriteLine(message) {
        JSIL.Host.logWriteLine(message);
      }
    );

    $.Method({ Static: true, Public: true }, "Write",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function Write(message) {
        JSIL.Host.logWrite(message);
      }
    );
  }
);

JSIL.ImplementExternals("System.Diagnostics.Debug", function ($) {

  $.Method({ Static: true, Public: true }, "Assert",
    (new JSIL.MethodSignature(null, [$.Boolean], [])),
    function Assert(condition) {
      if (!condition)
        JSIL.Host.assertionFailed("Assertion Failed");
    }
  );

  $.Method({ Static: true, Public: true }, "Assert",
    (new JSIL.MethodSignature(null, [$.Boolean, $.String], [])),
    function Assert(condition, message) {
      if (!condition)
        JSIL.Host.assertionFailed(message);
    }
  );

});