JSIL.ImplementExternals("System.Diagnostics.Trace", function ($) {
  $.Method({ Static: true, Public: true }, "TraceError",
    new JSIL.MethodSignature(null, [$.String], []),
    function TraceError(message) {
      var svc = JSIL.Host.getService("trace", true);
      if (svc)
        svc.error(message);
    }
  );

  $.Method({ Static: true, Public: true }, "TraceError",
    new JSIL.MethodSignature(null, [$.String, $jsilcore.TypeRef("System.Array", [$.Object])], []),
    function TraceError(format, args) {
      var svc = JSIL.Host.getService("trace", true);
      var message = System.String.Format(format, args);
      if (svc)
        svc.error(message);
    }
  );

  $.Method({ Static: true, Public: true }, "TraceInformation",
    new JSIL.MethodSignature(null, [$.String], []),
    function TraceInformation(message) {
      var svc = JSIL.Host.getService("trace", true);
      if (svc)
        svc.information(message);
    }
  );

  $.Method({ Static: true, Public: true }, "TraceInformation",
    new JSIL.MethodSignature(null, [$.String, $jsilcore.TypeRef("System.Array", [$.Object])], []),
    function TraceInformation(format, args) {
      var svc = JSIL.Host.getService("trace", true);
      var message = System.String.Format(format, args);
      if (svc)
        svc.information(message);
    }
  );

  $.Method({ Static: true, Public: true }, "TraceWarning",
    new JSIL.MethodSignature(null, [$.String], []),
    function TraceWarning(message) {
      var svc = JSIL.Host.getService("trace", true);
      if (svc)
        svc.warning(message);
    }
  );

  $.Method({ Static: true, Public: true }, "TraceWarning",
    new JSIL.MethodSignature(null, [$.String, $jsilcore.TypeRef("System.Array", [$.Object])], []),
    function TraceWarning(format, args) {
      var svc = JSIL.Host.getService("trace", true);
      var message = System.String.Format(format, args);
      if (svc)
        svc.warning(message);
    }
  );

  $.Method({ Static: true, Public: true }, "WriteLine",
    new JSIL.MethodSignature(null, [$.String], []),
    function WriteLine(message) {
      var svc = JSIL.Host.getService("trace", true);
      if (svc)
        svc.write(message);
    }
  );

  $.Method({ Static: true, Public: true }, "WriteLine",
    new JSIL.MethodSignature(null, [$.String, $.String], []),
    function WriteLine(message, category) {
      var svc = JSIL.Host.getService("trace", true);
      if (svc)
        svc.write(message, category);
    }
  );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeStaticClass("System.Diagnostics.Trace", true, [], function ($) {
});
//? }