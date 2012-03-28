"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

var $jsiljson = JSIL.DeclareAssembly("JSIL.JSON");

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.JSON");

JSIL.JSON.MapObject = function (input) {
  var result;
  var mscorlib = JSIL.GetAssembly("mscorlib", true);
  var tArrayList = JSIL.GetTypeFromAssembly(
    mscorlib, "System.Collections.ArrayList", [], true
  );
  var tDictionary = JSIL.GetTypeFromAssembly(
    mscorlib, "System.Collections.Generic.Dictionary`2", [
      JSIL.GetTypeFromAssembly(mscorlib, "System.String", [], true),
      JSIL.GetTypeFromAssembly(mscorlib, "System.Object", [], true)
    ], true
  );

  if (JSIL.IsArray(input)) {
    result = JSIL.CreateInstanceOfType(tArrayList);

    for (var i = 0; i < input.length; i++)
      result.Add(JSIL.JSON.MapObject(input[i]));

  } else if (typeof (input) === "object") {
    var result = JSIL.CreateInstanceOfType(tDictionary);

    for (var k in input)
      result.Add(k, JSIL.JSON.MapObject(input[k]));

  } else if (typeof (input) === "number") {
    result = input.toString();
  } else {
    result = input;
  }

  return result;
};

JSIL.JSON.Parse = function (json) {
  var parsed = JSON.parse(json);

  return JSIL.JSON.MapObject(parsed);
};