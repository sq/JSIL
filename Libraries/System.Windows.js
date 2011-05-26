"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");
  
JSIL.DeclareAssembly("JSIL.Windows");

JSIL.DeclareNamespace("System.Windows");
JSIL.DeclareNamespace("System.Windows.Forms");

JSIL.MakeClass(System.Object, "System.Windows.Forms.Control", true);
JSIL.MakeClass(System.Windows.Forms.Control, "System.Windows.Forms.ScrollableControl", true);
JSIL.MakeClass(System.Windows.Forms.ScrollableControl, "System.Windows.Forms.Panel", true);
JSIL.MakeClass(System.Windows.Forms.ScrollableControl, "System.Windows.Forms.ContainerControl", true);
JSIL.MakeClass(System.Windows.Forms.ContainerControl, "System.Windows.Forms.Form", true);
JSIL.MakeClass(System.Windows.Forms.Control, "System.Windows.Forms.TextBox", true);