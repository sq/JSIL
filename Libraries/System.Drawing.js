"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");
  
var $jsildrawing = JSIL.DeclareAssembly("JSIL.Drawing");

JSIL.DeclareNamespace("System");
JSIL.DeclareNamespace("System.Drawing");

if (JSIL.HostType.IsBrowser) {
  JSIL.ImplementExternals("System.Drawing.Image", function ($) {
    var mscorlib = JSIL.GetAssembly("mscorlib", true);

    $.Method({Static:false, Public:true }, "Save", 
      new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.String")], []),
      function (filename) {
        this.context.putImageData(this.buffer, 0, 0);
      }
    );

  });

  JSIL.ImplementExternals("System.Drawing.Bitmap", function ($) {
    var systemDrawing = JSIL.GetAssembly("System.Drawing", true);
    var mscorlib = JSIL.GetAssembly("mscorlib", true);

    var constructFromFile = function (filename) {
      // System.Drawing.Image.prototype._ctor.call(this);

      this.image = JSIL.Host.getImage(filename);

      this.canvas = JSIL.Host.createCanvas(this.image.naturalWidth, this.image.naturalHeight);
      this.context = this.canvas.getContext('2d');
      this.context.globalCompositeOperation = "copy";

      this.context.drawImage(this.image, 0, 0);
      /*
      try {
        this.buffer = this.context.getImageData(0, 0, this.image.naturalWidth, this.image.naturalHeight);
      } catch (e) {
        JSIL.Host.warning("Failed to read image pixels for '" + filename + "'", e);
        this.buffer = this.context.createImageData(this.image.naturalWidth, this.image.naturalHeight);
      }
      */
      this.buffer = null;
    };

    $.Method({Static:false, Public:true }, "get_Width", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Width () {
        return this.canvas.width;
      }
    );

    $.Method({Static:false, Public:true }, "get_Height", 
      (new JSIL.MethodSignature($.Int32, [], [])), 
      function get_Height () {
        return this.canvas.height;
      }
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.String")], []),
      constructFromFile
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.String"), mscorlib.TypeRef("System.Boolean")], []),
      constructFromFile
    );

    $.Method({Static:false, Public:true }, ".ctor", 
      new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Int32"), mscorlib.TypeRef("System.Int32")], []),
      function (width, height) {
        // System.Drawing.Image.prototype._ctor.call(this);

        this.canvas = JSIL.Host.getCanvas(width, height);
        this.context = this.canvas.getContext('2d');
        this.context.globalCompositeOperation = "copy";

        this.buffer = this.context.createImageData(width, height);
        this.context.putImageData(this.buffer, 0, 0);

        this.setPixelCount = 0;
        this.flushInterval = width - 1;
      }
    );

    $.Method({Static:false, Public:true }, "SetPixel", 
      new JSIL.MethodSignature(null, [
          mscorlib.TypeRef("System.Int32"), mscorlib.TypeRef("System.Int32"), 
          systemDrawing.TypeRef("System.Drawing.Color")
        ], []),
      function (x, y, color) {
        var index = ((y * this.buffer.width) + x) * 4;
        var data = this.buffer.data;
        data[index] = color.R;
        data[index + 1] = color.G;
        data[index + 2] = color.B;
        data[index + 3] = 255;

        if (this.setPixelCount++ >= this.flushInterval) {
          this.setPixelCount = 0;
          this.context.putImageData(this.buffer, 0, 0);
        }
      }
    );

  });
}

JSIL.ImplementExternals("System.Drawing.Color", function ($) {
  var systemDrawing = JSIL.GetAssembly("System.Drawing", true);
  var mscorlib = JSIL.GetAssembly("mscorlib", true);

  var makeColor = function (a, r, g, b, name) {
    var prototype = systemDrawing.System.Drawing.Color.prototype;
    var result = Object.create(prototype);

    result.a = a;
    result.r = r;
    result.g = g;
    result.b = b;
    result.name = name;

    return result;
  };

  $.Method({Static:true , Public:false}, ".cctor2", 
    new JSIL.MethodSignature(null, [], []), 
    function _cctor2 () {
      var sdc = systemDrawing.System.Drawing.Color;

      var makeNamedColor = function (a, r, g, b, name) {
        var color = makeColor(a, r, g, b, name);

        JSIL.SetValueProperty(sdc, "get_" + name, function () {
          return color;
        });

        JSIL.SetValueProperty(sdc, name, color);
      };

      makeNamedColor(0xFF, 0x00, 0x00, 0x00, "Black");
      makeNamedColor(0xFF, 0xFF, 0xFF, 0xFF, "White");
      makeNamedColor(0xFF, 0xFD, 0xF5, 0xE6, "OldLace");
      makeNamedColor(0xFF, 0x8A, 0x2B, 0xE2, "BlueViolet");
      makeNamedColor(0xFF, 0x7F, 0xFF, 0xD4, "Aquamarine");
    }
  );

  $.Method({Static:true , Public:true }, "FromArgb", 
    new JSIL.MethodSignature(systemDrawing.TypeRef("System.Drawing.Color"), [
        mscorlib.TypeRef("System.Int32"), mscorlib.TypeRef("System.Int32"), 
        mscorlib.TypeRef("System.Int32"), mscorlib.TypeRef("System.Int32")
      ], []),
    function (alpha, red, green, blue) {
      return makeColor(alpha, red, green, blue, null);
    }
  );

  $.Method({Static:true , Public:true }, "FromArgb", 
    new JSIL.MethodSignature(systemDrawing.TypeRef("System.Drawing.Color"), [
        mscorlib.TypeRef("System.Int32"), mscorlib.TypeRef("System.Int32"), 
        mscorlib.TypeRef("System.Int32")
      ], []),
    function (red, green, blue) {
      return makeColor(255, red, green, blue, null);
    }
  );

  $.Method({Static:false, Public:true }, "get_A", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Byte"), [], []),
    function () {
      return this.a;
    }
  );

  $.Method({Static:false, Public:true }, "get_R", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Byte"), [], []),
    function () {
      return this.r;
    }
  );

  $.Method({Static:false, Public:true }, "get_G", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Byte"), [], []),
    function () {
      return this.g;
    }
  );

  $.Method({Static:false, Public:true }, "get_B", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.Byte"), [], []),
    function () {
      return this.b;
    }
  );

  $.Method({Static:false, Public:true }, "get_Name", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.String"), [], []),
    function () {
      return this.name;
    }
  );

  $.Method({Static:false, Public:true }, "toString", 
    new JSIL.MethodSignature(mscorlib.TypeRef("System.String"), [], []),
    function () {
      if ((typeof (this.name) != "undefined") && (this.name != null))
        return this.name;
      else
        return System.String.Format("({0}, {1}, {2}, {3})", this.a, this.r, this.g, this.b);
    }
  );

  $.Method({Static:false, Public:true }, "MemberwiseClone", 
    new JSIL.MethodSignature("System.Object", [], [], $jsilcore),
    function () {
      if ((typeof (this.name) != "undefined") && (this.name != null)) {
        return this;
      } else {
        var result = Object.create(systemDrawing.System.Drawing.Color.prototype);
        result.a = this.a;
        result.r = this.r;
        result.g = this.g;
        result.b = this.b;
        return result;
      }
    }
  );
});

