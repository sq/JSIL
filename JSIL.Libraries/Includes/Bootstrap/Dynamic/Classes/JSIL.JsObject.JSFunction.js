(function JsFunction$Members() {
  var $, $thisType;
  var $T00 = function () {
    return ($T00 = JSIL.Memoize($jsilcore.JSIL.JsObject))();
  };

  function JsFunction__ctor() {
    $T00().prototype._ctor.call(this);
  };

  JSIL.MakeType({
    BaseType: $jsilcore.TypeRef("JSIL.JsObject"),
    Name: "JSIL.JsObject+JsFunction",
    IsPublic: false,
    IsReferenceType: true,
    MaximumConstructorArguments: 0,
  }, function ($ib) {
    $ = $ib;

    $.Method({ Static: false, Public: false }, ".ctor",
      JSIL.MethodSignature.Void,
      JsFunction__ctor
    );


    return function (newThisType) { $thisType = newThisType; };
  });

})();