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

  $.Method({Static:true , Public:true }, "AttachShader", 
    new JSIL.MethodSignature(null, [$.Int32, $.Int32], []), 
    function AttachShader (program, shader) {
      var ctx = JSIL.GL.getContext();
      ctx.attachShader(program, shader);
    }
  );

  $.Method({Static:true , Public:false}, "Clear", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.ClearBufferMask")], []), 
    function Clear (mask) {
      var ctx = JSIL.GL.getContext();
      ctx.clear(mask.value);
    }
  );

  $.Method({Static:true , Public:true }, "ClearColor", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.Color4")], []), 
    function ClearColor (color) {
      var ctx = JSIL.GL.getContext();
      ctx.clearColor(color.R, color.G, color.B, color.A);      
    }
  );

  $.Method({Static:true , Public:true }, "ClearColor", 
    new JSIL.MethodSignature(null, [
        $.Single, $.Single, 
        $.Single, $.Single
      ], []), 
    function ClearColor (red, green, blue, alpha) {
      var ctx = JSIL.GL.getContext();
      ctx.clearColor(red, green, blue, alpha);
    }
  );

  $.Method({Static:true , Public:true }, "ClearDepth", 
    new JSIL.MethodSignature(null, [$.Double], []), 
    function ClearDepth (depth) {
      var ctx = JSIL.GL.getContext();
      ctx.clearDepth(depth);
    }
  );

  $.Method({Static:true , Public:true }, "ClearDepth", 
    new JSIL.MethodSignature(null, [$.Single], []), 
    function ClearDepth (d) {
      var ctx = JSIL.GL.getContext();
      ctx.clearDepth(depth);
    }
  );

  $.Method({Static:true , Public:false}, "ClearStencil", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function ClearStencil (s) {
      var ctx = JSIL.GL.getContext();
      ctx.clearStencil(s);
    }
  );

  $.Method({Static:true , Public:false}, "CreateProgram", 
    new JSIL.MethodSignature($.Int32, [], []), 
    function CreateProgram () {
      var ctx = JSIL.GL.getContext();
      return ctx.createProgram();
    }
  );

  $.Method({Static:true , Public:false}, "CreateShader", 
    new JSIL.MethodSignature($.Int32, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.ShaderType")], []), 
    function CreateShader (type) {
      var ctx = JSIL.GL.getContext();
      return ctx.createShader(type.value);
    }
  );

  $.Method({Static:true , Public:true }, "CompileShader", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function CompileShader (shader) {
      var ctx = JSIL.GL.getContext();
      ctx.compileShader(shader);
    }
  );

  function checkDepthRange (near, far) {
    if ((near === 0) && (far === 1))
      return;

    JSIL.Host.warning("WebGL does not support depthRange and the default is locked to (0 - 1); ignored (" + near + ", " + far + ")");
  };

  $.Method({Static:true , Public:true }, "DepthRange", 
    new JSIL.MethodSignature(null, [$.Double, $.Double], []), 
    function DepthRange (near, far) {
      checkDepthRange(near, far);
    }
  );

  $.Method({Static:true , Public:true }, "DepthRange", 
    new JSIL.MethodSignature(null, [$.Single, $.Single], []), 
    function DepthRange (n, f) {
      checkDepthRange(n, f);
    }
  );

  $.Method({Static:true , Public:true }, "GetShader", 
    new JSIL.MethodSignature(null, [
        $.Int32, $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.ShaderParameter"), 
        $jsilcore.TypeRef("JSIL.Reference", [$.Int32])
      ], []), 
    function GetShader (shader, pname, /* ref */ params) {
      var ctx = JSIL.GL.getContext();
      var value = ctx.getShaderParameter(shader, pname.value);
      params.set(value);
    }
  );

  $.Method({Static:true , Public:true }, "GetProgram", 
    new JSIL.MethodSignature(null, [
        $.Int32, $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.ProgramParameter"), 
        $jsilcore.TypeRef("JSIL.Reference", [$.Int32])
      ], []), 
    function GetProgram (program, pname, /* ref */ params) {
      var ctx = JSIL.GL.getContext();
      var value = ctx.getProgramParameter(program, pname.value);
      params.set(value);
    }
  );

  $.Method({Static:true , Public:true }, "LinkProgram", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function LinkProgram (program) {
      var ctx = JSIL.GL.getContext();
      ctx.linkProgram(program);
    }
  );

  $.Method({Static:true , Public:true }, "GetShaderInfoLog", 
    new JSIL.MethodSignature($.String, [$.Int32], []), 
    function GetShaderInfoLog (shader) {
      var ctx = JSIL.GL.getContext();
      return ctx.getShaderInfoLog(shader);
    }
  );

  $.Method({Static:true , Public:true }, "GetProgramInfoLog", 
    new JSIL.MethodSignature($.String, [$.Int32], []), 
    function GetProgramInfoLog (shader) {
      var ctx = JSIL.GL.getContext();
      return ctx.getProgramInfoLog(shader);
    }
  );

  $.Method({Static:true , Public:true }, "IsShader", 
    new JSIL.MethodSignature($.Boolean, [$.Int32], []), 
    function IsShader (shader) {
      var ctx = JSIL.GL.getContext();
      return !!ctx.isShader(shader);
    }
  )

  $.Method({Static:true , Public:true }, "ShaderSource", 
    new JSIL.MethodSignature(null, [$.Int32, $.String], []), 
    function ShaderSource (shader, string) {
      var ctx = JSIL.GL.getContext();
      ctx.shaderSource(shader, string);
    }
  );

  $.Method({Static:true , Public:true }, "UseProgram", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function UseProgram (program) {
      var ctx = JSIL.GL.getContext();
      ctx.useProgram(program);
    }
  );

  $.Method({Static:true , Public:true }, "Viewport", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $.Int32, $.Int32
      ], []), 
    function Viewport (x, y, width, height) {
      var ctx = JSIL.GL.getContext();
      ctx.viewport(x, y, width, height);
    }
  );

  $.Method({Static:true , Public:true }, "Scissor", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $.Int32, $.Int32
      ], []),
    function Scissor (x, y, width, height) {
      var ctx = JSIL.GL.getContext();
      ctx.scissor(x, y, width, height);
    }
  );

  $.Method({Static:true , Public:true }, "Flush", 
    new JSIL.MethodSignature(null, [], []), 
    function Flush () {
      var ctx = JSIL.GL.getContext();
      ctx.flush();
    }
  );

  $.Method({Static:true , Public:false}, "DepthFunc", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.DepthFunction")], []), 
    function DepthFunc (func) {
      var ctx = JSIL.GL.getContext();
      ctx.depthFunc(func.value);
    }
  );

  $.Method({Static:true , Public:false}, "DepthMask", 
    new JSIL.MethodSignature(null, [$.Boolean], []), 
    function DepthMask (flag) {
      var ctx = JSIL.GL.getContext();
      ctx.depthMask(flag);
    }
  );

  $.Method({Static:true , Public:true }, "BindBuffer", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.BufferTarget"), $.Int32], []), 
    function BindBuffer (target, buffer) {
      var ctx = JSIL.GL.getContext();
      ctx.bindBuffer(target.value, (buffer === 0) ? null : buffer);
    }
  );

  $.Method({Static:true , Public:true }, "DrawElements", 
    new JSIL.MethodSignature(null, [
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.BeginMode"), $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.DrawElementsType"), $.Int32
      ], []), 
    function DrawElements (mode, count, type, offset) {
      var ctx = JSIL.GL.getContext();
      ctx.drawElements(mode.value, count, type.value, offset);
    }
  );

  var warnedAboutIndices = false;

  $.Method({Static:true , Public:true }, "DrawElements", 
    new JSIL.MethodSignature(null, [
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.BeginMode"), $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.DrawElementsType"), $mgasms[2].TypeRef("System.IntPtr")
      ], []), 
    function DrawElements (mode, count, type, indices) {
      if (!indices.pinnedPointer)
        throw new Error("indices must be provided in the form of a pinned pointer");

      // FIXME: Allocate a temporary index buffer and fill it with the indices.
      if (!warnedAboutIndices) {
        JSIL.Host.warning("DrawElements overloads accepting index pointer not implemented");
        warnedAboutIndices = true;      
      }

      var ctx = JSIL.GL.getContext();
      ctx.drawElements(mode.value, count, type.value, 0);
    }
  );

  $.Method({Static:true , Public:true }, "GenTextures", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.Int32])], []),
    function GenTextures (count, /* ref */ result) {
      if (count !== 1)
        throw new Error("Cannot create more than one texture");

      var ctx = JSIL.GL.getContext();
      result.set(ctx.createTexture());
    }
  );
});