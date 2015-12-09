(function Services$Members() {
  var $, $thisType;

  function Services_Get(serviceName, throwIfMissing) {
    return JSIL.Host.getService(serviceName, !throwIfMissing);
  };

  JSIL.MakeStaticClass("JSIL.Services", true, [], function ($ib) {
    $ = $ib;

    $.Method({ Static: true, Public: true }, "Get",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [$.String, $.Boolean]),
      Services_Get
    );


    return function (newThisType) { $thisType = newThisType; };
  });

})();