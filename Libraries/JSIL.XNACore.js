"use strict";

if (typeof (JSIL) === "undefined") throw new Error("JSIL.Core required");

var $jsilxna = JSIL.DeclareAssembly("JSIL.XNA");

var $sig = new JSIL.MethodSignatureCache();

var $asms = new JSIL.AssemblyCollection({
  corlib: "mscorlib",
  xna: "Microsoft.Xna.Framework",
  xnaGraphics: "Microsoft.Xna.Framework.Graphics",
  xnaGame: "Microsoft.Xna.Framework.Game",
  0: "Microsoft.Xna.Framework", 
  1: "Microsoft.Xna.Framework.Game", 
  3: "Microsoft.Xna.Framework.Graphics", 
  5: "mscorlib",
  11: "System.Drawing", 
  15: "System.Windows.Forms", 
  18: "Microsoft.Xna.Framework.Xact"
});

$jsilxna.nextImageId = 0;

$jsilxna.multipliedImageCache = {};
$jsilxna.multipliedImageCache.accessHistory = {};
$jsilxna.multipliedImageCache.capacity = 256; // unique images
$jsilxna.multipliedImageCache.capacityBytes = (1024 * 1024) * 16; // total image bytes (at 32bpp)
$jsilxna.multipliedImageCache.evictionMinimumAge = 250; // milliseconds
$jsilxna.multipliedImageCache.count = 0;
$jsilxna.multipliedImageCache.countBytes = 0;
$jsilxna.multipliedImageCache.evictionPending = false;

$jsilxna.colorRef = function () {
  var graphicsAsm = JSIL.GetAssembly("Microsoft.Xna.Framework.Graphics", true);
  if (graphicsAsm !== null)
    return $asms.xna.TypeRef("Microsoft.Xna.Framework.Color");
  else 
    return $asms.xna.TypeRef("Microsoft.Xna.Framework.Graphics.Color");
};

$jsilxna.graphicsRef = function (name) {
  var graphicsAsm = JSIL.GetAssembly("Microsoft.Xna.Framework.Graphics", true);
  if (graphicsAsm !== null)
    return graphicsAsm.TypeRef(name);
  else 
    return $asms.xna.TypeRef(name);
};

$jsilxna.multipliedImageCache.getItem = function (key) {
  this.accessHistory[key] = Date.now();

  this.maybeEvictItems();

  return this[key];
}.bind($jsilxna.multipliedImageCache);

$jsilxna.multipliedImageCache.setItem = function (key, value) {
  if (typeof (this[key]) === "undefined") {
    this.count += 1;
    this.countBytes += (value.width * value.height * 4);
  }

  this.accessHistory[key] = Date.now();
  this[key] = value;

  this.maybeEvictItems();
}.bind($jsilxna.multipliedImageCache);

$jsilxna.multipliedImageCache.maybeEvictItems = function () {
  if (this.evictionPending) return;

  if ((this.count >= this.capacity) || (this.countBytes >= this.capacityBytes)) {
    this.evictionPending = true;
    JSIL.Host.runLater(this.evictExtraItems);
  }
}.bind($jsilxna.multipliedImageCache);

$jsilxna.multipliedImageCache.evictExtraItems = function () {
  this.evictionPending = false;
  var keys = Object.keys(this.accessHistory);
  keys.sort(function (lhs, rhs) {
    var lhsTimestamp = this.accessHistory[lhs];
    var rhsTimestamp = this.accessHistory[rhs];
    if (lhsTimestamp > rhsTimestamp) return 1;
    else if (rhsTimestamp > lhsTimestamp) return -1;
    else return 0;
  }.bind(this));

  var now = Date.now();

  for (var i = 0, l = this.count; i < l; i++) {
    var age = now - this.accessHistory[keys[i]];
    if (age <= this.evictionMinimumAge) continue;

    var item = this[keys[i]];

    delete this.accessHistory[keys[i]];
    delete this[keys[i]];

    this.count -= 1;
    this.countBytes -= (item.width * item.height * 4);

    if (this.count <= this.capacity) break;
  }
}.bind($jsilxna.multipliedImageCache);

$jsilxna.getCachedMultipliedImage = function (image, color) {
  var imageId = image.getAttribute("__imageId") || null;
  if (imageId === null) image.setAttribute("__imageId", imageId = new String($jsilxna.nextImageId++));

  var key = imageId + color.toCss(255);
  var result = $jsilxna.multipliedImageCache.getItem(key) || null;
  return result;
};

$jsilxna.setCachedMultipliedImage = function (image, color, value) {
  var imageId = image.getAttribute("__imageId") || null;
  if (imageId === null) image.setAttribute("__imageId", imageId = new String($jsilxna.nextImageId++));

  var key = imageId + color.toCss(255);
  $jsilxna.multipliedImageCache.setItem(key, value);
};

$jsilxna.getImageMultiplied = function (image, color) {
  var cached = $jsilxna.getCachedMultipliedImage(image, color);
  if (cached !== null) {
    if (typeof (cached.canvas) !== "undefined") 
      return cached.canvas;
    else 
      return cached;
  }

  // Workaround for chromium bug where sometimes images aren't fully initialized.
  if ((image.naturalWidth < 1) || (image.naturalHeight < 1))
    return null;

  var canvas = document.createElement("canvas");
  var context = canvas.getContext("2d");

  // Workaround for bug in Firefox's canvas implementation that treats the outside of a canvas as solid white
  canvas.width = image.naturalWidth + 2;
  canvas.height = image.naturalHeight + 2;

  context.globalCompositeOperation = "copy";
  context.globalCompositeAlpha = 1.0;
  context.clearRect(0, 0, image.naturalWidth + 2, image.naturalHeight + 2);
  context.drawImage(image, 1, 1);

  try {
    var imageData = context.getImageData(1, 1, image.naturalWidth, image.naturalHeight);
    var rmul = color.r / 255;
    var gmul = color.g / 255;
    var bmul = color.b / 255;
    var bytes = imageData.data;

    for (var i = 0, l = image.naturalWidth * image.naturalHeight * 4; i < l; i += 4) {
      bytes[i] *= rmul;
      bytes[i + 1] *= gmul;
      bytes[i + 2] *= bmul;
    }

    context.putImageData(imageData, 1, 1);

    $jsilxna.setCachedMultipliedImage(image, color, canvas);
  } catch (exc) {
    return image;
  }

  return canvas;
};

$jsilxna.getImageTopLeftPixelMultiplied = function (image, color) {
  var cached = image.topLeftPixel;
  if (typeof (cached) === "string") return cached;

  var canvas = document.createElement("canvas");
  var context = canvas.getContext("2d");

  // Workaround for bug in Firefox's canvas implementation that treats the outside of a canvas as solid white
  canvas.width = 1;
  canvas.height = 1;

  context.globalCompositeOperation = "copy";
  context.globalCompositeAlpha = 1.0;
  context.clearRect(0, 0, 1, 1);
  context.drawImage(image, 0, 0);

  var result = "rgba(0, 0, 0, 0)";
  try {
    var imageData = context.getImageData(0, 0, 1, 1);

    var r = imageData.data[0];
    var g = imageData.data[1];
    var b = imageData.data[2];
    var a = imageData.data[3] / 255;

    var m = 1.0 / a;

    if ((typeof (color) === "object") && (color !== null)) {
      r = (r * color.R) / 255;
      g = (g * color.G) / 255;
      b = (b * color.B) / 255;
    }

    r = $jsilxna.ClampByte(r * m);
    g = $jsilxna.ClampByte(g * m);
    b = $jsilxna.ClampByte(b * m);

    image.topLeftPixel = result = "rgba(" + r + ", " + g + ", " + b + ", " + a + ")";
  } catch (exc) {}

  return result;
};

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentLoadException", function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (message) {
    this._Message = String(message);
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentManager", function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$asms.corlib.TypeRef("System.IServiceProvider")], []), function (serviceProvider) {});
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$asms.corlib.TypeRef("System.IServiceProvider"), $.String], []), function (serviceProvider, rootDirectory) {
    this._rootDirectory = rootDirectory;
  });
  $.Method({
    Static: false,
    Public: true
  }, "Load", new JSIL.MethodSignature("!!0", [$.String], ["T"]), 
  function (T, assetName) {
    var asset;

    try {
      asset = JSIL.Host.getAsset(assetName);
    } catch (exc) {
      if (exc.Message.indexOf("is not in the asset manifest") >= 0) {
        throw new Microsoft.Xna.Framework.Content.ContentLoadException(exc.Message);
      } else {
        throw exc;
      }
    }

    var rawXnb = JSIL.TryCast(asset, RawXNBAsset);
    if (rawXnb !== null) {
      rawXnb.contentManager = this;
      var result = rawXnb.ReadAsset(T);

      if (result === null)
        JSIL.Host.warning("Asset '" + assetName + "' loader returned null.");
      return result;
    }

    if (JSIL.CheckType(asset, HTML5Asset)) {
      if (asset === null)
        JSIL.Host.warning("Asset '" + assetName + "' loader returned null.");

      return asset;
    }

    throw new Microsoft.Xna.Framework.Content.ContentLoadException("Asset '" + assetName + "' is not an instance of HTML5Asset.");
  }),
  $.Method({
    Static: false,
    Public: true
  }, "Unload", new JSIL.MethodSignature(null, [], []), function () {
    // Unnecessary since we rely on the host to preload our assets.
  });
  $.Method({
    Static: false,
    Public: true
  }, "set_RootDirectory", new JSIL.MethodSignature(null, [$.String], []), function (rootDirectory) {
    this._rootDirectory = rootDirectory;
  });
  $.Method({
    Static: false,
    Public: true
  }, "get_RootDirectory", new JSIL.MethodSignature($.String, [], []), function () {
    return this._rootDirectory;
  });
});

JSIL.MakeClass($jsilcore.System.Object, "HTML5Asset", true, [], function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (assetName) {
    this.name = assetName;
  });
  $.Method({
    Static: false,
    Public: true
  }, "toString", new JSIL.MethodSignature(null, [], []), function () {
    return "<XNA Asset '" + this.name + "'>";
  });
});

JSIL.MakeClass("HTML5Asset", "HTML5ImageAsset", true, [], function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (assetName, image) {
    HTML5Asset.prototype._ctor.call(this, assetName);
    this.image = image;
    this.Width = image.naturalWidth;
    this.Height = image.naturalHeight;
  });
});

JSIL.MakeClass("HTML5Asset", "HTML5SoundAsset", true, [], function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (assetName, sound) {
    HTML5Asset.prototype._ctor.call(this, assetName);
    this.sound = sound;
    this.freeInstances = [
      this.$createInstance(0)
    ];
  });

  $.RawMethod(false, "$createInstance", function (loopCount) {
    var instance = this.sound.cloneNode(true);

    if (loopCount > 0) {
      var state = [loopCount];

      instance.addEventListener("ended", function () {
        if (state[0] > 0) {
          state[0]--;
          instance.play();
        }
      }.bind(this), true);
    } else {
      instance.addEventListener("ended", function () {
        if (this.freeInstances.length < 16) this.freeInstances.push(instance);
      }.bind(this), true);
    }

    return instance;
  });

  $.Method({Static:false, Public:true }, "Play", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function Play () {
      var instance;
      if (this.freeInstances.length > 0) {
        instance = this.freeInstances.pop();
      } else {
        instance = this.$createInstance(0);
      }

      instance.play();

      return true;
    }
  );

});

JSIL.MakeClass("HTML5Asset", "WebkitSoundAsset", true, [], function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (assetName, audioContext, buffer) {
    HTML5Asset.prototype._ctor.call(this, assetName);
    this.audioContext = audioContext;
    this.buffer = buffer;
  });

  $.RawMethod(false, "$createInstance", function (loopCount) {
    var instance = this.audioContext.createBufferSource();
    instance.buffer = this.buffer;
    instance.loop = loopCount > 0;
    instance.connect(this.audioContext.destination);
    return {
      source: instance,
      play: function () {
        instance.noteOn(0);
      },
      pause: function () {
        instance.noteOff(0);
      }
    };
  });

  $.Method({Static:false, Public:true }, "Play", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function Play () {
      var instance = this.$createInstance(0);

      instance.play();

      return true;
    }
  );
});