JSIL.ImplementExternals("System.Drawing.Size", function ($) {
  $.RawMethod(true, ".cctor", function () {
    System.Drawing.Size.prototype.width = 0;
    System.Drawing.Size.prototype.height = 0;
    System.Drawing.Size.Empty = new System.Drawing.Size();
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Int32, $.Int32], [])), 
    function _ctor (width, height) {
      this.width = width;
      this.height = height;
    }
  );

  $.Method({Static:false, Public:true }, "get_Height", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Height () {
      return this.height;
    }
  );

  $.Method({Static:false, Public:true }, "get_Width", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Width () {
      return this.width;
    }
  );

  $.Method({Static:false, Public:true }, "set_Height", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function set_Height (value) {
      this.height = value;
    }
  );

  $.Method({Static:false, Public:true }, "set_Width", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function set_Width (value) {
      this.width = value;
    }
  );
});

JSIL.ImplementExternals("System.Drawing.Point", function ($) {
  $.RawMethod(true, ".cctor", function () {
    System.Drawing.Point.prototype.x = 0;
    System.Drawing.Point.prototype.y = 0;
    System.Drawing.Point.Empty = new System.Drawing.Point();
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Int32, $.Int32], [])), 
    function _ctor (x, y) {
      this.x = x;
      this.y = y;
    }
  );

  $.Method({Static:false, Public:true }, "get_X", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_X () {
      return this.x;
    }
  );

  $.Method({Static:false, Public:true }, "get_Y", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Y () {
      return this.y;
    }
  );

  $.Method({Static:false, Public:true }, "set_X", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function set_X (value) {
      this.x = value;
    }
  );

  $.Method({Static:false, Public:true }, "set_Y", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function set_Y (value) {
      this.y = value;
    }
  );
});

JSIL.ImplementExternals("System.Drawing.Rectangle", function ($) {
  $.RawMethod(true, ".cctor", function () {
    System.Drawing.Rectangle.Empty = new System.Drawing.Rectangle();
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $.Int32, $.Int32, 
          $.Int32, $.Int32
        ], [])), 
    function _ctor (x, y, width, height) {
      this.x = x;
      this.y = y;
      this.width = width;
      this.height = height;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms[11].TypeRef("System.Drawing.Point"), $asms[11].TypeRef("System.Drawing.Size")], [])), 
    function _ctor (location, size) {
      this.x = location.X;
      this.y = location.Y;
      this.width = size.Width;
      this.height = size.Height;
    }
  );

  $.Method({Static:false, Public:true }, "get_Bottom", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Bottom () {
      return this.y + this.height;
    }
  );

  $.Method({Static:false, Public:true }, "get_Height", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Height () {
      return this.height;
    }
  );

  $.Method({Static:false, Public:true }, "get_Left", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Left () {
      return this.x;
    }
  );

  $.Method({Static:false, Public:true }, "get_Right", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Right () {
      return this.x + this.width;
    }
  );

  $.Method({Static:false, Public:true }, "get_Top", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Top () {
      return this.y;
    }
  );

  $.Method({Static:false, Public:true }, "get_Width", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Width () {
      return this.width;
    }
  );

  $.Method({Static:false, Public:true }, "get_X", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_X () {
      return this.x;
    }
  );

  $.Method({Static:false, Public:true }, "get_Y", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Y () {
      return this.y;
    }
  );

  $.Method({Static:false, Public:true }, "set_Height", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function set_Height (value) {
      this.height = value;
    }
  );

  $.Method({Static:false, Public:true }, "set_Width", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function set_Width (value) {
      this.width = value;
    }
  );

  $.Method({Static:false, Public:true }, "set_X", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function set_X (value) {
      this.x = value;
    }
  );

  $.Method({Static:false, Public:true }, "set_Y", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function set_Y (value) {
      this.y = value;
    }
  );
});