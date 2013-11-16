"use strict";

if (typeof (JSIL) === "undefined") 
  throw new Error("JSIL.Core required");

if (typeof ($jsilopentk) === "undefined")
  throw new Error("OpenTK required");

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.GL");

JSIL.GL.$nextHandle = 1;
JSIL.GL.$handleTable = {
};

JSIL.GL.createHandle = function (obj) {
  if (obj === null)
    return 0;

  var handle = JSIL.GL.$nextHandle++;
  JSIL.GL.$handleTable[handle] = obj;
  return handle;
};

JSIL.GL.destroyHandle = function (handle) {
  delete JSIL.GL.$handleTable[handle];
};

JSIL.GL.getHandle = function (handle) {
  if (handle <= 0)
    return null;

  return JSIL.GL.$handleTable[handle] || null;
};

JSIL.GL.$context = null;
JSIL.GL.$state = {
  temporaryIndexBuffer: null,
  temporaryVertexBuffer: null
};

JSIL.GL.getContext = function () {
  if (!JSIL.GL.$context) {
    var canvas = JSIL.Host.getCanvas();
    var context = canvas.getContext("webgl") || null;
    
    if (!context) {
      context = canvas.getContext("experimental-webgl") || null;

      if (context) {
        JSIL.Host.warning("Your browser's WebGL support is marked as experimental. You may encounter problems.");
      } else {
        JSIL.Host.warning("An attempt was made to activate WebGL, but your browser does not support it.");
        return null;
      }
    }

    JSIL.GL.$context = context;
    JSIL.GL.initializeContext(context);
  }

  return JSIL.GL.$context;
};

JSIL.GL.getState = function () {
  return JSIL.GL.$state;
};

JSIL.GL.fillTemporaryIndexBuffer = function (ctx, mode, count, type, indices) {
  var state = JSIL.GL.getState();

  var buffer = state.temporaryIndexBuffer;
  if (!buffer)
    buffer = state.temporaryIndexBuffer = ctx.createBuffer();

  // FIXME: Limit length of view
  var view = indices.pinnedPointer.asView($jsilcore.System.Byte);
  ctx.bindBuffer(ctx.ELEMENT_ARRAY_BUFFER, buffer);
  ctx.bufferData(ctx.ELEMENT_ARRAY_BUFFER, view, ctx.DYNAMIC_DRAW);

  return buffer;
};

OpenTK.Service.prototype.VertexAttribPointers = function (vertices, vertexStride, elements) {
  var ctx = JSIL.GL.getContext();
  var state = JSIL.GL.getState();

  if (!vertices.pinnedPointer)
    JSIL.RuntimeError("vertices must be a pinned pointer");

  if (!state.temporaryVertexBuffer)
    state.temporaryVertexBuffer = ctx.createBuffer();

  ctx.bindBuffer(ctx.ARRAY_BUFFER, state.temporaryVertexBuffer);

  // FIXME: Limit length of view
  var view = vertices.pinnedPointer.asView($jsilcore.System.Byte);
  ctx.bufferData(ctx.ARRAY_BUFFER, view, ctx.DYNAMIC_DRAW);

  for (var i = 0, l = elements.get_Count(); i < l; i++) {
    var element = elements.get_Item(i);

    ctx.vertexAttribPointer(
      element.AttributeLocation,
      element.NumberOfElements,
      element.VertexAttribPointerType.value,
      element.Normalized,
      vertexStride,
      element.Offset
    );
  }

  ctx.bindBuffer(ctx.ARRAY_BUFFER, null);
};

JSIL.GL.prepareToDraw = function (ctx, indexBuffer) {
  if (indexBuffer)
    ctx.bindBuffer(ctx.ELEMENT_ARRAY_BUFFER, indexBuffer);
};