JSIL.MakeClass("HTML5Asset", "HTML5FontAsset", true, [], function ($) {
  $.prototype._cachedCss = null;

  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (assetName, id, pointSize, lineHeight) {
    HTML5Asset.prototype._ctor.call(this, assetName);
    this.id = id;
    this.pointSize = pointSize;
    this.lineHeight = lineHeight;
    this.canvas = JSIL.Host.getCanvas();
    this.context = this.canvas.getContext("2d");
    Object.defineProperty(this, "LineSpacing", {
      get: function () {
        return this.lineHeight;
      }
    });
  });

  $.RawMethod(false, "toCss", function (scale) {
    scale = (scale || 1.0);
    if ((this._cachedCss != null) && (this._cachedScale === scale)) {
      return this._cachedScale;
    } else {
      this._cachedScale = scale;
      return this._cachedCss = (this.pointSize * scale) + 'pt "' + this.id + '"';
    }
  });

  $.Method({
    Static: false,
    Public: true
  }, "MeasureString", new JSIL.MethodSignature(null, [], []), function (text) {
    this.context.font = this.toCss();
    var lines = text.split("\n");

    var resultX = 0,
      resultY = 0;

    for (var i = 0, l = lines.length; i < l; i++) {
      var metrics = this.context.measureText(lines[i]);
      resultX = Math.max(resultX, metrics.width);
      resultY += this.lineHeight;
    }

    return new Microsoft.Xna.Framework.Vector2(resultX, resultY);
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentTypeReader", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms[5].TypeRef("System.Type")], [])), 
    function _ctor (targetType) {
      this.targetType = targetType;
      this.TargetIsValueType = !targetType.__IsReferenceType__;
    }
  );

  $.Method({Static:false, Public:true }, "get_CanDeserializeIntoExistingObject", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_CanDeserializeIntoExistingObject () {
      return false;
    }
  );

  $.Method({Static:false, Public:true }, "get_TargetType", 
    (new JSIL.MethodSignature($asms[5].TypeRef("System.Type"), [], [])), 
    function get_TargetType () {
      return this.targetType;
    }
  );

  $.Method({Static:false, Public:true }, "get_TypeVersion", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_TypeVersion () {
      return 0;
    }
  );

  $.Method({Static:false, Public:false}, "Initialize", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReaderManager")], [])), 
    function Initialize (manager) {
      
    }
  );

  /*
    This requires overload resolution.
  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Object, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Object], [])), 
    function Read (input, existingInstance) {
      throw new Error("Invoked abstract method (ContentTypeReader.Read)");
    }
  );
  */

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentTypeReader`1", function ($) {
  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      var assembly = $asms.xna;

      assembly.Microsoft.Xna.Framework.Content.ContentTypeReader.prototype._ctor.call(
        this, assembly.Microsoft.Xna.Framework.Content.ContentTypeReader$b1.T.get(this)
      );
    }
  );

  /*
    This requires overload resolution.

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Object, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Object], [])), 
    function Read (input, existingInstance) {
      throw new Error("Invoked abstract method (ContentTypeReader`1.Read)");
    }
  );

  var gp = new JSIL.GenericParameter("T", "Microsoft.Xna.Framework.Content.ContentTypeReader`1");

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature(gp, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), gp], [])), 
    function Read (input, existingInstance) {
      throw new Error("Invoked abstract method (ContentTypeReader`1.Read)");
    }
  );

  */

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.StringReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.String, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.String], [])), 
    function Read (input, existingInstance) {
      return input.ReadString();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ByteReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Byte, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Byte], [])), 
    function Read (input, existingInstance) {
      return input.ReadByte();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.CharReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Char, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Char], [])), 
    function Read (input, existingInstance) {
      return input.ReadChar();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Int16Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Int16, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Int16], [])), 
    function Read (input, existingInstance) {
      return input.ReadInt16();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Int32Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Int32, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Int32], [])), 
    function Read (input, existingInstance) {
      return input.ReadInt32();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.UInt16Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.UInt16, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.UInt16], [])), 
    function Read (input, existingInstance) {
      return input.ReadUInt16();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.UInt32Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.UInt32, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.UInt32], [])), 
    function Read (input, existingInstance) {
      return input.ReadUInt32();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.PointReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Point"), [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $asms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    function Read (input, existingInstance) {
      var x = input.ReadInt32();
      var y = input.ReadInt32();

      var result = new Microsoft.Xna.Framework.Point(x, y);
      return result;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.RectangleReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
    function Read (input, existingInstance) {
      var x = input.ReadInt32();
      var y = input.ReadInt32();
      var w = input.ReadInt32();
      var h = input.ReadInt32();

      var result = new Microsoft.Xna.Framework.Rectangle(x, y, w, h);
      return result;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Vector2Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2")], [])), 
    function Read (input, existingInstance) {
      return input.ReadVector2();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Vector3Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function Read (input, existingInstance) {
      return input.ReadVector3();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Vector4Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Vector4"), [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $asms[0].TypeRef("Microsoft.Xna.Framework.Vector4")], [])), 
    function Read (input, existingInstance) {
      return input.ReadVector4();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ArrayReader`1", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      var assembly = $asms.xna;
      assembly.Microsoft.Xna.Framework.Content.ContentTypeReader$b1.prototype._ctor.call(
        this, System.Array.Of(
          assembly.Microsoft.Xna.Framework.Content.ArrayReader$b1.T.get(this)
        ).__Type__
      );
    }
  );

  $.Method({Static:false, Public:false}, "Initialize", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReaderManager")], [])), 
    function Initialize (manager) {
      this.elementReader = manager.GetTypeReader(this.T);
    }
  );

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "Microsoft.Xna.Framework.Content.ArrayReader`1")]), [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "Microsoft.Xna.Framework.Content.ArrayReader`1")])], [])), 
    function Read (input, existingInstance) {
      var count = input.ReadInt32();
      if (existingInstance === null) {
        existingInstance = new Array(count);
      }

      for (var i = 0; i < count; i++) {
        existingInstance[i] = input.ReadObjectInternal(this.T)(this.elementReader, null);
      }

      return existingInstance;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ListReader`1", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      var assembly = $asms.xna;
      assembly.Microsoft.Xna.Framework.Content.ContentTypeReader$b1.prototype._ctor.call(
        this, System.Collections.Generic.List$b1.Of(
          assembly.Microsoft.Xna.Framework.Content.ListReader$b1.T.get(this)
        ).__Type__
      );
    }
  );

  $.Method({Static:false, Public:false}, "Initialize", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReaderManager")], [])), 
    function Initialize (manager) {
      this.elementReader = manager.GetTypeReader(this.T);
    }
  );

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($asms[5].TypeRef("System.Collections.Generic.List`1", [new JSIL.GenericParameter("T", "Microsoft.Xna.Framework.Content.ListReader`1")]), [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $asms[5].TypeRef("System.Collections.Generic.List`1", [new JSIL.GenericParameter("T", "Microsoft.Xna.Framework.Content.ListReader`1")])], [])), 
    function Read (input, existingInstance) {
      var count = input.ReadInt32();
      if (existingInstance === null) {
        existingInstance = new(System.Collections.Generic.List$b1.Of(this.T))();
      }

      while (count > 0) {
        var item = input.ReadObjectInternal(this.T)(this.elementReader, null);
        count--;
        existingInstance.Add(item);
      }

      return existingInstance;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Texture2DReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D")], [])), 
    function Read (input, existingInstance) {
      var asmGraphics = $asms.xnaGraphics || $asms.xna;
      var tTexture2D = JSIL.GetTypeFromAssembly(asmGraphics, "Microsoft.Xna.Framework.Graphics.Texture2D", [], true);
      var tSurfaceFormat = asmGraphics.Microsoft.Xna.Framework.Graphics.SurfaceFormat;

      var surfaceFormat = JSIL.Cast(input.ReadInt32(), tSurfaceFormat);
      var width = input.ReadInt32();
      var height = input.ReadInt32();
      var mipCount = input.ReadInt32();

      var result = existingInstance;
      if (result === null) result = JSIL.CreateInstanceOfType(tTexture2D, "$internalCtor", [null, width, height, mipCount > 1, surfaceFormat]);

      for (var i = 0; i < mipCount; i++) {
        var mipSize = input.ReadInt32();
        var mipBytes = input.ReadBytes(mipSize);

        if (i === 0) 
          result.SetData(System.Byte)(i, null, mipBytes, 0, mipSize);
      }

      return result;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.SpriteFontReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont"), [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont")], [])), 
    function Read (input, existingInstance) {
      var asmXna = $asms.xna;
      var asmGraphics = $asms.xnaGraphics || $asms.xna;

      var tList = System.Collections.Generic.List$b1;
      var tSpriteFont = asmGraphics.Microsoft.Xna.Framework.Graphics.SpriteFont;
      var tTexture2D = asmGraphics.Microsoft.Xna.Framework.Graphics.Texture2D;
      var tRectangle = asmXna.Microsoft.Xna.Framework.Rectangle;
      var tVector3 = asmXna.Microsoft.Xna.Framework.Vector3;

      var texture = input.ReadObject(tTexture2D)();

      var glyphs = input.ReadObject(tList.Of(tRectangle))();

      var cropping = input.ReadObject(tList.Of(tRectangle))();

      var charMap = input.ReadObject(tList.Of(System.Char))();

      var lineSpacing = input.ReadInt32();
      var spacing = input.ReadSingle();

      var kerning = input.ReadObject(tList.Of(tVector3))();

      var defaultCharacter = null;
      if (input.ReadBoolean()) defaultCharacter = input.ReadChar();

      var result = new tSpriteFont(
      texture, glyphs, cropping, charMap, lineSpacing, spacing, kerning, defaultCharacter);

      return result;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.EffectReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.Effect"), [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.Effect")], [])), 
    function Read (input, existingInstance) {
      var count = input.ReadInt32();
      var effectCode = input.ReadBytes(count);

      // FIXME
      return JSIL.CreateInstanceOfType(Microsoft.Xna.Framework.Graphics.Effect.__Type__, null);
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentTypeReaderManager", function ($) {
  $.Method({Static: true, Public: true}, ".cctor", 
    new JSIL.MethodSignature(null, [], []), 
    function () {
      var assembly = $asms.xna;
      var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

      thisType.nameToReader = {};
      thisType.targetTypeToReader = {};
      thisType.readerTypeToReader = {};
    }
  );

  $.Method({Static:true , Public:false}, "AddTypeReader", 
    (new JSIL.MethodSignature(null, [
          $.String, $asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader")
        ], [])), 
    function AddTypeReader (readerTypeName, contentReader, reader) {
      var assembly = $asms.xna;
      var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

      var targetType = reader.TargetType;
      thisType.targetTypeToReader[targetType.__TypeId__] = reader;
      thisType.readerTypeToReader[reader.GetType().__TypeId__] = reader;
      thisType.nameToReader[readerTypeName] = reader;
    }
  );

  $.Method({Static:true , Public:false}, "GetTypeReader", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader"), [$asms[5].TypeRef("System.Type"), $asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader")], [])), 
    function GetTypeReader (targetType, contentReader) {
      var assembly = $asms.xna;
      var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

      var result = thisType.targetTypeToReader[targetType.__TypeId__];

      if (typeof (result) !== "object") {
        JSIL.Host.error(new Error("No content type reader known for type '" + targetType + "'."));
        return null;
      }

      return result;
    }
  );

  $.Method({Static:true , Public:false}, "ReadTypeManifest", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader")]), [$.Int32, $asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader")], [])), 
    function ReadTypeManifest (typeCount, contentReader) {
      var assembly = $asms.xna;
      var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

      var result = new Array(typeCount);
      var readerManager = new thisType(contentReader);

      for (var i = 0; i < typeCount; i++) {
        var typeReaderName = contentReader.ReadString();
        var typeReaderVersionNumber = contentReader.ReadInt32();

        // We need to explicitly make the xna assembly the default search context since many of the readers are private classes
        var parsedTypeName = JSIL.ParseTypeName(typeReaderName);
        var typeReaderType = JSIL.GetTypeInternal(parsedTypeName, assembly, false);

        if (typeReaderType === null) {
          JSIL.Host.error(new Error("The type '" + typeReaderName + "' could not be found while loading asset '" + contentReader.assetName + "'."));
          return null;
        }

        var typeReaderInstance = JSIL.CreateInstanceOfType(typeReaderType);
        var targetType = typeReaderInstance.TargetType;
        var targetTypeName = targetType.toString();

        thisType.AddTypeReader(typeReaderName, contentReader, typeReaderInstance);

        readerManager.knownReaders[targetTypeName] = typeReaderInstance;

        result[i] = typeReaderInstance;
      }

      for (var i = 0; i < typeCount; i++) {
        result[i].Initialize(readerManager);
      }

      return result;
    }
  );

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader")], [])), 
    function _ctor (contentReader) {
      this.contentReader = contentReader;
      this.knownReaders = {};
    }
  );

  $.Method({Static:false, Public:true }, "GetTypeReader", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader"), [$asms[5].TypeRef("System.Type")], [])), 
    function GetTypeReader (targetType) {
      var typeName = targetType.toString();
      var reader = this.knownReaders[typeName];
      if (typeof (reader) === "object") 
        return reader;

      var assembly = $asms.xna;
      var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

      reader = thisType.GetTypeReader(targetType, this.contentReader);
      if (typeof (reader) === "object")
        return reader;

      JSIL.Host.error(new Error("No content type reader known for type '" + typeName + "'."));
      return null;
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentReader", function ($) {
  $.Method({Static:false, Public:false}, ".ctor", 
    $sig.get("newCr", null, [
        $asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentManager"), $asms[5].TypeRef("System.IO.Stream"), 
        $.String, $asms[5].TypeRef("System.Action`1", [$asms[5].TypeRef("System.IDisposable")]), 
        $.Int32
      ], []), 
    function _ctor (contentManager, input, assetName, recordDisposableObject, graphicsProfile) {
      var signature = new JSIL.MethodSignature(null, [$asms[5].TypeRef("System.IO.Stream")], []);
      signature.Call(System.IO.BinaryReader.prototype, "_ctor", null, this, input);

      this.contentManager = contentManager;
      this.assetName = assetName;
      this.recordDisposableObject = recordDisposableObject;
      this.graphicsProfile = graphicsProfile;

      this.typeReaders = null;
    }
  );

  $.Method({Static:false, Public:true }, "get_AssetName", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_AssetName () {
      return this.assetName;
    }
  );

  $.Method({Static:false, Public:true }, "get_ContentManager", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentManager"), [], [])), 
    function get_ContentManager () {
      return this.contentManager;
    }
  );


  $.Method({Static:false, Public:false}, "ReadHeader", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function ReadHeader () {
      var formatHeader = String.fromCharCode.apply(String, this.ReadBytes(3));
      if (formatHeader != "XNB") throw new Error("Invalid XNB format");

      var platformId = String.fromCharCode(this.ReadByte());
      switch (platformId) {
      case "w":
        break;
      default:
        throw new Error("Unsupported XNB platform: " + platformId);
      }

      var formatVersion = this.ReadByte();
      switch (formatVersion) {
      case 4:
      case 5:
        break;
      default:
        throw new Error("Unsupported XNB format version: " + formatVersion);
      }

      var formatFlags = this.ReadByte();

      var isHiDef = (formatFlags & 0x01) != 0;
      var isCompressed = (formatFlags & 0x80) != 0;

      if (isCompressed) throw new Error("Compressed XNBs are not supported");

      var uncompressedSize = this.ReadUInt32();

      var typeReaderCount = this.Read7BitEncodedInt();
      this.typeReaders = Microsoft.Xna.Framework.Content.ContentTypeReaderManager.ReadTypeManifest(typeReaderCount, this);
    }
  );

  $.Method({Static: false, Public: true}, "ReadString", 
    new JSIL.MethodSignature($.String, [], []), 
    function () {
      var length = this.Read7BitEncodedInt();
      var chars = this.ReadBytes(length);
      return String.fromCharCode.apply(String, chars);
    }
  );

  $.Method({Static:false, Public:true }, "ReadVector2", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [], [])), 
    function ReadVector2 () {
      var x = this.ReadSingle();
      var y = this.ReadSingle();
      return new Microsoft.Xna.Framework.Vector2(x, y);
    }
  );

  $.Method({Static:false, Public:true }, "ReadVector3", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), [], [])), 
    function ReadVector3 () {
      var x = this.ReadSingle();
      var y = this.ReadSingle();
      var z = this.ReadSingle();
      return new Microsoft.Xna.Framework.Vector3(x, y, z);
    }
  );

  $.Method({Static:false, Public:true }, "ReadVector4", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Vector4"), [], [])), 
    function ReadVector4 () {
      var x = this.ReadSingle();
      var y = this.ReadSingle();
      var z = this.ReadSingle();
      var w = this.ReadSingle();
      return new Microsoft.Xna.Framework.Vector4(x, y, z, w);
    }
  );

  var readObjectImpl = function (self, T, typeReader, existingInstance) {
    if ((typeReader !== null) && (typeReader.TargetIsValueType))
      return typeReader.Read(self, existingInstance);

    var typeId = self.Read7BitEncodedInt();

    if (typeId === 0)
      return null;

    typeReader = self.typeReaders[typeId - 1];
    if (typeof (typeReader) !== "object") {
      JSIL.Host.error(new Error("No type reader for typeId '" + typeId + "'. Misaligned XNB read is likely."));
      return null;
    }

    return typeReader.Read(self, existingInstance);
  };

  $.Method({Static:false, Public:true }, "ReadObject", 
    (new JSIL.MethodSignature("!!0", [], ["T"])), 
    function ReadObject$b1 (T) {
      return readObjectImpl(this, T, null, JSIL.DefaultValue(T));
    }
  );

  $.Method({Static:false, Public:true }, "ReadObject", 
    (new JSIL.MethodSignature("!!0", ["!!0"], ["T"])), 
    function ReadObject$b1 (T, existingInstance) {
      return readObjectImpl(this, T, null, existingInstance);
    }
  );

  $.Method({Static:false, Public:true }, "ReadObject", 
    (new JSIL.MethodSignature("!!0", [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader"), "!!0"], ["T"])), 
    function ReadObject$b1 (T, typeReader, existingInstance) {
      return readObjectImpl(this, T, contentTypeReader, existingInstance);
    }
  );

  $.Method({Static:false, Public:false}, "ReadObjectInternal", 
    (new JSIL.MethodSignature("!!0", [$.Object], ["T"])), 
    function ReadObjectInternal$b1 (T, existingInstance) {
      return readObjectImpl(this, T, null, existingInstance);
    }
  );

  $.Method({Static:false, Public:false}, "ReadObjectInternal", 
    (new JSIL.MethodSignature("!!0", [$asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader"), $.Object], ["T"])), 
    function ReadObjectInternal$b1 (T, typeReader, existingInstance) {
      return readObjectImpl(this, T, typeReader, existingInstance);
    }
  );

  var readRawObjectImpl = function (self, T, existingInstance) {
    var assembly = $asms.xna;
    var ctrm = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

    var typeReader = ctrm.GetTypeReader(T, self);
    return typeReader.Read(self, existingInstance);
  };

  $.Method({Static:false, Public:true }, "ReadRawObject", 
    (new JSIL.MethodSignature("!!0", [], ["T"])), 
    function ReadRawObject$b1 (T) {
      return readRawObjectImpl(this, T, JSIL.DefaultValue(T));
    }
  );

  $.Method({Static:false, Public:true }, "ReadRawObject", 
    (new JSIL.MethodSignature("!!0", ["!!0"], ["T"])), 
    function ReadRawObject$b1 (T, existingInstance) {
      return readRawObjectImpl(this, T, existingInstance);
    }
  );

});

JSIL.MakeClass("HTML5Asset", "RawXNBAsset", true, [], function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (assetName, rawBytes) {
    if (JSIL.GetAssembly("JSIL.IO", true) === null) {
      throw new Error("JSIL.IO is required");
    }

    HTML5Asset.prototype._ctor.call(this, assetName);
    this.bytes = rawBytes;
    this.contentManager = null;
  });
  $.Method({
    Static: false,
    Public: true
  }, "ReadAsset", new JSIL.MethodSignature(null, [], []), function (type) {
    var memoryStream = new System.IO.MemoryStream(this.bytes, false);

    var tContentReader = JSIL.GetTypeFromAssembly(
      $asms.xna, "Microsoft.Xna.Framework.Content.ContentReader", [], true
    );
    var contentReader = $sig.get("newCr").Construct(
      tContentReader, this.contentManager, memoryStream, this.name, null, 0
    );

    contentReader.ReadHeader();

    var sharedResourceCount = contentReader.Read7BitEncodedInt();
    var sharedResources = new Array(sharedResourceCount);

    var mainObject = contentReader.ReadObject(type)();

    for (var i = 0; i < sharedResourceCount; i++)
    sharedResources[i] = content.ReadObject(System.Object)();

    return mainObject;
  });
});

JSIL.MakeClass("RawXNBAsset", "SpriteFontAsset", true, [], function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (assetName, rawBytes) {
    RawXNBAsset.prototype._ctor.call(this, assetName, rawBytes);
  });
});

