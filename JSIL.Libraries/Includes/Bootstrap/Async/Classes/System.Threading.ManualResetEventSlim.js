JSIL.ImplementExternals("System.Threading.ManualResetEventSlim", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.Method({ Static: false, Public: true }, ".ctor",
    JSIL.MethodSignature.Void,
    function _ctor() {
      // FIXME
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    JSIL.MethodSignature.Action($.Boolean),
    function _ctor(initialState) {
      // FIXME
    }
  );

  $.Method({ Static: false, Public: false }, "Set",
    JSIL.MethodSignature.Action($.Boolean),
    function () {
      // FIXME
    }
  );

  $.Method({ Static: false, Public: true }, "Wait",
    JSIL.MethodSignature.Void,
    function () {
      // FIXME
    }
  );

  $.Method({ Static: false, Public: true }, "Wait",
    new JSIL.MethodSignature($.Boolean, [$.Int32]),
    function (duration) {
      // FIXME
    }
  );
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.Object"),
  Name: "System.Threading.ManualResetEventSlim",
  IsPublic: true,
  IsReferenceType: true,
  MaximumConstructorArguments: 2,
}, function ($) {
  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    JSIL.MethodSignature.Void
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    JSIL.MethodSignature.Action($.Boolean)
  );

  $.ExternalMethod({ Static: false, Public: false }, "Set",
    JSIL.MethodSignature.Action($.Boolean)
  );

  $.ExternalMethod({ Static: false, Public: true }, "Wait",
    JSIL.MethodSignature.Void
  );

  $.ExternalMethod({ Static: false, Public: true }, "Wait",
    new JSIL.MethodSignature($.Boolean, [$.Int32])
  );
}
);