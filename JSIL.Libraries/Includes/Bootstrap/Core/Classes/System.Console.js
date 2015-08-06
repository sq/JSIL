JSIL.ImplementExternals("System.Console", function ($) {
  $.RawMethod(true, "WriteLine", function () {
    var text = "";
    if ((arguments.length > 0) && (arguments[0] !== null)) {
      text = System.String.Format.apply(System.String, arguments);
    }

    JSIL.Host.logWriteLine(text);
  });

  $.RawMethod(true, "Write", function () {
    var text = "";
    if ((arguments.length > 0) && (arguments[0] !== null)) {
      text = System.String.Format.apply(System.String, arguments);
    }

    JSIL.Host.logWrite(text);
  });
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeStaticClass("System.Console", true, [], function ($) {
  $.Property({ Static: true, Public: true }, "In");
  $.Property({ Static: true, Public: true }, "Out");
});
//? }