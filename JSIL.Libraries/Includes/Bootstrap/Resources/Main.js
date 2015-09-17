"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

$jsilcore.getCurrentUICultureImpl = function () {
  var language;
  var svc = JSIL.Host.getService("window", true);

  if (svc) {
    language = svc.getNavigatorLanguage() || "en-US";
  } else {
    language = "en-US";
  }

  return $jsilcore.System.Globalization.CultureInfo.GetCultureInfo(language);
};

//? include("Classes/System.Resources.ResourceManager.js"); writeln();
//? include("Classes/System.Resources.ResourceSet.js"); writeln();
//? include("Classes/System.Globalization.CultureInfo.js"); writeln();
//? include("Classes/System.Threading.Thread.js"); writeln();