JSIL.MakeClass("RawXNBAsset", "Texture2DAsset", true, [], function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (assetName, rawBytes) {
    RawXNBAsset.prototype._ctor.call(this, assetName, rawBytes);
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Audio.AudioEngine", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function _ctor (settingsFile) {
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $.String, $asms[5].TypeRef("System.TimeSpan"), 
          $.String
        ], [])), 
    function _ctor (settingsFile, lookAheadTime, rendererId) {
    }
  );

  $.Method({Static:false, Public:true }, "Update", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Update () {
    }
  );

  $.Method({Static:false, Public:true }, "GetCategory", 
    (new JSIL.MethodSignature($asms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioCategory"), [$.String], [])), 
    function GetCategory (name) {
      // FIXME
      return new Microsoft.Xna.Framework.Audio.AudioCategory(this, name);
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Audio.AudioCategory", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioEngine"), $.String], [])), 
    function _ctor (engine, name) {
      this._engine = engine;
      this._name = name;
    }
  );

  $.Method({Static:false, Public:true }, "get_Name", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_Name () {
      return this._name;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Audio.WaveBank", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioEngine"), $.String], [])), 
    function _ctor (audioEngine, nonStreamingWaveBankFilename) {
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $asms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioEngine"), $.String, 
          $.Int32, $.Int16
        ], [])), 
    function _ctor (audioEngine, streamingWaveBankFilename, offset, packetsize) {
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Audio.Cue", function ($) {
  $.RawMethod(false, "internalCtor", function (name, soundBank, sounds, audioEngine) {
    this._name = name;
    this.soundBank = soundBank;
    this.sounds = sounds;
    this.audioEngine = audioEngine;
    this.wavesPlaying = [];
  });

  $.Method({
    Static: false,
    Public: true
  }, "get_Name", new JSIL.MethodSignature($.String, [], []), function () {
    return this._name;
  });

  $.Method({Static:false, Public:true }, "Pause", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Pause () {
      for (var i = 0; i < this.wavesPlaying.length; i++) {
        var wave = this.wavesPlaying[i];
        wave.pause()
      }
    }
  );

  $.Method({Static:false, Public:true }, "Play", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Play () {
      var soundName = this.sounds[0];
      var sound = this.soundBank.sounds[soundName];

      for (var i = 0; i < sound.Tracks.length; i++) {
        var track = sound.Tracks[i];
        for (var j = 0; j < track.Events.length; j++) {
          var evt = track.Events[j];

          switch (evt.Type) {
          case "PlayWaveEvent":
            var waveName = evt.Wave;
            var wave = this.soundBank.waves[waveName];

            // Handle broken audio implementations
            if (wave !== null) {
              var instance = wave.$createInstance(evt.LoopCount);
              instance.play();

              this.wavesPlaying.push(instance);
            }

            break;
          }
        }
      }
    }
  );

  $.Method({Static:false, Public:true }, "Resume", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Resume () {
      for (var i = 0; i < this.wavesPlaying.length; i++) {
        var wave = this.wavesPlaying[i];
        wave.play()
      }
    }
  );

  $.Method({Static:false, Public:true }, "Stop", 
    (new JSIL.MethodSignature(null, [$asms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioStopOptions")], [])), 
    function Stop (options) {
      while (this.wavesPlaying.length > 0) {
        var wave = this.wavesPlaying.shift();
        wave.pause()
      }
    }
  );

  $.Method({Static:false, Public:true }, "Dispose", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Dispose () {
      this.Stop();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Audio.SoundBank", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioEngine"), $.String], [])), 
    function _ctor (audioEngine, filename) {
      var json = JSIL.Host.getAsset(filename, true);

      this.name = json.Name;

      this.cues = {};
      this.sounds = {};
      this.waves = {};

      for (var i = 0, l = json.Cues.length; i < l; i++) {
        var cue = json.Cues[i];

        this.cues[cue.Name] = cue;
      }

      for (var i = 0, l = json.Sounds.length; i < l; i++) {
        var sound = json.Sounds[i];

        this.sounds[sound.Name] = sound;
      }

      for (var name in json.Waves) {
        var filename = json.Waves[name];
        var waveAsset = JSIL.Host.getAsset(filename);

        this.waves[name] = waveAsset;
      }
    }
  );

  $.Method({Static:false, Public:true }, "GetCue", 
    (new JSIL.MethodSignature($asms[18].TypeRef("Microsoft.Xna.Framework.Audio.Cue"), [$.String], [])), 
    function GetCue (name) {
      var cue = this.cues[name];
      var result = JSIL.CreateInstanceOfType(
        Microsoft.Xna.Framework.Audio.Cue.__Type__, "internalCtor", [cue.Name, this, cue.Sounds, this.audioEngine]
      );
      return result;
    }
  );

  $.Method({Static:false, Public:true }, "PlayCue", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function PlayCue (name) {
      var cue = this.GetCue(name);
      cue.Play();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Media.MediaPlayer", function ($) {
  $.Method({
    Static: true,
    Public: true
  }, ".cctor", new JSIL.MethodSignature(null, [], []), function () {
    Microsoft.Xna.Framework.Media.MediaPlayer.repeat = false;
    Microsoft.Xna.Framework.Media.MediaPlayer.currentSong = null;
  });

  $.Method({Static:true , Public:true }, "get_IsRepeating", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsRepeating () {
      return Microsoft.Xna.Framework.Media.MediaPlayer.repeat;
    }
  );

  var playImpl = function (song) {
    var oldInstance = Microsoft.Xna.Framework.Media.MediaPlayer.currentSong;
    var newInstance = null;

    if (song !== null) {
      newInstance = song.$createInstance(
        Microsoft.Xna.Framework.Media.MediaPlayer.repeat ? 9999 : 0
      );
    }

    if (oldInstance !== null) 
      oldInstance.pause();

    if (newInstance !== null) 
      newInstance.play();

    Microsoft.Xna.Framework.Media.MediaPlayer.currentSong = newInstance;
  };

  $.Method({Static:true , Public:true }, "Play", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Media.Song")], [])), 
    playImpl
  );

  $.Method({Static:true , Public:true }, "set_IsRepeating", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_IsRepeating (value) {
      Microsoft.Xna.Framework.Media.MediaPlayer.repeat = value;
    }
  );
});

var vectorUtil = {
  makeOperatorCore: function (name, tVector, body, argCount, leftScalar, rightScalar) {
    var js = body.join("\r\n");

    var typeName = String(tVector.typeName);
    var suffixedName;

    if (argCount < 1) {
      suffixedName = name;
    } else {
      suffixedName = name + "[";
      if (argCount == 1) {
        suffixedName += (leftScalar ? "float" : "vec");
      } else if (argCount == 2) {
        suffixedName += (leftScalar ? "float" : "vec") + "," +
          (rightScalar ? "float" : "vec");
      }
      suffixedName += "]";
    }

    var uri = "jsil://operator/" + typeName + "/" + suffixedName;

    return new Function(
      "lhs", "rhs", 
      "//@ sourceURL=" + uri + "\r\n" + js
    );
  },

  bindToPrototype: function (fn, typeRef) {
    var state = {
      resolvedType: null,
      typeRef: typeRef
    };

    Object.defineProperty(state, "prototype", {
      configurable: true,
      enumerable: false,
      get: function () {
        if (state.resolvedType === null)
          state.resolvedType = state.typeRef.get();
        
        return state.resolvedType.prototype;
      }
    });

    return fn.bind(state);
  },

  makeArithmeticOperator: function ($, name, staticMethodName, operator, dataMembers, tLeft, tRight, tResult) {
    var leftScalar = (tLeft !== tResult);
    var rightScalar = (tRight !== tResult);

    if (leftScalar && rightScalar)
      throw new Error("Invalid type combination");

    var body = [];
    body.push("var result = Object.create( this.prototype );");

    for (var i = 0; i < dataMembers.length; i++) {
      var dataMember = dataMembers[i];
      var line = "result." + dataMember + " = ";

      if (leftScalar)
        line += "lhs ";
      else
        line += "lhs." + dataMember + " ";

      line += operator;

      if (rightScalar)
        line += " rhs;";
      else
        line += " rhs." + dataMember + ";";

      body.push(line);
    }

    body.push("return result;")

    var fn = vectorUtil.makeOperatorCore(name, tResult, body, 2, leftScalar, rightScalar);
    fn = vectorUtil.bindToPrototype(fn, tResult);

    $.Method({Static: true , Public: true }, name, 
      new JSIL.MethodSignature(tResult, [tLeft, tRight], []),
      fn
    );

    $.Method({Static: true , Public: true }, staticMethodName, 
      new JSIL.MethodSignature(tResult, [tLeft, tRight], []),
      fn
    );

    var makeRef = function (t) {
      return $jsilcore.TypeRef("JSIL.Reference", [t]);
    };

    $.Method({Static: true , Public: true }, staticMethodName,
      new JSIL.MethodSignature(null, [makeRef(tLeft), makeRef(tRight), makeRef(tResult)], []),
      function (lhs, rhs, result) {
        result.value = fn(lhs.value, rhs.value);
      }
    );
  },

  makeLogicOperator: function ($, name, operator, bindingOperator, dataMembers, tVector) {
    var body = [];
    body.push("return (");

    for (var i = 0; i < dataMembers.length; i++) {
      var dataMember = dataMembers[i];
      var line = "  (lhs." + dataMember + " ";

      line += operator;

      line += " rhs." + dataMember + ") ";

      if (i < dataMembers.length - 1)
        line += bindingOperator;

      body.push(line);
    }

    body.push(");")

    var fn = vectorUtil.makeOperatorCore(name, tVector, body, 2, false, false);

    $.Method({Static: true , Public: true }, name, 
      new JSIL.MethodSignature($.Boolean, [tVector, tVector], []),
      fn
    );
  },

  makeNegationOperator: function ($, dataMembers, tVector) {
    var body = [];
    body.push("var result = Object.create( this.prototype );");

    for (var i = 0; i < dataMembers.length; i++) {
      var dataMember = dataMembers[i];
      var line = "result." + dataMember + " = -value." + dataMember + ";";
      body.push(line);
    }

    body.push("return result;");

    var fn = vectorUtil.makeOperatorCore("op_UnaryNegation", tVector, body, 1, false, false);
    fn = vectorUtil.bindToPrototype(fn, tVector);

    $.Method({Static: true , Public: true }, "op_UnaryNegation", 
      new JSIL.MethodSignature(tVector, [tVector], []),
      fn
    );
  },

  makeLengthGetter: function ($, name, isSquared, dataMembers, tVector) {
    var body = [];
    body.push("return " + (isSquared ? "(" : "Math.sqrt("));

    for (var i = 0; i < dataMembers.length; i++) {
      var dataMember = dataMembers[i];
      var line = "  (this." + dataMember + " * this." + dataMember + ")";
      if (i < dataMembers.length - 1)
        line += " + ";
      body.push(line);
    }

    body.push(");");

    var fn = vectorUtil.makeOperatorCore(name, tVector, body, 0);

    $.Method({Static: false, Public: true }, name, 
      new JSIL.MethodSignature($.Single, [], []),
      fn
    );
  },

  makeNormalizer: function ($, dataMembers, tVector) {
    var body = [];
    body.push("var factor = 1.0 / this.Length();");

    for (var i = 0; i < dataMembers.length; i++) {
      var dataMember = dataMembers[i];
      var line = "this." + dataMember + " *= factor;";
      body.push(line);
    }

    var fn = vectorUtil.makeOperatorCore("Normalize", tVector, body, 0);

    $.Method({Static: false, Public: true }, "Normalize", 
      new JSIL.MethodSignature(null, [], []),
      fn
    );

    $.Method({Static: true , Public: true }, "Normalize",
      new JSIL.MethodSignature(tVector, [tVector], []),
      function (v) {
        fn.call(v);
        return v;
      }
    );
  },

  makeLengthMethods: function ($, dataMembers, tVector) {
    vectorUtil.makeLengthGetter($, "LengthSquared", true, dataMembers, tVector);
    vectorUtil.makeLengthGetter($, "Length", false, dataMembers, tVector);

    vectorUtil.makeNormalizer($, dataMembers, tVector);
  },

  makeOperators: function ($, dataMembers, tVector) {
    var operators = [
      ["op_Addition", "+", false, "Add"],
      ["op_Subtraction", "-", false, "Subtract"],
      ["op_Division", "/", true, "Divide"],
      ["op_Multiply", "*", true, "Multiply"]
    ];

    for (var i = 0; i < operators.length; i++) {
      var name = operators[i][0];
      var operator = operators[i][1];
      var withScalar = operators[i][2];
      var staticMethodName = operators[i][3];

      vectorUtil.makeArithmeticOperator($, name, staticMethodName, operator, dataMembers, tVector, tVector, tVector);

      if (withScalar) {
        vectorUtil.makeArithmeticOperator($, name, staticMethodName, operator, dataMembers, tVector, $.Single, tVector);
        vectorUtil.makeArithmeticOperator($, name, staticMethodName, operator, dataMembers, $.Single, tVector, tVector);
      }
    }

    vectorUtil.makeNegationOperator($, dataMembers, tVector);

    vectorUtil.makeLogicOperator($, "op_Equality", "===", "&&", dataMembers, tVector);
    vectorUtil.makeLogicOperator($, "op_Inequality", "!==", "||", dataMembers, tVector);

    vectorUtil.makeLengthMethods($, dataMembers, tVector);
  },

  makeConstants: function ($, tVector, constants) {
    var makeGetter = function (values) {
      var state = [null];

      return function () {
        if (state[0] === null)
          state[0] = JSIL.CreateInstanceOfType(
            tVector.get().__Type__, values
          );

        return state[0];
      }
    };

    for (var k in constants) {
      var values = constants[k];
      var getter = makeGetter(values);

      $.Method({Static: true , Public: true}, "get_" + k,
        new JSIL.MethodSignature(tVector, [], []),
        getter
      );
    }
  }
};

JSIL.ImplementExternals("Microsoft.Xna.Framework.Vector2", function ($) {
  vectorUtil.makeConstants(
    $, $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), {
      "UnitX": [1, 0],
      "UnitY": [0, 1],
      "Zero": [0, 0],
      "One": [1, 1]
    }
  );

  vectorUtil.makeOperators(
    $, ["X", "Y"], $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2")
  );

  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single, $.Single], []), function (x, y) {
    this.X = x;
    this.Y = y;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single], []), function (value) {
    this.X = this.Y = value;
  });
  $.Method({
    Static: false,
    Public: true
  }, "MemberwiseClone", new JSIL.MethodSignature($.Object, [], []), function () {
    var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
    result.X = this.X;
    result.Y = this.Y;
    return result;
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Vector3", function ($) {
  vectorUtil.makeConstants(
    $, $asms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), {
      "Backward": [0, 0, 1],
      "Forward": [0, 0, -1],
      "Left": [-1, 0, 0],
      "Right": [1, 0, 0],
      "Up": [0, 1, 0],
      "Down": [0, -1, 0],
      "UnitX": [1, 0, 0],
      "UnitY": [0, 1, 0],
      "UnitZ": [0, 0, 1],
      "Zero": [0, 0, 0],
      "One": [1, 1, 1]
    }
  );

  vectorUtil.makeOperators(
    $, ["X", "Y", "Z"], $asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")
  );

  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single, $.Single, $.Single], []), function (x, y, z) {
    this.X = x;
    this.Y = y;
    this.Z = z;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single], []), function (value) {
    this.X = this.Y = this.Z = value;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (xy, z) {
    this.X = xy.X;
    this.Y = xy.Y;
    this.Z = z;
  });
  $.Method({
    Static: false,
    Public: true
  }, "MemberwiseClone", new JSIL.MethodSignature($.Object, [], []), function () {
    var result = Object.create(Microsoft.Xna.Framework.Vector3.prototype);
    result.X = this.X;
    result.Y = this.Y;
    result.Z = this.Z;
    return result;
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Vector4", function ($) {
  $.Method({
    Static: true,
    Public: true
  }, ".cctor", new JSIL.MethodSignature(null, [], []), function () {
    Microsoft.Xna.Framework.Vector4._zero = new Microsoft.Xna.Framework.Vector4();
  });
  $.Method({
    Static: false,
    Public: true
  }, "get_Zero", new JSIL.MethodSignature(null, [], []), function () {
    return Microsoft.Xna.Framework.Vector4._zero;
  });

  vectorUtil.makeOperators(
    $, ["X", "Y", "Z", "W"], $asms[0].TypeRef("Microsoft.Xna.Framework.Vector4")
  );

  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single, $.Single, $.Single, $.Single], []), function (x, y, z, w) {
    this.X = x;
    this.Y = y;
    this.Z = z;
    this.W = w;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (xy, z, w) {
    this.X = xy.X;
    this.Y = xy.Y;
    this.Z = z;
    this.W = w;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function (xyz, w) {
    this.X = xyz.X;
    this.Y = xyz.Y;
    this.Z = xyz.Z;
    this.W = w;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single], []), function (value) {
    this.X = this.Y = this.Z = this.W = value;
  });
  $.Method({
    Static: false,
    Public: true
  }, "MemberwiseClone", new JSIL.MethodSignature($.Object, [], []), function () {
    var result = Object.create(Microsoft.Xna.Framework.Vector4.prototype);
    result.X = this.X;
    result.Y = this.Y;
    result.Z = this.Z;
    return result;
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Matrix", function ($) {
  var matrix = $asms[0].TypeRef("Microsoft.Xna.Framework.Matrix");

  $.Method({
    Static: true,
    Public: true
  }, ".cctor", new JSIL.MethodSignature(null, [], []), function () {
    // FIXME
    Microsoft.Xna.Framework.Matrix._identity = new Microsoft.Xna.Framework.Matrix();
  });

  $.RawMethod(false, "__CopyMembers__", function (target) {
    target.xScale = this.xScale;
    target.yScale = this.yScale;
    target.zScale = this.zScale;
    target.xTranslation = this.xTranslation;
    target.yTranslation = this.yTranslation;
    target.zTranslation = this.zTranslation;
  });

  $.Method({Static:true , Public:true }, "get_Identity", 
    (new JSIL.MethodSignature(matrix, [], [])), 
    function get_Identity () {
      return Microsoft.Xna.Framework.Matrix._identity;
    }
  );

  $.Method({Static:true , Public:true }, "CreateLookAt", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [
          $asms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), $asms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")
        ], [])), 
    function CreateLookAt (cameraPosition, cameraTarget, cameraUpVector) {
      // FIXME
      return new Microsoft.Xna.Framework.Matrix();
    }
  );

  $.Method({Static:true , Public:true }, "CreateLookAt", 
    (new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")]), $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")]), 
          $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")]), $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Matrix")])
        ], [])), 
    function CreateLookAt (/* ref */ cameraPosition, /* ref */ cameraTarget, /* ref */ cameraUpVector, /* ref */ result) {
      // FIXME
      result.value = new Microsoft.Xna.Framework.Matrix();
    }
  );

  $.Method({Static:true , Public:true }, "CreateOrthographic", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [
          $.Single, $.Single, 
          $.Single, $.Single
        ], [])), 
    function CreateOrthographic (width, height, zNearPlane, zFarPlane) {
      // FIXME
      return new Microsoft.Xna.Framework.Matrix();
    }
  );

  $.Method({Static:true , Public:true }, "CreateOrthographic", 
    (new JSIL.MethodSignature(null, [
          $.Single, $.Single, 
          $.Single, $.Single, 
          $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Matrix")])
        ], [])), 
    function CreateOrthographic (width, height, zNearPlane, zFarPlane, /* ref */ result) {
      // FIXME
      result.value = new Microsoft.Xna.Framework.Matrix();
    }
  );

  $.Method({Static:true , Public:true }, "CreateOrthographicOffCenter", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [
          $.Single, $.Single, 
          $.Single, $.Single, 
          $.Single, $.Single
        ], [])), 
    function CreateOrthographicOffCenter (left, right, bottom, top, zNearPlane, zFarPlane) {
      // FIXME
      return new Microsoft.Xna.Framework.Matrix();
    }
  );

  $.Method({Static:true , Public:true }, "CreateOrthographicOffCenter", 
    (new JSIL.MethodSignature(null, [
          $.Single, $.Single, 
          $.Single, $.Single, 
          $.Single, $.Single, 
          $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Matrix")])
        ], [])), 
    function CreateOrthographicOffCenter (left, right, bottom, top, zNearPlane, zFarPlane, /* ref */ result) {
      // FIXME
      result.value = new Microsoft.Xna.Framework.Matrix();
    }
  );

  $.Method({Static:true , Public:true }, "CreateScale", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [
          $.Single, $.Single, 
          $.Single
        ], [])), 
    function CreateScale (xScale, yScale, zScale) {
      // FIXME
      var result = Object.create(Microsoft.Xna.Framework.Matrix.prototype);
      result.xScale = xScale;
      result.yScale = yScale;
      result.zScale = zScale;
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "CreateTranslation", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [
          $.Single, $.Single, 
          $.Single
        ], [])), 
    function CreateTranslation (xPosition, yPosition, zPosition) {
      // FIXME
      var result = Object.create(Microsoft.Xna.Framework.Matrix.prototype);
      result.xTranslation = xPosition;
      result.yTranslation = yPosition;
      result.zTranslation = zPosition;
      return result;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.BasicEffect", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice")], [])), 
    function _ctor (device) {
    }
  );

  $.Method({Static:false, Public:true }, "set_Alpha", 
    (new JSIL.MethodSignature(null, [$.Single], [])), 
    function set_Alpha (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_AmbientLightColor", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function set_AmbientLightColor (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_DiffuseColor", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function set_DiffuseColor (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_EmissiveColor", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function set_EmissiveColor (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_FogColor", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function set_FogColor (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_FogEnabled", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_FogEnabled (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_FogEnd", 
    (new JSIL.MethodSignature(null, [$.Single], [])), 
    function set_FogEnd (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_FogStart", 
    (new JSIL.MethodSignature(null, [$.Single], [])), 
    function set_FogStart (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_LightingEnabled", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_LightingEnabled (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_PreferPerPixelLighting", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_PreferPerPixelLighting (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_Projection", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Matrix")], [])), 
    function set_Projection (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_SpecularColor", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function set_SpecularColor (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_SpecularPower", 
    (new JSIL.MethodSignature(null, [$.Single], [])), 
    function set_SpecularPower (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_Texture", 
    (new JSIL.MethodSignature(null, [$asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.Texture2D")], [])), 
    function set_Texture (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_TextureEnabled", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_TextureEnabled (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_VertexColorEnabled", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_VertexColorEnabled (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_View", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Matrix")], [])), 
    function set_View (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_World", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Matrix")], [])), 
    function set_World (value) {
      // FIXME
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.Effect", function ($) {
  $.Method({Static:false, Public:true }, "get_CurrentTechnique", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.EffectTechnique"), [], [])), 
    function get_CurrentTechnique () {
      // FIXME
      return new Microsoft.Xna.Framework.Graphics.EffectTechnique();
    }
  );

  $.Method({Static:false, Public:true }, "get_Parameters", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.EffectParameterCollection"), [], [])), 
    function get_Parameters () {
      // FIXME
      return new Microsoft.Xna.Framework.Graphics.EffectParameterCollection();
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.EffectParameterCollection", function ($) {
  $.Method({Static:false, Public:true }, "get_Count", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Count () {
      // FIXME
      return 0;
    }
  );

  $.Method({Static:false, Public:true }, "get_Item", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.EffectParameter"), [$.Int32], [])), 
    function get_Item (index) {
      // FIXME
      return new Microsoft.Xna.Framework.Graphics.EffectParameter();
    }
  );

  $.Method({Static:false, Public:true }, "get_Item", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.EffectParameter"), [$.String], [])), 
    function get_Item (name) {
      // FIXME
      return new Microsoft.Xna.Framework.Graphics.EffectParameter();
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.EffectParameter", function ($) {  
  $.Method({Static:false, Public:true }, "get_Elements", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.EffectParameterCollection"), [], [])), 
    function get_Elements () {
      // FIXME
      return new Microsoft.Xna.Framework.Graphics.EffectParameterCollection();
    }
  );

  $.Method({Static:false, Public:true }, "SetValue", 
    (new JSIL.MethodSignature(null, [$.Single], [])), 
    function SetValue (value) {
      // FIXME
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.EffectTechnique", function ($) {
  $.Method({Static:false, Public:true }, "get_Passes", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.EffectPassCollection"), [], [])), 
    function get_Passes () {
      // FIXME
      return [new Microsoft.Xna.Framework.Graphics.EffectPass()];
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.EffectPass", function ($) {
  $.Method({Static:false, Public:true }, "Apply", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Apply () {
      // FIXME
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GameServiceContainer", function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function () {});
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GameComponentCollection", function ($) {
  $.RawMethod(false, "$internalCtor", function (game) {
    this._game = game;

    this._ctor();
  });

  $.RawMethod(false, "$OnItemAdded", function (item) {
    if (this._game.initialized) {
      if (typeof (item.Initialize) === "function")
        item.Initialize();
    }
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Game", function ($) {
  $.Method({Static: false, Public: true}, "ForceQuit", 
    new JSIL.MethodSignature(null, [], []), 
    function () {
      Microsoft.Xna.Framework.Game._QuitForced = true;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      var tContentManager = JSIL.GetTypeFromAssembly(
        $asms.xna, "Microsoft.Xna.Framework.Content.ContentManager", [], true
      );

      var tGameTime = JSIL.GetTypeFromAssembly(
        $asms.xnaGame, "Microsoft.Xna.Framework.GameTime", [], true
      );

      this.gameServices = new Microsoft.Xna.Framework.GameServiceContainer();
      this.content = JSIL.CreateInstanceOfType(tContentManager, [this.gameServices]);
      this.components = JSIL.CreateInstanceOfType(
        Microsoft.Xna.Framework.GameComponentCollection.__Type__, "$internalCtor", [this]
      );
      this.targetElapsedTime = System.TimeSpan.FromTicks(166667);
      this.isFixedTimeStep = true;
      this.forceElapsedTimeToZero = true;
      this._isDead = false;
      this.initialized = false;

      if (typeof (Date.now) === "function") {
        Object.defineProperty(this, "_GetNow", {
          configurable: true,
          enumerable: true,
          value: Date.now
        });
      }

      this._runHandle = null;
      this._gameTime = JSIL.CreateInstanceOfType(tGameTime, null);
      this._lastFrame = this._nextFrame = this._started = 0;
    }
  );

  $.Method({Static:false, Public:true }, "get_Components", 
    (new JSIL.MethodSignature($asms[1].TypeRef("Microsoft.Xna.Framework.GameComponentCollection"), [], [])), 
    function get_Components () {
      return this.components;
    }
  );

  $.Method({Static:false, Public:true }, "get_Content", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentManager"), [], [])), 
    function get_Content () {
      return this.content;
    }
  );

  $.Method({Static:false, Public:true }, "get_GraphicsDevice", 
    (new JSIL.MethodSignature($jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), [], [])), 
    function get_GraphicsDevice () {
      return this.graphicsDeviceService.GraphicsDevice;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsActive", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsActive () {
      return true && !Microsoft.Xna.Framework.Game._QuitForced && !this._isDead;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsFixedTimeStep", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsFixedTimeStep () {
      return this.isFixedTimeStep;
    }
  );

  $.Method({Static:false, Public:true }, "get_TargetElapsedTime", 
    (new JSIL.MethodSignature($asms[5].TypeRef("System.TimeSpan"), [], [])), 
    function get_TargetElapsedTime () {
      return this.targetElapsedTime;
    }
  );

  $.Method({Static:false, Public:true }, "get_Services", 
    (new JSIL.MethodSignature($asms[1].TypeRef("Microsoft.Xna.Framework.GameServiceContainer"), [], [])), 
    function get_Services () {
      return this.gameServices;
    }
  );

  $.Method({Static:false, Public:true }, "get_Window", 
    (new JSIL.MethodSignature($asms[1].TypeRef("Microsoft.Xna.Framework.GameWindow"), [], [])), 
    function get_Window () {
      // FIXME
      return {};
    }
  );

  $.Method({Static:false, Public:true }, "set_IsFixedTimeStep", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_IsFixedTimeStep (value) {
      this.isFixedTimeStep = value;
    }
  );

  $.Method({Static:false, Public:true }, "set_TargetElapsedTime", 
    (new JSIL.MethodSignature(null, [$asms[5].TypeRef("System.TimeSpan")], [])), 
    function set_TargetElapsedTime (value) {
      this.targetElapsedTime = value;
    }
  );

  $.Method({
    Static: false,
    Public: true
  }, "Initialize", new JSIL.MethodSignature(null, [], []), function () {    
    this.initialized = true;

    for (var i = 0, l = this.components._size; i < l; i++) {
      var component = this.components._items[i];
      component.Initialize();
    }

    this.LoadContent();
  });
  $.Method({
    Static: false,
    Public: true
  }, "LoadContent", new JSIL.MethodSignature(null, [], []), function () {

  });
  $.Method({
    Static: false,
    Public: true
  }, "UnloadContent", new JSIL.MethodSignature(null, [], []), function () {

  });
  $.Method({
    Static: false,
    Public: true
  }, "ResetElapsedTime", new JSIL.MethodSignature(null, [], []), function () {
    this.forceElapsedTimeToZero = true;
  });
  $.RawMethod(false, "$ComponentsOfType", function (type) {
    var result = new Array();
    for (var i = 0, l = this.components._size; i < l; i++) {
      var item = this.components._items[i];

      if (JSIL.CheckType(item, type)) result.push(item);
    }
    return result;
  });

  $.Method({Static:false, Public:false}, "Draw", 
    (new JSIL.MethodSignature(null, [$asms[1].TypeRef("Microsoft.Xna.Framework.GameTime")], [])), 
    function Draw (gameTime) {
      if (Microsoft.Xna.Framework.Game._QuitForced || this._isDead) return;

      var drawableComponents = this.$ComponentsOfType(Microsoft.Xna.Framework.IDrawable);
      for (var i = 0, l = drawableComponents.length; i < l; i++) {
        var drawable = drawableComponents[i];

        if (drawable.Visible) drawable.Draw(gameTime);
      }
    }
  );

  $.Method({Static:false, Public:false}, "Update", 
    (new JSIL.MethodSignature(null, [$asms[1].TypeRef("Microsoft.Xna.Framework.GameTime")], [])), 
    function Update (gameTime) {
      if (Microsoft.Xna.Framework.Game._QuitForced || this._isDead) return;

      var updateableComponents = this.$ComponentsOfType(Microsoft.Xna.Framework.IUpdateable);
      for (var i = 0, l = updateableComponents.length; i < l; i++) {
        var updateable = updateableComponents[i];

        if (updateable.Enabled) updateable.Update(gameTime);
      }
    }
  );

  $.Method({
    Static: false,
    Public: true
  }, "Run", new JSIL.MethodSignature(null, [], []), function () {
    Microsoft.Xna.Framework.Game._QuitForced = false;
    this.Initialize();
    this._QueueStep();
  });

  $.RawMethod(false, "_GetNow", function () {
    return (new Date()).getTime();
  });

  $.RawMethod(false, "_DeferCall", function (callback, lng) {
    setTimeout(callback, 0);
  });

  $.RawMethod(false, "_QueueStep", function () {
    if (Microsoft.Xna.Framework.Game._QuitForced) return;

    var self = this;
    var stepCallback = self._Step.bind(self);

    if (typeof (mozRequestAnimationFrame) !== "undefined") {
      mozRequestAnimationFrame(stepCallback);
    } else if (typeof (webkitRequestAnimationFrame) !== "undefined") {
      webkitRequestAnimationFrame(stepCallback);
    } else if (false && (typeof (msRequestAnimationFrame) !== "undefined")) {
      // The version of msRequestAnimationFrame in the current IE Platform Preview has a bug that
      //  causes it to sometimes never invoke the callback. As a result, we can't currently rely on it.
      msRequestAnimationFrame(stepCallback, JSIL.Host.getCanvas());
    } else {
      var shouldStepCallback = function () {
          var now = self._GetNow();
          var delay = self._nextFrame - now;

          if (delay <= 0) stepCallback();
          else self._DeferCall(shouldStepCallback, delay >= 5);
        };

      // It's important that we use setTimeout at least once after every frame in order to let the browser pump messages
      this._DeferCall(shouldStepCallback, true);
    }
  });

  $.RawMethod(false, "_Step", function () {
    var now = this._GetNow();

    if (this._lastFrame === 0) {
      var elapsed = 0;
      var total = 0;
      this._started = now;
      this._lastSecond = now;
      this._updateCount = this._drawCount = 0;
      this._extraTime = 0;
      this.suppressFrameskip = false;
    } else {
      var elapsed = now - this._lastFrame;
      var total = now - this._started;
    }

    if ((now - this._lastSecond) > 1000) {
      this._lastSecond = now;
      
      if (typeof (JSIL.Host.reportFps) === "function") {
        JSIL.Host.reportFps(this._drawCount, this._updateCount);
      }

      this._updateCount = this._drawCount = 0;
    }

    if (this.forceElapsedTimeToZero) {
      this.forceElapsedTimeToZero = false;
      this._extraTime = 0;
      elapsed = 0;
    }

    var frameDelay = this.targetElapsedTime.get_TotalMilliseconds();

    this._lastFrame = now;
    this._nextFrame = now + frameDelay;

    var millisecondInTicks = 10000;
    var maxElapsedTimeMs = 500;
    var longFrame = frameDelay * 2;

    var failed = true;
    try {

      var doUpdate = function () {
        var updateStarted = this._GetNow();
        this.Update(this._gameTime);
        var updateEnded = this._GetNow();

        // Detect long updates and suppress frameskip.
        if ((updateEnded - updateStarted) > longFrame) {
          this.suppressFrameskip = true;
        }
      }.bind(this);

      if (this.isFixedTimeStep && !this.suppressFrameskip) {
        this._gameTime.elapsedGameTime._ticks = (frameDelay * millisecondInTicks);

        elapsed += this._extraTime;
        this._extraTime = 0;

        if (elapsed > maxElapsedTimeMs) 
          elapsed = maxElapsedTimeMs;

        var numFrames = Math.floor(elapsed / frameDelay);

        if (numFrames > 0) {
          this._extraTime = elapsed - (numFrames * frameDelay);
          for (var i = 0; i < numFrames; i++) {
            this._gameTime.totalGameTime._ticks += (frameDelay * millisecondInTicks);

            doUpdate();
            this._updateCount += 1;
          }
        } else {
          this._extraTime = elapsed;
        }
      } else {
        this._extraTime = 0;
        this.suppressFrameskip = false;

        this._gameTime.elapsedGameTime._ticks = (elapsed * millisecondInTicks);
        this._gameTime.totalGameTime._ticks += (elapsed * millisecondInTicks);

        doUpdate();
        this._updateCount += 1;
      }

      this.get_GraphicsDevice().$Clear();
      this.Draw(this._gameTime);
      this._drawCount += 1;
      failed = false;
    } finally {
      if (failed || Microsoft.Xna.Framework.Game._QuitForced) this.Exit();
      else this._QueueStep();
    }
  });

  $.Method({
    Static: false,
    Public: true
  }, "Exit", new JSIL.MethodSignature(null, [], []), function () {
    this.Dispose();
  });

  $.Method({
    Static: false,
    Public: true
  }, "Dispose", new JSIL.MethodSignature(null, [], []), function () {
    if (this._runHandle !== null) window.clearInterval(this._runHandle);

    this._runHandle = null;
    this.UnloadContent();

    this._isDead = true;
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GameComponent", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms[1].TypeRef("Microsoft.Xna.Framework.Game")], [])), 
    function _ctor (game) {
      this.enabled = true;
      this.initialized = false;
      this.game = game;
    }
  );

  $.Method({Static:false, Public:true }, "get_Enabled", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_Enabled () {
      return this.enabled;
    }
  );

  $.Method({Static:false, Public:true }, "get_Game", 
    (new JSIL.MethodSignature($asms[1].TypeRef("Microsoft.Xna.Framework.Game"), [], [])), 
    function get_Game () {
      return this.game;
    }
  );

  $.Method({Static:false, Public:true }, "set_Enabled", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_Enabled (value) {
      this.enabled = value;
    }
  );

  $.Method({Static:false, Public:true }, "Initialize", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Initialize () {
      if (this.initialized) return;

      this.initialized = true;
    }
  );

  $.Method({Static:false, Public:true }, "Update", 
    (new JSIL.MethodSignature(null, [$asms[1].TypeRef("Microsoft.Xna.Framework.GameTime")], [])), 
    function Update (gameTime) {
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.DrawableGameComponent", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms[1].TypeRef("Microsoft.Xna.Framework.Game")], [])), 
    function _ctor (game) {
      Microsoft.Xna.Framework.GameComponent.prototype._ctor.call(this, game);

      this.visible = true;
    }
  );  

  $.Method({Static:false, Public:true }, "Draw", 
    (new JSIL.MethodSignature(null, [$asms[1].TypeRef("Microsoft.Xna.Framework.GameTime")], [])), 
    function Draw (gameTime) {
    }
  );

  $.Method({Static:false, Public:true }, "get_GraphicsDevice", 
    (new JSIL.MethodSignature($jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), [], [])), 
    function get_GraphicsDevice () {
      return this.game.graphicsDeviceService.GraphicsDevice;
    }
  );

  $.Method({Static:false, Public:true }, "get_Visible", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_Visible () {
      return this.visible;
    }
  );

  $.Method({Static:false, Public:true }, "Initialize", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Initialize () {
      if (this.initialized) return;

      Microsoft.Xna.Framework.GameComponent.prototype.Initialize.call(this);

      this.LoadContent();
    }
  );

  $.Method({Static:false, Public:false}, "LoadContent", 
    (new JSIL.MethodSignature(null, [], [])), 
    function LoadContent () {
    }
  );

  $.Method({Static:false, Public:true }, "set_Visible", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_Visible (value) {
      this.visible = value;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.Keyboard", function ($) {
  var getStateImpl = function (playerIndex) {
    var keys = JSIL.Host.getHeldKeys();
    return new Microsoft.Xna.Framework.Input.KeyboardState(keys);
  };

  $.Method({Static:true , Public:true }, "GetState", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.KeyboardState"), [], [])), 
    getStateImpl
  );

  $.Method({Static:true , Public:true }, "GetState", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.KeyboardState"), [$asms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex")], [])), 
    getStateImpl
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.KeyboardState", function ($) {
  $.RawMethod(false, "__CopyMembers__", function (target) {
    if (this.keys)
      target.keys = Array.prototype.slice.call(this.keys);
    else
      target.keys = [];
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$asms[0].TypeRef("Microsoft.Xna.Framework.Input.Keys")])], [])), 
    function _ctor (keys) {
      this.keys = [];

      for (var i = 0; i < keys.length; i++)
        this.keys.push(Number(keys[i]));
    }
  );

  $.Method({Static:false, Public:true }, "IsKeyDown", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Input.Keys")], [])), 
    function IsKeyDown (key) {
      return this.keys.indexOf(key.value) !== -1;
    }
  );

  $.Method({Static:false, Public:true }, "IsKeyUp", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Input.Keys")], [])), 
    function IsKeyUp (key) {
      return this.keys.indexOf(key.value) === -1;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.Mouse", function ($) {
  var getStateImpl = function (playerIndex) {
    var buttons = JSIL.Host.getHeldButtons();
    var position = JSIL.Host.getMousePosition();

    var pressed = $asms[0].Microsoft.Xna.Framework.Input.ButtonState.Pressed;
    var released = $asms[0].Microsoft.Xna.Framework.Input.ButtonState.Released;

    // FIXME
    return new Microsoft.Xna.Framework.Input.MouseState(
      position[0], position[1], 0,
      (buttons.indexOf(0) >= 0) ? pressed : released,
      (buttons.indexOf(2) >= 0) ? pressed : released,
      (buttons.indexOf(1) >= 0) ? pressed : released,
      released, released
    );
  };

  $.Method({Static:true , Public:true }, "GetState", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.MouseState"), [], [])), 
    getStateImpl
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.MouseState", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $.Int32, $.Int32, 
          $.Int32, $asms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), $asms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), $asms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState")
        ], [])), 
    function _ctor (x, y, scrollWheel, leftButton, middleButton, rightButton, xButton1, xButton2) {
      // FIXME
      this.x = x;
      this.y = y;
      this.leftButton = leftButton;
      this.middleButton = middleButton;
      this.rightButton = rightButton;
    }
  );

  $.Method({Static:false, Public:true }, "get_LeftButton", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
    function get_LeftButton () {
      return this.leftButton;
    }
  );

  $.Method({Static:false, Public:true }, "get_MiddleButton", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
    function get_MiddleButton () {
      return this.middleButton;
    }
  );

  $.Method({Static:false, Public:true }, "get_RightButton", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
    function get_RightButton () {
      return this.rightButton;
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
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePad", function ($) {
  var getStateImpl = function (playerIndex) {
    var buttons = new Microsoft.Xna.Framework.Input.GamePadButtons();
    var thumbs = new Microsoft.Xna.Framework.Input.GamePadThumbSticks();
    var triggers = new Microsoft.Xna.Framework.Input.GamePadTriggers();
    var dpad = new Microsoft.Xna.Framework.Input.GamePadDPad();

    return new Microsoft.Xna.Framework.Input.GamePadState(thumbs, triggers, buttons, dpad);
  };

  $.Method({Static:true , Public:true }, "GetState", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadState"), [$asms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex")], [])), 
    getStateImpl
  );

  $.Method({Static:true , Public:true }, "GetState", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadState"), [$asms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex"), $asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadDeadZone")], [])), 
    getStateImpl
  );

  $.Method({Static:true , Public:true }, "GetCapabilities", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadCapabilities"), [$asms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex")], [])), 
    function GetCapabilities (playerIndex) {
      return new Microsoft.Xna.Framework.Input.GamePadCapabilities(null, null);
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePadCapabilities", function ($) {
  
  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Input.XINPUT_CAPABILITIES")]), $asms[0].TypeRef("Microsoft.Xna.Framework.ErrorCodes")], [])), 
    function _ctor (/* ref */ caps, result) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "get_IsConnected", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsConnected () {
      return false;
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePadState", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadThumbSticks"), $asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadTriggers"), 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadButtons"), $asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadDPad")
        ], [])), 
    function _ctor (thumbSticks, triggers, buttons, dPad) {
      this._thumbs = thumbSticks;
      this._buttons = buttons;
      this._triggers = triggers;
      this._dpad = dPad;
    }
  );

  $.Method({Static:false, Public:true }, "get_Buttons", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadButtons"), [], [])), 
    function get_Buttons () {
      return this._buttons;
    }
  );

  $.Method({Static:false, Public:true }, "get_DPad", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadDPad"), [], [])), 
    function get_DPad () {
      return this._dpad;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsConnected", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsConnected () {
      // FIXME
      return false;
    }
  );

  $.Method({Static:false, Public:true }, "get_ThumbSticks", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadThumbSticks"), [], [])), 
    function get_ThumbSticks () {
      return this._thumbs;
    }
  );

  $.Method({Static:false, Public:true }, "get_Triggers", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadTriggers"), [], [])), 
    function get_Triggers () {
      return this._triggers;
    }
  );

  $.Method({Static:false, Public:true }, "IsButtonDown", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Input.Buttons")], [])), 
    function IsButtonDown (button) {
      // FIXME
      return false;
    }
  );

  $.Method({Static:false, Public:true }, "IsButtonUp", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Input.Buttons")], [])), 
    function IsButtonUp (button) {
      // FIXME
      return false;
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePadButtons", function ($) {
  var buttonNames = [
    "A", "B", "Back", "BigButton", 
    "LeftShoulder", "LeftStick", "RightShoulder", "RightStick",
    "Start", "X", "Y"
  ];

  var pressed = $asms[0].Microsoft.Xna.Framework.Input.ButtonState.Pressed;
  var released = $asms[0].Microsoft.Xna.Framework.Input.ButtonState.Released;

  for (var i = 0; i < buttonNames.length; i++) {
    $.Method({Static:false, Public:true }, "get_" + buttonNames[i], 
      (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
      function () {
        return released;
      }
    );
  }

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePadThumbSticks", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2")], [])), 
    function _ctor (leftThumbstick, rightThumbstick) {
      this._left = leftThumbstick;
      this._right = rightThumbstick;
    }
  );

  $.Method({Static:false, Public:true }, "get_Left", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [], [])), 
    function get_Left () {
      return this._left;
    }
  );

  $.Method({Static:false, Public:true }, "get_Right", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [], [])), 
    function get_Right () {
      return this._right;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GraphicsDeviceManager", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms.xnaGame.TypeRef("Microsoft.Xna.Framework.Game")], [])), 
    function _ctor (game) {
      this.game = game;
      this.device = new Microsoft.Xna.Framework.Graphics.GraphicsDevice();
      game.graphicsDeviceService = this;
      game.graphicsDeviceManager = this;
    }
  );

  $.Method({
    Static: false,
    Public: true
  }, "get_GraphicsDevice", new JSIL.MethodSignature($jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), [], []), function () {
    return this.device;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_PreferredBackBufferWidth", new JSIL.MethodSignature(null, [$.Int32], []), function (value) {
    this._width = value;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_PreferredBackBufferHeight", new JSIL.MethodSignature(null, [$.Int32], []), function (value) {
    this._height = value;
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.Viewport", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({
    Static: false,
    Public: true
  }, "get_X", new JSIL.MethodSignature($.Int32, [], []), function () {
    return this._x;
  });

  $.Method({
    Static: false,
    Public: true
  }, "get_Y", new JSIL.MethodSignature($.Int32, [], []), function () {
    return this._y;
  });

  $.Method({
    Static: false,
    Public: true
  }, "get_Width", new JSIL.MethodSignature($.Int32, [], []), function () {
    return this._width;
  });

  $.Method({
    Static: false,
    Public: true
  }, "get_Height", new JSIL.MethodSignature($.Int32, [], []), function () {
    return this._height;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_X", new JSIL.MethodSignature(null, [$.Int32], []), function (value) {
    this._x = value;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_Y", new JSIL.MethodSignature(null, [$.Int32], []), function (value) {
    this._y = value;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_Width", new JSIL.MethodSignature(null, [$.Int32], []), function (value) {
    this._width = value;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_Height", new JSIL.MethodSignature(null, [$.Int32], []), function (value) {
    this._height = value;
  });

  $.Method({Static:false, Public:true }, "get_TitleSafeArea", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [], [])), 
    function get_TitleSafeArea () {
      return new Microsoft.Xna.Framework.Rectangle(this._x, this._y, this._width, this._height);
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GameTime", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this.totalGameTime = new System.TimeSpan();
      this.elapsedGameTime = new System.TimeSpan();
      this.isRunningSlowly = false;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $asms[5].TypeRef("System.TimeSpan"), $asms[5].TypeRef("System.TimeSpan"), 
          $.Boolean
        ], [])), 
    function _ctor (totalGameTime, elapsedGameTime, isRunningSlowly) {
      this.totalGameTime = totalGameTime;
      this.elapsedGameTime = elapsedGameTime;
      this.isRunningSlowly = isRunningSlowly;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$asms[5].TypeRef("System.TimeSpan"), $asms[5].TypeRef("System.TimeSpan")], [])), 
    function _ctor (totalGameTime, elapsedGameTime) {
      this.totalGameTime = totalGameTime;
      this.elapsedGameTime = elapsedGameTime;
      this.isRunningSlowly = false;
    }
  );

  $.Method({Static:false, Public:true }, "get_TotalGameTime", 
    (new JSIL.MethodSignature($asms[5].TypeRef("System.TimeSpan"), [], [])), 
    function get_TotalGameTime () {
      return this.totalGameTime;
    }
  );

  $.Method({Static:false, Public:true }, "get_ElapsedGameTime", 
    (new JSIL.MethodSignature($asms[5].TypeRef("System.TimeSpan"), [], [])), 
    function get_ElapsedGameTime () {
      return this.elapsedGameTime;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Rectangle", function ($) {
  $.Method({
    Static: true,
    Public: true
  }, ".cctor", new JSIL.MethodSignature(null, [], []), function () {
    Microsoft.Xna.Framework.Rectangle._empty = new Microsoft.Xna.Framework.Rectangle();
  });

  $.Method({Static: true, Public: true}, "get_Empty", 
    new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [], []), 
    function () {
      return Microsoft.Xna.Framework.Rectangle._empty;
    }
  );

  $.Method({Static:true , Public:true }, "op_Equality", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), $asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
    function op_Equality (lhs, rhs) {
      return lhs.X === rhs.X && lhs.Y === rhs.Y && lhs.Width === rhs.Width && lhs.Height === rhs.Height;
    }
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), $asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
    function op_Inequality (lhs, rhs) {
      return lhs.X !== rhs.X || lhs.Y !== rhs.Y || lhs.Width !== rhs.Width || lhs.Height !== rhs.Height;
    }
  );

  var intersectImpl = function (lhs, rhs) {
    var lhsX2 = lhs.X + lhs.Width;
    var rhsX2 = rhs.X + rhs.Width;
    var lhsY2 = lhs.Y + lhs.Height;
    var rhsY2 = rhs.Y + rhs.Height;

    var x1 = (lhs.X > rhs.X) ? lhs.X : rhs.X;
    var y1 = (lhs.Y > rhs.Y) ? lhs.Y : rhs.Y;
    var x2 = (lhsX2 < rhsX2) ? lhsX2 : rhsX2;
    var y2 = (lhsY2 < rhsY2) ? lhsY2 : rhsY2;

    if (x2 > x1 && y2 > y1) return new Microsoft.Xna.Framework.Rectangle(x1, y1, x2 - x1, y2 - y1);

    return Microsoft.Xna.Framework.Rectangle._empty;
  };

  $.Method({Static:true , Public:true }, "Intersect", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), $asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
    intersectImpl
  );

  $.Method({Static:true , Public:true }, "Intersect", 
    (new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), 
          $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")])
        ], [])), 
    function Intersect (/* ref */ value1, /* ref */ value2, /* ref */ result) {
      result.value = intersectImpl(value1.value, value2.value);
    }
  );

  var unionImpl = function (lhs, rhs) {
    var lhsX2 = lhs.X + lhs.Width;
    var rhsX2 = rhs.X + rhs.Width;
    var lhsY2 = lhs.Y + lhs.Height;
    var rhsY2 = rhs.Y + rhs.Height;

    var x1 = (lhs.X < rhs.X) ? lhs.X : rhs.X;
    var y1 = (lhs.Y < rhs.Y) ? lhs.Y : rhs.Y;
    var x2 = (lhsX2 > rhsX2) ? lhsX2 : rhsX2;
    var y2 = (lhsY2 > rhsY2) ? lhsY2 : rhsY2;

    if (x2 > x1 && y2 > y1) 
      return new Microsoft.Xna.Framework.Rectangle(x1, y1, x2 - x1, y2 - y1);

    return Microsoft.Xna.Framework.Rectangle._empty;
  };

  $.Method({Static:true , Public:true }, "Union", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), $asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
    unionImpl
  );

  $.Method({Static:true , Public:true }, "Union", 
    (new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), 
          $jsilcore.TypeRef("JSIL.Reference", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")])
        ], [])), 
    function Union (/* ref */ value1, /* ref */ value2, /* ref */ result) {
      result.value = unionImpl(value1.value, value2.value);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $.Int32, $.Int32, 
          $.Int32, $.Int32
        ], [])), 
    function _ctor (x, y, width, height) {
      this.X = x;
      this.Y = y;
      this.Width = width;
      this.Height = height;
    }
  );

  $.Method({Static:false, Public:true }, "Contains", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    function Contains (value) {
      return this.X <= value.X && value.X < this.X + this.Width && this.Y <= value.Y && value.Y < this.Y + this.Height;
    }
  );

  $.Method({Static:false, Public:true }, "get_Bottom", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Bottom () {
      return this.Y + this.Height;
    }
  );

  $.Method({Static:false, Public:true }, "get_Center", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Point"), [], [])), 
    function get_Center () {
      return new Microsoft.Xna.Framework.Point(
        Math.floor(this.X + (this.Width / 2)), 
        Math.floor(this.Y + (this.Height / 2))
      );
    }
  );

  $.Method({Static:false, Public:true }, "get_Left", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Left () {
      return this.X;
    }
  );

  $.Method({Static:false, Public:true }, "get_Location", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Point"), [], [])), 
    function get_Location () {
      return new Microsoft.Xna.Framework.Point(this.X, this.Y);
    }
  );

  $.Method({Static:false, Public:true }, "get_Right", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Right () {
      return this.X + this.Width;
    }
  );

  $.Method({Static:false, Public:true }, "get_Top", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Top () {
      return this.Y;
    }
  );

  $.Method({Static:false, Public:true }, "Inflate", 
    (new JSIL.MethodSignature(null, [$.Int32, $.Int32], [])), 
    function Inflate (x, y) {
      this.X -= x;
      this.Y -= y;
      this.Width += (x * 2);
      this.Height += (y * 2);
    }
  );

  $.Method({Static:false, Public:true }, "Intersects", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
    function Intersects (value) {
      return value.X < this.X + this.Width && 
              this.X < value.X + value.Width && 
              value.Y < this.Y + this.Height && 
              this.Y < value.Y + value.Height;
    }
  );

  $.Method({Static:false, Public:true }, "Offset", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    function Offset (amount) {
      this.X += amount.X;
      this.Y += amount.Y;
    }
  );

  $.Method({Static:false, Public:true }, "Offset", 
    (new JSIL.MethodSignature(null, [$.Int32, $.Int32], [])), 
    function Offset (offsetX, offsetY) {
      this.X += offsetX;
      this.Y += offsetY;
    }
  );

  $.Method({Static:false, Public:true }, "set_Location", 
    (new JSIL.MethodSignature(null, [$asms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    function set_Location (value) {
      this.X = value.X;
      this.Y = value.Y;

      return value;
    }
  );

  $.Method({Static: false, Public: true }, "MemberwiseClone", 
    new JSIL.MethodSignature($.Object, [], []), 
    function () {
      var result = Object.create(Microsoft.Xna.Framework.Rectangle.prototype);
      result.X = this.X;
      result.Y = this.Y;
      result.Width = this.Width;
      result.Height = this.Height;
      return result;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Point", function ($) {
  $.Method({
    Static: true,
    Public: true
  }, ".cctor", new JSIL.MethodSignature(null, [], []), function () {
    Microsoft.Xna.Framework.Point._zero = new Microsoft.Xna.Framework.Point();
  });

  $.Method({Static:true , Public:true }, "get_Zero", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Point"), [], [])), 
    function get_Zero () {
      return Microsoft.Xna.Framework.Point._zero;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Int32, $.Int32], [])), 
    function _ctor (x, y) {
      this.X = x;
      this.Y = y;
    }
  );

  var equalsImpl = function (lhs, rhs) {
    return lhs.X === rhs.X && lhs.Y === rhs.Y;
  };

  $.Method({Static:true , Public:true }, "op_Equality", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Point"), $asms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    equalsImpl
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Point"), $asms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    function op_Inequality (a, b) {
      return !equalsImpl(a, b);
    }
  );

  $.Method({Static:false, Public:true }, "Equals", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    function Equals (other) {
      return equalsImpl(this, other);
    }
  );

  $.Method({
    Static: false,
    Public: true
  }, "Equals", new JSIL.MethodSignature(null, [], []), function (rhs) {
    return this.X === rhs.X && this.Y === rhs.Y;
  });
});

