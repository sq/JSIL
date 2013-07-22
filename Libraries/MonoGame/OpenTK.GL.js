"use strict";

if (typeof (JSIL) === "undefined") 
  throw new Error("JSIL.Core required");

if (typeof ($jsilopentk) === "undefined")
  throw new Error("OpenTK required");

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.GL");

JSIL.GL.$context = null;

JSIL.GL.getContext = function () {
  if (!JSIL.GL.$context) {
    var canvas = JSIL.Host.getCanvas();
    JSIL.GL.$context = canvas.getContext("webgl");
    JSIL.GL.initializeContext(JSIL.GL.$context);
  }

  return JSIL.GL.$context;
};

JSIL.GL.initializeContext = function (ctx) {
  var extensionNames = [
    "EXT_texture_filter_anisotropic",
    "WEBGL_draw_buffers"
  ];

  for (var i = 0, l = extensionNames.length; i < l; i++) {
    var name = extensionNames[i];
    ctx.getExtension(name);
    ctx.getExtension("WEBKIT_" + name);
    ctx.getExtension("MOZ_" + name);
  }
};

JSIL.DeclareNamespace("OpenTK.Graphics.OpenGL");

JSIL.ImplementExternals("OpenTK.Graphics.OpenGL.GL", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.Method({Static:true , Public:true }, "GetError", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.ErrorCode"), [], []), 
    function GetError () {
      var ctx = JSIL.GL.getContext();
      if (!ctx)
        return OpenTK.Graphics.OpenGL.ErrorCode.InvalidOperation;

      // FIXME
      var err = ctx.getError();
      if (!err)
        return OpenTK.Graphics.OpenGL.ErrorCode.NoError;
      else {
        err = err | 0;
        var result = OpenTK.Graphics.OpenGL.ErrorCode.$Cast(err);
        System.Console.WriteLine("getError returning {0}", result);
        return result;
      }
    }
  );

  $.Method({Static:true , Public:true }, "GetString", 
    new JSIL.MethodSignature($.String, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.StringName")], []), 
    function GetString (name) {
      var ctx = JSIL.GL.getContext();

      if (name.value === 7939) {
        // EXTENSIONS
        return ctx.getSupportedExtensions().join(" ");
      } else {
        var result = ctx.getParameter(name.value);
        System.Console.WriteLine("getParameter({0}) === {1} (for name {2})", name.value, result, name.name);
        return result;
      }
    }
  );

  $.Method({Static:true , Public:true }, "GetInteger", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.GetPName"), $jsilcore.TypeRef("JSIL.Reference", [$.Int32])], []), 
    function GetInteger (pname, /* ref */ params) {
      var ctx = JSIL.GL.getContext();

      var result = ctx.getParameter(pname.value);
      System.Console.WriteLine("getParameter({0}) === {1} (for pname {2})", pname.value, result, pname.name);
      params.set(result);
    }
  );

  $.Method({Static:true , Public:true }, "GetFloat", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.GetPName"), $jsilcore.TypeRef("JSIL.Reference", [$.Single])], []), 
    function GetFloat (pname, /* ref */ params) {
      var ctx = JSIL.GL.getContext();

      var result = ctx.getParameter(pname.value);
      System.Console.WriteLine("getParameter({0}) === {1} (for pname {2})", pname.value, result, pname.name);
      params.set(result);
    }
  );

});