"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

var $jsiljson = JSIL.DeclareAssembly("JSIL.JSON");

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.JSON");

JSIL.JSON.MapObject = function (input) {
  var result;

  if (JSIL.IsArray(input)) {
    result = new System.Collections.ArrayList();

    for (var i = 0; i < input.length; i++)
      result.Add(JSIL.JSON.MapObject(input[i]));

  } else if (typeof (input) === "object") {
    var tDict = System.Collections.Generic.Dictionary$b2.Of(System.String, System.Object);
    var result = new tDict();

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