$jsilxna.makeColor = function (proto, r, g, b, a) {
  var result = Object.create(proto);
  result.r = r;
  result.g = g;
  result.b = b;
  if (typeof (a) === "number") result.a = a;
  else result.a = 255;
  return result;
};

$jsilxna.Color = function ($) {
  $.RawMethod(false, "__CopyMembers__", function (target) {
    target.a = this.a;
    target.r = this.r;
    target.g = this.g;
    target.b = this.b;
  });

  $.Method({
    Static: true,
    Public: false
  }, ".cctor", new JSIL.MethodSignature(null, [], []), function () {
    var self = this;
    var proto = this.prototype;
    var makeColor = $jsilxna.makeColor;
    var colors = $jsilxna.colors || [];

    var bindColor = function (c) {
        return function () {
          return c;
        };
      };

    var typeName1 = JSIL.ParseTypeName("Microsoft.Xna.Framework.Color,Microsoft.Xna.Framework");
    var typeName2 = JSIL.ParseTypeName("Microsoft.Xna.Framework.Graphics.Color,Microsoft.Xna.Framework");

    var context = JSIL.GetTypeInternal(typeName1, $jsilxna, false) || JSIL.GetTypeInternal(typeName2, $jsilxna, false);

    var publicInterface = context.__PublicInterface__;

    for (var i = 0, l = colors.length; i < l; i++) {
      var colorName = colors[i][0];
      var color = makeColor(proto, colors[i][1], colors[i][2], colors[i][3], colors[i][4]);

      Object.defineProperty(publicInterface, "get_" + colorName, {
        value: bindColor(color),
        enumerable: true,
        configurable: true
      });

      Object.defineProperty(publicInterface, colorName, {
        value: color,
        enumerable: true,
        configurable: true
      });
    }
  });

  var ctorRgba = function (_, r, g, b, a) {
      _.a = a;
      _.r = r;
      _.g = g;
      _.b = b;
    };

  var ctorRgbaFloat = function (_, r, g, b, a) {
      _.a = $jsilxna.ClampByte(a * 255);
      _.r = $jsilxna.ClampByte(r * 255);
      _.g = $jsilxna.ClampByte(g * 255);
      _.b = $jsilxna.ClampByte(b * 255);
    };

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $.Int32, $.Int32, 
          $.Int32
        ], [])), 
    function _ctor (r, g, b) {
      ctorRgba(this, r, g, b, 255);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $.Int32, $.Int32, 
          $.Int32, $.Int32
        ], [])), 
    function _ctor (r, g, b, a) {
      ctorRgba(this, r, g, b, a);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $.Single, $.Single, 
          $.Single
        ], [])), 
    function _ctor (r, g, b) {
      ctorRgbaFloat(this, r, g, b, 1);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $.Single, $.Single, 
          $.Single, $.Single
        ], [])), 
    function _ctor (r, g, b, a) {
      ctorRgbaFloat(this, r, g, b, a);
    }
  );

  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$asms.xna.TypeRef("Microsoft.Xna.Framework.Vector3")], []), function (v) {
    ctorRgbaFloat(this, v.X, x.Y, v.Z, 1.0);
  });

  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$asms.xna.TypeRef("Microsoft.Xna.Framework.Vector4")], []), function (v) {
    ctorRgbaFloat(this, v.X, v.Y, v.Z, v.W);
  });

  $.Method({
    Static: false,
    Public: true
  }, "get_A", new JSIL.MethodSignature($.Byte, [], []), function () {
    return this.a;
  });

  $.Method({
    Static: false,
    Public: true
  }, "get_B", new JSIL.MethodSignature($.Byte, [], []), function () {
    return this.b;
  });

  $.Method({
    Static: false,
    Public: true
  }, "get_G", new JSIL.MethodSignature($.Byte, [], []), function () {
    return this.g;
  });

  $.Method({
    Static: false,
    Public: true
  }, "get_R", new JSIL.MethodSignature($.Byte, [], []), function () {
    return this.r;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_A", new JSIL.MethodSignature(null, [$.Byte], []), function (value) {
    this.a = value;
    this._cachedCss = null;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_B", new JSIL.MethodSignature(null, [$.Byte], []), function (value) {
    this.b = value;
    this._cachedCss = null;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_G", new JSIL.MethodSignature(null, [$.Byte], []), function (value) {
    this.g = value;
    this._cachedCss = null;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_R", new JSIL.MethodSignature(null, [$.Byte], []), function (value) {
    this.r = value;
    this._cachedCss = null;
  });

  var equalsImpl = function (lhs, rhs) {
    return (lhs.r === rhs.r) && (lhs.g === rhs.g) &&
      (lhs.b === rhs.b) && (lhs.a === rhs.a);
  };

  $.Method({Static:true , Public:true }, "op_Equality", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Color"), $asms[0].TypeRef("Microsoft.Xna.Framework.Color")], [])), 
    function op_Equality (a, b) {
      return equalsImpl(a, b);
    }
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, [$asms[0].TypeRef("Microsoft.Xna.Framework.Color"), $asms[0].TypeRef("Microsoft.Xna.Framework.Color")], [])), 
    function op_Inequality (a, b) {
      return !equalsImpl(a, b);
    }
  );

  $.Method({
    Static: true,
    Public: true
  }, "op_Multiply", new JSIL.MethodSignature($.Type, [$.Type, $.Single], []), function (color, multiplier) {
    var result = Object.create(Object.getPrototypeOf(color));
    result.a = Math.floor(color.a * multiplier);
    result.r = Math.floor(color.r * multiplier);
    result.g = Math.floor(color.g * multiplier);
    result.b = Math.floor(color.b * multiplier);
    return result;
  });

  $.RawMethod(false, "toCss", function (alpha) {
    if ((typeof(this._cachedCss) === "string") && (this._cachedAlpha === alpha)) {
      return this._cachedCss;
    }

    var a = alpha || this.a;
    if (a < 255) {
      this._cachedAlpha = a;
      return this._cachedCss = "rgba(" + this.r + "," + this.g + "," + this.b + "," + a + ")";
    } else {
      this._cachedAlpha = a;
      return this._cachedCss = "rgb(" + this.r + "," + this.g + "," + this.b + ")";
    }
  });
};

