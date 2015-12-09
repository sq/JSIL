(function JsObject$Members() {
  var $, $thisType;

  function JsObject__ctor() {
  };

  function JsObject_Global(key) {
    return JSIL.GlobalNamespace[key];
  };

  JSIL.MakeType({
    BaseType: $jsilcore.TypeRef("System.Object"),
    Name: "JSIL.JsObject",
    IsPublic: true,
    IsReferenceType: true,
    MaximumConstructorArguments: 0,
  }, function ($ib) {
    $ = $ib;

    $.Method({ Static: false, Public: false }, ".ctor",
      JSIL.MethodSignature.Void,
      JsObject__ctor
    );

    $.Method({ Static: true, Public: true }, "Global",
      new JSIL.MethodSignature($.Type, [$.String]),
      JsObject_Global
    );


    return function (newThisType) { $thisType = newThisType; };
  });

})();