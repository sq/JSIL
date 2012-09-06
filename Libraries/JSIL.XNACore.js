"use strict";

if (typeof (JSIL) === "undefined") throw new Error("JSIL.Core required");

var $drawDebugRects = false, $drawDebugBoxes = false;
var $useTextCaching = true, $textCachingSupported = true;

var $jsilxna = JSIL.DeclareAssembly("JSIL.XNA");

var $sig = new JSIL.MethodSignatureCache();

var $xnaasms = new JSIL.AssemblyCollection({
  corlib: "mscorlib",
  xna: "Microsoft.Xna.Framework",
  xnaGraphics: "Microsoft.Xna.Framework.Graphics",
  xnaGame: "Microsoft.Xna.Framework.Game",
  xnaStorage: "Microsoft.Xna.Framework.Storage",
  0: "Microsoft.Xna.Framework", 
  1: "Microsoft.Xna.Framework.Game", 
  2: "Microsoft.Xna.Framework.GamerServices", 
  5: "mscorlib",
  11: "System.Drawing", 
  15: "System.Windows.Forms", 
  18: "Microsoft.Xna.Framework.Xact",
});

var getXnaGraphics = function () {
  return $xnaasms.xnaGraphics || $xnaasms.xna;
};

var getXnaStorage = function () {
  return $xnaasms.xnaStorage || $xnaasms.xna;
};

$jsilxna.nextImageId = 0;

$jsilxna.ImageCache = function (
  capacity, capacityBytes, 
  evictionMinimumAge, evictionAutomaticAge, evictionInterval
) {
  this.entries = {};
  this.accessHistory = {};
  this.count = 0;
  this.countBytes = 0;
  this.evictionPending = false;
  this.lastEvicted = this.now = Date.now();

  this.capacity = capacity; // total unique images
  this.capacityBytes = capacityBytes; // total 32bpp image bytes
  this.evictionMinimumAge = evictionMinimumAge; // if the age of an image is less than this (in ms) it is never evicted
  this.evictionAutomaticAge = evictionAutomaticAge; // if the age of an image is over this (in ms) it is automatically evicted
  this.evictionInterval = evictionInterval; // ms
};

$jsilxna.ImageCache.prototype.getItem = function (key) {
  this.accessHistory[key] = this.now;

  this.maybeEvictItems();

  return this.entries[key];
};

$jsilxna.ImageCache.prototype.setItem = function (key, value) {
  if (typeof (this.entries[key]) === "undefined") {
    this.count += 1;
    this.countBytes += value.sizeBytes;
  }

  this.accessHistory[key] = this.now;
  this.entries[key] = value;

  this.maybeEvictItems();
};

$jsilxna.ImageCache.prototype.maybeEvictItems = function () {
  if (this.evictionPending) 
    return;

  var nextEviction = this.lastEvicted + this.evictionInterval;

  if (this.now >= nextEviction) {
    this.lastEvicted = this.now;
    this.evictionPending = true;
    JSIL.Host.runLater(this.evictExtraItems.bind(this));
  }
};

$jsilxna.ImageCache.prototype.evictExtraItems = function () {
  this.evictionPending = false;
  var keys = Object.keys(this.accessHistory);

  keys.sort(function (lhs, rhs) {
    var lhsTimestamp = this.accessHistory[lhs];
    var rhsTimestamp = this.accessHistory[rhs];

    if (lhsTimestamp > rhsTimestamp) 
      return 1;
    else if (rhsTimestamp > lhsTimestamp) 
      return -1;
    else 
      return 0;
  }.bind(this));

  for (var i = 0, l = this.count; i < l; i++) {
    var age = this.now - this.accessHistory[keys[i]];
    if (age <= this.evictionMinimumAge) 
      continue;

    if (age >= this.evictionAutomaticAge) {
    } else {
      if ((this.count <= this.capacity) && (this.countBytes <= this.capacityBytes))
        continue;
    }

    var item = this.entries[keys[i]];

    delete this.accessHistory[keys[i]];
    delete this.entries[keys[i]];

    this.count -= 1;
    if ((typeof (item) !== "undefined") && (item !== null)) {
      this.countBytes -= item.sizeBytes;
    }
  }
};


$jsilxna.imageChannelCache = new $jsilxna.ImageCache(
  1024,
  (1024 * 1024) * 256,
  2500,
  30000,
  500
);

$jsilxna.textCache = new $jsilxna.ImageCache(
  1024,
  (1024 * 1024) * 64,
  500,
  3000,
  250
);


$jsilxna.colorRef = function () {
  var graphicsAsm = JSIL.GetAssembly("Microsoft.Xna.Framework.Graphics", true);
  if (graphicsAsm !== null)
    return $xnaasms.xna.TypeRef("Microsoft.Xna.Framework.Color");
  else 
    return $xnaasms.xna.TypeRef("Microsoft.Xna.Framework.Graphics.Color");
};

$jsilxna.graphicsRef = function (name) {
  var graphicsAsm = JSIL.GetAssembly("Microsoft.Xna.Framework.Graphics", true);
  if (graphicsAsm !== null)
    return graphicsAsm.TypeRef(name);
  else 
    return $xnaasms.xna.TypeRef(name);
};

$jsilxna.allowWebGL = true;
$jsilxna.testedWebGL = false;
$jsilxna.workingWebGL = false;

$jsilxna.get2DContext = function (canvas, enableWebGL) {
  var hasWebGL = typeof (WebGL2D) !== "undefined";
  var extraMessage = "";

  var forceCanvas = (document.location.search.indexOf("forceCanvas") >= 0);
  var forceWebGL = (document.location.search.indexOf("forceWebGL") >= 0);

  $textCachingSupported = (window.navigator.userAgent.indexOf("; MSIE ") < 0);

  if (forceWebGL && enableWebGL) {
    $jsilxna.testedWebGL = $jsilxna.workingWebGL = true;
  }

  if (
    (hasWebGL && enableWebGL && 
    ($jsilxna.allowWebGL !== false) && 
    !forceCanvas) || (enableWebGL && forceWebGL)
  ) {
    if (!$jsilxna.testedWebGL) {
      try {
        var testCanvas = document.createElement("canvas");
        WebGL2D.enable(testCanvas);
        var testContext = testCanvas.getContext("webgl-2d");

        $jsilxna.workingWebGL = (testContext != null) && (testContext.isWebGL);
      } catch (exc) {
        extraMessage = String(exc);
        $jsilxna.workingWebGL = false;
      }

      $jsilxna.testedWebGL = true;
    }

    // WebGL is broken in Firefox 14.0a1/a2
    if (
      (window.navigator.userAgent.indexOf("Firefox/14.0a1") >= 0) ||
      (window.navigator.userAgent.indexOf("Firefox/14.0a2") >= 0)
    ) {
      $jsilxna.workingWebGL = false;
      extraMessage = "Firefox 14.0 alpha has broken WebGL support.";
    }

    if ($jsilxna.workingWebGL) {
      WebGL2D.enable(canvas);
      return canvas.getContext("webgl-2d");
    } else {
      var msg = "WARNING: WebGL not available or broken. Using HTML5 canvas instead. " + extraMessage;
      if (window.console && (typeof (window.console.error) === "function"))
        console.error(msg);

      JSIL.Host.logWriteLine(msg);
    }
  }

  return canvas.getContext("2d");
};

$jsilxna.channelNames = ["_r", "_g", "_b", "_a"];
$jsilxna.channelKeys = ["r", "g", "b", "a"];

$jsilxna.getCachedImageChannels = function (image, key) {
  var result = $jsilxna.imageChannelCache.getItem(key) || null;
  return result;
};

$jsilxna.setCachedImageChannels = function (image, key, value) {
  $jsilxna.imageChannelCache.setItem(key, value);
};

$jsilxna.imageChannels = function (image) {
  this.sourceImage = image;
  this.width = image.naturalWidth || image.width;
  this.height = image.naturalHeight || image.height;
  this.xOffset = 1;
  this.yOffset = 1;
  // 32BPP * one image per channel
  this.sizeBytes = (this.width * this.height * 4) * 4;

  var createChannel = (function (ch) {
    var canvas = this[ch] = document.createElement("canvas");
    var context = this[ch + "Context"] = $jsilxna.get2DContext(canvas, false);

    canvas.width = this.width + 2;
    canvas.height = this.height + 2;

    context.globalCompositeOperation = "copy";
    context.globalCompositeAlpha = 1.0;
  }).bind(this);

  createChannel("r");
  createChannel("g");
  createChannel("b");
  createChannel("a");

  if (image.tagName.toLowerCase() === "canvas") {
    this.sourceImageData = $jsilxna.get2DContext(image, false).getImageData(0, 0, image.width, image.height);
  } else {
    // Workaround for bug in Firefox's canvas implementation that treats the outside of a canvas as solid white
    this.aContext.clearRect(0, 0, this.width + 2, this.height + 2);
    this.aContext.drawImage(image, this.xOffset, this.yOffset);

    this.sourceImageData = this.aContext.getImageData(this.xOffset, this.yOffset, this.width, this.height);
  }

  this.aContext.clearRect(0, 0, this.width + 2, this.height + 2);

  this.makeImageData = (function () {
    return this.aContext.createImageData(this.width, this.height);
  }).bind(this);

  this.putImageData = (function (ch, data) {
    var context = this[ch + "Context"];

    context.putImageData(data, this.xOffset, this.yOffset);
  }).bind(this);
};

$jsilxna.getImageChannels = function (image, key) {
  var cached = $jsilxna.getCachedImageChannels(image, key);
  if (cached !== null)
    return cached;

  var width = image.naturalWidth || image.width;
  var height = image.naturalHeight || image.height;

  // Workaround for chromium bug where sometimes images aren't fully initialized.
  if ((width < 1) || (height < 1))
    return null;

  var result = null;

  // If pre-generated channel images are available, use them instead
  if (image.assetName) {
    result = {
      sourceImage: image,
      width: width,
      height: height,
      xOffset: 0,
      yOffset: 0,
      sizeBytes: 0
    };

    for (var i = 0; i < 4; i++) {
      var channelAssetName = image.assetName + $jsilxna.channelNames[i];
      if (!JSIL.Host.doesAssetExist(channelAssetName)) {
        result = null;
        break;
      }

      var channelAsset = JSIL.Host.getAsset(channelAssetName);
      result[$jsilxna.channelKeys[i]] = channelAsset.image;
    }

    if (result) {
      $jsilxna.imageChannelCache.setItem(key, result);
      return result;
    }
  }

  result = new $jsilxna.imageChannels(image);

  try {
    var rData = result.makeImageData(), gData = result.makeImageData(), bData = result.makeImageData(), aData = result.sourceImageData;
    var rBytes = rData.data, gBytes = gData.data, bBytes = bData.data, aBytes = aData.data;

    for (var i = 0, l = (result.width * result.height * 4); i < l; i += 4) {
      var alpha = aBytes[i + 3];

      rBytes[i + 0] = alpha;
      rBytes[i + 3] = aBytes[i + 0]; 

      gBytes[i + 1] = alpha;
      gBytes[i + 3] = aBytes[i + 1];

      bBytes[i + 2] = alpha;
      bBytes[i + 3] = aBytes[i + 2];

      aBytes[i + 0] = aBytes[i + 1] = aBytes[i + 2] = 0;
      aBytes[i + 3] = alpha;
    }

    result.putImageData("r", rData);
    result.putImageData("g", gData);
    result.putImageData("b", bData);
    result.putImageData("a", aData);

    $jsilxna.setCachedImageChannels(image, key, result);
  } catch (exc) {
    return null;
  }

  return result;
};

$jsilxna.getImageTopLeftPixel = function (image) {
  var cached = image.topLeftPixel;
  if (typeof (cached) === "string") 
    return cached;

  var canvas = document.createElement("canvas");
  var context = $jsilxna.get2DContext(canvas, false);

  canvas.width = 1;
  canvas.height = 1;

  var imageData;
  if (image.tagName.toLowerCase() === "canvas") {
    imageData = $jsilxna.get2DContext(image, false).getImageData(0, 0, 1, 1);
  } else {
    context.globalCompositeOperation = "copy";
    context.globalCompositeAlpha = 1.0;
    context.clearRect(0, 0, 1, 1);
    context.drawImage(image, 0, 0);
    imageData = context.getImageData(0, 0, 1, 1);
  }

  var result = "0,0,0,0";
  try {
    var r = imageData.data[0];
    var g = imageData.data[1];
    var b = imageData.data[2];
    var a = imageData.data[3] / 255;

    image.topLeftPixel = result = r + "," + g + "," + b + "," + a;
  } catch (exc) {}

  return result;
};

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentLoadException", function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.String], []), function (message) {
    this._message = String(message);
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentManager", function ($) {
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$xnaasms.corlib.TypeRef("System.IServiceProvider")], []), function (serviceProvider) {});
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$xnaasms.corlib.TypeRef("System.IServiceProvider"), $.String], []), function (serviceProvider, rootDirectory) {
    this._rootDirectory = rootDirectory;
  });
  $.Method({
    Static: false,
    Public: true
  }, "Load", new JSIL.MethodSignature("!!0", [$.String], ["T"]), 
  function ContentManager_Load (T, assetName) {
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

    var rawXnb = RawXNBAsset.$As(asset);
    if (rawXnb !== null) {
      rawXnb.contentManager = this;
      var result = rawXnb.ReadAsset(T);

      if (result === null)
        JSIL.Host.warning("Asset '" + assetName + "' loader returned null.");
      return result;
    }

    if (HTML5Asset.$Is(asset)) {
      return asset;
    }

    if (asset === null)
      JSIL.Host.warning("Asset '" + assetName + "' loader returned null.");
    else
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
  $.RawMethod(false, ".ctor", function (assetName) {
    this.name = assetName;
  });

  $.RawMethod(false, "toString", function () {
    return "<XNA Asset '" + this.name + "'>";
  });
});

JSIL.MakeClass("HTML5Asset", "HTML5ImageAsset", true, [], function ($) {
  $.RawMethod(false, ".ctor", function (assetName, image) {
    HTML5Asset.prototype._ctor.call(this, assetName);
    image.assetName = assetName;

    this.image = image;
    this.Width = image.naturalWidth;
    this.Height = image.naturalHeight;
    this.IsDisposed = false;
    this.id = String(++$jsilxna.nextImageId);

    Object.defineProperty(this, "Bounds", {
      configurable: true,
      enumerable: true,
      get: this.get_Bounds
    });
  });

  $.RawMethod(false, "get_IsDisposed", function () {
    return false;
  });

  $.RawMethod(false, "get_Width", function () {
    return this.Width;
  });

  $.RawMethod(false, "get_Height", function () {
    return this.Height;
  });

  $.RawMethod(false, "get_Bounds", function () {
    if (!this._bounds)
      this._bounds = new Microsoft.Xna.Framework.Rectangle(0, 0, this.Width, this.Height);

    return this._bounds;
  });
});

JSIL.MakeClass("HTML5Asset", "SoundAssetBase", true, [], function ($) {

  $.Method({Static:false, Public:true }, "Play", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function Play () {
      return this.Play(1, 0, 0);
    }
  );

  $.Method({Static:false, Public:true }, "Play", 
    (new JSIL.MethodSignature($.Boolean, [$.Single, $.Single, $.Single], [])), 
    function Play (volume, pitch, pan) {
      var instance = this.$newInstance();

      instance.volume = volume;

      // FIXME: No pitch or pan

      instance.play();

      return true;
    }
  );

  $.Method({Static:false, Public:true }, "CreateInstance",
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Audio.SoundEffectInstance"), [], [])),
    function CreateInstance () {
      return new Microsoft.Xna.Framework.Audio.SoundEffectInstance(this, false);
    }
  );

});

