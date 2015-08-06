JSIL.ImplementExternals("System.Threading.Volatile", function ($) {
  $.Method({ Static: true, Public: true }, "Write",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), "!!0"], ["T"]),
    function Write(T, /* ref */ location, value) {
      location.set(JSIL.CloneParameter(T, value));
    }
  );

  $.Method({ Static: true, Public: true }, "Read",
    new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("JSIL.Reference", ["!!0"])], ["T"]),
    function Read(T, /* ref */ location) {
      return location.get();
    }
  );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeStaticClass("System.Threading.Volatile", true, [], function ($) {
  $.ExternalMethod({ Public: true, Static: true }, "Read",
    new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("JSIL.Reference", ["!!0"])], ["T"])
  );
  $.ExternalMethod({ Public: true, Static: true }, "Write",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), "!!0"], ["T"])
  );
});
//? }