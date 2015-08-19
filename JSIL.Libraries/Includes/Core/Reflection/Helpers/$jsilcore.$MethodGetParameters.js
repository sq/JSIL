$jsilcore.$MethodGetParameters = function (method) {
  var result = method._cachedParameters;

  if (typeof (result) === "undefined") {
    result = method._cachedParameters = [];
    method.InitResolvedSignature();

    var argumentTypes = method._data.resolvedSignature.argumentTypes;
    var parameterInfos = method._data.parameterInfo;
    var tParameterInfo = $jsilcore.System.Reflection.ParameterInfo.__Type__;

    if (argumentTypes) {
      for (var i = 0; i < argumentTypes.length; i++) {
        var parameterInfo = parameterInfos[i] || null;

        // FIXME: Missing non-type information
        var pi = JSIL.CreateInstanceOfType(tParameterInfo, "$fromArgumentTypeAndPosition", [argumentTypes[i], i]);
        if (parameterInfo)
          pi.$populateWithParameterInfo(parameterInfo);

        result.push(pi);
      }
    }
  }

  return result;
};