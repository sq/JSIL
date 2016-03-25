"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core is required");

if (!$jsilcore)
    throw new Error("JSIL.Core is required");

JSIL.DeclareNamespace("System.Runtime.CompilerServices");
JSIL.DeclareNamespace("Microsoft");
JSIL.DeclareNamespace("Microsoft.CSharp");
JSIL.DeclareNamespace("Microsoft.CSharp.RuntimeBinder");
JSIL.DeclareNamespace("System.Linq");
JSIL.DeclareNamespace("System.Linq.Expressions");

//? include("Classes/System.Runtime.CompilerServices.CallSite.js"); writeln();
//? include("Classes/System.Runtime.CompilerServices.CallSiteBinder.js"); writeln();
//? include("Classes/Microsoft.CSharp.RuntimeBinder.Binder.js"); writeln();
//? include("Classes/Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.js"); writeln();

//? include("Enums/Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags.js"); writeln();
//? include("Enums/Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.js"); writeln();
//? include("Enums/System.Linq.Expressions.ExpressionType.js"); writeln();