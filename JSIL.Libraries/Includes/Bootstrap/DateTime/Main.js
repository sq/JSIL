"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core is required");

if (!$jsilcore)
    throw new Error("JSIL.Core is required");

//? include("Structs/System.TimeSpan.js"); writeln();

JSIL.MakeEnum(
  "System.DateTimeKind", true, {
      Unspecified: 0,
      Utc: 1,
      Local: 2
  }, false
);

//? include("Structs/System.DateTime.js"); writeln();