JSIL.GL.afterDraw = function (ctx, indexBuffer) {
  if (indexBuffer)
    ctx.bindBuffer(ctx.ELEMENT_ARRAY_BUFFER, null);
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


  function VertexAttribPointerRecord (index, size, type, normalized, stride, pointer) {
    this.index = index;
    this.size = size;
    this.type = type.value;
    this.normalized = normalized;
    this.stride = stride;
    this.pointer = pointer;
    this.cachedView = null;
  };

  VertexAttribPointerRecord.prototype.getView = function () {
    if (!this.cachedView) {
      var elementType = null;
      switch (this.type.value) {
        case vapt.Float:
          elementType = $jsilcore.System.Single;
          break;

        case vapt.UnsignedByte:
          elementType = $jsilcore.System.Byte;
          break;

        case vapt.Short:
          elementType = $jsilcore.System.Int16;
          break;

        default:
          JSIL.RuntimeError("Vertex attrib pointer type '" + this.type + "' not implemented");
      }

      this.cachedView = this.pointer.asView(elementType, this.size);
    }

    return this.cachedView;
  };


  function TemporaryIndexBuffer () {
  };


  $.Method({Static:true , Public:true }, "GetError", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.ErrorCode"), [], []), 
    function GetError () {
      if (jsilConfig.suppressGLErrors === true)
        return OpenTK.Graphics.OpenGL.ErrorCode.NoError;

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
        return result;
      }
    }
  );

  $.Method({Static:true , Public:true }, "GetInteger", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.GetPName"), $jsilcore.TypeRef("JSIL.Reference", [$.Int32])], []), 
    function GetInteger (pname, /* ref */ params) {
      var ctx = JSIL.GL.getContext();

      var result = ctx.getParameter(pname.value);
      params.set(result);
    }
  );

  $.Method({Static:true , Public:true }, "GetFloat", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.GetPName"), $jsilcore.TypeRef("JSIL.Reference", [$.Single])], []), 
    function GetFloat (pname, /* ref */ params) {
      var ctx = JSIL.GL.getContext();

      var result = ctx.getParameter(pname.value);
      params.set(result);
    }
  );

  $.Method({Static:true , Public:true }, "AttachShader", 
    new JSIL.MethodSignature(null, [$.Int32, $.Int32], []), 
    function AttachShader (program, shader) {
      var ctx = JSIL.GL.getContext();
      ctx.attachShader(JSIL.GL.getHandle(program), JSIL.GL.getHandle(shader));
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
      var program = ctx.createProgram();
      return JSIL.GL.createHandle(program);
    }
  );

  $.Method({Static:true , Public:false}, "CreateShader", 
    new JSIL.MethodSignature($.Int32, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.ShaderType")], []), 
    function CreateShader (type) {
      var ctx = JSIL.GL.getContext();
      var shader = ctx.createShader(type.value);
      return JSIL.GL.createHandle(shader);
    }
  );

  $.Method({Static:true , Public:true }, "CompileShader", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function CompileShader (shader) {
      var ctx = JSIL.GL.getContext();
      ctx.compileShader(JSIL.GL.getHandle(shader));
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
      var value = ctx.getShaderParameter(JSIL.GL.getHandle(shader), pname.value);
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
      var value = ctx.getProgramParameter(JSIL.GL.getHandle(program), pname.value);
      params.set(value);
    }
  );

  $.Method({Static:true , Public:true }, "LinkProgram", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function LinkProgram (program) {
      var ctx = JSIL.GL.getContext();
      ctx.linkProgram(JSIL.GL.getHandle(program));
    }
  );

  $.Method({Static:true , Public:true }, "GetShaderInfoLog", 
    new JSIL.MethodSignature($.String, [$.Int32], []), 
    function GetShaderInfoLog (shader) {
      var ctx = JSIL.GL.getContext();
      return ctx.getShaderInfoLog(JSIL.GL.getHandle(shader));
    }
  );

  $.Method({Static:true , Public:true }, "GetProgramInfoLog", 
    new JSIL.MethodSignature($.String, [$.Int32], []), 
    function GetProgramInfoLog (shader) {
      var ctx = JSIL.GL.getContext();
      return ctx.getProgramInfoLog(JSIL.GL.getHandle(shader));
    }
  );

  $.Method({Static:true , Public:true }, "IsShader", 
    new JSIL.MethodSignature($.Boolean, [$.Int32], []), 
    function IsShader (shader) {
      var ctx = JSIL.GL.getContext();
      return !!ctx.isShader(JSIL.GL.getHandle(shader));
    }
  )

  $.Method({Static:true , Public:true }, "ShaderSource", 
    new JSIL.MethodSignature(null, [$.Int32, $.String], []), 
    function ShaderSource (shader, string) {
      var ctx = JSIL.GL.getContext();
      ctx.shaderSource(JSIL.GL.getHandle(shader), string);
    }
  );

  $.Method({Static:true , Public:true }, "UseProgram", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function UseProgram (program) {
      var ctx = JSIL.GL.getContext();
      ctx.useProgram(JSIL.GL.getHandle(program));
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
      ctx.bindBuffer(target.value, JSIL.GL.getHandle(buffer));
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

  $.Method({Static:true , Public:true }, "DrawElements", 
    new JSIL.MethodSignature(null, [
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.BeginMode"), $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.DrawElementsType"), $mgasms[2].TypeRef("System.IntPtr")
      ], []), 
    function DrawElements (mode, count, type, indices) {
      if (!indices.pinnedPointer)
        JSIL.RuntimeError("indices must be provided in the form of a pinned pointer");

      var ctx = JSIL.GL.getContext();
      var tib = JSIL.GL.fillTemporaryIndexBuffer(ctx, mode, count, type, indices);

      JSIL.GL.prepareToDraw(ctx, tib);
      ctx.drawElements(mode.value, count, type.value, 0);
      JSIL.GL.afterDraw(ctx, tib);
    }
  );

  $.Method({Static:true , Public:true }, "GenTextures", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.Int32])], []),
    function GenTextures (count, /* ref */ result) {
      if (count !== 1)
        JSIL.RuntimeError("Cannot create more than one texture");

      var ctx = JSIL.GL.getContext();
      var texture = ctx.createTexture();
      var handle = JSIL.GL.createHandle(texture);
      result.set(handle);
    }
  );

  $.Method({Static:true , Public:true }, "BindFramebuffer", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.FramebufferTarget"), $.Int32], []), 
    function BindFramebuffer (target, framebuffer) {
      var ctx = JSIL.GL.getContext();
      ctx.bindFramebuffer(target.value, JSIL.GL.getHandle(framebuffer));
    }
  );

  $.Method({Static:true , Public:true }, "Disable", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.EnableCap")], []), 
    function Disable (cap) {
      var ctx = JSIL.GL.getContext();
      ctx.disable(cap.value);
    }
  );

  $.Method({Static:true , Public:true }, "Disable", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.IndexedEnableCap"), $.Int32], []), 
    function Disable (target, index) {
      var ctx = JSIL.GL.getContext();
      ctx.disable(target.value, index);      
    }
  );

  $.Method({Static:true , Public:true }, "Enable", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.EnableCap")], []), 
    function Enable (cap) {
      var ctx = JSIL.GL.getContext();
      ctx.enable(cap.value);
    }
  );

  $.Method({Static:true , Public:true }, "Enable", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.IndexedEnableCap"), $.Int32], []), 
    function Enable (target, index) {
      var ctx = JSIL.GL.getContext();
      ctx.enable(target.value, index);      
    }
  );

  $.Method({Static:true , Public:true }, "ActiveTexture", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.TextureUnit")], []), 
    function ActiveTexture (texture) {
      var ctx = JSIL.GL.getContext();
      ctx.activeTexture(texture.value);
    }
  );

  $.Method({Static:true , Public:true }, "TexParameter", 
    new JSIL.MethodSignature(null, [
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.TextureTarget"), $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.TextureParameterName"), 
        $.Int32
      ], []), 
    function TexParameter (target, pname, param) {
      var ctx = JSIL.GL.getContext();
      ctx.texParameteri(target.value, pname.value, param | 0);
    }
  );

  $.Method({Static:true , Public:true }, "TexParameter", 
    new JSIL.MethodSignature(null, [
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.TextureTarget"), $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.TextureParameterName"), 
        $.Single
      ], []), 
    function TexParameter (target, pname, param) {
      var ctx = JSIL.GL.getContext();
      ctx.texParameterf(target.value, pname.value, param);
    }
  );

  $.Method({Static:true , Public:true }, "BindTexture", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.TextureTarget"), $.Int32], []), 
    function BindTexture (target, texture) {
      var ctx = JSIL.GL.getContext();
      ctx.bindTexture(target.value, JSIL.GL.getHandle(texture));
    }
  );

  $.Method({Static:true , Public:false}, "TexImage2D", 
    new JSIL.MethodSignature(null, [
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.TextureTarget"), $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.PixelInternalFormat"), $.Int32, 
        $.Int32, $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.PixelFormat"), $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.PixelType"), 
        $mgasms[2].TypeRef("System.IntPtr")
      ], []), 
    function TexImage2D (target, level, internalformat, width, height, border, format, type, pixels) {
      var nullPixels = (pixels === System.IntPtr.Zero);
      if (!pixels.pinnedPointer && !nullPixels)
        JSIL.RuntimeError("pixels must be provided in the form of a pinned pointer");

      var ctx = JSIL.GL.getContext();
      var pixelView = null;

      if (!nullPixels) {
        // FIXME: Do we have to compute exact size here and limit the view?
        pixelView = pixels.pinnedPointer.asView($jsilcore.System.Byte);
      }

      ctx.texImage2D(
        target.value, level, internalformat.value, 
        width, height, border, 
        format.value, type.value,
        pixelView
      );
    }
  );

  $.Method({Static:true , Public:true }, "GetAttribLocation", 
    new JSIL.MethodSignature($.Int32, [$.Int32, $.String], []), 
    function GetAttribLocation (program, name) {
      var ctx = JSIL.GL.getContext();
      var location = ctx.getAttribLocation(JSIL.GL.getHandle(program), name);
      // Thanks for not being consistent, WebGL
      return location;
    }
  );

  $.Method({Static:true , Public:true }, "GetUniformLocation", 
    new JSIL.MethodSignature($.Int32, [$.Int32, $.String], []), 
    function GetUniformLocation (program, name) {
      var ctx = JSIL.GL.getContext();
      var location = ctx.getUniformLocation(JSIL.GL.getHandle(program), name);
      return JSIL.GL.createHandle(location);
    }
  );

  $.Method({Static:true , Public:true }, "Uniform1", 
    new JSIL.MethodSignature(null, [$.Int32, $.Int32], []), 
    function Uniform1 (location, x) {
      var ctx = JSIL.GL.getContext();
      ctx.uniform1i(JSIL.GL.getHandle(location), x);
    }
  );

  $.Method({Static:true , Public:true }, "Uniform1", 
    new JSIL.MethodSignature(null, [$.Int32, $.Single], []), 
    function Uniform1 (location, x) {
      var ctx = JSIL.GL.getContext();
      ctx.uniform1f(JSIL.GL.getHandle(location), x);
    }
  );

  $.Method({Static:true , Public:true }, "Uniform4", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $jsilcore.TypeRef("System.Array", [$.Single])
      ], []), 
    function Uniform4 (location, count, v) {
      var expectedCount = ((v.length / 4) | 0);
      count = count | 0;

      if (count !== expectedCount)
        JSIL.RuntimeError("count must be (v.length / 4)");

      var ctx = JSIL.GL.getContext();
      ctx.uniform4fv(JSIL.GL.getHandle(location), v);
    }
  );

  $.Method({Static:true , Public:true }, "Uniform4", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $jsilcore.TypeRef("JSIL.Pointer", [$.Single])
      ], []), 
    function Uniform4 (location, count, pointer) {
      var ctx = JSIL.GL.getContext();
      var narrowView = pointer.asView($jsilcore.System.Single, (count | 0) * 4 * 4);
      ctx.uniform4fv(JSIL.GL.getHandle(location), narrowView);
    }
  );

  $.Method({Static:true , Public:true }, "EnableVertexAttribArray", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function EnableVertexAttribArray (index) {
      var ctx = JSIL.GL.getContext();
      ctx.enableVertexAttribArray(index);
    }
  );

  $.Method({Static:true , Public:false}, "CullFace", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.CullFaceMode")], []), 
    function CullFace (mode) {
      var ctx = JSIL.GL.getContext();
      ctx.cullFace(mode.value);
    }
  );

  $.Method({Static:true , Public:false}, "PolygonMode", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.MaterialFace"), $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.PolygonMode")], []), 
    function PolygonMode (face, mode) {
      // FIXME: Not available in WebGL
    }
  );

  $.Method({Static:true , Public:false}, "FrontFace", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.FrontFaceDirection")], []), 
    function FrontFace (mode) {
      var ctx = JSIL.GL.getContext();
      ctx.frontFace(mode.value);
    }
  );

  $.Method({Static:true , Public:true }, "ColorMask", 
    new JSIL.MethodSignature(null, [
        $.Boolean, $.Boolean, 
        $.Boolean, $.Boolean
      ], []), 
    function ColorMask (red, green, blue, alpha) {
      var ctx = JSIL.GL.getContext();
      ctx.colorMask(red, green, blue, alpha);
    }
  );

  $.Method({Static:true , Public:true }, "BlendColor", 
    new JSIL.MethodSignature(null, [
        $.Single, $.Single, 
        $.Single, $.Single
      ], []), 
    function BlendColor (red, green, blue, alpha) {
      var ctx = JSIL.GL.getContext();
      ctx.blendColor(red, green, blue, alpha);
    }
  );

  $.Method({Static:true , Public:false}, "BlendEquationSeparate", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.BlendEquationMode"), $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.BlendEquationMode")], []), 
    function BlendEquationSeparate (modeRGB, modeAlpha) {
      var ctx = JSIL.GL.getContext();
      ctx.blendEquationSeparate(modeRGB.value, modeAlpha.value);
    }
  );

  $.Method({Static:true , Public:false}, "BlendFuncSeparate", 
    new JSIL.MethodSignature(null, [
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.BlendingFactorSrc"), $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.BlendingFactorDest"), 
        $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.BlendingFactorSrc"), $mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.BlendingFactorDest")
      ], []), 
    function BlendFuncSeparate (sfactorRGB, dfactorRGB, sfactorAlpha, dfactorAlpha) {
      var ctx = JSIL.GL.getContext();
      ctx.blendFuncSeparate(
        sfactorRGB.value, dfactorRGB.value,
        sfactorAlpha.value, dfactorAlpha.value
      );
    }
  );

  $.Method({Static:true , Public:true }, "PixelStore", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.Graphics.OpenGL.PixelStoreParameter"), $.Int32], []), 
    function PixelStore (pname, param) {
      var ctx = JSIL.GL.getContext();
      ctx.pixelStorei(pname.value, param);
    }
  );

  $.Method({Static:true , Public:false}, "Finish", 
    new JSIL.MethodSignature(null, [], []), 
    function Finish () {
      var ctx = JSIL.GL.getContext();
      ctx.finish();
    }
  );
  
});