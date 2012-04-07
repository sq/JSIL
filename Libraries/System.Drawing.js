"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");
  
var $jsildrawing = JSIL.DeclareAssembly("JSIL.Drawing");

JSIL.DeclareNamespace("System");
JSIL.DeclareNamespace("System.Drawing");

if (JSIL.HostType.IsBrowser) {
  JSIL.ImplementExternals(
    "System.Drawing.Image", function ($) {
      var mscorlib = JSIL.GetAssembly("mscorlib", true);

      $.Method({Static:false, Public:true }, "Save", 
        new JSIL.MethodSignature(null, [mscorlib.TypeRef("System.Array") /* AnyType[] */ ], []),
        function (filename) {
          this.context.putImageData(this.buffer, 0, 0);
        }
      );

    }
  );

  JSIL.ImplementExternals(
    "System.Drawing.Bitmap", function ($) {
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
          System.Drawing.Image.prototype._ctor.call(this);

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

    }
  );
}

JSIL.ImplementExternals(
  "System.Drawing.Color", function ($) {
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

          sdc["get_" + name] = function () {
            return color;
          };

          Object.defineProperty(
            sdc, name, {
              enumerable: true,
              configurable: true,
              value: color
            }
          );
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
      new JSIL.MethodSignature($.Type, [], []),
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
  }
);

JSIL.ImplementExternals(
  "System.Drawing.Size", true, {
    _ctor: function (w, h) {
      this.width = w;
      this.height = h;
    },
    get_Width: function () {
      return this.width;
    },
    get_Height: function () {
      return this.height;
    },
    set_Width: function (value) {
      this.width = value;
    },
    set_Height: function (value) {
      this.height = value;
    }
  }
);
JSIL.ImplementExternals(
  "System.Drawing.Size", false, {
    _cctor: function () {
      System.Drawing.Size.prototype.width = 0;
      System.Drawing.Size.prototype.height = 0;
      System.Drawing.Size.Empty = new System.Drawing.Size();
    }
  }
);

JSIL.ImplementExternals(
  "System.Drawing.Point", false, {
    _cctor: function () {
      System.Drawing.Point.prototype.x = 0;
      System.Drawing.Point.prototype.y = 0;
      System.Drawing.Point.Empty = new System.Drawing.Point();
    }
  }
);
JSIL.ImplementExternals(
  "System.Drawing.Point", true, {
    _ctor: function (x, y) {
      this.x = x;
      this.y = y;
    },
    get_X: function () {
      return this.x;
    },
    get_Y: function () {
      return this.y;
    },
    set_X: function (value) {
      this.x = value;
    },
    set_Y: function (value) {
      this.y = value;
    }
  }
);

JSIL.ImplementExternals(
  "System.Drawing.Rectangle", false, {
    _cctor: function () {
      System.Drawing.Rectangle.Empty = new System.Drawing.Rectangle();
    }
  }
);
JSIL.ImplementExternals(
  "System.Drawing.Rectangle", true, {
    x: 0,
    y: 0,
    width: 0,
    height: 0,
    _ctor: function (x, y, w, h) {
      if ((typeof (x) === "object") && (typeof (y) === "object")) {
        this.x = x.X;
        this.y = x.Y;
        this.width = y.Width;
        this.height = y.Height;
      } else {
        this.x = x;
        this.y = y;
        this.width = w;
        this.height = h;
      }
    },
    get_X: function () {
      return this.x;
    },
    get_Y: function () {
      return this.y;
    },
    set_X: function (value) {
      this.x = value;
    },
    set_Y: function (value) {
      this.y = value;
    },
    get_Width: function () {
      return this.width;
    },
    get_Height: function () {
      return this.height;
    },
    set_Width: function (value) {
      this.width = value;
    },
    set_Height: function (value) {
      this.height = value;
    },
    get_Left: function () {
      return this.x;
    },
    get_Top: function () {
      return this.y;
    },
    get_Right: function () {
      return this.x + this.width;
    },
    get_Bottom: function () {
      return this.y + this.height;
    },
    MemberwiseClone: function () {
      var result = Object.create(System.Drawing.Rectangle.prototype);
      result.x = this.x;
      result.y = this.y;
      result.width = this.width;
      result.height = this.height;
      return result;
    }
  }
);