JSIL.ImplementExternals("System.MulticastDelegate", function ($) {
  $.Method({ Static: false, Public: true }, "GetInvocationList",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Delegate")]), [], [])),
    function GetInvocationList() {
      return this.__delegates__;
    }
  );
});

JSIL.MulticastDelegate.New = function (delegates) {
  var delegatesCopy = Array.prototype.slice.call(delegates);
  var delegateCount = delegates.length;

  var resultDelegate = function MulticastDelegate_Invoke() {
    var result;

    for (var i = 0; i < delegateCount; i++) {
      var d = delegatesCopy[i];
      // FIXME: bind, call and apply suck
      result = d.apply(d.__object__ || null, arguments);
    }

    return result;
  };

  JSIL.SetValueProperty(resultDelegate, "__delegates__", delegatesCopy);
  JSIL.SetValueProperty(resultDelegate, "__isMulticast__", true);
  JSIL.SetValueProperty(resultDelegate, "__ThisType__", delegatesCopy[0].__ThisType__);
  JSIL.SetValueProperty(resultDelegate, "toString", delegatesCopy[0].toString);
  JSIL.SetValueProperty(resultDelegate, "__method__", resultDelegate);
  JSIL.SetValueProperty(resultDelegate, "Invoke", resultDelegate);
  JSIL.SetValueProperty(resultDelegate, "get_Method", function () { return delegatesCopy[delegateCount - 1].get_Method(); });

  return resultDelegate;
};

JSIL.MakeClass("System.Delegate", "System.MulticastDelegate", true, []);