JSIL.MakeClass("SoundAssetBase", "CallbackSoundAsset", true, [], function ($) {
  $.RawMethod(false, ".ctor", function (assetName, createInstance) {
    HTML5Asset.prototype._ctor.call(this, assetName);

    this.$createInstance = createInstance;
    this.freeInstances = [];
  });

  $.RawMethod(false, "$newInstance", function () {
    if (this.freeInstances.length > 0) {
      return this.freeInstances.pop();
    } else {
      return this.$createInstance(0);
    }
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentTypeReader", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[5].TypeRef("System.Type")], [])), 
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
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.Type"), [], [])), 
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
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReaderManager")], [])), 
    function Initialize (manager) {
      
    }
  );

  /*
    This requires overload resolution.
  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Object, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Object], [])), 
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
      var assembly = $xnaasms.xna;

      assembly.Microsoft.Xna.Framework.Content.ContentTypeReader.prototype._ctor.call(
        this, assembly.Microsoft.Xna.Framework.Content.ContentTypeReader$b1.T.get(this)
      );
    }
  );

  /*
    This requires overload resolution.

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Object, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Object], [])), 
    function Read (input, existingInstance) {
      throw new Error("Invoked abstract method (ContentTypeReader`1.Read)");
    }
  );

  var gp = new JSIL.GenericParameter("T", "Microsoft.Xna.Framework.Content.ContentTypeReader`1");

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature(gp, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), gp], [])), 
    function Read (input, existingInstance) {
      throw new Error("Invoked abstract method (ContentTypeReader`1.Read)");
    }
  );

  */

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.StringReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.String, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.String], [])), 
    function Read (input, existingInstance) {
      return input.ReadString();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ByteReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Byte, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Byte], [])), 
    function Read (input, existingInstance) {
      return input.ReadByte();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.CharReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Char, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Char], [])), 
    function Read (input, existingInstance) {
      return input.ReadChar();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Int16Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Int16, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Int16], [])), 
    function Read (input, existingInstance) {
      return input.ReadInt16();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Int32Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.Int32, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.Int32], [])), 
    function Read (input, existingInstance) {
      return input.ReadInt32();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.UInt16Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.UInt16, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.UInt16], [])), 
    function Read (input, existingInstance) {
      return input.ReadUInt16();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.UInt32Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($.UInt32, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $.UInt32], [])), 
    function Read (input, existingInstance) {
      return input.ReadUInt32();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.PointReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
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
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
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
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2")], [])), 
    function Read (input, existingInstance) {
      return input.ReadVector2();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Vector3Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function Read (input, existingInstance) {
      return input.ReadVector3();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Vector4Reader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector4"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector4")], [])), 
    function Read (input, existingInstance) {
      return input.ReadVector4();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ArrayReader`1", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      var assembly = $xnaasms.xna;
      assembly.Microsoft.Xna.Framework.Content.ContentTypeReader$b1.prototype._ctor.call(
        this, System.Array.Of(
          assembly.Microsoft.Xna.Framework.Content.ArrayReader$b1.T.get(this)
        ).__Type__
      );
    }
  );

  $.Method({Static:false, Public:false}, "Initialize", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReaderManager")], [])), 
    function Initialize (manager) {
      this.elementReader = manager.GetTypeReader(this.T);
    }
  );

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "Microsoft.Xna.Framework.Content.ArrayReader`1")]), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "Microsoft.Xna.Framework.Content.ArrayReader`1")])], [])), 
    function Read (input, existingInstance) {
      var count = input.ReadInt32();
      if (existingInstance === null) {
        existingInstance = JSIL.Array.New(this.T, count);
      }

      for (var i = 0; i < count; i++) {
        existingInstance[i] = input.ReadObjectInternal$b1(this.T)(this.elementReader, null);
      }

      return existingInstance;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ListReader`1", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      var assembly = $xnaasms.xna;
      assembly.Microsoft.Xna.Framework.Content.ContentTypeReader$b1.prototype._ctor.call(
        this, System.Collections.Generic.List$b1.Of(
          assembly.Microsoft.Xna.Framework.Content.ListReader$b1.T.get(this)
        ).__Type__
      );
    }
  );

  $.Method({Static:false, Public:false}, "Initialize", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReaderManager")], [])), 
    function Initialize (manager) {
      this.elementReader = manager.GetTypeReader(this.T);
    }
  );

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.Collections.Generic.List`1", [new JSIL.GenericParameter("T", "Microsoft.Xna.Framework.Content.ListReader`1")]), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $xnaasms[5].TypeRef("System.Collections.Generic.List`1", [new JSIL.GenericParameter("T", "Microsoft.Xna.Framework.Content.ListReader`1")])], [])), 
    function Read (input, existingInstance) {
      var count = input.ReadInt32();
      if (existingInstance === null) {
        existingInstance = new(System.Collections.Generic.List$b1.Of(this.T))();
      }

      while (count > 0) {
        var item = input.ReadObjectInternal$b1(this.T)(this.elementReader, null);
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
    (new JSIL.MethodSignature($jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D")], [])), 
    function Read (input, existingInstance) {
      var asmGraphics = $xnaasms.xnaGraphics || $xnaasms.xna;
      var tTexture2D = JSIL.GetTypeFromAssembly(asmGraphics, "Microsoft.Xna.Framework.Graphics.Texture2D", [], true);
      var tSurfaceFormat = asmGraphics.Microsoft.Xna.Framework.Graphics.SurfaceFormat.__Type__;

      var surfaceFormat = tSurfaceFormat.$Cast(input.ReadInt32());
      var width = input.ReadInt32();
      var height = input.ReadInt32();
      var mipCount = input.ReadInt32();

      var result = existingInstance;
      if (result === null) result = JSIL.CreateInstanceOfType(tTexture2D, "$internalCtor", [null, width, height, mipCount > 1, surfaceFormat]);

      for (var i = 0; i < mipCount; i++) {
        var mipSize = input.ReadInt32();
        var mipBytes = input.ReadBytes(mipSize);

        if (i === 0) 
          result.SetData$b1(System.Byte)(i, null, mipBytes, 0, mipSize);
      }

      return result;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.SpriteFontReader", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:false}, "Read", 
    (new JSIL.MethodSignature($jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont")], [])), 
    function Read (input, existingInstance) {
      var asmXna = $xnaasms.xna;
      var asmGraphics = $xnaasms.xnaGraphics || $xnaasms.xna;

      var tList = System.Collections.Generic.List$b1;
      var tSpriteFont = asmGraphics.Microsoft.Xna.Framework.Graphics.SpriteFont;
      var tTexture2D = asmGraphics.Microsoft.Xna.Framework.Graphics.Texture2D;
      var tRectangle = asmXna.Microsoft.Xna.Framework.Rectangle;
      var tVector3 = asmXna.Microsoft.Xna.Framework.Vector3;

      var texture = input.ReadObject$b1(tTexture2D)();

      var glyphs = input.ReadObject$b1(tList.Of(tRectangle))();

      var cropping = input.ReadObject$b1(tList.Of(tRectangle))();

      var charMap = input.ReadObject$b1(tList.Of(System.Char))();

      var lineSpacing = input.ReadInt32();
      var spacing = input.ReadSingle();

      var kerning = input.ReadObject$b1(tList.Of(tVector3))();

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
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.Effect"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.Effect")], [])), 
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
      var assembly = $xnaasms.xna;
      var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

      thisType.nameToReader = {};
      thisType.targetTypeToReader = {};
      thisType.readerTypeToReader = {};
    }
  );

  $.Method({Static:true , Public:false}, "AddTypeReader", 
    (new JSIL.MethodSignature(null, [
          $.String, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader")
        ], [])), 
    function AddTypeReader (readerTypeName, contentReader, reader) {
      var assembly = $xnaasms.xna;
      var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

      var targetType = reader.TargetType;
      thisType.targetTypeToReader[targetType.__TypeId__] = reader;
      thisType.readerTypeToReader[reader.GetType().__TypeId__] = reader;
      thisType.nameToReader[readerTypeName] = reader;
    }
  );

  $.Method({Static:true , Public:false}, "GetTypeReader", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader"), [$xnaasms[5].TypeRef("System.Type"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader")], [])), 
    function GetTypeReader (targetType, contentReader) {
      var assembly = $xnaasms.xna;
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
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader")]), [$.Int32, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader")], [])), 
    function ReadTypeManifest (typeCount, contentReader) {
      var assembly = $xnaasms.xna;
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
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentReader")], [])), 
    function _ctor (contentReader) {
      this.contentReader = contentReader;
      this.knownReaders = {};
    }
  );

  $.Method({Static:false, Public:true }, "GetTypeReader", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader"), [$xnaasms[5].TypeRef("System.Type")], [])), 
    function GetTypeReader (targetType) {
      var typeName = targetType.toString();
      var reader = this.knownReaders[typeName];
      if (typeof (reader) === "object") 
        return reader;

      var assembly = $xnaasms.xna;
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
        $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentManager"), $xnaasms[5].TypeRef("System.IO.Stream"), 
        $.String, $xnaasms[5].TypeRef("System.Action`1", [$xnaasms[5].TypeRef("System.IDisposable")]), 
        $.Int32
      ], []), 
    function _ctor (contentManager, input, assetName, recordDisposableObject, graphicsProfile) {
      var signature = new JSIL.MethodSignature(null, [$xnaasms[5].TypeRef("System.IO.Stream")], []);
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
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentManager"), [], [])), 
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

      if (isCompressed) 
        throw new Error("Compressed XNBs are not supported");

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
      return JSIL.StringFromByteArray(chars);
    }
  );

  $.Method({Static:false, Public:true }, "ReadVector2", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [], [])), 
    function ReadVector2 () {
      var x = this.ReadSingle();
      var y = this.ReadSingle();
      return new Microsoft.Xna.Framework.Vector2(x, y);
    }
  );

  $.Method({Static:false, Public:true }, "ReadVector3", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), [], [])), 
    function ReadVector3 () {
      var x = this.ReadSingle();
      var y = this.ReadSingle();
      var z = this.ReadSingle();
      return new Microsoft.Xna.Framework.Vector3(x, y, z);
    }
  );

  $.Method({Static:false, Public:true }, "ReadVector4", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector4"), [], [])), 
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
    (new JSIL.MethodSignature("!!0", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader"), "!!0"], ["T"])), 
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
    (new JSIL.MethodSignature("!!0", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentTypeReader"), $.Object], ["T"])), 
    function ReadObjectInternal$b1 (T, typeReader, existingInstance) {
      return readObjectImpl(this, T, typeReader, existingInstance);
    }
  );

  var readRawObjectImpl = function (self, T, existingInstance) {
    var assembly = $xnaasms.xna;
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
  }, "ReadAsset", new JSIL.MethodSignature(null, [], []), function RawXNBAsset_ReadAsset (type) {
    var memoryStream = new System.IO.MemoryStream(this.bytes, false);

    var tContentReader = JSIL.GetTypeFromAssembly(
      $xnaasms.xna, "Microsoft.Xna.Framework.Content.ContentReader", [], true
    );
    var contentReader = $sig.get("newCr").Construct(
      tContentReader, this.contentManager, memoryStream, this.name, null, 0
    );

    contentReader.ReadHeader();

    var sharedResourceCount = contentReader.Read7BitEncodedInt();
    var sharedResources = new Array(sharedResourceCount);

    var mainObject = contentReader.ReadObject$b1(type)();

    for (var i = 0; i < sharedResourceCount; i++)
    sharedResources[i] = content.ReadObject$b1(System.Object)();

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
          $.String, $xnaasms[5].TypeRef("System.TimeSpan"), 
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
    (new JSIL.MethodSignature($xnaasms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioCategory"), [$.String], [])), 
    function GetCategory (name) {
      // FIXME
      return new Microsoft.Xna.Framework.Audio.AudioCategory(this, name);
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Audio.AudioCategory", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioEngine"), $.String], [])), 
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

  $.Method({Static:false, Public:true }, "SetVolume", 
    (new JSIL.MethodSignature(null, [$.Single], [])), 
    function SetVolume (volume) {
      // FIXME
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Audio.WaveBank", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioEngine"), $.String], [])), 
    function _ctor (audioEngine, nonStreamingWaveBankFilename) {
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $xnaasms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioEngine"), $.String, 
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

  $.Method({Static:false, Public:true }, "SetVariable", 
    (new JSIL.MethodSignature(null, [$.String, $.Single], [])), 
    function SetVariable (name, value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "Pause", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Pause () {
      this.$gc();

      for (var i = 0; i < this.wavesPlaying.length; i++) {
        var wave = this.wavesPlaying[i];
        wave.pause()
      }

      // FIXME: AudioContext sucks and has no way to pause streams.
      this.wavesPlaying = [];
    }
  );

  $.Method({Static:false, Public:true }, "Play", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Play () {
      this.$gc();

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

  $.RawMethod(false, "$gc", function () {
    for (var i = 0; i < this.wavesPlaying.length; i++) {
      var w = this.wavesPlaying[i];

      if (!w.isPlaying) {
        w.pause();
        this.wavesPlaying.splice(i, 1);
        i--;
      }
    }
  });

  $.Method({Static:false, Public:true }, "get_IsPlaying", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsPlaying () {
      this.$gc();
      
      return (this.wavesPlaying.length > 0);
    }
  );

  $.Method({Static:false, Public:true }, "Resume", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Resume () {
      this.$gc();

      // FIXME: AudioContext sucks and has no way to pause streams.
      this.Stop();
      this.Play();
    }
  );

  $.Method({Static:false, Public:true }, "Stop", 
    (new JSIL.MethodSignature(null, [$xnaasms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioStopOptions")], [])), 
    function Stop (options) {
      this.$gc();

      while (this.wavesPlaying.length > 0) {
        var wave = this.wavesPlaying.shift();
        wave.pause();
      }

      this.wavesPlaying = [];
    }
  );

  $.Method({Static:false, Public:true }, "Dispose", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Dispose () {
      this.Stop();
      this.$gc();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Audio.SoundBank", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[18].TypeRef("Microsoft.Xna.Framework.Audio.AudioEngine"), $.String], [])), 
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
    (new JSIL.MethodSignature($xnaasms[18].TypeRef("Microsoft.Xna.Framework.Audio.Cue"), [$.String], [])), 
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

    if (song) {
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
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Media.Song")], [])), 
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

    var functionName = typeName + "." + suffixedName;

    switch (argCount) {
      case 0:
        return JSIL.CreateNamedFunction(
          functionName,
          [],
          js
        );
      case 1:
        return JSIL.CreateNamedFunction(
          functionName,
          ["value"],
          js
        );
      case 2:
        return JSIL.CreateNamedFunction(
          functionName,
          ["lhs", "rhs"],
          js
        );
      case 3:
        return JSIL.CreateNamedFunction(
          functionName,
          ["lhs", "rhs", "amount"],
          js
        );      
      default:
        throw new Error("Invalid argument count");
    }
  },

  bindToPrototype: function (fn, typeRef) {
    var state = {
      resolvedType: null,
      typeRef: typeRef
    };

    JSIL.SetLazyValueProperty(
      state, "create", 
      function VectorMethod_GetCreator () {
        if (state.resolvedType === null)
          state.resolvedType = state.typeRef.get();
        
        var create = Object.create;
        var proto = state.resolvedType.prototype;
        return create.bind(Object, proto);
      }
    );

    return fn.bind(state);
  },

  makeArithmeticOperator: function ($, name, staticMethodName, operator, dataMembers, tLeft, tRight, tResult) {
    var leftScalar = (tLeft !== tResult);
    var rightScalar = (tRight !== tResult);

    if (leftScalar && rightScalar)
      throw new Error("Invalid type combination");

    var body = [];
    body.push("var result = this.create();");

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

    var wrapper;

    if (leftScalar) {
      wrapper = function VectorOperator_Scalar_Ref (lhs, rhs, result) {
        result.value = fn(lhs, rhs.value);
      }
    } else if (rightScalar) {
      wrapper = function VectorOperator_Ref_Scalar (lhs, rhs, result) {
        result.value = fn(lhs.value, rhs);
      }
    } else {
      wrapper = function VectorOperator_Ref_Ref (lhs, rhs, result) {
        result.value = fn(lhs.value, rhs.value);
      }
    }

    $.Method({Static: true , Public: true }, staticMethodName,
      new JSIL.MethodSignature(null, [
        leftScalar ? tLeft : makeRef(tLeft), 
        rightScalar ? tRight : makeRef(tRight), 
        makeRef(tResult)
      ], []),
      wrapper
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
    body.push("var result = this.create();");

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

  makeDistanceFunction: function ($, name, isSquared, dataMembers, tVector) {
    var body = [];
    body.push("var result = 0, current;");

    for (var i = 0; i < dataMembers.length; i++) {
      var dataMember = dataMembers[i];

      body.push("current = (rhs." + dataMember + " - lhs." + dataMember + ");");
      body.push("result += (current * current);");
    }

    if (isSquared)
      body.push("return result;");
    else
      body.push("return Math.sqrt(result);");

    var fn = vectorUtil.makeOperatorCore(name, tVector, body, 2);

    $.Method({Static: true, Public: true }, name, 
      new JSIL.MethodSignature($.Single, [tVector, tVector], []),
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

    vectorUtil.makeDistanceFunction($, "DistanceSquared", true, dataMembers, tVector);
    vectorUtil.makeDistanceFunction($, "Distance", false, dataMembers, tVector);

    vectorUtil.makeNormalizer($, dataMembers, tVector);
  },

  makeLerpMethod: function ($, dataMembers, tVector) {
    var name = "Lerp";
    var body = [];
    body.push("var result = lhs.MemberwiseClone();");

    for (var i = 0; i < dataMembers.length; i++) {
      var dataMember = dataMembers[i];

      body.push("result." + dataMember + " += (rhs." + dataMember + " - lhs." + dataMember + ") * amount;");
    }

    body.push("return result;");

    var fn = vectorUtil.makeOperatorCore(name, tVector, body, 3);

    $.Method({Static: true, Public: true }, name, 
      new JSIL.MethodSignature(tVector, [tVector, tVector, $.Single], []),
      fn
    );
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

    vectorUtil.makeLerpMethod($, dataMembers, tVector);
  },

  makeConstants: function ($, tVector, constants) {
    var makeGetter = function (values) {
      var state = null;

      return function () {
        if (state === null)
          state = JSIL.CreateInstanceOfType(
            tVector.get().__Type__, values
          );

        return state;
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
    $, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), {
      "UnitX": [1, 0],
      "UnitY": [0, 1],
      "Zero": [0, 0],
      "One": [1, 1]
    }
  );

  vectorUtil.makeOperators(
    $, ["X", "Y"], $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2")
  );

  $.Method({Static:true , Public:true }, "Transform", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix")], [])), 
    function Transform (position, matrix) {
      var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
      result.X = (position.X * matrix.xScale) + matrix.xTranslation;
      result.Y = (position.Y * matrix.yScale) + matrix.yTranslation;
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "Dot", 
    (new JSIL.MethodSignature($.Single, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2")], [])), 
    function Dot (vector1, vector2) {
      return vector1.X * vector2.X + vector1.Y * vector2.Y;
    }
  );

  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single, $.Single], []), function Vector2_ctor (x, y) {
    this.X = x;
    this.Y = y;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single], []), function Vector2_ctor (value) {
    this.X = this.Y = value;
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Vector3", function ($) {
  vectorUtil.makeConstants(
    $, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), {
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
    $, ["X", "Y", "Z"], $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")
  );

  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single, $.Single, $.Single], []), function Vector3_ctor (x, y, z) {
    this.X = x;
    this.Y = y;
    this.Z = z;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single], []), function Vector3_ctor (value) {
    this.X = this.Y = this.Z = value;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function Vector3_ctor (xy, z) {
    this.X = xy.X;
    this.Y = xy.Y;
    this.Z = z;
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
    $, ["X", "Y", "Z", "W"], $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector4")
  );

  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single, $.Single, $.Single, $.Single], []), function Vector4_ctor (x, y, z, w) {
    this.X = x;
    this.Y = y;
    this.Z = z;
    this.W = w;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function Vector4_ctor (xy, z, w) {
    this.X = xy.X;
    this.Y = xy.Y;
    this.Z = z;
    this.W = w;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [], []), function Vector4_ctor (xyz, w) {
    this.X = xyz.X;
    this.Y = xyz.Y;
    this.Z = xyz.Z;
    this.W = w;
  });
  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$.Single], []), function Vector4_ctor (value) {
    this.X = this.Y = this.Z = this.W = value;
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Matrix", function ($) {
  var matrix = $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix");

  $.Method({
    Static: true,
    Public: true
  }, ".cctor2", new JSIL.MethodSignature(null, [], []), function () {
    // FIXME
    var identity = Microsoft.Xna.Framework.Matrix._identity = new Microsoft.Xna.Framework.Matrix();

    identity.xTranslation = identity.yTranslation = identity.zTranslation = 0;
    identity.xRotation = identity.yRotation = identity.zRotation = 0;
    identity.xScale = identity.yScale = identity.zScale = 1;
  });

  $.RawMethod(false, "__CopyMembers__", 
    function Matrix_CopyMembers (source, target) {
      target.xScale = source.xScale || 0;
      target.yScale = source.yScale || 0;
      target.zScale = source.zScale || 0;
      target.xTranslation = source.xTranslation || 0;
      target.yTranslation = source.yTranslation || 0;
      target.zTranslation = source.zTranslation || 0;
      target.xRotation = source.xRotation || 0;
      target.yRotation = source.yRotation || 0;
      target.zRotation = source.zRotation || 0;
    }
  );

  $.Method({Static:true , Public:true }, "get_Identity", 
    (new JSIL.MethodSignature(matrix, [], [])), 
    function get_Identity () {
      return Microsoft.Xna.Framework.Matrix._identity;
    }
  );

  $.Method({Static:true , Public:true }, "CreateLookAt", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")
        ], [])), 
    function CreateLookAt (cameraPosition, cameraTarget, cameraUpVector) {
      // FIXME
      return Microsoft.Xna.Framework.Matrix._identity;
    }
  );

  $.Method({Static:true , Public:true }, "CreateLookAt", 
    (new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")]), $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")]), 
          $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")]), $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix")])
        ], [])), 
    function CreateLookAt (/* ref */ cameraPosition, /* ref */ cameraTarget, /* ref */ cameraUpVector, /* ref */ result) {
      // FIXME
      result.value = Microsoft.Xna.Framework.Matrix._identity;
    }
  );

  $.Method({Static:true , Public:true }, "CreateOrthographic", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [
          $.Single, $.Single, 
          $.Single, $.Single
        ], [])), 
    function CreateOrthographic (width, height, zNearPlane, zFarPlane) {
      // FIXME
      return Microsoft.Xna.Framework.Matrix._identity;
    }
  );

  $.Method({Static:true , Public:true }, "CreateOrthographic", 
    (new JSIL.MethodSignature(null, [
          $.Single, $.Single, 
          $.Single, $.Single, 
          $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix")])
        ], [])), 
    function CreateOrthographic (width, height, zNearPlane, zFarPlane, /* ref */ result) {
      // FIXME
      result.value = Microsoft.Xna.Framework.Matrix._identity;
    }
  );

  $.Method({Static:true , Public:true }, "CreateOrthographicOffCenter", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [
          $.Single, $.Single, 
          $.Single, $.Single, 
          $.Single, $.Single
        ], [])), 
    function CreateOrthographicOffCenter (left, right, bottom, top, zNearPlane, zFarPlane) {
      // FIXME
      return Microsoft.Xna.Framework.Matrix._identity;
    }
  );

  $.Method({Static:true , Public:true }, "CreateOrthographicOffCenter", 
    (new JSIL.MethodSignature(null, [
          $.Single, $.Single, 
          $.Single, $.Single, 
          $.Single, $.Single, 
          $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix")])
        ], [])), 
    function CreateOrthographicOffCenter (left, right, bottom, top, zNearPlane, zFarPlane, /* ref */ result) {
      // FIXME
      result.value = Microsoft.Xna.Framework.Matrix._identity;
    }
  );

  $.Method({Static:true , Public:true }, "CreateRotationX", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [$.Single], [])), 
    function CreateRotationX (radians) {
      // FIXME
      var result = Microsoft.Xna.Framework.Matrix._identity.MemberwiseClone();

      result.xRotation = radians;
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "CreateRotationY", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [$.Single], [])), 
    function CreateRotationY (radians) {
      // FIXME
      var result = Microsoft.Xna.Framework.Matrix._identity.MemberwiseClone();

      result.yRotation = radians;
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "CreateRotationZ", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [$.Single], [])), 
    function CreateRotationZ (radians) {
      // FIXME
      var result = Microsoft.Xna.Framework.Matrix._identity.MemberwiseClone();

      result.zRotation = radians;
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "CreateScale", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function CreateScale (scales) {
      // FIXME
      var result = Microsoft.Xna.Framework.Matrix._identity.MemberwiseClone();

      result.xScale = scales.X;
      result.yScale = scales.Y;
      result.zScale = scales.Z;
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "CreateScale", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [
          $.Single, $.Single, 
          $.Single
        ], [])), 
    function CreateScale (xScale, yScale, zScale) {
      // FIXME
      var result = Microsoft.Xna.Framework.Matrix._identity.MemberwiseClone();

      result.xScale = xScale;
      result.yScale = yScale;
      result.zScale = zScale;
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "CreateTranslation", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function CreateTranslation (position) {
      // FIXME
      var result = Microsoft.Xna.Framework.Matrix._identity.MemberwiseClone();

      result.xTranslation = position.X;
      result.yTranslation = position.Y;
      result.zTranslation = position.Z;
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "CreateTranslation", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [
          $.Single, $.Single, 
          $.Single
        ], [])), 
    function CreateTranslation (xPosition, yPosition, zPosition) {
      // FIXME
      var result = Microsoft.Xna.Framework.Matrix._identity.MemberwiseClone();

      result.xTranslation = xPosition;
      result.yTranslation = yPosition;
      result.zTranslation = zPosition;
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "Invert", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix")], [])), 
    function Invert (matrix) {
      // FIXME
      var result = Microsoft.Xna.Framework.Matrix._identity.MemberwiseClone();

      result.xTranslation = -matrix.xTranslation;
      result.yTranslation = -matrix.yTranslation;
      result.zTranslation = -matrix.zTranslation;

      result.xScale = 1 / (matrix.xScale + 0.000001);
      result.yScale = 1 / (matrix.yScale + 0.000001);
      result.zScale = 1 / (matrix.zScale + 0.000001);

      return result;
    }
  );

  $.Method({Static:true , Public:true }, "op_Multiply", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix")], [])), 
    function Multiply (matrix1, matrix2) {
      // FIXME
      var result = Microsoft.Xna.Framework.Matrix._identity.MemberwiseClone();

      result.xTranslation = matrix1.xTranslation + matrix2.xTranslation;
      result.yTranslation = matrix1.yTranslation + matrix2.yTranslation;
      result.zTranslation = matrix1.zTranslation + matrix2.zTranslation;

      result.xScale = matrix1.xScale * matrix2.xScale;
      result.yScale = matrix1.yScale * matrix2.yScale;
      result.zScale = matrix1.zScale * matrix2.zScale;

      return result;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.BasicEffect", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice")], [])), 
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
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function set_AmbientLightColor (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_DiffuseColor", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function set_DiffuseColor (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_EmissiveColor", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function set_EmissiveColor (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_FogColor", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
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
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix")], [])), 
    function set_Projection (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_SpecularColor", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
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
    (new JSIL.MethodSignature(null, [getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.Texture2D")], [])), 
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
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix")], [])), 
    function set_View (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_World", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Matrix")], [])), 
    function set_World (value) {
      // FIXME
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.Effect", function ($) {
  $.Method({Static:false, Public:true }, "get_CurrentTechnique", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.EffectTechnique"), [], [])), 
    function get_CurrentTechnique () {
      // FIXME
      return new Microsoft.Xna.Framework.Graphics.EffectTechnique();
    }
  );

  $.Method({Static:false, Public:true }, "get_Parameters", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.EffectParameterCollection"), [], [])), 
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
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.EffectParameter"), [$.Int32], [])), 
    function get_Item (index) {
      // FIXME
      return new Microsoft.Xna.Framework.Graphics.EffectParameter();
    }
  );

  $.Method({Static:false, Public:true }, "get_Item", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.EffectParameter"), [$.String], [])), 
    function get_Item (name) {
      // FIXME
      return new Microsoft.Xna.Framework.Graphics.EffectParameter();
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.EffectParameter", function ($) {  
  $.Method({Static:false, Public:true }, "get_Elements", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.EffectParameterCollection"), [], [])), 
    function get_Elements () {
      // FIXME
      return new Microsoft.Xna.Framework.Graphics.EffectParameterCollection();
    }
  );

  $.Method({Static:false, Public:true }, "SetValue", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector4")])], [])), 
    function SetValue (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "SetValue", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector4")], [])), 
    function SetValue (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "SetValue", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")])], [])), 
    function SetValue (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "SetValue", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")], [])), 
    function SetValue (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "SetValue", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2")])], [])), 
    function SetValue (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "SetValue", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2")], [])), 
    function SetValue (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "SetValue", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.Single])], [])), 
    function SetValue (value) {
      // FIXME
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
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.EffectPassCollection"), [], [])), 
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
        $xnaasms.xna, "Microsoft.Xna.Framework.Content.ContentManager", [], true
      );

      var tGameTime = JSIL.GetTypeFromAssembly(
        $xnaasms.xnaGame, "Microsoft.Xna.Framework.GameTime", [], true
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
    (new JSIL.MethodSignature($xnaasms[1].TypeRef("Microsoft.Xna.Framework.GameComponentCollection"), [], [])), 
    function get_Components () {
      return this.components;
    }
  );

  $.Method({Static:false, Public:true }, "get_Content", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Content.ContentManager"), [], [])), 
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
      return JSIL.Host.isPageVisible() && !Microsoft.Xna.Framework.Game._QuitForced && !this._isDead;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsFixedTimeStep", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsFixedTimeStep () {
      return this.isFixedTimeStep;
    }
  );

  $.Method({Static:false, Public:true }, "get_TargetElapsedTime", 
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.TimeSpan"), [], [])), 
    function get_TargetElapsedTime () {
      return this.targetElapsedTime;
    }
  );

  $.Method({Static:false, Public:true }, "get_Services", 
    (new JSIL.MethodSignature($xnaasms[1].TypeRef("Microsoft.Xna.Framework.GameServiceContainer"), [], [])), 
    function get_Services () {
      return this.gameServices;
    }
  );

  $.Method({Static:false, Public:true }, "get_Window", 
    (new JSIL.MethodSignature($xnaasms[1].TypeRef("Microsoft.Xna.Framework.GameWindow"), [], [])), 
    function get_Window () {
      // FIXME
      if (!this._window)
        this._window = new Microsoft.Xna.Framework.GameWindow();

      return this._window;
    }
  );

  $.Method({Static:false, Public:false}, "get_IsMouseVisible", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsMouseVisible () {
      var oc = this.graphicsDeviceService.GraphicsDevice.originalCanvas;
      return (oc.style.cursor !== "none");
    }
  );

  $.Method({Static:false, Public:true }, "set_IsMouseVisible", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_IsMouseVisible (value) {
      var oc = this.graphicsDeviceService.GraphicsDevice.originalCanvas;
      oc.style.cursor = value ? "default" : "none";
    }
  );

  $.Method({Static:false, Public:true }, "set_IsFixedTimeStep", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_IsFixedTimeStep (value) {
      this.isFixedTimeStep = value;
    }
  );

  $.Method({Static:false, Public:true }, "set_TargetElapsedTime", 
    (new JSIL.MethodSignature(null, [$xnaasms[5].TypeRef("System.TimeSpan")], [])), 
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

      if (type.$Is(item)) 
        result.push(item);
    }
    return result;
  });

  var cmp = function (lhs, rhs) {
    if (lhs > rhs)
      return 1;
    else if (rhs > lhs)
      return -1;
    else
      return 0;
  };

  $.Method({Static:false, Public:false}, "Draw", 
    (new JSIL.MethodSignature(null, [$xnaasms[1].TypeRef("Microsoft.Xna.Framework.GameTime")], [])), 
    function Game_Draw (gameTime) {
      if (Microsoft.Xna.Framework.Game._QuitForced || this._isDead) 
        return;

      var drawableComponents = this.$ComponentsOfType(Microsoft.Xna.Framework.IDrawable.__Type__);
      drawableComponents.sort(function (lhs, rhs) {
        return cmp(lhs.get_DrawOrder(), rhs.get_DrawOrder());
      });

      for (var i = 0, l = drawableComponents.length; i < l; i++) {
        var drawable = drawableComponents[i];

        if (drawable.Visible) 
          drawable.Draw(gameTime);
      }
    }
  );

  $.Method({Static:false, Public:false}, "Update", 
    (new JSIL.MethodSignature(null, [$xnaasms[1].TypeRef("Microsoft.Xna.Framework.GameTime")], [])), 
    function Game_Update (gameTime) {
      if (Microsoft.Xna.Framework.Game._QuitForced || this._isDead) 
        return;

      var updateableComponents = this.$ComponentsOfType(Microsoft.Xna.Framework.IUpdateable.__Type__);
      updateableComponents.sort(function (lhs, rhs) {
        return cmp(lhs.get_UpdateOrder(), rhs.get_UpdateOrder());
      });

      for (var i = 0, l = updateableComponents.length; i < l; i++) {
        var updateable = updateableComponents[i];

        if (updateable.Enabled) 
          updateable.Update(gameTime);
      }
    }
  );

  $.Method({
    Static: false,
    Public: true
  }, "Run", new JSIL.MethodSignature(null, [], []), function () {
    this._profilingMode = (document.location.search.indexOf("profile") >= 0);
    this._balanceFPSCheckbox = (document.getElementById("balanceFramerate") || null);
    if (this._balanceFPSCheckbox)
      this._balanceFPSCheckbox.checked = !this._profilingMode;

    Microsoft.Xna.Framework.Game._QuitForced = false;
    this.Initialize();
    this._QueueStep();
  });

  $.RawMethod(false, "_GetNow", function Game_GetNow () {
    return (new Date()).getTime();
  });

  $.RawMethod(false, "_DeferCall", function Game_DeferCall (callback, lng) {
    setTimeout(callback, 0);
  });

  $.RawMethod(false, "_QueueStep", function Game_EnqueueTick () {
    if (Microsoft.Xna.Framework.Game._QuitForced || this._isDead) 
      return;

    var self = this;
    var stepCallback = self._Step.bind(self);

    var forceSetTimeout = false || 
      (document.location.search.indexOf("forceSetTimeout") >= 0) ||
      (typeof (msRequestAnimationFrame) !== "undefined") // IE10 currently has broken requestAnimationFrame
      ;

    var requestAnimationFrame = window.requestAnimationFrame ||
      window.mozRequestAnimationFrame || 
      window.webkitRequestAnimationFrame ||
      window.msRequestAnimationFrame ||
      window.oRequestAnimationFrame;

    if (requestAnimationFrame && !forceSetTimeout) {
      requestAnimationFrame(stepCallback);
    } else {
      var shouldStepCallback = function ShouldStep () {
        var now = self._GetNow();
        var delay = self._nextFrame - now;

        if (delay <= 0) 
          stepCallback();
        else 
          self._DeferCall(shouldStepCallback, delay >= 3);
      };

      // It's important that we use setTimeout at least once after every frame in order to let the browser pump messages
      this._DeferCall(shouldStepCallback, true);
    }
  });

  $.RawMethod(false, "_TimedUpdate", function Game_TimedUpdate (longFrame) {
    var updateStarted = this._GetNow();
    this.Update(this._gameTime);
    var updateEnded = this._GetNow();

    // Detect long updates and suppress frameskip.
    if ((updateEnded - updateStarted) > longFrame) {
      this.suppressFrameskip = true;
    }
    
    this._updateCount += 1;
  });

  $.RawMethod(false, "_ReportFPS", function Game_ReportFPS (now) {
    this._lastSecond = now;
    
    if (typeof (JSIL.Host.reportFps) === "function") {
      var isWebGL = this.graphicsDeviceService.GraphicsDevice.context.isWebGL || false;
      var cacheBytes = ($jsilxna.imageChannelCache.countBytes + $jsilxna.textCache.countBytes);

      JSIL.Host.reportFps(
        this._drawCount, this._updateCount, 
        cacheBytes, isWebGL
      );
    }

    this._updateCount = this._drawCount = 0;
  });

  $.RawMethod(false, "_FixedTimeStep", function Game_FixedTimeStep (
    elapsed, frameDelay, millisecondInTicks, maxElapsedTimeMs, longFrame
  ) {
    this._gameTime.elapsedGameTime._ticks = (frameDelay * millisecondInTicks);

    elapsed += this._extraTime;
    this._extraTime = 0;

    if (elapsed > maxElapsedTimeMs) 
      elapsed = maxElapsedTimeMs;

    var numFrames = Math.floor(elapsed / frameDelay);
    if (numFrames < 1) {
      numFrames = 1;
      this._extraTime = elapsed - frameDelay;
    } else {
      this._extraTime = elapsed - (numFrames * frameDelay);
    }

    for (var i = 0; i < numFrames; i++) {
      this._gameTime.totalGameTime._ticks += (frameDelay * millisecondInTicks);

      this._TimedUpdate(longFrame);
    }
  });

  $.RawMethod(false, "_VariableTimeStep", function Game_VariableTimeStep (
    elapsed, frameDelay, millisecondInTicks, maxElapsedTimeMs, longFrame
  ) {
    this._extraTime = 0;
    this.suppressFrameskip = false;

    if (elapsed > maxElapsedTimeMs)
      elapsed = maxElapsedTimeMs;

    this._gameTime.elapsedGameTime._ticks = (elapsed * millisecondInTicks);
    this._gameTime.totalGameTime._ticks += (elapsed * millisecondInTicks);

    this._TimedUpdate(longFrame);
  });

  $.RawMethod(false, "_RenderAFrame", function Game_RenderAFrame () {
    var device = this.get_GraphicsDevice();

    device.$UpdateViewport();      
    device.$Clear();

    this.Draw(this._gameTime);

    this._drawCount += 1;
  });

  $.RawMethod(false, "_Step", function Game_Step () {
    var now = this._GetNow();

    var frameDelay = this.targetElapsedTime.get_TotalMilliseconds();

    if (this._lastFrame === 0) {
      var elapsed = frameDelay;
      var total = 0;
      this._started = now;
      this._lastSecond = now;
      this._updateCount = this._drawCount = 0;
      this._extraTime = 0;
      this.suppressFrameskip = true;
    } else {
      var elapsed = now - this._lastFrame;
      var total = now - this._started;
    }

    if ((now - this._lastSecond) > 1000) {
      this._ReportFPS(now);

      $jsilxna.imageChannelCache.maybeEvictItems();
      $jsilxna.textCache.maybeEvictItems();
    }

    if (this.forceElapsedTimeToZero) {
      this.forceElapsedTimeToZero = false;
      this._extraTime = 0;
      elapsed = 0;
    }

    this._lastFrame = now;
    this._nextFrame = now + frameDelay;

    var millisecondInTicks = 10000;
    var maxElapsedTimeMs = frameDelay * 4;
    var longFrame = frameDelay * 3;

    this._profilingMode = (document.location.search.indexOf("profile") >= 0);
    if (this._balanceFPSCheckbox)
      this._profilingMode = !this._balanceFPSCheckbox.checked;

    var failed = true;
    try {

      if (this.isFixedTimeStep && !this.suppressFrameskip && !this._profilingMode) {
        this._FixedTimeStep(elapsed, frameDelay, millisecondInTicks, maxElapsedTimeMs, longFrame);
      } else {
        this._VariableTimeStep(elapsed, frameDelay, millisecondInTicks, maxElapsedTimeMs, longFrame);
      }

      this._RenderAFrame();

      failed = false;
    } finally {
      if (failed || Microsoft.Xna.Framework.Game._QuitForced) 
        this.Exit();
      else 
        this._QueueStep();
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
    if (this._runHandle !== null) 
      window.clearInterval(this._runHandle);

    this._runHandle = null;
    this.UnloadContent();

    this._isDead = true;

    try {
      var canvas = JSIL.Host.getCanvas();      
      var ctx = canvas.getContext("2d") || canvas.getContext("webgl-2d");
      ctx.setTransform(1, 0, 0, 1, 0, 0);
      ctx.globalAlpha = 1;
      ctx.globalCompositeOperation = "source-over";
      ctx.fillStyle = "black";
      ctx.fillRect(0, 0, 99999, 99999);

      var fsb = document.getElementById("fullscreenButton");
      if (fsb)
        fsb.style.display = "none";

      var stats = document.getElementById("stats");
      if (stats)
        stats.style.display = "none";
    } catch (exc) {
    }
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GameComponent", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[1].TypeRef("Microsoft.Xna.Framework.Game")], [])), 
    function _ctor (game) {
      this.enabled = true;
      this.initialized = false;
      this.game = game;
    }
  );

  $.Method({Static:false, Public:true }, "get_UpdateOrder", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_UpdateOrder () {
      return 0;
    }
  );

  $.Method({Static:false, Public:true }, "get_Enabled", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_Enabled () {
      return this.enabled;
    }
  );

  $.Method({Static:false, Public:true }, "get_Game", 
    (new JSIL.MethodSignature($xnaasms[1].TypeRef("Microsoft.Xna.Framework.Game"), [], [])), 
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
    (new JSIL.MethodSignature(null, [$xnaasms[1].TypeRef("Microsoft.Xna.Framework.GameTime")], [])), 
    function Update (gameTime) {
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.DrawableGameComponent", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[1].TypeRef("Microsoft.Xna.Framework.Game")], [])), 
    function _ctor (game) {
      Microsoft.Xna.Framework.GameComponent.prototype._ctor.call(this, game);

      this.visible = true;
    }
  );  

  $.Method({Static:false, Public:true }, "Draw", 
    (new JSIL.MethodSignature(null, [$xnaasms[1].TypeRef("Microsoft.Xna.Framework.GameTime")], [])), 
    function Draw (gameTime) {
    }
  );

  $.Method({Static:false, Public:true }, "get_DrawOrder", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_DrawOrder () {
      return 0;
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
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.KeyboardState"), [], [])), 
    getStateImpl
  );

  $.Method({Static:true , Public:true }, "GetState", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.KeyboardState"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex")], [])), 
    getStateImpl
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.KeyboardState", function ($) {
  $.RawMethod(false, "__CopyMembers__", function (source, target) {
    if (source.keys)
      target.keys = Array.prototype.slice.call(source.keys);
    else
      target.keys = [];
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.Keys")])], [])), 
    function _ctor (keys) {
      this.keys = [];

      for (var i = 0; i < keys.length; i++)
        this.keys.push(keys[i].valueOf());
    }
  );

  $.Method({Static:false, Public:true }, "GetPressedKeys", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.Keys")]), [], [])), 
    function GetPressedKeys () {
      if (!this.keys)
        return [];

      var result = [];
      var tKeys = $xnaasms[0].Microsoft.Xna.Framework.Input.Keys.__Type__;

      for (var i = 0, l = this.keys.length; i < l; i++)
        result.push(tKeys.$Cast(this.keys[i]));

      return result;
    }
  );

  $.Method({Static:false, Public:true }, "IsKeyDown", 
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.Keys")], [])), 
    function IsKeyDown (key) {
      if (!this.keys)
        return false;

      return this.keys.indexOf(key.valueOf()) !== -1;
    }
  );

  $.Method({Static:false, Public:true }, "IsKeyUp", 
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.Keys")], [])), 
    function IsKeyUp (key) {
      if (!this.keys)
        return true;

      return this.keys.indexOf(key.valueOf()) === -1;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.Mouse", function ($) {
  var getStateImpl = function (playerIndex) {
    var buttons = JSIL.Host.getHeldButtons();
    var position = JSIL.Host.getMousePosition();

    var pressed = $xnaasms[0].Microsoft.Xna.Framework.Input.ButtonState.Pressed;
    var released = $xnaasms[0].Microsoft.Xna.Framework.Input.ButtonState.Released;

    // FIXME
    return new Microsoft.Xna.Framework.Input.MouseState(
      position[0], position[1], 0,
      (buttons.indexOf(0) >= 0) ? pressed : released,
      (buttons.indexOf(1) >= 0) ? pressed : released,
      (buttons.indexOf(2) >= 0) ? pressed : released,
      released, released
    );
  };

  $.Method({Static:true , Public:true }, "GetState", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.MouseState"), [], [])), 
    getStateImpl
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.MouseState", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $.Int32, $.Int32, 
          $.Int32, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState")
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
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
    function get_LeftButton () {
      return this.leftButton;
    }
  );

  $.Method({Static:false, Public:true }, "get_MiddleButton", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
    function get_MiddleButton () {
      return this.middleButton;
    }
  );

  $.Method({Static:false, Public:true }, "get_RightButton", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
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

$jsilxna.deadZone = function (value, max, deadZoneSize) {
  if (value < -deadZoneSize)
    value += deadZoneSize;
  else if (value <= deadZoneSize)
    return 0;
  else
    value -= deadZoneSize;

  var scaled = value / (max - deadZoneSize);
  if (scaled < -1)
    scaled = -1;
  else if (scaled > 1)
    scaled = 1;

  return scaled;
};

$jsilxna.deadZoneToPressed = function (value, max, deadZoneSize) {
  var pressed = $xnaasms[0].Microsoft.Xna.Framework.Input.ButtonState.Pressed;
  var released = $xnaasms[0].Microsoft.Xna.Framework.Input.ButtonState.Released;
  
  var scaled = $jsilxna.deadZone(value, max, deadZoneSize);
  if (Math.abs(scaled) > 0)
    return pressed;
  else
    return released;
};

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePad", function ($) {
  var buttons = $xnaasms[0].Microsoft.Xna.Framework.Input.Buttons;
  var pressed = $xnaasms[0].Microsoft.Xna.Framework.Input.ButtonState.Pressed;
  var released = $xnaasms[0].Microsoft.Xna.Framework.Input.ButtonState.Released;

  var buttonsFromGamepadState = function (state) {
    var buttonStates = 0;

    if (state.faceButton0)
      buttonStates |= buttons.A;
    if (state.faceButton1)
      buttonStates |= buttons.B;
    if (state.faceButton2)
      buttonStates |= buttons.X;
    if (state.faceButton3)
      buttonStates |= buttons.Y;

    if (state.leftShoulder0)
      buttonStates |= buttons.LeftShoulder;
    if (state.rightShoulder0)
      buttonStates |= buttons.RightShoulder;

    if (state.select)
      buttonStates |= buttons.Back;
    if (state.start)
      buttonStates |= buttons.Start;

    if (state.leftStickButton)
      buttonStates |= buttons.LeftStick;
    if (state.rightStickButton)
      buttonStates |= buttons.RightStick;

    var result = buttons.$Cast(buttonStates);
    return result;
  };

  var getRawStateForPlayerIndex = function (playerIndex) {
    if (window && window.Gamepad && window.Gamepad.supported) {
      var states = window.Gamepad.getStates();
      if (states) {
        var state = states[playerIndex.valueOf()];
        if (state) {
          return state;
        }
      }
    }

    return null;
  }

  var getStateImpl = function (playerIndex) {
    var connected = false;
    var buttonStates = 0;
    var leftThumbstick = new Microsoft.Xna.Framework.Vector2(0, 0);
    var rightThumbstick = new Microsoft.Xna.Framework.Vector2(0, 0);
    var leftTrigger = 0, rightTrigger = 0;
    var dpadUp = released, dpadDown = released, dpadLeft = released, dpadRight = released;

    var state = getRawStateForPlayerIndex(playerIndex);

    if (state) {
      connected = true;

      buttonStates = buttonsFromGamepadState(state);

      // FIXME: This is IndependentAxes mode. Maybe handle Circular too?
      var leftStickDeadZone = 7849 / 32767;
      var rightStickDeadZone = 8689 / 32767;

      leftThumbstick.X  = $jsilxna.deadZone(state.leftStickX, 1, leftStickDeadZone);
      rightThumbstick.X = $jsilxna.deadZone(state.rightStickX, 1, rightStickDeadZone);

      // gamepad.js returns inverted Y compared to XInput... weird.
      leftThumbstick.Y  = -$jsilxna.deadZone(state.leftStickY, 1, leftStickDeadZone);
      rightThumbstick.Y = -$jsilxna.deadZone(state.rightStickY, 1, rightStickDeadZone);

      leftTrigger  = state.leftShoulder1;
      rightTrigger = state.rightShoulder1;

      dpadUp    = state.dpadUp    ? pressed : released;
      dpadDown  = state.dpadDown  ? pressed : released;
      dpadLeft  = state.dpadLeft  ? pressed : released;
      dpadRight = state.dpadRight ? pressed : released;          
    }

    var buttons = new Microsoft.Xna.Framework.Input.GamePadButtons(
      buttonStates
    );

    var thumbs = new Microsoft.Xna.Framework.Input.GamePadThumbSticks(
      leftThumbstick, rightThumbstick
    );

    var triggers = new Microsoft.Xna.Framework.Input.GamePadTriggers(
      leftTrigger, rightTrigger
    );

    var dpad = new Microsoft.Xna.Framework.Input.GamePadDPad(
      dpadUp, dpadDown, dpadLeft, dpadRight
    );

    var result = JSIL.CreateInstanceOfType(
      Microsoft.Xna.Framework.Input.GamePadState.__Type__,
      "$internalCtor",
      [connected, thumbs, triggers, buttons, dpad]
    );
    return result;
  };

  $.Method({Static:true , Public:true }, "GetState", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadState"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex")], [])), 
    getStateImpl
  );

  $.Method({Static:true , Public:true }, "GetState", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadState"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadDeadZone")], [])), 
    getStateImpl
  );

  $.Method({Static:true , Public:true }, "SetVibration", 
    (new JSIL.MethodSignature($.Boolean, [
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex"), $.Single, 
          $.Single
        ], [])), 
    function SetVibration (playerIndex, leftMotor, rightMotor) {
      // FIXME
    }
  );

  $.Method({Static:true , Public:true }, "GetCapabilities", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadCapabilities"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex")], [])), 
    function GetCapabilities (playerIndex) {
      var state = getRawStateForPlayerIndex(playerIndex);

      var result = JSIL.CreateInstanceOfType(
        Microsoft.Xna.Framework.Input.GamePadCapabilities.__Type__,
        "$internalCtor",
        [Boolean(state)]
      );
      return result;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePadCapabilities", function ($) {
  
  $.RawMethod(false, "$internalCtor", function (connected) {
    this._connected = connected;
  });

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.XINPUT_CAPABILITIES")]), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.ErrorCodes")], [])), 
    function _ctor (/* ref */ caps, result) {
      this._connected = false;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsConnected", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsConnected () {
      return this._connected;
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePadState", function ($) {
  var buttons = $xnaasms[0].Microsoft.Xna.Framework.Input.Buttons;
  var pressed = $xnaasms[0].Microsoft.Xna.Framework.Input.ButtonState.Pressed;
  var released = $xnaasms[0].Microsoft.Xna.Framework.Input.ButtonState.Released;

  $.RawMethod(false, "$internalCtor", function GamePadState_internalCtor (connected, thumbSticks, triggers, buttons, dPad) {
    this._connected = connected;
    this._thumbs = thumbSticks;
    this._buttons = buttons;
    this._triggers = triggers;
    this._dpad = dPad;
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadThumbSticks"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadTriggers"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadButtons"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadDPad")
        ], [])), 
    function _ctor (thumbSticks, triggers, buttons, dPad) {
      this.$internalCtor(false, thumbSticks, triggers, buttons, dPad);
    }
  );

  $.Method({Static:false, Public:true }, "get_Buttons", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadButtons"), [], [])), 
    function get_Buttons () {
      return this._buttons;
    }
  );

  $.Method({Static:false, Public:true }, "get_DPad", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadDPad"), [], [])), 
    function get_DPad () {
      return this._dpad;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsConnected", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsConnected () {
      // FIXME
      return this._connected;
    }
  );

  $.Method({Static:false, Public:true }, "get_ThumbSticks", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadThumbSticks"), [], [])), 
    function get_ThumbSticks () {
      return this._thumbs;
    }
  );

  $.Method({Static:false, Public:true }, "get_Triggers", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.GamePadTriggers"), [], [])), 
    function get_Triggers () {
      return this._triggers;
    }
  );

  var getButtonState = function (self, button) {
    var s = self._buttons._state;
    var key = button.valueOf();

    if (s && s[key])
      return s[key].valueOf();

    var triggerDeadZone = 30 / 255;

    switch (key) {
      // DPad
      case 1:
        return self._dpad._up;
      case 2:
        return self._dpad._down;
      case 4:
        return self._dpad._left;
      case 8:
        return self._dpad._right;

      // Triggers
      case 8388608:
        return $jsilxna.deadZoneToPressed(self._triggers._left, 1, triggerDeadZone);
      case 4194304:
        return $jsilxna.deadZoneToPressed(self._triggers._right, 1, triggerDeadZone);
    }

    return released;
  };

  $.Method({Static:false, Public:true }, "IsButtonDown", 
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.Buttons")], [])), 
    function IsButtonDown (button) {
      return (getButtonState(this, button).valueOf() !== 0);
    }
  );

  $.Method({Static:false, Public:true }, "IsButtonUp", 
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.Buttons")], [])), 
    function IsButtonUp (button) {
      return (getButtonState(this, button).valueOf() === 0);
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePadButtons", function ($) {
  var buttonNames = [
    "A", "B", "Back", "BigButton", 
    "LeftShoulder", "LeftStick", "RightShoulder", "RightStick",
    "Start", "X", "Y"
  ];

  var buttons = $xnaasms[0].Microsoft.Xna.Framework.Input.Buttons;
  var pressed = $xnaasms[0].Microsoft.Xna.Framework.Input.ButtonState.Pressed;
  var released = $xnaasms[0].Microsoft.Xna.Framework.Input.ButtonState.Released;

  var makeButtonGetter = function (buttonName) {
    return function getButtonState () {
      var key = buttons[buttonName].valueOf();
      return this._state[key] || released;
    };
  }

  for (var i = 0; i < buttonNames.length; i++) {
    var buttonName = buttonNames[i];

    $.Method({Static:false, Public:true }, "get_" + buttonName, 
      (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
      makeButtonGetter(buttonName)
    );
  }

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.Buttons")], [])), 
    function _ctor (buttonState) {
      this._state = {};

      buttonState = buttonState.valueOf();

      for (var i = 0; i < buttonNames.length; i++) {
        var buttonName = buttonNames[i];
        var buttonMask = buttons[buttonName].valueOf();

        this._state[buttonMask] = (buttonState & buttonMask) ? pressed : released;
      }
    }
  );

  $.RawMethod(false, "__CopyMembers__", 
    function GamePadButtons_CopyMembers (source, target) {
      target._state = {};

      for (var k in source._state) {
        if (!source._state.hasOwnProperty(k))
          continue;

        target._state[k] = source._state[k];
      }
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePadThumbSticks", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2")], [])), 
    function _ctor (leftThumbstick, rightThumbstick) {
      this._left = leftThumbstick;
      this._right = rightThumbstick;
    }
  );

  $.Method({Static:false, Public:true }, "get_Left", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [], [])), 
    function get_Left () {
      return this._left;
    }
  );

  $.Method({Static:false, Public:true }, "get_Right", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [], [])), 
    function get_Right () {
      return this._right;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePadDPad", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState")
        ], [])), 
    function _ctor (upValue, downValue, leftValue, rightValue) {
      this._up = upValue;
      this._down = downValue;
      this._left = leftValue;
      this._right = rightValue;
    }
  );

  $.Method({Static:false, Public:true }, "get_Down", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
    function get_Down () {
      return this._down;
    }
  );

  $.Method({Static:false, Public:true }, "get_Left", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
    function get_Left () {
      return this._left;
    }
  );

  $.Method({Static:false, Public:true }, "get_Right", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
    function get_Right () {
      return this._right;
    }
  );

  $.Method({Static:false, Public:true }, "get_Up", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Input.ButtonState"), [], [])), 
    function get_Up () {
      return this._up;
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Input.GamePadTriggers", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Single, $.Single], [])), 
    function _ctor (leftTrigger, rightTrigger) {
      this._left = leftTrigger;
      this._right = rightTrigger;
    }
  );

  $.Method({Static:false, Public:true }, "get_Left", 
    (new JSIL.MethodSignature($.Single, [], [])), 
    function get_Left () {
      return this._left;
    }
  );

  $.Method({Static:false, Public:true }, "get_Right", 
    (new JSIL.MethodSignature($.Single, [], [])), 
    function get_Right () {
      return this._right;
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GraphicsDeviceManager", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms.xnaGame.TypeRef("Microsoft.Xna.Framework.Game")], [])), 
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

  $.Method({
    Static: false,
    Public: true
  }, "get_IsFullScreen", new JSIL.MethodSignature($.Boolean, [], []), function (value) {
    return true;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_IsFullScreen", new JSIL.MethodSignature(null, [$.Boolean], []), function (value) {
    // FIXME
  });

  $.Method({
    Static: false,
    Public: true
  }, "ApplyChanges", new JSIL.MethodSignature(null, [], []), function () {
    var oc = this.device.originalCanvas;

    $jsilbrowserstate.nativeWidth = this.device.originalWidth = oc.width = this._width;
    $jsilbrowserstate.nativeHeight = this.device.originalHeight = oc.height = this._height;
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

  $.Method({Static:false, Public:true }, "get_Bounds", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [], [])), 
    function get_Bounds () {
      return new Microsoft.Xna.Framework.Rectangle(this._x, this._y, this._width, this._height);
    }
  );

  $.Method({Static:false, Public:true }, "get_TitleSafeArea", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [], [])), 
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
          $xnaasms[5].TypeRef("System.TimeSpan"), $xnaasms[5].TypeRef("System.TimeSpan"), 
          $.Boolean
        ], [])), 
    function _ctor (totalGameTime, elapsedGameTime, isRunningSlowly) {
      this.totalGameTime = totalGameTime;
      this.elapsedGameTime = elapsedGameTime;
      this.isRunningSlowly = isRunningSlowly;
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[5].TypeRef("System.TimeSpan"), $xnaasms[5].TypeRef("System.TimeSpan")], [])), 
    function _ctor (totalGameTime, elapsedGameTime) {
      this.totalGameTime = totalGameTime;
      this.elapsedGameTime = elapsedGameTime;
      this.isRunningSlowly = false;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsRunningSlowly", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsRunningSlowly () {
      return this.isRunningSlowly;
    }
  );

  $.Method({Static:false, Public:true }, "get_TotalGameTime", 
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.TimeSpan"), [], [])), 
    function get_TotalGameTime () {
      return this.totalGameTime;
    }
  );

  $.Method({Static:false, Public:true }, "get_ElapsedGameTime", 
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.TimeSpan"), [], [])), 
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
    new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [], []), 
    function () {
      return Microsoft.Xna.Framework.Rectangle._empty;
    }
  );

  $.Method({Static:true , Public:true }, "op_Equality", 
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
    function op_Equality (lhs, rhs) {
      return lhs.X === rhs.X && lhs.Y === rhs.Y && lhs.Width === rhs.Width && lhs.Height === rhs.Height;
    }
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
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
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
    intersectImpl
  );

  $.Method({Static:true , Public:true }, "Intersect", 
    (new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), 
          $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")])
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
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
    unionImpl
  );

  $.Method({Static:true , Public:true }, "Union", 
    (new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), 
          $jsilcore.TypeRef("JSIL.Reference", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")])
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
    (new JSIL.MethodSignature($.Boolean, [$.Int32, $.Int32], [])), 
    function Contains (x, y) {
      return this.X <= x && 
        x < this.X + this.Width && 
        this.Y <= y && 
        y < this.Y + this.Height;
    }
  );
  
  $.Method({Static:false, Public:true }, "ContainsPoint", 
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    function ContainsPoint (value) {
      return this.X <= value.X && 
        value.X < this.X + this.Width && 
        this.Y <= value.Y && 
        value.Y < this.Y + this.Height;
    }
  );

  $.Method({Static:false, Public:true }, "ContainsRectangle", 
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
    function ContainsRectangle (value) {
      return this.X <= value.X && 
        value.X + value.Width <= this.X + this.Width && 
        this.Y <= value.Y && 
        value.Y + value.Height <= this.Y + this.Height;
    }
  );

  $.Method({Static:false, Public:true }, "get_Bottom", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Bottom () {
      return this.Y + this.Height;
    }
  );

  $.Method({Static:false, Public:true }, "get_Center", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point"), [], [])), 
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
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point"), [], [])), 
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
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")], [])), 
    function Intersects (value) {
      return value.X < this.X + this.Width && 
              this.X < value.X + value.Width && 
              value.Y < this.Y + this.Height && 
              this.Y < value.Y + value.Height;
    }
  );

  $.Method({Static:false, Public:true }, "Offset", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
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
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    function set_Location (value) {
      this.X = value.X;
      this.Y = value.Y;

      return value;
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
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point"), [], [])), 
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
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    equalsImpl
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    function op_Inequality (a, b) {
      return !equalsImpl(a, b);
    }
  );

  $.Method({Static:false, Public:true }, "Object.Equals", 
    (new JSIL.MethodSignature($.Boolean, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Point")], [])), 
    function Equals (other) {
      return equalsImpl(this, other);
    }
  );
});

$jsilxna.makeColor = function (proto, r, g, b, a) {
  var result = Object.create(proto);
  result.r = r;
  result.g = g;
  result.b = b;

  if (typeof (a) === "number") 
    result.a = a;
  else 
    result.a = 255;

  return result;
};

$jsilxna.Color = function ($) {
  (function BindColorExternals () {
    var makeColor = $jsilxna.makeColor;
    var colors = $jsilxna.colors || [];

    var tColor = null;

    var makeLazyColor = function (r, g, b, a) {
      var state = null;
      return function () {
        if (state === null) {
          if (tColor === null) {
            var typeName1 = JSIL.ParseTypeName("Microsoft.Xna.Framework.Color,Microsoft.Xna.Framework");
            var typeName2 = JSIL.ParseTypeName("Microsoft.Xna.Framework.Graphics.Color,Microsoft.Xna.Framework");

            tColor = JSIL.GetTypeInternal(typeName1, $jsilxna, false) || JSIL.GetTypeInternal(typeName2, $jsilxna, false);
          }

          state = JSIL.CreateInstanceOfType(tColor, null);
          state.a = a;
          state.r = r;
          state.g = g;
          state.b = b;
        }

        return state;
      };
    };

    for (var i = 0, l = colors.length; i < l; i++) {
      var colorName = colors[i][0];

      $.RawMethod(
        true, "get_" + colorName, makeLazyColor(colors[i][1], colors[i][2], colors[i][3], colors[i][4])
      );
    }
  }) ();

  $.RawMethod(false, "__CopyMembers__", function Color_CopyMembers (source, target) {
    target.a = source.a;
    target.r = source.r;
    target.g = source.g;
    target.b = source.b;
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
      var bound = bindColor(color);

      Object.defineProperty(publicInterface, "get_" + colorName, {
        value: bound,
        enumerable: true,
        configurable: true,
        writable: false
      });

      Object.defineProperty(publicInterface, colorName, {
        value: color,
        enumerable: true,
        configurable: true,
        writable: false
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
  }, ".ctor", new JSIL.MethodSignature(null, [$xnaasms.xna.TypeRef("Microsoft.Xna.Framework.Vector3")], []), function (v) {
    ctorRgbaFloat(this, v.X, x.Y, v.Z, 1.0);
  });

  $.Method({
    Static: false,
    Public: true
  }, ".ctor", new JSIL.MethodSignature(null, [$xnaasms.xna.TypeRef("Microsoft.Xna.Framework.Vector4")], []), function (v) {
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

  $.Method({Static:false, Public:true }, "set_PackedValue", 
    (new JSIL.MethodSignature(null, [$.UInt32], [])), 
    function set_PackedValue (value) {
      this._cachedCss = null;
      this.a = (value >> 24) & 0xFF;
      this.b = (value >> 16) & 0xFF;
      this.g = (value >> 8) & 0xFF;
      this.r = value & 0xFF;
    }
  );

  $.Method({
    Static: false,
    Public: true
  }, "set_A", new JSIL.MethodSignature(null, [$.Byte], []), function (value) {
    this.a = $jsilxna.ClampByte(value);
    this._cachedCss = null;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_B", new JSIL.MethodSignature(null, [$.Byte], []), function (value) {
    this.b = $jsilxna.ClampByte(value);
    this._cachedCss = null;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_G", new JSIL.MethodSignature(null, [$.Byte], []), function (value) {
    this.g = $jsilxna.ClampByte(value);
    this._cachedCss = null;
  });

  $.Method({
    Static: false,
    Public: true
  }, "set_R", new JSIL.MethodSignature(null, [$.Byte], []), function (value) {
    this.r = $jsilxna.ClampByte(value);
    this._cachedCss = null;
  });

  var equalsImpl = function (lhs, rhs) {
    return (lhs.r === rhs.r) && (lhs.g === rhs.g) &&
      (lhs.b === rhs.b) && (lhs.a === rhs.a);
  };

  $.Method({Static:true , Public:true }, "op_Equality", 
    (new JSIL.MethodSignature($.Boolean, [$jsilxna.colorRef(), $jsilxna.colorRef()], [])), 
    function op_Equality (a, b) {
      return equalsImpl(a, b);
    }
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    (new JSIL.MethodSignature($.Boolean, [$jsilxna.colorRef(), $jsilxna.colorRef()], [])), 
    function op_Inequality (a, b) {
      return !equalsImpl(a, b);
    }
  );

  $.Method({
    Static: true,
    Public: true
  }, "op_Multiply", new JSIL.MethodSignature($jsilxna.colorRef(), [$jsilxna.colorRef(), $.Single], []), function (color, multiplier) {
    var result = Object.create(Object.getPrototypeOf(color));
    result.a = $jsilxna.ClampByte(color.a * multiplier);
    result.r = $jsilxna.ClampByte(color.r * multiplier);
    result.g = $jsilxna.ClampByte(color.g * multiplier);
    result.b = $jsilxna.ClampByte(color.b * multiplier);
    return result;
  });

  $.Method({
    Static: true,
    Public: true
  }, "Lerp", new JSIL.MethodSignature($jsilxna.colorRef(), [$jsilxna.colorRef(), $jsilxna.colorRef(), $.Single], []), function (a, b, amount) {
    var result = Object.create(Object.getPrototypeOf(a));
    result.a = $jsilxna.ClampByte(a.a + (b.a - a.a) * amount);
    result.r = $jsilxna.ClampByte(a.r + (b.r - a.r) * amount);
    result.g = $jsilxna.ClampByte(a.g + (b.g - a.g) * amount);
    result.b = $jsilxna.ClampByte(a.b + (b.b - a.b) * amount);
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

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.SpriteBatch", function ($) {
  if (false) {
    var $canvasDrawImage = function canvasDrawImage (image, sourceX, sourceY, sourceW, sourceH, positionX, positionY, destW, destH) {
      try {
        this.device.context.drawImage(
          image, sourceX, sourceY, sourceW, sourceH, positionX, positionY, destW, destH
        );
      } catch (exc) {
        console.log("Error calling drawImage with arguments ", Array.prototype.slice.call(arguments), ": ", exc);
      }
    }
  } else {
    var $canvasDrawImage = function canvasDrawImage (image, sourceX, sourceY, sourceW, sourceH, positionX, positionY, destW, destH) {
      this.device.context.drawImage(
        image, sourceX, sourceY, sourceW, sourceH, positionX, positionY, destW, destH
      );
    }
  }

  $.RawMethod(false, "$canvasDrawImage", $canvasDrawImage);

  $.RawMethod(false, "$save", function canvasSave () {
    this.saveCount += 1;
    this.device.context.save();
  });

  $.RawMethod(false, "$restore", function canvasRestore () {
    this.restoreCount += 1;
    this.device.context.restore();
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice")], [])), 
    function _ctor (graphicsDevice) {
      this.device = graphicsDevice;
      this.defer = false;
      this.deferSorter = null;

      this.deferredPoolSize = 1024;

      this.deferredDrawPool = [];
      this.deferredDrawStringPool = [];

      this.deferredDraws = [];

      this.oldBlendState = null;
      this.isWebGL = false;
      this.spriteEffects = Microsoft.Xna.Framework.Graphics.SpriteEffects;
      this.flipHorizontally = this.spriteEffects.FlipHorizontally.value;
      this.flipVertically = this.spriteEffects.FlipVertically.value;
    }
  );

  $.RawMethod(false, "$cloneExisting", function (spriteBatch) {
    this.device = spriteBatch.device;
    this.defer = false;
    this.deferSorter = null;
    this.oldBlendState = null;
    this.isWebGL = spriteBatch.isWebGL;
    this.spriteEffects = spriteBatch.spriteEffects;
    this.flipHorizontally = spriteBatch.flipHorizontally;
    this.flipVertically = spriteBatch.flipVertically;
  });

  $.RawMethod(false, "$applyBlendState", function () {
    if ((typeof (this.blendState) === "object") && (this.blendState !== null))
      this.device.set_BlendState(this.blendState);
    else
      this.device.set_BlendState(Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend);
  });

  $.RawMethod(false, "$applySamplerState", function () {
    if ((typeof (this.samplerState) === "object") && (this.samplerState !== null))
      this.device.SamplerStates.set_Item(0, this.samplerState);
    else
      this.device.SamplerStates.set_Item(0, Microsoft.Xna.Framework.Graphics.SamplerState.LinearClamp);
  });

  $.RawMethod(false, "$updateMatrices", function () {
    var viewport = this.device.get_Viewport();
    var xTranslation = 0, yTranslation = 0;      
    var xScale = 1, yScale = 1;

    if ((typeof (this.transformMatrix) === "object") && (this.transformMatrix !== null)) {
      xTranslation += this.transformMatrix.xTranslation;
      yTranslation += this.transformMatrix.yTranslation;
      xScale *= this.transformMatrix.xScale;
      yScale *= this.transformMatrix.yScale;
    }

    this.device.$UpdateViewport();
    this.device.context.translate(xTranslation, yTranslation);
    this.device.context.scale(xScale, yScale);
  });

  $.Method({Static:false, Public:true }, "Begin", 
    (new JSIL.MethodSignature(null, [$xnaasms[5].TypeRef("System.Array") /* AnyType[] */ ], [])), 
    function SpriteBatch_Begin (sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix) {
      this.saveCount = 0;
      this.restoreCount = 0;

      $jsilxna.imageChannelCache.now = Date.now();
      $jsilxna.textCache.now = Date.now();

      this.isWebGL = this.device.context.isWebGL || false;

      this.$save();
      this.deferSorter = null;

      this.blendState = blendState;
      this.samplerState = samplerState;

      var textureIndex = 0;

      if (sortMode === Microsoft.Xna.Framework.Graphics.SpriteSortMode.Immediate) {
        this.defer = false;
        this.$applyBlendState();
        this.$applySamplerState();
      } else if (sortMode === Microsoft.Xna.Framework.Graphics.SpriteSortMode.BackToFront) {
        this.defer = true;
        this.deferSorter = function Sort_BackToFront (lhs, rhs) {
          var result = rhs.depth - lhs.depth;
          if (result === 0)
            result = rhs.index - lhs.index;

          return result;
        };
      } else if (sortMode === Microsoft.Xna.Framework.Graphics.SpriteSortMode.FrontToBack) {
        this.defer = true;
        this.deferSorter = function Sort_FrontToBack (lhs, rhs) {
          var result = lhs.depth - rhs.depth;
          if (result === 0)
            result = rhs.index - lhs.index;

          return result;
        };
      } else if (sortMode === Microsoft.Xna.Framework.Graphics.SpriteSortMode.Texture) {
        this.defer = true;
        this.deferSorter = function Sort_Texture (lhs, rhs) {
          var result = JSIL.CompareValues(lhs.texture.id, rhs.texture.id);
          if (result === 0)
            result = JSIL.CompareValues(lhs.index, rhs.index);

          return result;
        };
      } else if (sortMode === Microsoft.Xna.Framework.Graphics.SpriteSortMode.Deferred) {
        this.defer = true;
      }

      this.transformMatrix = transformMatrix;
      this.$updateMatrices();
    }
  );

  $.Method({Static:false, Public:true }, "End", 
    (new JSIL.MethodSignature(null, [], [])), 
    function SpriteBatch_End () {
      if (this.defer) {
        this.defer = false;

        this.$applyBlendState();
        this.$applySamplerState();
        this.$updateMatrices();

        if (this.deferSorter !== null) 
          this.deferredDraws.sort(this.deferSorter);

        for (var i = 0, l = this.deferredDraws.length; i < l; i++) {
          var draw = this.deferredDraws[i];
          draw.function.apply(this, draw.arguments);

          // FIXME: Leaks references to textures, fonts, and colors.
          if (draw.pool.length < this.deferredPoolSize)
            draw.pool.push(draw);
        }
      }

      this.deferredDraws.length = 0;

      this.$restore();

      this.$applyBlendState();
      this.$applySamplerState();

      this.device.$UpdateViewport();

      if (this.saveCount !== this.restoreCount)
        JSIL.Host.warning("Unbalanced canvas save/restore");
    }
  );

  $.RawMethod(false, "DeferBlit",
    function SpriteBatch_DeferBlit (
      texture, positionX, positionY, width, height,
      sourceX, sourceY, sourceW, sourceH,
      color, rotation, originX, originY,
      scaleX, scaleY, effects, depth
    ) {
      var entry = null, deferArguments = null;

      var pool = this.deferredDrawPool;
      var dd = this.deferredDraws;

      if (pool.length > 0) {
        entry = pool.pop();

        deferArguments = entry.arguments;
        deferArguments[9].__CopyMembers__(color, deferArguments[9]);
      } else {
        entry = {
          function: null,
          index: 0,
          depth: 0.0,
          texture: null,
          pool: null,
          arguments: new Array(17)
        };

        deferArguments = entry.arguments;
        deferArguments[9] = color.MemberwiseClone();
      }

      entry.function = this.InternalDraw;
      entry.index = dd.length;
      entry.pool = pool;

      entry.depth = depth;
      entry.texture = texture;

      deferArguments[0] = texture;
      deferArguments[1] = positionX;
      deferArguments[2] = positionY;
      deferArguments[3] = width;
      deferArguments[4] = height;
      deferArguments[5] = sourceX;
      deferArguments[6] = sourceY;
      deferArguments[7] = sourceW;
      deferArguments[8] = sourceH;
      // deferArguments[9] = color.MemberwiseClone();
      deferArguments[10] = rotation;
      deferArguments[11] = originX;
      deferArguments[12] = originY;
      deferArguments[13] = scaleX;
      deferArguments[14] = scaleY;
      deferArguments[15] = effects;
      deferArguments[16] = depth;

      dd.push(entry);
    }
  );

  $.RawMethod(false, "DeferDrawString",
    function SpriteBatch_DeferDrawString (
      font, text, 
      positionX, positionY, 
      color, rotation,
      originX, originY,
      scaleX, scaleY, 
      effects, depth
    ) {
      var entry = null, deferArguments = null;

      var pool = this.deferredDrawStringPool;
      var dd = this.deferredDraws;

      if (pool.length > 0) {
        entry = pool.pop();

        deferArguments = entry.arguments;
        deferArguments[4].__CopyMembers__(color, deferArguments[4]);
      } else {
        entry = {
          function: null,
          index: 0,
          depth: 0.0,
          texture: null,
          pool: null,
          arguments: new Array(12)
        };

        deferArguments = entry.arguments;
        deferArguments[4] = color.MemberwiseClone();
      }

      entry.function = this.InternalDrawString;
      entry.index = dd.length;
      entry.pool = pool;

      entry.depth = depth;
      entry.texture = font.texture || null;

      deferArguments[0] = font;
      deferArguments[1] = text;
      deferArguments[2] = positionX;
      deferArguments[3] = positionY;
      // deferArguments[4] = color.MemberwiseClone();
      deferArguments[5] = rotation;
      deferArguments[6] = originX;
      deferArguments[7] = originY;
      deferArguments[8] = scaleX;
      deferArguments[9] = scaleY;
      deferArguments[10] = effects;
      deferArguments[11] = depth;

      dd.push(entry);
    }
  );

  $.Method({Static:false, Public:true }, "Draw", 
    (new JSIL.MethodSignature(null, [
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
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
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $xnaasms[5].TypeRef("System.Nullable`1", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilxna.colorRef()
        ], [])), 
    function Draw (texture, position, sourceRectangle, color) {
      var sourceX = 0, sourceY = 0, sourceWidth = 0, sourceHeight = 0;
      if (sourceRectangle !== null) {
        sourceX = sourceRectangle.X;
        sourceY = sourceRectangle.Y;
        sourceWidth = sourceRectangle.Width;
        sourceHeight = sourceRectangle.Height;
      } else {
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
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $xnaasms[5].TypeRef("System.Nullable`1", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilxna.colorRef(), 
          $.Single, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $.Single, $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), 
          $.Single
        ], [])), 
    function DrawScaleF (texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth) {
      var sourceX = 0, sourceY = 0, sourceWidth = 0, sourceHeight = 0;
      if (sourceRectangle !== null) {
        sourceX = sourceRectangle.X;
        sourceY = sourceRectangle.Y;
        sourceWidth = sourceRectangle.Width;
        sourceHeight = sourceRectangle.Height;
      } else {
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
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $xnaasms[5].TypeRef("System.Nullable`1", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilxna.colorRef(), 
          $.Single, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), 
          $.Single
        ], [])), 
    function Draw (texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth) {
      var sourceX = 0, sourceY = 0, sourceWidth = 0, sourceHeight = 0;
      if (sourceRectangle !== null) {
        sourceX = sourceRectangle.X;
        sourceY = sourceRectangle.Y;
        sourceWidth = sourceRectangle.Width;
        sourceHeight = sourceRectangle.Height;
      } else {
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
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), 
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
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), 
          $xnaasms[5].TypeRef("System.Nullable`1", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilxna.colorRef()
        ], [])), 
    function DrawRect (texture, destinationRectangle, sourceRectangle, color) {
      var sourceX = 0, sourceY = 0, sourceWidth = 0, sourceHeight = 0;
      if (sourceRectangle !== null) {
        sourceX = sourceRectangle.X;
        sourceY = sourceRectangle.Y;
        sourceWidth = sourceRectangle.Width;
        sourceHeight = sourceRectangle.Height;
      } else {
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
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), 
          $xnaasms[5].TypeRef("System.Nullable`1", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $jsilxna.colorRef(), 
          $.Single, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), $.Single
        ], [])), 
    function DrawRect (texture, destinationRectangle, sourceRectangle, color, rotation, origin, effects, layerDepth) {
      var sourceX = 0, sourceY = 0, sourceWidth = 0, sourceHeight = 0;
      if (sourceRectangle !== null) {
        sourceX = sourceRectangle.X;
        sourceY = sourceRectangle.Y;
        sourceWidth = sourceRectangle.Width;
        sourceHeight = sourceRectangle.Height;
      } else {
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
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef()
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
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont"), $xnaasms[5].TypeRef("System.Text.StringBuilder"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef()
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
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef(), 
          $.Single, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
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
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont"), $xnaasms[5].TypeRef("System.Text.StringBuilder"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef(), 
          $.Single, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
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
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef(), 
          $.Single, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), 
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
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont"), $xnaasms[5].TypeRef("System.Text.StringBuilder"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.colorRef(), 
          $.Single, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteEffects"), 
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

  var blitSinglePixel = function (context, originalImage, width, height, colorR, colorG, colorB, colorA) {
    colorR /= 255;
    colorG /= 255;
    colorB /= 255;
    colorA /= 255;

    var topLeftPixelText = $jsilxna.getImageTopLeftPixel(originalImage);
    var topLeftPixel = topLeftPixelText.split(",");

    var unpremultiplyFactor = 1 / colorA;

    var imageColor = "rgba(" + 
      $jsilxna.ClampByte(parseFloat(topLeftPixel[0] * colorR * unpremultiplyFactor)) + ", " + 
      $jsilxna.ClampByte(parseFloat(topLeftPixel[1] * colorG * unpremultiplyFactor)) + ", " + 
      $jsilxna.ClampByte(parseFloat(topLeftPixel[2] * colorB * unpremultiplyFactor)) + ", " + 
      topLeftPixel[3] + 
    ")";

    context.globalAlpha = colorA;
    context.fillStyle = imageColor;
    context.fillRect(
      0, 0, width, height
    );
  };

  var blitChannels = function (
    context, channels, 
    sourceX, sourceY, sourceW, sourceH, 
    width, height, 
    colorR, colorG, colorB, colorA
  ) {
    var alpha = colorA / 255;

    sourceX += channels.xOffset;
    sourceY += channels.yOffset;

    var compositeOperation = context.globalCompositeOperation;
    if (compositeOperation !== "lighter") {
      context.globalCompositeOperation = "source-over";
      context.globalAlpha = alpha;
      context.drawImage(
        channels.a, sourceX, sourceY, sourceW, sourceH, 
        0, 0, width, height
      );
    }

    context.globalCompositeOperation = "lighter";

    if (colorR > 0) {
      context.globalAlpha = colorR / 255;
      context.drawImage(
        channels.r, sourceX, sourceY, sourceW, sourceH, 
        0, 0, width, height
      );
    }

    if (colorG > 0) {
      context.globalAlpha = colorG / 255;
      context.drawImage(
        channels.g, sourceX, sourceY, sourceW, sourceH, 
        0, 0, width, height
      );
    }

    if (colorB > 0) {
      context.globalAlpha = colorB / 255;
      context.drawImage(
        channels.b, sourceX, sourceY, sourceW, sourceH, 
        0, 0, width, height
      );
    }
  };

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
        this.DeferBlit(
          texture, positionX, positionY, width, height,
          sourceX, sourceY, sourceW, sourceH,
          color, rotation, originX, originY,
          scaleX, scaleY, effects, depth
        );

        return;
      }

      var image = texture.image;
      var originalImage = image;
      var context = this.device.context;

      if (sourceX < 0) {
        sourceW += sourceX;
        sourceX = 0;
      }
      if (sourceY < 0) {
        sourceH += sourceY;
        sourceY = 0;
      }

      var maxWidth = texture.Width - sourceX, maxHeight = texture.Height - sourceY;

      if (sourceW > maxWidth) 
        sourceW = maxWidth;
      if (sourceH > maxHeight) 
        sourceH = maxHeight;

      var isSinglePixel = ((sourceW === 1) && (sourceH === 1) && (sourceX === 0) && (sourceY === 0));
      var channels = null;

      var colorA = color.a;
      if (colorA < 1)
        return;

      var colorR = color.r, colorG = color.g, colorB = color.b;

      if (!isSinglePixel && !this.isWebGL) {
        // Since the color is premultiplied, any r/g/b value >= alpha is basically white.
        if ((colorR < colorA) || (colorG < colorA) || (colorB < colorA)) {
          channels = $jsilxna.getImageChannels(image, texture.id);
        }
      }

      this.$save();

      context.translate(positionX, positionY);
      context.rotate(rotation);
      context.scale(scaleX, scaleY);

      // Negative width/height cause an exception in Firefox
      if (width < 0) {
        context.scale(-1, 1);
        width = -width;
      }
      if (height < 0) {
        context.scale(1, -1);
        height = -height;
      }

      context.translate(-originX, -originY);

      if (effects) {
        var e = effects.value;
        
        if (e & this.flipHorizontally) {
          context.translate(width, 0);
          context.scale(-1, 1);
        }

        if (e & this.flipVertically) {
          context.translate(0, height);
          context.scale(1, -1);
        }
      }      

      // 0x0 blits cause an exception in IE
      if (
        (width > 0) && (height > 0) && 
        (sourceW > 0) && (sourceH > 0)
      ) {
        if ($drawDebugRects) {
          context.fillStyle = "rgba(255, 0, 0, 0.33)";
          context.fillRect(
            0, 0, width, height
          );
        }

        if ($drawDebugBoxes) {
          context.strokeStyle = "rgba(255, 255, 0, 0.66)";
          context.strokeRect(
            0, 0, width, height
          );
        }

        if (isSinglePixel) {
          blitSinglePixel(context, originalImage, width, height, colorR, colorG, colorB, colorA);
        } else if (channels !== null) {
          blitChannels(
            context, channels, 
            sourceX, sourceY, sourceW, sourceH, 
            width, height, 
            colorR, colorG, colorB, colorA
          );
        } else if (this.isWebGL) {
          context.drawImage(
            image, sourceX, sourceY, sourceW, sourceH, 
            0, 0, width, height, 
            colorR / 255, colorG / 255, colorB / 255, colorA / 255
          );
        } else {
          if (colorA < 255)
            context.globalAlpha = colorA / 255;

          this.$canvasDrawImage(
            image, sourceX, sourceY, sourceW, sourceH, 0, 0, width, height
          );
        }
      }

      this.$restore();
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
    if (text.length <= 0)
      return;

    if (this.defer) {
      this.DeferDrawString(
        font, text, 
        positionX, positionY, 
        color, rotation,
        originX, originY,
        scaleX, scaleY, 
        effects, depth
      );

      return;
    }

    var asmGraphics = $xnaasms.xnaGraphics || $xnaasms.xna;
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
      if (!needRestore) {
        this.$save();
        needRestore = true;
      }

      this.device.context.scale(-1, 1);
      positionX = -positionX;
    }

    if ((effects & Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically) == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically) {
      if (!needRestore) {
        this.$save();
        needRestore = true;
      }

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
      this.$restore();
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

    this.originalWidth = this.canvas.width;
    this.originalHeight = this.canvas.height;

    this.originalContext = this.context = $jsilxna.get2DContext(this.canvas, true);

    this.viewport = new Microsoft.Xna.Framework.Graphics.Viewport();
    this.viewport.Width = this.canvas.width;
    this.viewport.Height = this.canvas.height;
    this.blendState = Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend;
    this.samplerStates = new Microsoft.Xna.Framework.Graphics.SamplerStateCollection(this, 0, 4);
    this.vertexSamplerStates = new Microsoft.Xna.Framework.Graphics.SamplerStateCollection(this, 0, 4);
    this.textures = new Microsoft.Xna.Framework.Graphics.TextureCollection(this, 0, 4);
    this.vertexTextures = new Microsoft.Xna.Framework.Graphics.TextureCollection(this, 0, 4);
    this.presentationParameters = JSIL.CreateInstanceOfType(
      Microsoft.Xna.Framework.Graphics.PresentationParameters.__Type__, 
      "$internalCtor", [this]
    );

    this.displayMode = new $jsilxna.CurrentDisplayMode(this);

    this.$UpdateBlendState();
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

  $.Method({Static:false, Public:true }, "get_BlendState", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.BlendState"), [], [])), 
    function get_BlendState () {
      return this.blendState;
    }
  );

  $.Method({Static:false, Public:true }, "get_DisplayMode", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.DisplayMode"), [], [])), 
    function get_DisplayMode () {
      return this.displayMode;
    }
  );

  $.Method({Static:false, Public:true }, "get_PresentationParameters", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.PresentationParameters"), [], [])), 
    function get_PresentationParameters () {
      return this.presentationParameters;
    }
  );

  $.Method({Static:false, Public:true }, "get_SamplerStates", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.SamplerStateCollection"), [], [])), 
    function get_SamplerStates () {
      return this.samplerStates;
    }
  );

  $.Method({Static:false, Public:true }, "get_VertexSamplerStates", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.SamplerStateCollection"), [], [])), 
    function get_VertexSamplerStates () {
      return this.vertexSamplerStates;
    }
  );

  $.Method({Static:false, Public:true }, "get_Textures", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.TextureCollection"), [], [])), 
    function get_Textures () {
      return this.textures;
    }
  );

  $.Method({Static:false, Public:true }, "get_VertexTextures", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.TextureCollection"), [], [])), 
    function get_VertexTextures () {
      return this.vertexTextures;
    }
  );

  $.Method({Static:false, Public:true }, "set_BlendState", 
    (new JSIL.MethodSignature(null, [getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.BlendState")], [])), 
    function set_BlendState (value) {
      this.blendState = value;
      this.$UpdateBlendState();
    }
  );

  $.Method({Static:false, Public:true }, "set_DepthStencilState", 
    (new JSIL.MethodSignature(null, [getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.DepthStencilState")], [])), 
    function set_DepthStencilState (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_RasterizerState", 
    (new JSIL.MethodSignature(null, [getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.RasterizerState")], [])), 
    function set_RasterizerState (value) {
      // FIXME
    }
  );

  $.RawMethod(false, "$UpdateBlendState", function GraphicsDevice_$UpdateBlendState () {
    if (this.blendState === Microsoft.Xna.Framework.Graphics.BlendState.Opaque) {
      this.context.globalCompositeOperation = "copy";
    } else if (this.blendState === Microsoft.Xna.Framework.Graphics.BlendState.Additive) {
      this.context.globalCompositeOperation = "lighter";
    } else {
      this.context.globalCompositeOperation = "source-over";
    }
  });

  $.RawMethod(false, "$UpdateViewport", function GraphicsDevice_$UpdateViewport () {
    this.context.setTransform(1, 0, 0, 1, 0, 0);

    var scaleX = 1, scaleY = 1;

    if (this.canvas === this.originalCanvas) {
      scaleX = this.viewport.Width / this.originalWidth;
      scaleY = this.viewport.Height / this.originalHeight;
    } else {
      scaleX = this.viewport.Width / this.canvas.width;
      scaleY = this.viewport.Height / this.canvas.height;
    }

    this.context.translate(this.viewport.X, this.viewport.Y);

    if (this.canvas === this.originalCanvas) {
      if (this.context.isWebGL) {
        this.context.viewport(0, 0, this.canvas.width, this.canvas.height);
      } else {
        scaleX *= (this.canvas.width / this.originalWidth);
        scaleY *= (this.canvas.height / this.originalHeight);
      }
    }

    this.context.scale(scaleX, scaleY);
  });

  $.RawMethod(false, "$Clear", function GraphicsDevice_$Clear (colorCss) {
    this.context.save();
    this.context.setTransform(1, 0, 0, 1, 0, 0);
    this.context.globalCompositeOperation = "source-over";
    this.context.globalAlpha = 1.0;
    this.context.fillStyle = colorCss || "rgba(0, 0, 0, 1)";
    this.context.clearRect(0, 0, this.canvas.width, this.canvas.height);
    this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);
    this.context.restore();
  });

  $.RawMethod(false, "InternalClear", function GraphicsDevice_InternalClear (color) {
    this.$Clear(color.toCss());
  });

  var warnedTypes = {};

  $.RawMethod(false, "InternalDrawUserPrimitives", function GraphicsDevice_InternalDrawUserPrimitives (T, primitiveType, vertices, vertexOffset, primitiveCount) {
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
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.PrimitiveType"), $jsilcore.TypeRef("System.Array", ["!!0"]), 
          $.Int32, $.Int32
        ], ["T"])), 
    function DrawUserPrimitives$b1 (T, primitiveType, vertexData, vertexOffset, primitiveCount) {
      return this.InternalDrawUserPrimitives(T, primitiveType, vertexData, vertexOffset, primitiveCount);
    }
  );

  $.Method({Static:false, Public:true }, "DrawUserPrimitives", 
    (new JSIL.MethodSignature(null, [
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.PrimitiveType"), $jsilcore.TypeRef("System.Array", ["!!0"]), 
          $.Int32, $.Int32, 
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.VertexDeclaration")
        ], ["T"])), 
    function DrawUserPrimitives$b1 (T, primitiveType, vertexData, vertexOffset, primitiveCount, vertexDeclaration) {
      return this.InternalDrawUserPrimitives(T, primitiveType, vertexData, vertexOffset, primitiveCount);
    }
  );

  $.Method({Static:false, Public:true }, "GetRenderTargets", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.RenderTargetBinding")]), [], [])), 
    function GetRenderTargets () {
      var tRenderTargetBinding = getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.RenderTargetBinding").get();

      if (this.renderTarget === null)
        return [];
      else
        return [ new tRenderTargetBinding(this.renderTarget) ];
    }
  );

  $.Method({Static:false, Public:true }, "SetRenderTarget", 
    (new JSIL.MethodSignature(null, [getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.RenderTarget2D")], [])), 
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

      if (this.canvas === this.originalCanvas) {
        this.viewport.Width = this.originalWidth;
        this.viewport.Height = this.originalHeight;
      } else {
        this.viewport.Width = this.canvas.width;
        this.viewport.Height = this.canvas.height;
      }

      this.$UpdateBlendState();
      this.$UpdateViewport();

      if (oldRenderTarget !== null) 
        oldRenderTarget.$ResynthesizeImage();
    }
  );
});

(function () {
  // XNA3 doesn't have a BlendState class, so we substitute one.

  var graphicsAsm = JSIL.GetAssembly("Microsoft.Xna.Framework.Graphics", true);
  if (graphicsAsm === null) {
    JSIL.DeclareNamespace("Microsoft");
    JSIL.DeclareNamespace("Microsoft.Xna");
    JSIL.DeclareNamespace("Microsoft.Xna.Framework");
    JSIL.DeclareNamespace("Microsoft.Xna.Framework.Graphics");

    JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "Microsoft.Xna.Framework.Graphics.BlendState", true, [], function ($) {
      $.Field({Static:true , Public:true }, "Additive", $.Type, function ($) {
          return null;
        });
      $.Field({Static:true , Public:true }, "AlphaBlend", $.Type, function ($) {
          return null;
        });
      $.Field({Static:true , Public:true }, "NonPremultiplied", $.Type, function ($) {
          return null;
        });
      $.Field({Static:true , Public:true }, "Opaque", $.Type, function ($) {
          return null;
        });
    });
  }
}) ();

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.BlendState", function ($) {
  $.Method({Static: true, Public: true}, ".cctor", 
    new JSIL.MethodSignature(null, [], []),
    function () {
      Microsoft.Xna.Framework.Graphics.BlendState.Opaque = new Microsoft.Xna.Framework.Graphics.BlendState();
      Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend = new Microsoft.Xna.Framework.Graphics.BlendState();
      Microsoft.Xna.Framework.Graphics.BlendState.Additive = new Microsoft.Xna.Framework.Graphics.BlendState();
      Microsoft.Xna.Framework.Graphics.BlendState.NonPremultiplied = new Microsoft.Xna.Framework.Graphics.BlendState();
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.PresentationParameters", function ($) {
  $.RawMethod(false, "__CopyMembers__", function (source, target) {
    target._device = source._device;
  });

  $.RawMethod(false, "$internalCtor", function (graphicsDevice) {
    this._device = graphicsDevice;
  });

  $.Method({Static:false, Public:true }, "get_BackBufferFormat", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.SurfaceFormat"), [], [])), 
    function get_BackBufferFormat () {
      return Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color;
    }
  );

  $.Method({Static:false, Public:true }, "get_BackBufferHeight", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_BackBufferHeight () {
      return this._device.originalHeight;
    }
  );

  $.Method({Static:false, Public:true }, "get_BackBufferWidth", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_BackBufferWidth () {
      return this._device.originalWidth;
    }
  );

  $.Method({Static:false, Public:true }, "get_DepthStencilFormat", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.DepthFormat"), [], [])), 
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

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.TextureCollection", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $.Int32, 
          $.Int32
        ], [])), 
    function _ctor (parent, textureOffset, maxTextures) {
      this.textures = new Array(maxTextures);

      for (var i = 0; i < maxTextures; i++)
        this.textures[i] = null;
    }
  );

  $.Method({Static:false, Public:true }, "get_Item", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.Texture"), [$.Int32], [])), 
    function get_Item (index) {
      return this.textures[index];
    }
  );

  $.Method({Static:false, Public:true }, "set_Item", 
    (new JSIL.MethodSignature(null, [$.Int32, getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.Texture")], [])), 
    function set_Item (index, value) {
      this.textures[index] = value;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.SamplerStateCollection", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $.Int32, 
          $.Int32
        ], [])), 
    function _ctor (pParent, samplerOffset, maxSamplers) {
      // FIXME
      this.parent = pParent;
      this.states = new Array(maxSamplers);

      var tState = Microsoft.Xna.Framework.Graphics.SamplerState.__Type__;

      for (var i = 0; i < maxSamplers; i++) {
        this.states = JSIL.CreateInstanceOfType(tState, null);
      }
    }
  );

  $.Method({Static:false, Public:true }, "get_Item", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.SamplerState"), [$.Int32], [])), 
    function get_Item (index) {
      return this.states[index];
    }
  );

  $.Method({Static:false, Public:true }, "set_Item", 
    (new JSIL.MethodSignature(null, [$.Int32, getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.SamplerState")], [])), 
    function set_Item (index, value) {
      this.states[index] = value;

      var enableSmoothing = true;
      if (value) {
        enableSmoothing = value.get_Filter() != Microsoft.Xna.Framework.Graphics.TextureFilter.Point;
      }

      this.parent.context.mozImageSmoothingEnabled = this.parent.context.webkitImageSmoothingEnabled = enableSmoothing;
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

  $.Method({Static:true , Public:true }, "Lerp", 
    (new JSIL.MethodSignature($.Single, [
          $.Single, $.Single, 
          $.Single
        ], [])), 
    function Lerp (value1, value2, amount) {
      return value1 + (value2 - value1) * amount;
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
          $jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.Texture2D"), $xnaasms[5].TypeRef("System.Collections.Generic.List`1", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), 
          $xnaasms[5].TypeRef("System.Collections.Generic.List`1", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), $xnaasms[5].TypeRef("System.Collections.Generic.List`1", [$.Char]), 
          $.Int32, $.Single, 
          $xnaasms[5].TypeRef("System.Collections.Generic.List`1", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3")]), $xnaasms[5].TypeRef("System.Nullable`1", [$.Char])
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
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.Collections.ObjectModel.ReadOnlyCollection`1", [$.Char]), [], [])), 
    function get_Characters () {
      return this.characters;
    }
  );

  $.Method({Static:false, Public:true }, "get_DefaultCharacter", 
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.Nullable`1", [$.Char]), [], [])), 
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

  $.Method({Static:false, Public:true }, "set_LineSpacing", 
    (new JSIL.MethodSignature(null, [$.Int32], [])), 
    function set_LineSpacing (value) {
      this.lineSpacing = value;
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
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [$jsilcore.TypeRef("JSIL.Reference", [$jsilxna.graphicsRef("Microsoft.Xna.Framework.Graphics.SpriteFont/StringProxy")])], [])), 
    function InternalMeasure (/* ref */ text) {
      var tVector2 = Microsoft.Xna.Framework.Vector2;
      var result = new tVector2(0, 0);
      var lineWidth = 0;
      var lineCount = 1;

      for (var i = 0, l = text.length; i < l; i++) {
        var ch = text[i];

        var lineBreak = false;
        if (ch === "\r") {
          if (text[i + 1] === "\n")
            i += 1;

          lineBreak = true;
        } else if (ch === "\n") {
          lineBreak = true;
        }
        if (lineBreak) {
          result.X = Math.max(lineWidth, result.X);
          lineWidth = 0;

          if (i < (l - 1)) 
            lineCount += 1;

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
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [$.String], [])), 
    function MeasureString (text) {
      return this.InternalMeasure(text);
    }
  );

  $.Method({Static:false, Public:true }, "MeasureString", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2"), [$xnaasms[5].TypeRef("System.Text.StringBuilder")], [])), 
    function MeasureString (text) {
      return this.InternalMeasure(stringBuilder.toString());
    }
  );

  $.RawMethod(false, "InternalDraw", 
    function SpriteFont_InternalDraw (
      text, spriteBatch, textblockPositionX, textblockPositionY, 
      color, rotation, 
      originX, originY, 
      scaleX, scaleY, 
      spriteEffects, layerDepth,
      forCache
    ) {

      // Draw calls are really expensive, so cache entire strings as single textures.

      if ($useTextCaching && $textCachingSupported && (forCache !== true)) {
        var cacheKey = this.textureValue.id + ":" + text;

        var cachedTexture = $jsilxna.textCache.getItem(cacheKey);

        var xPad = 2;
        var yPad = 8;

        if (!cachedTexture) {
          var measured = this.InternalMeasure(text);

          var asmGraphics = $xnaasms.xnaGraphics || $xnaasms.xna;
          var tSpriteBatch = asmGraphics.Microsoft.Xna.Framework.Graphics.SpriteBatch.__Type__;

          var tColor;
          if (JSIL.GetAssembly("Microsoft.Xna.Framework.Graphics", true))
            tColor = $xnaasms.xna.Microsoft.Xna.Framework.Color;
          else 
            tColor = $xnaasms.xna.Microsoft.Xna.Framework.Graphics.Color;

          var tempCanvas = document.createElement("canvas");
          var tempSpriteBatch = JSIL.CreateInstanceOfType(tSpriteBatch, "$cloneExisting", [spriteBatch]);
          // Force the isWebGL flag to false since the temporary canvas isn't using webgl-2d
          tempSpriteBatch.isWebGL = false;

          tempCanvas.width = Math.ceil(measured.X + xPad + xPad);
          tempCanvas.height = Math.ceil(measured.Y + yPad + yPad);

          // FIXME: Terrible hack
          tempSpriteBatch.device = {
            context: tempCanvas.getContext("2d")
          };

          this.InternalDraw(
            text, tempSpriteBatch, xPad, yPad,
            tColor.White, 0,
            0, 0, 1, 1,
            null, 0, 
            true
          );

          cachedTexture = {
            image: tempCanvas,
            id: "text:'" + text + "'",
            width: tempCanvas.width,
            height: tempCanvas.height
          };

          cachedTexture.sizeBytes = tempCanvas.sizeBytes = tempCanvas.width * tempCanvas.height * 4;

          $jsilxna.textCache.setItem(cacheKey, cachedTexture);
        }

        var cachedTextureWidth = cachedTexture.width;
        var cachedTextureHeight = cachedTexture.height;

        spriteBatch.InternalDraw(
          cachedTexture, textblockPositionX, textblockPositionY, cachedTextureWidth, cachedTextureHeight,
          0, 0, cachedTextureWidth, cachedTextureHeight,
          color, rotation, 
          originX + xPad, originY + yPad, 
          scaleX, scaleY, 
          spriteEffects, layerDepth
        );

        return;
      }

      textblockPositionX -= (originX * scaleX);
      textblockPositionY -= (originY * scaleY);

      var tVector2 = Microsoft.Xna.Framework.Vector2;
      var positionX = textblockPositionX;
      var positionY = textblockPositionY;
      var drawX, drawY;

      for (var i = 0, l = text.length; i < l; i++) {
        var ch = text[i];

        var lineBreak = false;
        if (ch === "\r") {
          if (text[i + 1] === "\n")
            i += 1;

          lineBreak = true;
        } else if (ch === "\n") {
          lineBreak = true;
        }
        if (lineBreak) {
          positionX = textblockPositionX;
          positionY += (this.lineSpacing * scaleY);
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
          0, 0, 
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
    this.id = String(++$jsilxna.nextImageId);

    if (typeof ($jsilxna.ImageFormats[format.name]) === "undefined") 
      throw new System.NotImplementedException("The pixel format '" + format.name + "' is not supported.");

    this.image = document.createElement("img");

    var textures = document.getElementById("textures");
    if (textures) 
      textures.appendChild(this.image);
  });

  $.RawMethod(false, "$fromUri", function (graphicsDevice, uri) {
    this._parent = graphicsDevice;
    this.mipMap = false;
    this.format = Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color;
    this.isDisposed = false;
    this.id = String(++$jsilxna.nextImageId);

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
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.Texture2D"), [
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $xnaasms[5].TypeRef("System.IO.Stream"), 
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
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.Texture2D"), [getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $xnaasms[5].TypeRef("System.IO.Stream")], [])), 
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

  $.Method({Static:false, Public:true }, "get_Bounds", 
    (new JSIL.MethodSignature($xnaasms.xna.TypeRef("Microsoft.Xna.Framework.Rectangle"), [], [])), 
    function get_Bounds () {
      if (!this._bounds)
        this._bounds = new Microsoft.Xna.Framework.Rectangle(0, 0, this.width, this.height);
      else
        this._bounds._ctor(0, 0, this.width, this.height);

      return this._bounds;
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
    var textures = document.getElementById("textures");
    if (textures) 
      textures.removeChild(this.image);

    this.image = this.$getImageForBytes(bytes, startIndex, elementCount, shouldUnpremultiply, swapRedAndBlue);
    if (textures) 
      textures.appendChild(this.image);
  });

  $.Method({Static:false, Public:true }, "SetData", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", ["!!0"])], ["T"])), 
    function SetData$b1 (T, data) {
      this.$setDataInternal(T, data, 0, data.length);
    }
  );

  $.Method({Static:false, Public:true }, "SetData", 
    (new JSIL.MethodSignature(null, [
          $.Int32, $xnaasms[5].TypeRef("System.Nullable`1", [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle")]), 
          $jsilcore.TypeRef("System.Array", ["!!0"]), $.Int32, 
          $.Int32
        ], ["T"])), 
    function SetData$b1 (T, level, rect, data, startIndex, elementCount) {
      if (level !== 0) return;

      if (rect !== null) throw new System.NotImplementedException();

      this.$setDataInternal(T, data, startIndex, elementCount);
    }
  );

  $.RawMethod(false, "$getImageForBytes", function (bytes, startIndex, elementCount, unpremultiply, swapRedAndBlue) {
    var canvas = document.createElement("canvas");
    canvas.width = this.width;
    canvas.height = this.height;
    var ctx = $jsilxna.get2DContext(canvas, false);

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

    return canvas;
  });

  $.Method({
    Static: false,
    Public: true
  }, "Dispose", new JSIL.MethodSignature(null, [], []), function () {
    var textures = document.getElementById("textures");
    if (textures) textures.removeChild(this.image);
  });
});

$jsilxna.renderTargetTotalBytes = 0;

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.RenderTarget2D", function ($) {
  $.RawMethod(false, "$internalCtor", function (graphicsDevice, width, height, mipMap, format) {
    this._parent = graphicsDevice;
    this.width = width;
    this.height = height;
    this.mipMap = mipMap;
    this.format = format;
    this.isDisposed = false;
    this.id = String(++$jsilxna.nextImageId);

    this.image = this.canvas = JSIL.Host.createCanvas(width, height);
    this.canvas.naturalWidth = width;
    this.canvas.naturalHeight = height;

    // Can't use WebGL here since it'll disable the ability to copy from the RT to the framebuffer.
    this.context = $jsilxna.get2DContext(this.canvas, false);

    var targets = document.getElementById("rendertargets");
    if (targets) targets.appendChild(this.canvas);

    $jsilxna.renderTargetTotalBytes += (this.width * this.height * 4);
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $.Int32, 
          $.Int32, $.Boolean, 
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.SurfaceFormat"), getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.DepthFormat"), 
          $.Int32, getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.RenderTargetUsage")
        ], [])), 
    function _ctor (graphicsDevice, width, height, mipMap, colorFormat, preferredDepthFormat, preferredMultiSampleCount, usage) {
      this.$internalCtor(graphicsDevice, width, height, mipMap, colorFormat);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $.Int32, 
          $.Int32, $.Boolean, 
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.SurfaceFormat"), getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.DepthFormat")
        ], [])), 
    function _ctor (graphicsDevice, width, height, mipMap, colorFormat, preferredDepthFormat) {
      this.$internalCtor(graphicsDevice, width, height, mipMap, colorFormat);
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), $.Int32, 
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
  }, "$ResynthesizeImage", new JSIL.MethodSignature(null, [], []), function () {
    this.image.isDirty = true;
  });
  $.Method({
    Static: false,
    Public: true
  }, "Dispose", new JSIL.MethodSignature(null, [], []), function () {
    if (!this.canvas)
      return;

    $jsilxna.renderTargetTotalBytes -= (this.width * this.height * 4);

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
  var result = JSIL.Array.New(System.Byte, totalSizeBytes);

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
  var result = JSIL.Array.New(System.Byte, totalSizeBytes);

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
  var result = JSIL.Array.New(System.Byte, totalSizeBytes);

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
  var result = JSIL.Array.New(System.Byte, count);

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
  var result = JSIL.Array.New(System.Byte, colors.length * 4);

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

JSIL.ImplementExternals("Microsoft.Xna.Framework.TitleContainer", function ($) {

  $.Method({Static:true , Public:true }, "OpenStream", 
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.IO.Stream"), [$.String], [])), 
    function OpenStream (name) {
      return new System.IO.FileStream(name, System.IO.FileMode.Open);
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GamerServices.Gamer", function ($) {
  var signedInGamers = null;

  $.Method({Static:true , Public:true }, "get_SignedInGamers", 
    (new JSIL.MethodSignature($xnaasms[2].TypeRef("Microsoft.Xna.Framework.GamerServices.SignedInGamerCollection"), [], [])), 
    function get_SignedInGamers () {
      // FIXME
      if (signedInGamers === null)
        signedInGamers = new $xnaasms[2].Microsoft.Xna.Framework.GamerServices.SignedInGamerCollection();

      return signedInGamers;
    }
  );

  $.Method({Static:false, Public:true }, "get_DisplayName", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_DisplayName () {
      // FIXME
      return "Player";
    }
  );

  $.Method({Static:false, Public:true }, "get_Gamertag", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_Gamertag () {
      // FIXME
      return "Player";
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GamerServices.GamerCollection`1", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      // FIXME
      this.gamer = new $xnaasms[2].Microsoft.Xna.Framework.GamerServices.SignedInGamer();
      this.tEnumerator = JSIL.ArrayEnumerator.Of(this.T);
    }
  );

  $.Method({Static:false, Public:true }, "GetEnumerator", 
    (new JSIL.MethodSignature($xnaasms[2].TypeRef("Microsoft.Xna.Framework.GamerServices.GamerCollection`1/GamerCollectionEnumerator", [new JSIL.GenericParameter("T", "Microsoft.Xna.Framework.GamerServices.GamerCollection`1")]), [], [])), 
    function GetEnumerator () {
      return new (tEnumerator)([this.gamer]);
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GamerServices.SignedInGamerCollection", function ($) {
  $.InheritDefaultConstructor();

  $.Method({Static:false, Public:true }, "get_Item", 
    (new JSIL.MethodSignature($xnaasms[2].TypeRef("Microsoft.Xna.Framework.GamerServices.SignedInGamer"), [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex")], [])), 
    function get_Item (index) {
      // FIXME
      return this.gamer;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GamerServices.SignedInGamer", function ($) {
  $.Method({Static:false, Public:true }, "get_GameDefaults", 
    (new JSIL.MethodSignature($xnaasms[2].TypeRef("Microsoft.Xna.Framework.GamerServices.GameDefaults"), [], [])), 
    function get_GameDefaults () {
      // FIXME
      return null;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsGuest", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsGuest () {
      // FIXME
      return true;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsSignedInToLive", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsSignedInToLive () {
      // FIXME
      return false;
    }
  );

  $.Method({Static:false, Public:true }, "get_PartySize", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_PartySize () {
      // FIXME
      return 0;
    }
  );

  $.Method({Static:false, Public:true }, "get_PlayerIndex", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex"), [], [])), 
    function get_PlayerIndex () {
      // FIXME
      return $xnaasms[0].Microsoft.Xna.Framework.PlayerIndex.One;
    }
  );

  $.Method({Static:false, Public:true }, "get_Presence", 
    (new JSIL.MethodSignature($xnaasms[2].TypeRef("Microsoft.Xna.Framework.GamerServices.GamerPresence"), [], [])), 
    function get_Presence () {
      // FIXME
      return null;
    }
  );

  $.Method({Static:false, Public:true }, "get_Privileges", 
    (new JSIL.MethodSignature($xnaasms[2].TypeRef("Microsoft.Xna.Framework.GamerServices.GamerPrivileges"), [], [])), 
    function get_Privileges () {
      // FIXME
      return null;
    }
  );

  $.Method({Static:false, Public:true }, "GetAchievements", 
    (new JSIL.MethodSignature($xnaasms[2].TypeRef("Microsoft.Xna.Framework.GamerServices.AchievementCollection"), [], [])), 
    function GetAchievements () {
      // FIXME
      return null;
    }
  );

  $.Method({Static:false, Public:true }, "GetFriends", 
    (new JSIL.MethodSignature($xnaasms[2].TypeRef("Microsoft.Xna.Framework.GamerServices.FriendCollection"), [], [])), 
    function GetFriends () {
      // FIXME
      return null;
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.GraphicsResource", function ($) {
  $.Method({Static:false, Public:true }, "get_GraphicsDevice", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.GraphicsDevice"), [], [])), 
    function get_GraphicsDevice () {
      return this.device;
    }
  );
});

JSIL.MakeClass("Microsoft.Xna.Framework.Graphics.DisplayMode", "CurrentDisplayMode", true, [], function ($) {
  $.Method({Static:false, Public:true}, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Object], [])), 
    function _ctor (device) {
      this.device = device;
    }
  );

  $.Method({Static:false, Public:true }, "get_AspectRatio", 
    (new JSIL.MethodSignature($.Single, [], [])), 
    function get_AspectRatio () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_Format", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.SurfaceFormat"), [], [])), 
    function get_Format () {
      return Microsoft.Xna.Framework.Graphics.SurfaceFormat.Color;
    }
  );

  $.Method({Static:false, Public:true }, "get_Height", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Height () {
      return this.device.canvas.height;
    }
  );

  $.Method({Static:false, Public:true }, "get_TitleSafeArea", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [], [])), 
    function get_TitleSafeArea () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_Width", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_Width () {
      return this.device.canvas.width;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Audio.SoundEffectInstance", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Audio.SoundEffect"), $.Boolean], [])), 
    function _ctor (parentEffect, fireAndForget) {
      this.soundEffect = parentEffect;
      this.isFireAndForget = fireAndForget;
      this.isDisposed = false;
      this.looped = false;
      this.instance = null;
      this.volume = 1;
    }
  );

  $.RawMethod(false, "$CreateInstanceIfNeeded", function () {
    if (this.instance === null)
      this.instance = this.soundEffect.$createInstance(this.looped ? 9999 : 0);
    else
      this.instance.loop = this.looped;

    this.instance.volume = this.volume;
  });

  $.Method({Static:false, Public:true }, "Dispose", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Dispose () {
      this.Dispose(true);
    }
  );

  $.Method({Static:false, Public:false}, "Dispose", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function Dispose (disposing) {
      this.isDisposed = true;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsDisposed", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsDisposed () {
      return this.isDisposed;
    }
  );

  $.Method({Static:false, Public:false}, "get_IsFireAndForget", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsFireAndForget () {
      return this.isFireAndForget;
    }
  );

  $.Method({Static:false, Public:true }, "get_IsLooped", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsLooped () {
      return this.looped;
    }
  );

  $.Method({Static:false, Public:false}, "get_SoundEffect", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Audio.SoundEffect"), [], [])), 
    function get_SoundEffect () {
      return this.soundEffect;
    }
  );

  $.Method({Static:false, Public:true }, "get_State", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Audio.SoundState"), [], [])), 
    function get_State () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_Volume", 
    (new JSIL.MethodSignature($.Single, [], [])), 
    function get_Volume () {
      return this.volume;
    }
  );

  $.Method({Static:false, Public:true }, "Pause", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Pause () {
      if (this.instance !== null)
        this.instance.pause();
    }
  );

  $.Method({Static:false, Public:true }, "Play", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Play () {
      this.$CreateInstanceIfNeeded();
      this.instance.play();
    }
  );

  $.Method({Static:false, Public:true }, "Resume", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Resume () {
      if (this.instance !== null)
        this.instance.play();
    }
  );

  $.Method({Static:false, Public:true }, "set_IsLooped", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_IsLooped (value) {
      if (this.looped === value)
        return;

      this.looped = value;

      if (this.instance !== null)
        this.instance.loop = this.looped;
    }
  );

  $.Method({Static:false, Public:true }, "set_Volume", 
    (new JSIL.MethodSignature(null, [$.Single], [])), 
    function set_Volume (value) {
      this.volume = value;

      if (this.instance !== null)
        this.instance.volume = value;
    }
  );

  $.Method({Static:false, Public:true }, "Stop", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Stop () {
      return this.Stop(true);
    }
  );

  $.Method({Static:false, Public:true }, "Stop", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function Stop (immediate) {
      if (this.instance !== null)
        this.instance.pause();

      this.instance = null;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Storage.StorageDevice", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [$.UInt32, $xnaasms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex")], [])), 
    function _ctor (deviceIndex, playerIndex) {
      this.deviceIndex = deviceIndex;
      this.playerIndex = playerIndex;
    }
  );

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [$.UInt32], [])), 
    function _ctor (deviceIndex) {
      this.deviceIndex = deviceIndex;
    }
  );

  $.Method({Static:true , Public:true }, "add_DeviceChanged", 
    (new JSIL.MethodSignature(null, [$xnaasms[5].TypeRef("System.EventHandler`1", [$xnaasms[5].TypeRef("System.EventArgs")])], [])), 
    function add_DeviceChanged (value) {
      throw new Error('Not implemented');
    }
  );

  var callAsyncCallback = function (callback, state, data) {
    // FIXME: Terrible hack
    var awh = {
      WaitOne: function () {
        return;
      },
      Close: function () {
        return;
      }
    };

    var asyncResult = {
      IsCompleted: true,
      IAsyncResult_IsCompleted: true,
      get_IsCompleted: function () { return true; },
      IAsyncResult_get_IsCompleted: function () { return true; },
      AsyncState: state,
      IAsyncResult_AsyncState: state,
      get_AsyncState: function () { return state; },
      IAsyncResult_get_AsyncState: function () { return state; },
      data: data,
      IAsyncResult_AsyncWaitHandle: awh,
      IAsyncResult_get_AsyncWaitHandle: function () { return awh; }
    };

    if (typeof (callback) === "function") {
      callback(asyncResult);
    }

    return asyncResult;
  };

  $.Method({Static:false, Public:true }, "BeginOpenContainer", 
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.IAsyncResult"), [
          $.String, $xnaasms[5].TypeRef("System.AsyncCallback"), 
          $.Object
        ], [])), 
    function BeginOpenContainer (displayName, callback, state) {
      return callAsyncCallback(callback, state, {device: this, displayName: displayName});
    }
  );

  $.Method({Static:true , Public:true }, "BeginShowSelector", 
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.IAsyncResult"), [
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex"), $xnaasms[5].TypeRef("System.AsyncCallback"), 
          $.Object
        ], [])), 
    function BeginShowSelector (player, callback, state) {
      return callAsyncCallback(callback, state, {player: player});
    }
  );

  $.Method({Static:true , Public:true }, "BeginShowSelector", 
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.IAsyncResult"), [
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.PlayerIndex"), $.Int32, 
          $.Int32, $xnaasms[5].TypeRef("System.AsyncCallback"), 
          $.Object
        ], [])), 
    function BeginShowSelector (player, sizeInBytes, directoryCount, callback, state) {
      return callAsyncCallback(callback, state, {player: player});
    }
  );

  $.Method({Static:true , Public:true }, "BeginShowSelector", 
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.IAsyncResult"), [$xnaasms[5].TypeRef("System.AsyncCallback"), $.Object], [])), 
    function BeginShowSelector (callback, state) {
      return callAsyncCallback(callback, state, {});
    }
  );

  $.Method({Static:true , Public:true }, "BeginShowSelector", 
    (new JSIL.MethodSignature($xnaasms[5].TypeRef("System.IAsyncResult"), [
          $.Int32, $.Int32, 
          $xnaasms[5].TypeRef("System.AsyncCallback"), $.Object
        ], [])), 
    function BeginShowSelector (sizeInBytes, directoryCount, callback, state) {
      return callAsyncCallback(callback, state, {});
    }
  );

  $.Method({Static:false, Public:true }, "DeleteContainer", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function DeleteContainer (titleName) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "EndOpenContainer", 
    (new JSIL.MethodSignature(getXnaStorage().TypeRef("Microsoft.Xna.Framework.Storage.StorageContainer"), [$xnaasms[5].TypeRef("System.IAsyncResult")], [])), 
    function EndOpenContainer (result) {
      return new Microsoft.Xna.Framework.Storage.StorageContainer(
        result.data.device, 0, result.data.displayName
      );
    }
  );

  $.Method({Static:true , Public:true }, "EndShowSelector", 
    (new JSIL.MethodSignature(getXnaStorage().TypeRef("Microsoft.Xna.Framework.Storage.StorageDevice"), [$xnaasms[5].TypeRef("System.IAsyncResult")], [])), 
    function EndShowSelector (result) {
      return new Microsoft.Xna.Framework.Storage.StorageDevice(
        0, result.data.player || 0
      );
    }
  );

  $.Method({Static:false, Public:true }, "get_FreeSpace", 
    (new JSIL.MethodSignature($.Int64, [], [])), 
    function get_FreeSpace () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_IsConnected", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsConnected () {
      return true;
    }
  );

  $.Method({Static:false, Public:true }, "get_TotalSpace", 
    (new JSIL.MethodSignature($.Int64, [], [])), 
    function get_TotalSpace () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:false}, "OnDeviceChanged", 
    (new JSIL.MethodSignature(null, [$.Object, $xnaasms[5].TypeRef("System.EventArgs")], [])), 
    function OnDeviceChanged (sender, args) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "remove_DeviceChanged", 
    (new JSIL.MethodSignature(null, [$xnaasms[5].TypeRef("System.EventHandler`1", [$xnaasms[5].TypeRef("System.EventArgs")])], [])), 
    function remove_DeviceChanged (value) {
      throw new Error('Not implemented');
    }
  );

});


JSIL.ImplementExternals("Microsoft.Xna.Framework.GameWindow", function ($) {
  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      var canvas = JSIL.Host.getCanvas();

      this._clientBounds = new Microsoft.Xna.Framework.Rectangle(
        0, 0, canvas.width, canvas.height
      );
    }
  );

  $.Method({Static:false, Public:true }, "get_ClientBounds", 
    (new JSIL.MethodSignature($xnaasms[0].TypeRef("Microsoft.Xna.Framework.Rectangle"), [], [])), 
    function get_ClientBounds () {
      var canvas = JSIL.Host.getCanvas();
      this._clientBounds._ctor(
        0, 0, canvas.width, canvas.height
      );
      
      return this._clientBounds;
    }
  );

  $.Method({Static:false, Public:false}, "get_IsMinimized", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsMinimized () {
      // FIXME
      return false;
    }
  );

  $.Method({Static:false, Public:true }, "get_Title", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_Title () {
      // FIXME
      return document.title;
    }
  );

  $.Method({Static:false, Public:false}, "get_Handle", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.IntPtr"), [], [])), 
    function get_Handle () {
      // FIXME
      return null;
    }
  );

  $.Method({Static:false, Public:true }, "set_AllowUserResizing", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_AllowUserResizing (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "set_Title", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function set_Title (value) {
      document.title = value;
    }
  );
});


JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.SamplerState", function ($) {
  $.Method({Static:true, Public:true }, ".cctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _cctor () {
      Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp = new Microsoft.Xna.Framework.Graphics.SamplerState(
        Microsoft.Xna.Framework.Graphics.TextureFilter.Point, 
        Microsoft.Xna.Framework.Graphics.TextureAddressMode.Clamp, 
        "SamplerState.PointClamp"
      );

      Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp = new Microsoft.Xna.Framework.Graphics.SamplerState(
        Microsoft.Xna.Framework.Graphics.TextureFilter.Point, 
        Microsoft.Xna.Framework.Graphics.TextureAddressMode.Wrap, 
        "SamplerState.PointWrap"
      );
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this.cachedFilter = Microsoft.Xna.Framework.Graphics.TextureFilter.Linear;
      this.name = null;
    }
  );

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [
          getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.TextureFilter"), getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.TextureAddressMode"), 
          $.String
        ], [])), 
    function _ctor (filter, address, name) {
      this.cachedFilter = filter;
      this.name = name;
    }
  );

  $.Method({Static:false, Public:true }, "get_Filter", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.TextureFilter"), [], [])), 
    function get_Filter () {
      return this.cachedFilter;
    }
  );

  $.Method({Static:false, Public:true }, "set_Filter", 
    (new JSIL.MethodSignature(null, [getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.TextureFilter")], [])), 
    function set_Filter (value) {
      this.cachedFilter = value;
    }
  );
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GamerServices.Guide", function ($) {
  $.Method({Static:true , Public:true }, "get_IsScreenSaverEnabled", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsScreenSaverEnabled () {
      // FIXME
    }
  );

  $.Method({Static:true , Public:true }, "get_IsTrialMode", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsTrialMode () {
      // FIXME
      return false;
    }
  );

  $.Method({Static:true , Public:true }, "get_IsVisible", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsVisible () {
      // FIXME
      return false;
    }
  );

  $.Method({Static:true , Public:false}, "get_IsVisibleNoThrow", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsVisibleNoThrow () {
      // FIXME
      return false;
    }
  );

  $.Method({Static:true , Public:true }, "get_SimulateTrialMode", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_SimulateTrialMode () {
      // FIXME
    }
  );

  $.Method({Static:true , Public:false}, "set_IsTrialMode", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_IsTrialMode (value) {
      // FIXME
    }
  );

  $.Method({Static:true , Public:false}, "set_IsVisible", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_IsVisible (value) {
      // FIXME
    }
  );

  $.Method({Static:true , Public:true }, "set_SimulateTrialMode", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function set_SimulateTrialMode (value) {
      // FIXME
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.VertexPositionColor", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), $jsilxna.colorRef()], [])), 
    function _ctor (position, color) {
      this.Position = position;
      this.Color = color;
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.VertexPositionTexture", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2")], [])), 
    function _ctor (position, textureCoordinate) {
      this.Position = position;
      this.TextureCoordinate = textureCoordinate;
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector3"), $jsilxna.colorRef(), 
          $xnaasms[0].TypeRef("Microsoft.Xna.Framework.Vector2")
        ], [])), 
    function _ctor (position, color, textureCoordinate) {
      this.Position = position;
      this.Color = color;
      this.TextureCoordinate = textureCoordinate;
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.GameServiceContainer", function ($) {

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this._services = {};
    }
  );

  $.Method({Static:false, Public:true }, "AddService", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Type"), $.Object], [])), 
    function AddService (type, provider) {
      this._services[type.__TypeId__] = provider;
    }
  );

  $.Method({Static:false, Public:true }, "GetService", 
    (new JSIL.MethodSignature($.Object, [$jsilcore.TypeRef("System.Type")], [])), 
    function GetService (type) {
      var result = this._services[type.__TypeId__];

      if (!result)
        return null;
      else
        return result;
    }
  );

  $.Method({Static:false, Public:true }, "RemoveService", 
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Type")], [])), 
    function RemoveService (type) {
      delete this._services[type.__TypeId__];
    }
  );

});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Graphics.RenderTargetBinding", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.RenderTarget2D")], [])), 
    function _ctor (renderTarget) {
      this._renderTarget = renderTarget;
    }
  );

  $.Method({Static:false, Public:true }, "get_RenderTarget", 
    (new JSIL.MethodSignature(getXnaGraphics().TypeRef("Microsoft.Xna.Framework.Graphics.Texture"), [], [])), 
    function get_RenderTarget () {
      return this._renderTarget;
    }
  );
});