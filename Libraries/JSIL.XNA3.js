"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Graphics.Color", $jsilxna.Color
);