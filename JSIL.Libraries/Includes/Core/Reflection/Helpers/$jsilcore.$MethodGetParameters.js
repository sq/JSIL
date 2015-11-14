$jsilcore.$MethodGetParameters = function (method) {
  var result = method._cachedParameters;

  if (typeof (result) === "undefined") {
    result = method._cachedParameters = JSIL.Array.New($jsilcore.System.Reflection.ParameterInfo, 0);
    method.InitResolvedSignature();

    var argumentTypes = method._data.resolvedSignature.argumentTypes;
    var parameterInfos = method._data.parameterInfo;
    var tParameterInfo = $jsilcore.System.Reflection.RuntimeParameterInfo.__Type__;

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

$jsilcore.$MethodGetParameterTypes = function (method) {
  var signature = method._data.signature;
  var argumentTypes = signature.argumentTypes;
  var result = JSIL.Array.New($jsilcore.System.Type, 0);

  for (var i = 0, l = argumentTypes.length; i < l; i++) {
    var argumentType = argumentTypes[i];
    result.push(signature.ResolveTypeReference(argumentType)[1]);
  }

  return result;
};

$jsilcore.$MethodGetReturnType = function (method) {
  if (!method._data.signature.returnType)
    return $jsilcore.System.Void.__Type__;
  method.InitResolvedSignature();
  return method._data.resolvedSignature.returnType;
};

$jsilcore.$MemberInfoGetName = function (memberInfo) {
  return memberInfo._descriptor.Name;
};

$jsilcore.$ParameterInfoGetParameterType = function (parameterInfo) {
  return parameterInfo.argumentType;
};