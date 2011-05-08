"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

JSIL.DeclareNamespace(System, "Windows");
JSIL.DeclareNamespace(System.Windows, "Forms");

JSIL.MakeClass(System.Object, System.Windows.Forms, "Control", "System.Windows.Forms.Control");
JSIL.MakeClass(System.Windows.Forms.Control, System.Windows.Forms, "ScrollableControl", "System.Windows.Forms.ScrollableControl");
JSIL.MakeClass(System.Windows.Forms.ScrollableControl, System.Windows.Forms, "Panel", "System.Windows.Forms.Panel");
JSIL.MakeClass(System.Windows.Forms.ScrollableControl, System.Windows.Forms, "ContainerControl", "System.Windows.Forms.ContainerControl");
JSIL.MakeClass(System.Windows.Forms.ContainerControl, System.Windows.Forms, "Form", "System.Windows.Forms.Form");
JSIL.MakeClass(System.Windows.Forms.Control, System.Windows.Forms, "TextBox", "System.Windows.Forms.TextBox");