"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

var $jsilxna = JSIL.DeclareAssembly("JSIL.XNA");

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Color", false, $jsilxna.Color
);
JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Color", true, $jsilxna.ColorPrototype
);