$jsilxna.ClampByte = function (v) {
  if (v < 0) return 0;
  else if (v > 255) return 255;
  else return Math.floor(v);
}

var $drawDebugRects = false, $drawDebugBoxes = false;

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.SpriteBatch", function ($) {
  var $canvasDrawImage = function canvasDrawImage (image, sourceX, sourceY, sourceW, sourceH, positionX, positionY, destW, destH) {
    this.device.context.drawImage(
      image, sourceX, sourceY, sourceW, sourceH, positionX, positionY, destW, destH
    );
  }

  $.RawMethod(false, "$canvasDrawImage", $canvasDrawImage);

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice")], [])), 
    function _ctor (graphicsDevice) {
      this.device = graphicsDevice;
      this.defer = false;
      this.deferSorter = null;
      this.deferredDraws = [];
    }
  );

  $.Method({Static:false, Public:true }, "Begin", 
    (new JSIL.MethodSignature(null, [$asms[5].TypeRef("System.Array") /* AnyType[] */ ], [])), 
    function SpriteBatch_Begin (sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix) {
      this.device.context.save();
      this.deferSorter = null;

      var textureIndex = 0;
      var depthIndex = 16;

      if (sortMode === Microsoft.Xna.Framework.Graphics.SpriteSortMode.Immediate) {
        this.defer = false;
      } else if (sortMode === Microsoft.Xna.Framework.Graphics.SpriteSortMode.BackToFront) {
        this.defer = true;
        this.deferSorter = function (lhs, rhs) {
          return -JSIL.CompareValues(lhs.arguments[depthIndex], rhs.arguments[depthIndex]);
        };
      } else if (sortMode === Microsoft.Xna.Framework.Graphics.SpriteSortMode.FrontToBack) {
        this.defer = true;
        this.deferSorter = function (lhs, rhs) {
          return JSIL.CompareValues(lhs.arguments[depthIndex], rhs.arguments[depthIndex]);
        };
      } else if (sortMode === Microsoft.Xna.Framework.Graphics.SpriteSortMode.Texture) {
        this.defer = true;
        this.deferSorter = function (lhs, rhs) {
          return JSIL.CompareValues(lhs.arguments[textureIndex], rhs.arguments[textureIndex]);
        };
      } else if (sortMode === Microsoft.Xna.Framework.Graphics.SpriteSortMode.Deferred) {
        this.defer = true;
      }

      if ((typeof (transformMatrix) === "object") && (transformMatrix !== null)) {
        this.device.context.translate(transformMatrix.xTranslation, transformMatrix.yTranslation);
        this.device.context.scale(transformMatrix.xScale, transformMatrix.yScale);
      }
    }
  );

  $.Method({Static:false, Public:true }, "End", 
    (new JSIL.MethodSignature(null, [], [])), 
    function SpriteBatch_End () {
      if (this.defer) {
        this.defer = false;

        if (this.deferSorter !== null) this.deferredDraws.sort(this.deferSorter);

        for (var i = 0, l = this.deferredDraws.length; i < l; i++) {
          var draw = this.deferredDraws[i];
          draw.fn.apply(this, draw.arguments);
        }
      }

      this.deferredDraws = [];

      this.device.context.restore();
    }
  );

  $.Method({Static:false, Public:true }, "Draw", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $jsilxna.colorRef()
        ], [])), 
    function Draw (texture, position, color) {
      this.InternalDraw(
        texture, position.X, position.Y, texture.Width, texture.Height,
        0, 0, texture.Width, texture.Height, 
        color, 0, 
        0, 0, 
        1, 1, 
        null, 0
      );
    }
  );

  $.Method({Static:false, Public:true }, "Draw", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $asms[5].TypeRef("System.Nullable`1", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilxna.colorRef()
        ], [])), 
    function Draw (texture, position, sourceRectangle, color) {
      var sourceX, sourceY, sourceWidth, sourceHeight;
      if (sourceRectangle !== null) {
        sourceX = sourceRectangle.X;
        sourceY = sourceRectangle.Y;
        sourceWidth = sourceRectangle.Width;
        sourceHeight = sourceRectangle.Height;
      } else {
        sourceX = sourceY = 0;
        sourceWidth = texture.Width;
        sourceHeight = texture.Height;
      }

      this.InternalDraw(
        texture, position.X, position.Y, sourceWidth, sourceHeight,
        sourceX, sourceY, sourceWidth, sourceHeight,
        color, 0,
        0, 0, 
        1, 1,
        null, 0
      );
    }
  );

  $.Method({Static:false, Public:true }, "DrawScaleF", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $asms[5].TypeRef("System.Nullable`1", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilxna.colorRef(), 
          $.Single, $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $.Single, $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), 
          $.Single
        ], [])), 
    function DrawScaleF (texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth) {
      var sourceX, sourceY, sourceWidth, sourceHeight;
      if (sourceRectangle !== null) {
        sourceX = sourceRectangle.X;
        sourceY = sourceRectangle.Y;
        sourceWidth = sourceRectangle.Width;
        sourceHeight = sourceRectangle.Height;
      } else {
        sourceX = sourceY = 0;
        sourceWidth = texture.Width;
        sourceHeight = texture.Height;
      }

      this.InternalDraw(
        texture, position.X, position.Y, sourceWidth, sourceHeight,
        sourceX, sourceY, sourceWidth, sourceHeight, 
        color, rotation, 
        origin.X, origin.Y, 
        scale, scale, 
        effects, layerDepth
      );
    }
  );

  $.Method({Static:false, Public:true }, "Draw", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $asms[5].TypeRef("System.Nullable`1", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilxna.colorRef(), 
          $.Single, $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), 
          $.Single
        ], [])), 
    function Draw (texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth) {
      var sourceX, sourceY, sourceWidth, sourceHeight;
      if (sourceRectangle !== null) {
        sourceX = sourceRectangle.X;
        sourceY = sourceRectangle.Y;
        sourceWidth = sourceRectangle.Width;
        sourceHeight = sourceRectangle.Height;
      } else {
        sourceX = sourceY = 0;
        sourceWidth = texture.Width;
        sourceHeight = texture.Height;
      }

      this.InternalDraw(
        texture, position.X, position.Y, sourceWidth, sourceHeight,
        sourceX, sourceY, sourceWidth, sourceHeight, 
        color, rotation, 
        origin.X, origin.Y, 
        scale.X, scale.Y, 
        effects, layerDepth
      );
    }
  );

  $.Method({Static:false, Public:true }, "DrawRect", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), 
          $jsilxna.colorRef()
        ], [])), 
    function DrawRect (texture, destinationRectangle, color) {
      this.InternalDraw(
        texture, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, 
        0, 0, texture.Width, texture.Height,
        color, 0, 
        0, 0, 
        1, 1,
        null, 0
      );
    }
  );

  $.Method({Static:false, Public:true }, "DrawRect", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), 
          $asms[5].TypeRef("System.Nullable`1", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilxna.colorRef()
        ], [])), 
    function DrawRect (texture, destinationRectangle, sourceRectangle, color) {
      var sourceX, sourceY, sourceWidth, sourceHeight;
      if (sourceRectangle !== null) {
        sourceX = sourceRectangle.X;
        sourceY = sourceRectangle.Y;
        sourceWidth = sourceRectangle.Width;
        sourceHeight = sourceRectangle.Height;
      } else {
        sourceX = sourceY = 0;
        sourceWidth = texture.Width;
        sourceHeight = texture.Height;
      }

      this.InternalDraw(
        texture, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, 
        sourceX, sourceY, sourceWidth, sourceHeight, 
        color, 0, 
        0, 0, 
        1, 1, 
        null, 0
      );
    }
  );

  $.Method({Static:false, Public:true }, "DrawRect", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), 
          $asms[5].TypeRef("System.Nullable`1", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilxna.colorRef(), 
          $.Single, $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), $.Single
        ], [])), 
    function DrawRect (texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth) {
      var sourceX, sourceY, sourceWidth, sourceHeight;
      if (sourceRectangle !== null) {
        sourceX = sourceRectangle.X;
        sourceY = sourceRectangle.Y;
        sourceWidth = sourceRectangle.Width;
        sourceHeight = sourceRectangle.Height;
      } else {
        sourceX = sourceY = 0;
        sourceWidth = texture.Width;
        sourceHeight = texture.Height;
      }

      this.InternalDraw(
        texture, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, 
        sourceX, sourceY, sourceWidth, sourceHeight, 
        color, rotation, 
        origin.X, origin.Y, 
        1, 1, 
        effects, layerDepth
      );
    }
  );

  $.Method({Static:false, Public:true }, "DrawString", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont"), $.String, 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef()
        ], [])), 
    function DrawString (spriteFont, text, position, color) {
      this.InternalDrawString(
        spriteFont, text, 
        position.X, position.Y,
        color, 0,
        0, 0,
        1, 1, 
        null, 0
      );
    }
  );

  $.Method({Static:false, Public:true }, "DrawStringBuilder", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont"), $asms[5].TypeRef("System.Text.StringBuilder"), 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef()
        ], [])), 
    function DrawStringBuilder (spriteFont, text, position, color) {
      this.InternalDrawString(
        spriteFont, text.toString(), 
        position.X, position.Y,
        color, 0,
        0, 0,
        1, 1, 
        null, 0
      );
    }
  );

  $.Method({Static:false, Public:true }, "DrawStringScaleF", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont"), $.String, 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef(), 
          $.Single, $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $.Single, $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), 
          $.Single
        ], [])), 
    function DrawStringScaleF (spriteFont, text, position, color, rotation, origin, scale, effects, layerDepth) {
      this.InternalDrawString(
        spriteFont, text, 
        position.X, position.Y, 
        color, rotation,
        origin.X, origin.Y,
        scale, scale, 
        effects, layerDepth
      );
    }
  );

  $.Method({Static:false, Public:true }, "DrawStringBuilderScaleF", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont"), $asms[5].TypeRef("System.Text.StringBuilder"), 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef(), 
          $.Single, $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $.Single, $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), 
          $.Single
        ], [])), 
    function DrawStringBuilderScaleF (spriteFont, text, position, color, rotation, origin, scale, effects, layerDepth) {
      this.InternalDrawString(
        spriteFont, text.toString(), 
        position.X, position.Y, 
        color, rotation,
        origin.X, origin.Y,
        scale, scale, 
        effects, layerDepth
      );
    }
  );

  $.Method({Static:false, Public:true }, "DrawString", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont"), $.String, 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef(), 
          $.Single, $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), 
          $.Single
        ], [])), 
    function DrawString (spriteFont, text, position, color, rotation, origin, scale, effects, layerDepth) {
      this.InternalDrawString(
        spriteFont, text, 
        position.X, position.Y, 
        color, rotation,
        origin.X, origin.Y,
        scale.X, scale.Y, 
        effects, layerDepth
      );
    }
  );

  $.Method({Static:false, Public:true }, "DrawStringBuilder", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont"), $asms[5].TypeRef("System.Text.StringBuilder"), 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef(), 
          $.Single, $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), 
          $.Single
        ], [])), 
    function DrawStringBuilder (spriteFont, text, position, color, rotation, origin, scale, effects, layerDepth) {
      this.InternalDrawString(
        spriteFont, text.toString(), 
        position.X, position.Y, 
        color, rotation,
        origin.X, origin.Y,
        scale.X, scale.Y, 
        effects, layerDepth
      );
    }
  );

  $.RawMethod(false, "InternalDraw", 
    function SpriteBatch_InternalDraw (
      texture, positionX, positionY, width, height, 
      sourceX, sourceY, sourceW, sourceH, 
      color, rotation, 
      originX, originY, 
      scaleX, scaleY, 
      effects, depth
    ) {
      if (this.defer) {
        color = color.MemberwiseClone();

        this.deferredDraws.push({
          fn: this.InternalDraw,
          index: this.deferredDraws.length,
          arguments: [
            texture, positionX, positionY, width, height, 
            sourceX, sourceY, sourceW, sourceH, 
            color, rotation, 
            originX, originY, 
            scaleX, scaleY, 
            effects, depth
          ]
        });

        return;
      }

      var needRestore = false;
      var image = texture.image,
        originalImage = texture.image;

      effects = effects || Microsoft.Xna.Framework.Graphics.SpriteEffects.None;

      if ((effects & Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally) == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally) {
        if (!needRestore) 
          this.device.context.save();
        needRestore = true;

        this.device.context.scale(-1, 1);
        positionX = -positionX;
      }

      if ((effects & Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically) == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically) {
        if (!needRestore) 
          this.device.context.save();
        needRestore = true;

        this.device.context.scale(1, -1);
        positionY = -positionY;
      }

      positionX -= originX;
      positionY -= originY;

      if (sourceX < 0) {
        sourceW += sourceX;
        sourceX = 0;
      }
      if (sourceY < 0) {
        sourceH += sourceY;
        sourceY = 0;
      }
      if (sourceW > texture.Width - sourceX) 
        sourceW = texture.Width - sourceX;
      if (sourceH > texture.Height - sourceY) 
        sourceH = texture.Height - sourceY;

      var isSinglePixel = ((sourceX == 0) && (sourceY == 0) && (sourceW == 1) && (sourceH == 1));

      if (!isSinglePixel) {
        if ((color.r < 255) || (color.g < 255) || (color.b < 255)) {
          var newImage = $jsilxna.getImageMultiplied(image, color);

          if ((newImage === image) || (newImage === null)) {
            // Broken browser
          } else {
            image = newImage;
            sourceX += 1;
            sourceY += 1;
          }
        }
      }

      if (color.a < 255) {
        if (!needRestore)
          this.device.context.save();
        needRestore = true;

        this.device.context.globalAlpha = color.a / 255;
      }

      // Negative width/height cause an exception in Firefox
      if (width < 0) {
        if (!needRestore) 
          this.device.context.save();
        needRestore = true;

        this.device.context.scale(-1, 1);
        positionX = -positionX;
        width = -width;
      }
      if (height < 0) {
        if (!needRestore) 
          this.device.context.save();
        needRestore = true;

        this.device.context.scale(1, -1);
        positionY = -positionY;
        height = -height;
      }

      // 0x0 blits cause an exception in IE
      if (
        (width > 0) && (height > 0) && 
        (sourceW > 0) && (sourceH > 0)
      ) {
        if ($drawDebugRects) {
          if (!needRestore)
            this.device.context.save();
          needRestore = true;

          this.device.context.fillStyle = "rgba(255, 0, 0, 0.33)";
          this.device.context.fillRect(
            positionX, positionY, width, height
          );
        }

        if ($drawDebugBoxes) {
          if (!needRestore) 
            this.device.context.save();
          needRestore = true;

          this.device.context.strokeStyle = "rgba(255, 255, 0, 0.66)";
          this.device.context.strokeRect(
            positionX, positionY, width, height
          );
        }

        if (isSinglePixel) {
          if (!needRestore) 
            this.device.context.save();
          needRestore = true;

          var imageColor = $jsilxna.getImageTopLeftPixelMultiplied(originalImage, color);

          this.device.context.fillStyle = imageColor;
          this.device.context.fillRect(
            positionX, positionY, width, height
          );
        } else {
          this.$canvasDrawImage(
            image, sourceX, sourceY, sourceW, sourceH, positionX, positionY, width, height
          );
        }
      }

      if (needRestore) 
        this.device.context.restore();
    }
  );

  $.RawMethod(false, "InternalDrawString", 
  function SpriteBatch_InternalDrawString (
    font, text, 
    positionX, positionY, 
    color, rotation,
    originX, originY,
    scaleX, scaleY, 
    effects, depth
  ) {
    if (this.defer) {
      color = color.MemberwiseClone();

      var args = [
        font, text, 
        positionX, positionY, 
        color, rotation,
        originX, originY,
        scaleX, scaleY, 
        effects, depth
      ];

      // Hack so depth sorting works
      args[16] = depth;

      this.deferredDraws.push({
        fn: this.InternalDrawString,
        index: this.deferredDraws.length,
        arguments: args
      });

      return;
    }

    var asmGraphics = $asms.xnaGraphics || $asms.xna;
    var tSpriteFont = asmGraphics.Microsoft.Xna.Framework.Graphics.SpriteFont;

    if (Object.getPrototypeOf(font) === tSpriteFont.prototype) {
      return font.InternalDraw(
        text, this, 
        positionX, positionY, 
        color, rotation,
        originX, originY, 
        scaleX, scaleY, 
        effects, depth
      );
    }

    var needRestore = false;

    effects = effects || Microsoft.Xna.Framework.Graphics.SpriteEffects.None;

    if ((effects & Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally) == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally) {
      if (!needRestore) this.device.context.save();
        needRestore = true;

      this.device.context.scale(-1, 1);
      positionX = -positionX;
    }

    if ((effects & Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically) == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically) {
      if (!needRestore) this.device.context.save();
        needRestore = true;

      this.device.context.scale(1, -1);
      positionY = -positionY;
    }

    this.device.context.textBaseline = "top";
    this.device.context.textAlign = "start";

    var fontCss = font.toCss(scaleX || 1.0);
    this.device.context.font = fontCss;

    if (this.device.context.font != fontCss) {
      // We failed to set the font onto the context; this may mean that the font failed to load.
      var hasWarned = font.$warnedAboutSetFailure || false;
      if (!hasWarned) {
        font.$warnedAboutSetFailure = true;
        JSIL.Host.warning("Failed to set font '" + font + "' onto canvas context for rendering.");
      }
    }

    this.device.context.fillStyle = color.toCss();

    var lines = text.split("\n");
    for (var i = 0, l = lines.length; i < l; i++) {
      this.device.context.fillText(lines[i], positionX, positionY);
      positionY += font.LineSpacing;
    }

    if (needRestore) 
      this.device.context.restore();
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Storage.StorageContainer", function ($) {
  $.Method({
    Static: false,
    Public: true
  }, "get_TitleLocation", new JSIL.MethodSignature(null, [], []), function () {
    return JSIL.Host.getRootDirectory();
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.GraphicsDevice", function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function () {
    this.originalCanvas = this.canvas = JSIL.Host.getCanvas();
    this.renderTarget = null;
    this.originalContext = this.context = this.canvas.getContext("2d");
    this.viewport = new Microsoft.Xna.Framework.Graphics.Viewport();
    this.viewport.Width = this.canvas.clientWidth || this.canvas.width;
    this.viewport.Height = this.canvas.clientHeight || this.canvas.height;
    this.samplerStates = new Microsoft.Xna.Framework.Graphics.SamplerStateCollection();
    this.vertexSamplerStates = new Microsoft.Xna.Framework.Graphics.SamplerStateCollection();
    this.textures = new Microsoft.Xna.Framework.Graphics.TextureCollection();
    this.vertexTextures = new Microsoft.Xna.Framework.Graphics.TextureCollection();
    this.presentationParameters = JSIL.CreateInstanceOfType(
      Microsoft.Xna.Framework.Graphics.PresentationParameters.__Type__, 
      "$internalCtor", [this]
    );
    this.$UpdateViewport();
  });

  $.Method({Static:false, Public:true }, "get_Viewport", 
    (new JSIL.MethodSignature($jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Viewport"), [], [])), 
    function get_Viewport () {
      return this.viewport;
    }
  );

  $.Method({Static:false, Public:true }, "set_Viewport", 
    (new JSIL.MethodSignature(null, [$jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Viewport")], [])), 
    function set_Viewport (value) {
      this.viewport = newViewport.MemberwiseClone();

      this.$UpdateViewport();
    }
  );

  $.Method({Static:false, Public:true }, "get_PresentationParameters", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.PresentationParameters"), [], [])), 
    function get_PresentationParameters () {
      return this.presentationParameters;
    }
  );

  $.Method({Static:false, Public:true }, "get_SamplerStates", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.SamplerStateCollection"), [], [])), 
    function get_SamplerStates () {
      return this.samplerStates;
    }
  );

  $.Method({Static:false, Public:true }, "get_VertexSamplerStates", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.SamplerStateCollection"), [], [])), 
    function get_VertexSamplerStates () {
      return this.vertexSamplerStates;
    }
  );

  $.Method({Static:false, Public:true }, "get_Textures", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.TextureCollection"), [], [])), 
    function get_Textures () {
      return this.textures;
    }
  );

  $.Method({Static:false, Public:true }, "get_VertexTextures", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.TextureCollection"), [], [])), 
    function get_VertexTextures () {
      return this.vertexTextures;
    }
  );

  $.Method({Static:false, Public:true }, "set_BlendState", 
    (new JSIL.MethodSignature(null, [$asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.BlendState")], [])), 
    function set_BlendState (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_DepthStencilState", 
    (new JSIL.MethodSignature(null, [$asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.DepthStencilState")], [])), 
    function set_DepthStencilState (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_RasterizerState", 
    (new JSIL.MethodSignature(null, [$asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.RasterizerState")], [])), 
    function set_RasterizerState (value) {
      // FIXME
    }
  );

  $.RawMethod(false, "$UpdateViewport", function () {
    this.context.setTransform(1, 0, 0, 1, 0, 0);
    this.context.translate(this.viewport.X, this.viewport.Y);
    this.context.scale(this.viewport.Width / this.canvas.width, this.viewport.Height / this.canvas.height);
  });

  $.RawMethod(false, "$Clear", function () {
    this.context.setTransform(1, 0, 0, 1, 0, 0);
    this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
    this.$UpdateViewport();
  });

  $.RawMethod(false, "InternalClear", function (color) {
    this.context.fillStyle = color.toCss();
    this.context.fillRect(0, 0, this.viewport.Width, this.viewport.Height);
  });

  var warnedTypes = {};

  $.RawMethod(false, "InternalDrawUserPrimitives", function (T, primitiveType, vertices, vertexOffset, primitiveCount) {
    switch (primitiveType) {
    case Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList:
      for (var i = 0; i < primitiveCount; i++) {
        var j = i * 2;
        this.context.lineWidth = 1;
        this.context.strokeStyle = vertices[j].Color.toCss();
        this.context.beginPath();
        this.context.moveTo(vertices[j].Position.X + 0.5, vertices[j].Position.Y + 0.5);
        this.context.lineTo(vertices[j + 1].Position.X + 0.5, vertices[j + 1].Position.Y + 0.5);
        this.context.closePath();
        this.context.stroke();
      }

      break;
    default:
      var ptype = primitiveType.toString();
      if (warnedTypes[ptype])
        return;

      warnedTypes[ptype] = true;
      JSIL.Host.error(new Error("The primitive type " + ptype + " is not implemented."));
      return;
    }
  });

  $.Method({Static:false, Public:true }, "DrawUserPrimitives", 
    (new JSIL.MethodSignature(null, [
          $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.PrimitiveType"), $jsilcore.TypeRef("System.Array", ["!!0"]), 
          $.Int32, $.Int32
        ], ["T"])), 
    function DrawUserPrimitives$b1 (T, primitiveType, vertexData, vertexOffset, primitiveCount) {
      return this.InternalDrawUserPrimitives(T, primitiveType, vertexData, vertexOffset, primitiveCount);
    }
  );

  $.Method({Static:false, Public:true }, "DrawUserPrimitives", 
    (new JSIL.MethodSignature(null, [
          $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.PrimitiveType"), $jsilcore.TypeRef("System.Array", ["!!0"]), 
          $.Int32, $.Int32, 
          $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.VertexDeclaration")
        ], ["T"])), 
    function DrawUserPrimitives$b1 (T, primitiveType, vertexData, vertexOffset, primitiveCount, vertexDeclaration) {
      return this.InternalDrawUserPrimitives(T, primitiveType, vertexData, vertexOffset, primitiveCount);
    }
  );

  $.Method({Static:false, Public:true }, "SetRenderTarget", 
    (new JSIL.MethodSignature(null, [$asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.RenderTarget2D")], [])), 
    function SetRenderTarget (renderTarget) {
      if (this.renderTarget === renderTarget) 
        return;

      var oldRenderTarget = this.renderTarget;
      this.renderTarget = renderTarget;

      if (renderTarget !== null) {
        this.canvas = renderTarget.canvas;
        this.context = renderTarget.context;
      } else {
        this.canvas = this.originalCanvas;
        this.context = this.originalContext;
      }

      this.context.setTransform(1, 0, 0, 1, 0, 0);
      this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);

      this.viewport.X = 0;
      this.viewport.Y = 0;
      this.viewport.Width = this.canvas.width;
      this.viewport.Height = this.canvas.height;

      this.$UpdateViewport();

      if (oldRenderTarget !== null) 
        oldRenderTarget.$ResynthesizeImage();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.PresentationParameters", function ($) {
  $.RawMethod(false, "__CopyMembers__", function (target) {
    target._device = this._device;
  });

  $.RawMethod(false, "$internalCtor", function (graphicsDevice) {
    this._device = graphicsDevice;
  });

  $.Method({Static:false, Public:true }, "get_BackBufferFormat", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.SurfaceFormat"), [], [])), 
    function get_BackBufferFormat () {
      return Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color;
    }
  );

  $.Method({Static:false, Public:true }, "get_BackBufferHeight", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_BackBufferHeight () {
      return this._device.originalCanvas.height;
    }
  );

  $.Method({Static:false, Public:true }, "get_BackBufferWidth", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_BackBufferWidth () {
      return this._device.originalCanvas.width;
    }
  );

  $.Method({Static:false, Public:true }, "get_DepthStencilFormat", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.DepthFormat"), [], [])), 
    function get_DepthStencilFormat () {
      return Microsoft.Xna.Framework.Graphics.DepthFormat.None;
    }
  );

  $.Method({Static:false, Public:true }, "get_MultiSampleCount", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_MultiSampleCount () {
      return 0;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.SamplerStateCollection", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $.Int32, 
          $.Int32
        ], [])), 
    function _ctor (pParent, samplerOffset, maxSamplers) {
      // FIXME
      this.states = new Array(maxSamplers);

      for (var i = 0; i < maxSamplers; i++) {
        this.states = new Microsoft.Xna.Framework.Graphics.SamplerState();
      }
    }
  );

  $.Method({Static:false, Public:true }, "get_Item", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.SamplerState"), [$.Int32], [])), 
    function get_Item (index) {
      return this.states[index];
    }
  );

  $.Method({Static:false, Public:true }, "set_Item", 
    (new JSIL.MethodSignature(null, [$.Int32, $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.SamplerState")], [])), 
    function set_Item (index, value) {
      this.states[index] = value;
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.MathHelper", function ($) {
  $.Method({Static:true , Public:true }, "Clamp", 
    (new JSIL.MethodSignature($.Single, [
          $.Single, $.Single, 
          $.Single
        ], [])), 
    function Clamp (value, min, max) {
      if (max < min) max = min;

      if (value < min) 
        return min;
      else if (value > max) 
        return max;
      else 
        return value;
    }
  );

  $.Method({Static:true , Public:true }, "Max", 
    (new JSIL.MethodSignature($.Single, [$.Single, $.Single], [])), 
    Math.max
  );

  $.Method({Static:true , Public:true }, "Min", 
    (new JSIL.MethodSignature($.Single, [$.Single, $.Single], [])), 
    Math.min
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.SpriteFont", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $asms[5].TypeRef("System.Collections.Generic.List`1", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), 
          $asms[5].TypeRef("System.Collections.Generic.List`1", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $asms[5].TypeRef("System.Collections.Generic.List`1", [$.Char]), 
          $.Int32, $.Single, 
          $asms[5].TypeRef("System.Collections.Generic.List`1", [$asms[0].TypeRef("Microsoft.Xna.Framework.Vector3")]), $asms[5].TypeRef("System.Nullable`1", [$.Char])
        ], [])), 
    function _ctor (texture, glyphs, cropping, charMap, lineSpacing, spacing, kerning, defaultCharacter) {
      this.textureValue = texture;
      this.glyphData = glyphs;
      this.croppingData = cropping;
      this.characterMap = charMap;
      this.lineSpacing = lineSpacing;
      this.spacing = spacing;
      this.kerning = kerning;
      this.defaultCharacter = defaultCharacter;
      this.characters = this.characterMap.AsReadOnly();
      this.charToIndex = {};

      for (var i = 0; i < charMap.Count; i++) {
        var ch = charMap.get_Item(i);
        this.charToIndex[ch.charCodeAt(0)] = i;
      }
    }
  );

  $.Method({Static:false, Public:true }, "get_Characters", 
    (new JSIL.MethodSignature($asms[5].TypeRef("System.Collections.ObjectModel.ReadOnlyCollection`1", [$.Char]), [], [])), 
    function get_Characters () {
      return this.characters;
    }
  );

  $.Method({Static:false, Public:true }, "get_DefaultCharacter", 
    (new JSIL.MethodSignature($asms[5].TypeRef("System.Nullable`1", [$.Char]), [], [])), 
    function get_DefaultCharacter () {
      return this.defaultCharacter;
    }
  );

  $.Method({Static:false, Public:true }, "get_LineSpacing", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_LineSpacing () {
      return this.lineSpacing;
    }
  );

  $.Method({Static:false, Public:true }, "get_Spacing", 
    (new JSIL.MethodSignature($.Single, [], [])), 
    function get_Spacing () {
      return this.spacing;
    }
  );

  $.Method({Static:false, Public:false}, "GetIndexForCharacter", 
    (new JSIL.MethodSignature($.Int32, [$.Char], [])), 
    function GetIndexForCharacter (character) {
      var result = this.charToIndex[character.charCodeAt(0)];

      if ((typeof (result) === "undefined") && (this.defaultCharacter !== null)) result = this.charToIndex[this.defaultCharacter.charCodeAt(0)];

      if (typeof (result) === "undefined") result = -1;

      return result;
    }
  );

  $.Method({Static:false, Public:false}, "InternalMeasure", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [$jsilcore.TypeRef("JSIL.Reference", [$jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont/StringProxy")])], [])), 
    function InternalMeasure (/* ref */ text) {
      var tVector2 = Microsoft.Xna.Framework.Vector2;
      var result = new tVector2(0, 0);
      var lineWidth = 0;
      var lineCount = 1;

      for (var i = 0, l = text.length; i < l; i++) {
        var ch = text[i];
        switch (ch) {
        case "\r":
          continue;
        case "\n":
          result.X = Math.max(lineWidth, result.X);
          lineWidth = 0;

          if (i < (l - 1)) lineCount += 1;

          continue;
        }

        lineWidth += this.spacing;

        var charIndex = this.GetIndexForCharacter(ch);
        if (charIndex < 0) {
          continue;
        }

        var kerning = this.kerning.get_Item(charIndex);
        var beforeGlyph = kerning.X;
        var glyphWidth = kerning.Y;
        var afterGlyph = kerning.Z;

        lineWidth += beforeGlyph;
        lineWidth += glyphWidth;
        lineWidth += afterGlyph;
      }

      result.X = Math.max(lineWidth, result.X);
      result.Y = lineCount * this.lineSpacing;

      return result;
    }
  );

  $.Method({Static:false, Public:true }, "MeasureString", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [$.String], [])), 
    function MeasureString (text) {
      return this.InternalMeasure(text);
    }
  );

  $.Method({Static:false, Public:true }, "MeasureString", 
    (new JSIL.MethodSignature($asms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [$asms[5].TypeRef("System.Text.StringBuilder")], [])), 
    function MeasureString (text) {
      return this.InternalMeasure(stringBuilder.toString());
    }
  );

  $.RawMethod(false, "InternalDraw", 
    function (
      text, spriteBatch, textblockPositionX, textblockPositionY, 
      color, rotation, 
      originX, originY, 
      scaleX, scaleY, 
      spriteEffects, layerDepth
    ) {
      var tVector2 = Microsoft.Xna.Framework.Vector2;
      var positionX = textblockPositionX;
      var positionY = textblockPositionY;
      var drawX, drawY;

      for (var i = 0, l = text.length; i < l; i++) {
        var ch = text[i];
        switch (ch) {
        case "\r":
          continue;
        case "\n":
          positionX = textblockPositionX;
          positionY += (this.lineSpacing * scaleY);
          continue;
        }

        positionX += (this.spacing * scaleX);

        var charIndex = this.GetIndexForCharacter(ch);
        if (charIndex < 0) {
          continue;
        }

        var kerning = this.kerning.get_Item(charIndex);
        var beforeGlyph = kerning.X * scaleX;
        var glyphWidth = kerning.Y * scaleX;
        var afterGlyph = kerning.Z * scaleX;

        positionX += beforeGlyph;

        var glyphRect = this.glyphData.get_Item(charIndex);
        var cropRect = this.croppingData.get_Item(charIndex);

        drawX = positionX + (cropRect.X * scaleX);
        drawY = positionY + (cropRect.Y * scaleY);

        spriteBatch.InternalDraw(
          this.textureValue, drawX, drawY, glyphRect.Width * scaleX, glyphRect.Height * scaleY,
          glyphRect.X, glyphRect.Y, glyphRect.Width, glyphRect.Height,
          color, rotation, 
          originX, originY, 
          1, 1, 
          spriteEffects, layerDepth
        );

        positionX += glyphWidth;
        positionX += afterGlyph;
    }
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.Texture2D", function ($) {
  $.RawMethod(false, "$internalCtor", function (graphicsDevice, width, height, mipMap, format) {
    this._parent = graphicsDevice;
    this.width = width;
    this.height = height;
    this.mipMap = mipMap;
    this.format = format;
    this.isDisposed = false;

    if (typeof ($jsilxna.ImageFormats[format.name]) === "undefined") 
      throw new System.NotImplementedException("The pixel format '" + format.name + "' is not supported.");

    this.image = document.createElement("img");
    this.image.src = this.$getDataUrlForBytes(null, 0, 0, false);
    var textures = document.getElementById("textures");
    if (textures) 
      textures.appendChild(this.image);
  });

  $.RawMethod(false, "$fromUri", function (graphicsDevice, uri) {
    this._parent = graphicsDevice;
    this.mipMap = false;
    this.format = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color;
    this.isDisposed = false;

    this.image = document.createElement("img");
    var self = this;
    this.image.addEventListener("load", function () {
      self.width = self.image.naturalWidth;
      self.height = self.image.naturalHeight;
    }, true);
    this.image.src = uri;

    this.width = this.height = 0;

    var textures = document.getElementById("textures");
    if (textures) 
      textures.appendChild(this.image);
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $.Int32, 
          $.Int32
        ], [])), 
    function _ctor (graphicsDevice, width, height) {
      this.$internalCtor(graphicsDevice, width, height, false, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $.Int32, 
          $.Int32, $.Boolean, 
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SurfaceFormat")
        ], [])), 
    function _ctor (graphicsDevice, width, height, mipMap, format) {
      this.$internalCtor(graphicsDevice, width, height, mipMap, format);
    }
  );

  $.Method({Static:true , Public:true }, "FromStream", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.Texture2D"), [
          $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $asms[5].TypeRef("System.IO.Stream"), 
          $.Int32, $.Int32, 
          $.Boolean
        ], [])), 
    function FromStream (graphicsDevice, stream, width, height, zoom) {
      var uri = stream.$GetURI();

      return JSIL.CreateInstanceOfType(
        Microsoft.Xna.Framework.Graphics.Texture2D.__Type__, 
        "$fromUri", [graphicsDevice, uri]
      );
    }
  );

  $.Method({Static:true , Public:true }, "FromStream", 
    (new JSIL.MethodSignature($asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.Texture2D"), [$asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $asms[5].TypeRef("System.IO.Stream")], [])), 
    function FromStream (graphicsDevice, stream) {
      var uri = stream.$GetURI();

      return JSIL.CreateInstanceOfType(
        Microsoft.Xna.Framework.Graphics.Texture2D.__Type__, 
        "$fromUri", [graphicsDevice, uri]
      );
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

  $.RawMethod(false, "$setDataInternal", function (T, data, startIndex, elementCount) {
    var bytes = null;
    var swapRedAndBlue = false;

    switch (T.toString()) {
    case "System.Byte":
      bytes = data;
      break;
    case "Microsoft.Xna.Framework.Color":
    case "Microsoft.Xna.Framework.Graphics.Color":
      bytes = $jsilxna.UnpackColorsToColorBytes(data, startIndex, elementCount);
      startIndex = 0;
      elementCount = bytes.length;
      swapRedAndBlue = true;
      break;
    default:
      throw new System.Exception("Pixel format '" + T.toString() + "' not implemented");
    }

    var shouldUnpremultiply = true;
    this.image.src = this.$getDataUrlForBytes(bytes, startIndex, elementCount, shouldUnpremultiply, swapRedAndBlue);
  });

  $.Method({Static:false, Public:true }, "SetData", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", ["!!0"])], ["T"])), 
    function SetData$b1 (T, data) {
      this.$setDataInternal(T, data, 0, data.length);
    }
  );

  $.Method({Static:false, Public:true }, "SetData", 
    (new JSIL.MethodSignature(null, [
          $.Int32, $asms[5].TypeRef("System.Nullable`1", [$asms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), 
          $jsilcore.TypeRef("System.Array", ["!!0"]), $.Int32, 
          $.Int32
        ], ["T"])), 
    function SetData$b1 (T, level, rect, data, startIndex, elementCount) {
      if (level !== 0) return;

      if (rect !== null) throw new System.NotImplementedException();

      this.$setDataInternal(T, data, startIndex, elementCount);
    }
  );

  $.RawMethod(false, "$getDataUrlForBytes", function (bytes, startIndex, elementCount, unpremultiply, swapRedAndBlue) {
    var canvas = document.createElement("canvas");
    canvas.width = this.width;
    canvas.height = this.height;
    var ctx = canvas.getContext("2d");

    if (bytes !== null) {
      var decoder = $jsilxna.ImageFormats[this.format.name];
      if (decoder !== null) {
        bytes = decoder(this.width, this.height, bytes, startIndex, elementCount, swapRedAndBlue);
        startIndex = 0;
        elementCount = bytes.length;
      }

      var imageData = ctx.createImageData(this.width, this.height);

      // XNA texture colors are premultiplied, but canvas pixels aren't, so we need to try
      //  to reverse the premultiplication.
      if (unpremultiply) {
        var pixelCount = elementCount / 4;
        for (var i = 0; i < pixelCount; i++) {
          var p = i * 4;

          var a = bytes[p + 3];

          if (a <= 0) {
            continue;
          } else if (a > 254) {
            imageData.data[p] = bytes[p];
            imageData.data[p + 1] = bytes[p + 1];
            imageData.data[p + 2] = bytes[p + 2];
            imageData.data[p + 3] = a;
          } else {
            var m = 255 / a;

            imageData.data[p] = bytes[p] * m;
            imageData.data[p + 1] = bytes[p + 1] * m;
            imageData.data[p + 2] = bytes[p + 2] * m;
            imageData.data[p + 3] = a;
          }
        }
      } else {
        for (var i = 0; i < elementCount; i++)
        imageData.data[i] = bytes[startIndex + i];
      }

      ctx.putImageData(imageData, 0, 0);
    }

    return canvas.toDataURL();
  });

  $.Method({
    Static: false,
    Public: true
  }, "Dispose", new JSIL.MethodSignature(null, [], []), function () {
    var textures = document.getElementById("textures");
    if (textures) textures.removeChild(this.image);
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.RenderTarget2D", function ($) {
  $.RawMethod(false, "$internalCtor", function (graphicsDevice, width, height, mipMap, format) {
    this._parent = graphicsDevice;
    this.width = width;
    this.height = height;
    this.mipMap = mipMap;
    this.format = format;
    this.isDisposed = false;

    this.image = this.canvas = document.createElement("canvas");
    this.canvas.naturalWidth = this.canvas.width = width;
    this.canvas.naturalHeight = this.canvas.height = height;
    this.context = this.canvas.getContext("2d");

    var targets = document.getElementById("rendertargets");
    if (targets) targets.appendChild(this.canvas);
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $.Int32, 
          $.Int32, $.Boolean, 
          $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.SurfaceFormat"), $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.DepthFormat"), 
          $.Int32, $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.RenderTargetUsage")
        ], [])), 
    function _ctor (graphicsDevice, width, height, mipMap, colorFormat, preferredDepthFormat, preferredMultiSampleCount, usage) {
      this.$internalCtor(graphicsDevice, width, height, mipMap, colorFormat);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $.Int32, 
          $.Int32, $.Boolean, 
          $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.SurfaceFormat"), $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.DepthFormat")
        ], [])), 
    function _ctor (graphicsDevice, width, height, mipMap, colorFormat, preferredDepthFormat) {
      this.$internalCtor(graphicsDevice, width, height, mipMap, colorFormat);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $asms[3].TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $.Int32, 
          $.Int32
        ], [])), 
    function _ctor (graphicsDevice, width, height) {
      this.$internalCtor(graphicsDevice, width, height, false, Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color);
    }
  );

  $.Method({
    Static: false,
    Public: true
  }, "SetData", new JSIL.MethodSignature(null, [], ["T"]), function (T, data) {
    throw new System.NotImplementedException();
  }), $.Method({
    Static: false,
    Public: true
  }, "SetData", new JSIL.MethodSignature(null, [], []), function (T, level, rect, data, startIndex, elementCount) {
    throw new System.NotImplementedException();
  }), $.Method({
    Static: false,
    Public: true
  }, "$ResynthesizeImage", new JSIL.MethodSignature(null, [], []), function () {});
  $.Method({
    Static: false,
    Public: true
  }, "Dispose", new JSIL.MethodSignature(null, [], []), function () {
    var targets = document.getElementById("rendertargets");
    if (targets) targets.removeChild(this.canvas);

    this.canvas = null;
    this.context = null;
  });
});

// Based on Benjamin Dobell's DXTC decompressors.
// http://www.glassechidna.com.au
$jsilxna.Unpack565 = function (sourceBuffer, sourceOffset) {
  var color565 = (sourceBuffer[sourceOffset + 1] << 8) + (sourceBuffer[sourceOffset]);
  if (color565 === 0) return [0, 0, 0];
  else if (color565 === 65535) return [255, 255, 255];

  var result = [];

  var temp = (color565 >> 11) * 255 + 16;
  result[0] = Math.floor((temp / 32 + temp) / 32);
  temp = ((color565 & 0x07E0) >> 5) * 255 + 32;
  result[1] = Math.floor((temp / 64 + temp) / 64);
  temp = (color565 & 0x001F) * 255 + 16;
  result[2] = Math.floor((temp / 32 + temp) / 32);

  return result;
};

$jsilxna.DecompressBlockBC12 = function (source, sourceOffset, writePixel, alphaSource) {
  var color0 = $jsilxna.Unpack565(source, sourceOffset);
  var color1 = $jsilxna.Unpack565(source, sourceOffset + 2);

  var r0 = color0[0],
    g0 = color0[1],
    b0 = color0[2];
  var r1 = color1[0],
    g1 = color1[1],
    b1 = color1[2];

  var bc2Mode = typeof (alphaSource) === "function";
  var readPosition = sourceOffset + 4;
  var finalColor;

  for (var y = 0; y < 4; y++) {
    var currentByte = source[readPosition];

    for (var x = 0; x < 4; x++) {
      var positionCode = (currentByte >> (2 * x)) & 0x03;
      var alpha = 255;
      if (bc2Mode) alpha = alphaSource(x, y);

      if (bc2Mode || (color0 > color1)) {
        // BC2 block mode or a BC1 block where (color0 > color1)
        switch (positionCode) {
        case 0:
          finalColor = [r0, g0, b0, alpha];
          break;
        case 1:
          finalColor = [r1, g1, b1, alpha];
          break;
        case 2:
          finalColor = [(2 * r0 + r1) / 3, (2 * g0 + g1) / 3, (2 * b0 + b1) / 3, alpha];
          break;
        case 3:
          finalColor = [(r0 + 2 * r1) / 3, (g0 + 2 * g1) / 3, (b0 + 2 * b1) / 3, alpha];
          break;
        }
      } else {
        // BC1 block mode
        switch (positionCode) {
        case 0:
          finalColor = [r0, g0, b0, 255];
          break;
        case 1:
          finalColor = [r1, g1, b1, 255];
          break;
        case 2:
          finalColor = [(r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2, 255];
          break;
        case 3:
          finalColor = [0, 0, 0, 255];
          break;
        }
      }

      writePixel(x, y, finalColor);
    }

    readPosition += 1;
  }
}

$jsilxna.DecompressAlphaBlockBC2 = function (source, sourceOffset) {
  return function (x, y) {
    var offset = Math.floor(((y * 4) + x) / 2) + sourceOffset;
    var byte = source[offset];
    var bits;

    if ((x & 1) == 1) {
      bits = (byte >> 4) & 0x0F;
    } else {
      bits = byte & 0x0F;
    }

    return bits * 0x11;
  };
};

$jsilxna.DecompressAlphaBlockBC3 = function (source, sourceOffset) {
  var result = new Array(4 * 4);

  var alpha0 = source[sourceOffset];
  var alpha1 = source[sourceOffset + 1];

  var readPosition = sourceOffset + 2;
  var readPositionBits = 0;
  var finalAlpha;

  // I'm too lazy to get the math for this right using JS integer arithmetic.
  // It's slower, but it works.
  var bits = "";
  for (var i = 0; i < 6; i++) {
    var byte = source[readPosition + i].toString(2);
    while (byte.length < 8)
    byte = "0" + byte;

    bits = byte + bits;
  }

  for (var y = 0; y < 4; y++) {
    for (var x = 0; x < 4; x++) {
      var currentBits = bits.substr(readPositionBits, 3);
      var positionCode = parseInt(currentBits, 2);

      if (alpha0 > alpha1) {
        switch (positionCode) {
        case 0:
          finalAlpha = alpha0;
          break;
        case 1:
          finalAlpha = alpha1;
          break;
        case 2:
          finalAlpha = ((6 * alpha0) + (1 * alpha1)) / 7;
          break;
        case 3:
          finalAlpha = ((5 * alpha0) + (2 * alpha1)) / 7;
          break;
        case 4:
          finalAlpha = ((4 * alpha0) + (3 * alpha1)) / 7;
          break;
        case 5:
          finalAlpha = ((3 * alpha0) + (4 * alpha1)) / 7;
          break;
        case 6:
          finalAlpha = ((2 * alpha0) + (5 * alpha1)) / 7;
          break;
        case 7:
          finalAlpha = ((1 * alpha0) + (6 * alpha1)) / 7;
          break;
        }
      } else {
        switch (positionCode) {
        case 0:
          finalAlpha = alpha0;
          break;
        case 1:
          finalAlpha = alpha1;
          break;
        case 2:
          finalAlpha = ((4 * alpha0) + (1 * alpha1)) / 5
          break;
        case 3:
          finalAlpha = ((3 * alpha0) + (2 * alpha1)) / 5
          break;
        case 4:
          finalAlpha = ((2 * alpha0) + (3 * alpha1)) / 5
          break;
        case 5:
          finalAlpha = ((1 * alpha0) + (4 * alpha1)) / 5
          break;
        case 6:
          finalAlpha = 0;
          break;
        case 7:
          finalAlpha = 255;
          break;
        }
      }

      readPositionBits += 3;

      var _x = 3 - x;
      var _y = 3 - y;
      result[_x + (_y * 4)] = finalAlpha;
    }
  }

  return function (x, y) {
    return result[x + (y * 4)];
  };
};

$jsilxna.makePixelWriter = function (buffer, width, x, y) {
  return function (_x, _y, color) {
    var offset = (((_y + y) * width) + (_x + x)) * 4;

    buffer[offset] = color[0];
    buffer[offset + 1] = color[1];
    buffer[offset + 2] = color[2];
    buffer[offset + 3] = color[3];
  };
};

$jsilxna.DecodeDxt1 = function (width, height, bytes, offset, count) {
  var totalSizeBytes = width * height * 4;
  var result = new Array(totalSizeBytes);

  var blockCountX = Math.floor((width + 3) / 4);
  var blockCountY = Math.floor((height + 3) / 4);
  var blockWidth = (width < 4) ? width : 4;
  var blockHeight = (height < 4) ? height : 4;

  var sourceOffset = offset;

  for (var y = 0; y < blockCountY; y++) {
    for (var x = 0; x < blockCountX; x++) {
      // Decode color data
      $jsilxna.DecompressBlockBC12(
      bytes, sourceOffset, $jsilxna.makePixelWriter(result, width, x * blockWidth, y * blockHeight), null);

      sourceOffset += 8;
    }
  }

  return result;
}

$jsilxna.DecodeDxt3 = function (width, height, bytes, offset, count) {
  var totalSizeBytes = width * height * 4;
  var result = new Array(totalSizeBytes);

  var blockCountX = Math.floor((width + 3) / 4);
  var blockCountY = Math.floor((height + 3) / 4);
  var blockWidth = (width < 4) ? width : 4;
  var blockHeight = (height < 4) ? height : 4;

  var sourceOffset = offset;

  for (var y = 0; y < blockCountY; y++) {
    for (var x = 0; x < blockCountX; x++) {
      // Decode alpha data
      var alphaSource = $jsilxna.DecompressAlphaBlockBC2(
      bytes, sourceOffset);

      sourceOffset += 8;

      // Decode color data
      $jsilxna.DecompressBlockBC12(
      bytes, sourceOffset, $jsilxna.makePixelWriter(result, width, x * blockWidth, y * blockHeight), alphaSource);

      sourceOffset += 8;
    }
  }

  return result;
};

$jsilxna.DecodeDxt5 = function (width, height, bytes, offset, count) {
  var totalSizeBytes = width * height * 4;
  var result = new Array(totalSizeBytes);

  var blockCountX = Math.floor((width + 3) / 4);
  var blockCountY = Math.floor((height + 3) / 4);
  var blockWidth = (width < 4) ? width : 4;
  var blockHeight = (height < 4) ? height : 4;

  var sourceOffset = offset;

  for (var y = 0; y < blockCountY; y++) {
    for (var x = 0; x < blockCountX; x++) {
      // Decode alpha data
      var alphaSource = $jsilxna.DecompressAlphaBlockBC3(
      bytes, sourceOffset);

      sourceOffset += 8;

      // Decode color data
      $jsilxna.DecompressBlockBC12(
      bytes, sourceOffset, $jsilxna.makePixelWriter(result, width, x * blockWidth, y * blockHeight), alphaSource);

      sourceOffset += 8;
    }
  }

  return result;
};

$jsilxna.ColorToCanvas = function (width, height, bytes, offset, count, swapRedAndBlue) {
  var result = new Array(count);

  if (swapRedAndBlue) {
    for (var i = 0, l = count; i < l; i += 4) {
      result[i + 0] = bytes[offset + i + 2];
      result[i + 1] = bytes[offset + i + 1];
      result[i + 2] = bytes[offset + i + 0];
      result[i + 3] = bytes[offset + i + 3];
    }
  } else {
    for (var i = 0, l = count; i < l; i++) {
      result[i] = bytes[offset + i];
    }
  }

  return result;
};

$jsilxna.UnpackColorsToColorBytes = function (colors, startIndex, elementCount) {
  var result = new Array(colors.length * 4);

  for (var i = 0, l = elementCount; i < l; i++) {
    var item = colors[startIndex + i];

    var p = i * 4;
    result[p + 0] = item.r & 0xFF;
    result[p + 1] = item.g & 0xFF;
    result[p + 2] = item.b & 0xFF;
    result[p + 3] = item.a & 0xFF;
  }

  return result;
};

$jsilxna.ImageFormats = {
  "Color": $jsilxna.ColorToCanvas,
  "Dxt1": $jsilxna.DecodeDxt1,
  "Dxt2": $jsilxna.DecodeDxt3,
  "Dxt3": $jsilxna.DecodeDxt3,
  "Dxt4": $jsilxna.DecodeDxt5,
  "Dxt5": $jsilxna.DecodeDxt5,
};