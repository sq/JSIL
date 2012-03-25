"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

var $jsilxna = JSIL.DeclareAssembly("JSIL.XNA");

$jsilxna.nextImageId = 0;
$jsilxna.multipliedImageCache = {};

$jsilxna.getCachedMultipliedImage = function (image, color) {
  var imageId = image.getAttribute("__imageId") || null;
  if (imageId === null)
    image.setAttribute("__imageId", imageId = new String($jsilxna.nextImageId++));

  var key = imageId + color.toCss(255);
  var result = $jsilxna.multipliedImageCache[key] || null;
  return result;
};

$jsilxna.setCachedMultipliedImage = function (image, color, value) {
  var imageId = image.getAttribute("__imageId") || null;
  if (imageId === null)
    image.setAttribute("__imageId", imageId = new String($jsilxna.nextImageId++));

  var key = imageId + color.toCss(255);
  $jsilxna.multipliedImageCache[key] = value;
};

$jsilxna.getImageMultiplied = function (image, color) {
  var cached = $jsilxna.getCachedMultipliedImage(image, color);
  if (cached !== null)
    return cached;

  var canvas = document.createElement("canvas");
  var context = canvas.getContext("2d");

  // Workaround for bug in Firefox's canvas implementation that treats the outside of a canvas as solid white
  canvas.width = image.naturalWidth + 2;
  canvas.height = image.naturalHeight + 2;

  context.save();
  context.globalCompositeOperation = "copy";
  context.globalCompositeAlpha = 1.0;
  context.clearRect(0, 0, image.naturalWidth + 2, image.naturalHeight + 2);
  context.drawImage(image, 1, 1);

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
  return canvas;
};

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Content.ContentManager", true, {
    _ctor$0: function (serviceProvider) {
    },
    _ctor$1: function (serviceProvider, rootDirectory) {
      this._rootDirectory = rootDirectory;
    },
    Load$b1: JSIL.GenericMethod(
      ["T"],
      function (T, assetName) {
        var asset = JSIL.Host.getAsset(assetName);

        var rawXnb = JSIL.TryCast(asset, RawXNBAsset);
        if (rawXnb !== null) {
          rawXnb.contentManager = this;
          return rawXnb.ReadAsset(T);
        }

        if (JSIL.CheckType(asset, HTML5Asset))
          return asset;

        throw new Error("Asset '" + assetName + "' is not an instance of HTML5Asset.");
      }
    ),
    Unload: function () {
      // Unnecessary since we rely on the host to preload our assets.
    },
    set_RootDirectory: function (rootDirectory) {
      this._rootDirectory = rootDirectory;
    },
    get_RootDirectory: function () {
      return this._rootDirectory;
    }
  }
);

JSIL.MakeClass($jsilcore.System.Object, "HTML5Asset", true, [], function ($) {
  $.prototype._ctor = function (assetName) {
    this.name = assetName;
  };
  $.prototype.toString = function () {
    return "<XNA Asset '" + this.name + "'>";
  };
});

JSIL.MakeClass("HTML5Asset", "HTML5ImageAsset", true, [], function ($) {
  $.prototype._ctor = function (assetName, image) {
    HTML5Asset.prototype._ctor.call(this, assetName);
    this.image = image;
    this.Width = image.naturalWidth;
    this.Height = image.naturalHeight;
  };
});

JSIL.MakeClass("HTML5Asset", "HTML5SoundAsset", true, [], function ($) {
  $.prototype._ctor = function (assetName, sound) {
    HTML5Asset.prototype._ctor.call(this, assetName);
    this.sound = sound;
  };
  $.prototype.Play$0 = function () {
    if (this.sound !== null) {
      this.sound.play();
    }
  };
});

JSIL.MakeClass("HTML5Asset", "HTML5FontAsset", true, [], function ($) {
  $.prototype._cachedCss = null;
  $.prototype._ctor = function (assetName, id, pointSize, lineHeight) {
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
  };
  $.prototype.toCss = function (scale) {
    scale = (scale || 1.0);
    if ((this._cachedCss != null) && (this._cachedScale === scale)) {
      return this._cachedScale;
    } else {
      this._cachedScale = scale;
      return this._cachedCss = (this.pointSize * scale) + 'pt "' + this.id + '"';
    }
  };
  $.prototype.MeasureString$0 = function (text) {
    this.context.font = this.toCss();
    var lines = text.split("\n");

    var resultX = 0, resultY = 0;

    for (var i = 0, l = lines.length; i < l; i++) {
      var metrics = this.context.measureText(lines[i]);
      resultX = Math.max(resultX, metrics.width);
      resultY += this.lineHeight;
    }

    return new Microsoft.Xna.Framework.Vector2(resultX, resultY);
  };
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentTypeReader", true, {
  _ctor: function (targetType) {
    this.targetType = targetType;
    this.TargetIsValueType = !targetType.__IsReferenceType__;
  },
  get_TargetType: function () {
    return this.targetType;
  },
  get_TypeVersion: function () {
    return 0;
  },
  get_CanDeserializeIntoExistingObject: function () {
    return false;
  },
  Initialize: function (manager) {
  },
  Read: function () {
    throw new Error("Invoked abstract method (ContentTypeReader.Read)");
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentTypeReader`1", true, {
  _ctor: function (targetType) {
    var assembly = JSIL.GetAssembly("Microsoft.Xna.Framework");
    assembly.Microsoft.Xna.Framework.Content.ContentTypeReader.prototype._ctor.call(
      this, 
      assembly.Microsoft.Xna.Framework.Content.ContentTypeReader$b1.T.get(this)
    );
  },
  Read: function () {
    throw new Error("Invoked abstract method (ContentTypeReader`1.Read)");
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.StringReader", true, {
  Read: function (input, existingInstance) {
    return input.ReadString();
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Int16Reader", true, {
  Read: function (input, existingInstance) {
    return input.ReadInt16();
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Int32Reader", true, {
  Read: function (input, existingInstance) {
    return input.ReadInt32();
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.UInt16Reader", true, {
  Read: function (input, existingInstance) {
    return input.ReadUInt16();
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.UInt32Reader", true, {
  Read: function (input, existingInstance) {
    return input.ReadUInt32();
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.PointReader", true, {
  Read: function (input, existingInstance) {
    var x = input.ReadInt32();
    var y = input.ReadInt32();

    var result = new Microsoft.Xna.Framework.Point(x, y);
    return result;
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.RectangleReader", true, {
  Read: function (input, existingInstance) {
    var x = input.ReadInt32();
    var y = input.ReadInt32();
    var w = input.ReadInt32();
    var h = input.ReadInt32();
    
    var result = new Microsoft.Xna.Framework.Rectangle(x, y, w, h);
    return result;
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Vector2Reader", true, {
  Read: function (input, existingInstance) {
    return input.ReadVector2();
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Vector3Reader", true, {
  Read: function (input, existingInstance) {
    return input.ReadVector3();
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.Vector4Reader", true, {
  Read: function (input, existingInstance) {
    return input.ReadVector4();
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ArrayReader`1", true, {
  _ctor: function () {
    var assembly = JSIL.GetAssembly("Microsoft.Xna.Framework");
    assembly.Microsoft.Xna.Framework.Content.ContentTypeReader$b1.prototype._ctor.call(
      this, 
      System.Array.Of(
        assembly.Microsoft.Xna.Framework.Content.ArrayReader$b1.T.get(this)
      )
    );
  },
  Initialize: function (manager) {
    this.elementReader = manager.GetTypeReader(this.T);
  },
  Read: function (input, existingInstance) {
    var count = input.ReadInt32();
    if (existingInstance === null) {
      existingInstance = new Array(count);
    }

    for (var i = 0; i < count; i++) {
      existingInstance[i] = input.ReadObjectInternal$b1$1(this.T)(this.elementReader, null);
    }

    return existingInstance;
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ListReader`1", true, {
  _ctor: function () {
    var assembly = JSIL.GetAssembly("Microsoft.Xna.Framework");
    assembly.Microsoft.Xna.Framework.Content.ContentTypeReader$b1.prototype._ctor.call(
      this, 
      System.Collections.Generic.List$b1.Of(
        assembly.Microsoft.Xna.Framework.Content.ListReader$b1.T.get(this)
      )
    );
  },
  Initialize: function (manager) {
    this.elementReader = manager.GetTypeReader(this.T);
  },
  Read: function (input, existingInstance) {
    var count = input.ReadInt32();
    if (existingInstance === null) {
      existingInstance = new (System.Collections.Generic.List$b1.Of(this.T))();
    }

    while (count > 0) {
      var item = input.ReadObjectInternal$b1$1(this.T)(this.elementReader, null);
      count--;
      existingInstance.Add(item);
    }

    return existingInstance;
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentTypeReaderManager", false, {
  _cctor: function () {
    var assembly = JSIL.GetAssembly("Microsoft.Xna.Framework");
    var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

    thisType.nameToReader = {};
    thisType.targetTypeToReader = {};
    thisType.readerTypeToReader = {};
  },
  AddTypeReader: function (readerTypeName, contentReader, reader) {
    var assembly = JSIL.GetAssembly("Microsoft.Xna.Framework");
    var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

    var targetType = reader.TargetType;
    thisType.targetTypeToReader[targetType] = reader;
    thisType.readerTypeToReader[reader.GetType()] = reader;
    thisType.nameToReader[readerTypeName] = reader;
  },
  GetTypeReader$1: function (targetType, contentReader) {
    var assembly = JSIL.GetAssembly("Microsoft.Xna.Framework");
    var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

    var result = thisType.targetTypeToReader[targetType];

    if (typeof (result) !== "object")
      JSIL.Host.error(new Error("No content type reader known for type '" + targetType + "'."));

    return result;
  },
  ReadTypeManifest: function (typeCount, contentReader) {
    var assembly = JSIL.GetAssembly("Microsoft.Xna.Framework");
    var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

    var result = new Array(typeCount);
    var readerManager = new thisType(contentReader);

    for (var i = 0; i < typeCount; i++) {
      var typeReaderName = contentReader.ReadString();
      var typeReaderVersionNumber = contentReader.ReadInt32();

      var parsedTypeName = JSIL.ParseTypeName(typeReaderName);
      // We need to explicitly make the xna assembly the default search context since many of the readers are private classes
      var typeReaderType = JSIL.GetTypeInternal(parsedTypeName, assembly);

      if (typeReaderType === null) {
        JSIL.Host.error(new Error("The type '" + typeReaderName + "' could not be found while loading asset '" + contentReader.assetName + "'."));
        return null;
      }

      var typeReaderInstance = new typeReaderType();
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
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentTypeReaderManager", true, {
  _ctor: function (contentReader) {
    this.contentReader = contentReader;
    this.knownReaders = {};
  },
  GetTypeReader: function (type) {
    var typeName = type.toString();
    var reader = this.knownReaders[typeName];
    if (typeof (reader) === "object")
      return reader;

    var assembly = JSIL.GetAssembly("Microsoft.Xna.Framework");
    var thisType = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

    reader = thisType.GetTypeReader$1(type, this.contentReader);
    if (typeof (reader) === "object")
      return reader;

    JSIL.Host.error(new Error("No content type reader known for type '" + typeName + "'."));
    return null;
    /*
    var t = JSIL.GetTypeInternal(JSIL.ParseTypeName("Microsoft.Xna.Framework.Content.ContentTypeReader"), JSIL.GetAssembly("Microsoft.Xna.Framework"));
    return new t(type);
    */
  }
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Content.ContentReader", true, {
  // This can't be a _ctor because BinaryReader has multiple overloaded constructors.
  // Once reflection lands this can probably work fine as a _ctor.
  $init: function (contentManager, input, assetName, recordDisposableObject, graphicsProfile) {
    System.IO.BinaryReader.prototype._ctor.call(this, input);

    this.contentManager = contentManager;
    this.assetName = assetName;
    this.recordDisposableObject = recordDisposableObject;
    this.graphicsProfile = graphicsProfile;

    this.typeReaders = null;
  },
  get_AssetName: function () {
    return this.assetName;
  },
  get_ContentManager: function () {
    return this.contentManager;
  },
  ReadHeader: function () {
    var formatHeader = String.fromCharCode.apply(String, this.ReadBytes(3));
    if (formatHeader != "XNB")
      throw new Error("Invalid XNB format");

    var platformId = String.fromCharCode(this.ReadByte());
    switch (platformId) {
      case "w":
        break;
      default:
        throw new Error("Unsupported XNB platform: " + platformId);
    }

    var formatVersion = this.ReadByte();
    if (formatVersion != 5)
      throw new Error("Unsupported XNB format version: " + formatVersion);

    var formatFlags = this.ReadByte();

    var isHiDef = (formatFlags & 0x01) != 0;
    var isCompressed = (formatFlags & 0x80) != 0;

    if (isCompressed)
      throw new Error("Compressed XNBs are not supported");

    var uncompressedSize = this.ReadUInt32();

    var typeReaderCount = this.Read7BitEncodedInt();
    this.typeReaders = Microsoft.Xna.Framework.Content.ContentTypeReaderManager.ReadTypeManifest(typeReaderCount, this);
  },
  ReadString: function () {
    var length = this.Read7BitEncodedInt();
    var chars = this.ReadBytes(length);
    return String.fromCharCode.apply(String, chars);
  },
  ReadVector2: function () {
    var x = this.ReadSingle();
    var y = this.ReadSingle();
    return new Microsoft.Xna.Framework.Vector2(x, y);
  },
  ReadVector3: function () {
    var x = this.ReadSingle();
    var y = this.ReadSingle();
    var z = this.ReadSingle();
    return new Microsoft.Xna.Framework.Vector3(x, y, z);
  },
  ReadVector4: function () {
    var x = this.ReadSingle();
    var y = this.ReadSingle();
    var z = this.ReadSingle();
    var w = this.ReadSingle();
    return new Microsoft.Xna.Framework.Vector4(x, y, z, w);
  },
  ReadObject$b1$0: JSIL.GenericMethod(["T"], function ReadObject (T) {
    return this.ReadObject$b1(T)(JSIL.DefaultValue(T));
  }),
  ReadObject$b1$1: JSIL.GenericMethod(["T"], function ReadObject (T, existingInstance) {
    return this.ReadObjectInternal$b1$0(T)(existingInstance);
  }),
  ReadObject$b1$2: JSIL.GenericMethod(["T"], function ReadObject (T, contentTypeReader) {
    return this.ReadObjectInternal$b1$1(T)(contentTypeReader, existingInstance);
  }),
  ReadObjectInternal$b1$0: JSIL.GenericMethod(["T"], function (T, existingInstance) {
    var typeId = this.Read7BitEncodedInt();

    if (typeId === 0)
      return null;

    var typeReader = this.typeReaders[typeId - 1];
    if (typeof (typeReader) !== "object") {
      JSIL.Host.error(new Error("No type reader for typeId '" + typeId + "'. Misaligned XNB read is likely."));
      return null;
    }
    
    return typeReader.Read(this, existingInstance);
  }),
  ReadObjectInternal$b1$1: JSIL.GenericMethod(["T"], function (T, contentTypeReader, existingInstance) {
    if (contentTypeReader.TargetIsValueType)
      return contentTypeReader.Read(this, existingInstance);
    else
      return this.ReadObjectInternal$b1$0(T)(existingInstance);
  }),
  ReadRawObject$b1$0: JSIL.GenericMethod(["T"], function (T) {
    return this.ReadRawObject$b1$1(T)(JSIL.DefaultValue(T));
  }),
  ReadRawObject$b1$1: JSIL.GenericMethod(["T"], function (T, existingInstance) {
    var assembly = JSIL.GetAssembly("Microsoft.Xna.Framework");
    var ctrm = assembly.Microsoft.Xna.Framework.Content.ContentTypeReaderManager;

    var typeReader = ctrm.GetTypeReader$1(T, this);
    return typeReader.Read(this, existingInstance);
  }),
});

JSIL.MakeClass("HTML5Asset", "RawXNBAsset", true, [], function ($) {
  $.prototype._ctor = function (assetName, rawBytes) {
    HTML5Asset.prototype._ctor.call(this, assetName);
    this.bytes = rawBytes;
    this.contentManager = null;
  };
  $.prototype.ReadAsset = function (type) {
    var memoryStream = new System.IO.MemoryStream(this.bytes, false);
    var contentReader = JSIL.New(
      Microsoft.Xna.Framework.Content.ContentReader, "$init", 
      [this.contentManager, memoryStream, this.name, null, 0]
    );

    contentReader.ReadHeader();

    var sharedResourceCount = contentReader.Read7BitEncodedInt();
    var sharedResources = new Array(sharedResourceCount);

    var mainObject = contentReader.ReadObject$b1(type)();

    for (var i = 0; i < sharedResourceCount; i++)
      sharedResources[i] = content.ReadObject$b1(System.Object)();

    return mainObject;
  };
});

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Audio.AudioEngine", true, {
    _ctor$0: function (settingsFile) {
    },
    _ctor$1: function (settingsFile, lookAheadTime, rendererId) {
    },
    Update: function () {
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Audio.WaveBank", true, {
    _ctor$0: function (audioEngine, nonStreamingWaveBankFilename) {
    },
    _ctor$1: function (audioEngine, streamingWaveBankFilename, offset, packetsize) {
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Audio.SoundBank", true, {
    _ctor: function (audioEngine, filename) {
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Media.MediaPlayer", false, {
    Play$0: function (song) {
      if (song !== null)
        song.Play$0();
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Vector2", false, {
    get_Zero: function () {
      return Object.create(Microsoft.Xna.Framework.Vector2.prototype);
    },
    Normalize$0: function (v) {
      v.Normalize();
      return v;
    },
    Multiply$0: function (lhs, rhs) {
      return Microsoft.Xna.Framework.Vector2.op_Multiply$0(lhs, rhs);
    },
    Multiply$2: function (lhs, rhs) {
      return Microsoft.Xna.Framework.Vector2.op_Multiply$1(lhs, rhs);
    },
    op_Addition: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
      result.X = lhs.X + rhs.X;
      result.Y = lhs.Y + rhs.Y;
      return result;
    },
    op_Subtraction: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
      result.X = lhs.X - rhs.X;
      result.Y = lhs.Y - rhs.Y;
      return result;
    },
    op_Division$0: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
      result.X = lhs.X / rhs.X;
      result.Y = lhs.Y / rhs.Y;
      return result;
    },
    op_Division$1: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
      result.X = lhs.X / rhs;
      result.Y = lhs.Y / rhs;
      return result;
    },
    op_Multiply$0: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
      result.X = lhs.X * rhs.X;
      result.Y = lhs.Y * rhs.Y;
      return result;
    },
    op_Multiply$1: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
      result.X = lhs.X * rhs;
      result.Y = lhs.Y * rhs;
      return result;
    },
    op_Multiply$2: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
      result.X = lhs * rhs.X;
      result.Y = lhs * rhs.Y;
      return result;
    },
    op_Equality: function (lhs, rhs) {
      return (lhs.X === rhs.X) && (lhs.Y === rhs.Y);
    },
    op_Inequality: function (lhs, rhs) {
      return (lhs.X !== rhs.X) || (lhs.Y !== rhs.Y);
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Vector2", true, {
    _ctor$0: function (x, y) {
      this.X = x;
      this.Y = y;
    },
    _ctor$1: function (value) {
      this.X = this.Y = value;
    },
    MemberwiseClone: function () {
      var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
      result.X = this.X;
      result.Y = this.Y;
      return result;
    },
    Normalize: function () {
      var factor = 1.0 / Math.sqrt(this.LengthSquared());
      this.X *= factor;
      this.Y *= factor;
    },
    LengthSquared: function () {
      return (this.X * this.X) + (this.Y * this.Y);
    },
    Length: function () {
      return Math.sqrt(this.LengthSquared());
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Vector3", false, {
    get_Zero: function () {
      return Object.create(Microsoft.Xna.Framework.Vector3.prototype);
    },
    Normalize$0: function (v) {
      v.Normalize();
      return v;
    },
    Multiply$0: function (lhs, rhs) {
      return Microsoft.Xna.Framework.Vector3.op_Multiply$0(lhs, rhs);
    },
    Multiply$2: function (lhs, rhs) {
      return Microsoft.Xna.Framework.Vector3.op_Multiply$1(lhs, rhs);
    },
    op_Addition: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector3.prototype);
      result.X = lhs.X + rhs.X;
      result.Y = lhs.Y + rhs.Y;
      result.Z = lhs.Z + rhs.Z;
      return result;
    },
    op_Subtraction: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector3.prototype);
      result.X = lhs.X - rhs.X;
      result.Y = lhs.Y - rhs.Y;
      result.Z = lhs.Z - rhs.Z;
      return result;
    },
    op_Division$0: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector3.prototype);
      result.X = lhs.X / rhs.X;
      result.Y = lhs.Y / rhs.Y;
      result.Z = lhs.Z / rhs.Z;
      return result;
    },
    op_Division$1: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector3.prototype);
      result.X = lhs.X / rhs;
      result.Y = lhs.Y / rhs;
      result.Z = lhs.Z / rhs;
      return result;
    },
    op_Multiply$0: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector3.prototype);
      result.X = lhs.X * rhs.X;
      result.Y = lhs.Y * rhs.Y;
      result.Z = lhs.Z * rhs.Z;
      return result;
    },
    op_Multiply$1: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector3.prototype);
      result.X = lhs.X * rhs;
      result.Y = lhs.Y * rhs;
      result.Z = lhs.Z * rhs;
      return result;
    },
    op_Multiply$2: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector3.prototype);
      result.X = lhs * rhs.X;
      result.Y = lhs * rhs.Y;
      result.Z = lhs * rhs.Z;
      return result;
    },
    op_Equality: function (lhs, rhs) {
      return (lhs.X === rhs.X) && (lhs.Y === rhs.Y) && (lhs.Z === rhs.Z);
    },
    op_Inequality: function (lhs, rhs) {
      return (lhs.X !== rhs.X) || (lhs.Y !== rhs.Y) || (lhs.Z !== rhs.Z);
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Vector3", true, {
    _ctor$0: function (x, y, z) {
      this.X = x;
      this.Y = y;
      this.Z = z;
    },
    _ctor$1: function (value) {
      this.X = this.Y = this.Z = value;
    },
    _ctor$2: function (xy, z) {
      this.X = xy.X;
      this.Y = xy.Y;
      this.Z = z;
    },
    MemberwiseClone: function () {
      var result = Object.create(Microsoft.Xna.Framework.Vector3.prototype);
      result.X = this.X;
      result.Y = this.Y;
      result.Z = this.Z;
      return result;
    },
    Normalize: function () {
      var factor = 1.0 / Math.sqrt(this.LengthSquared());
      this.X *= factor;
      this.Y *= factor;
      this.Z *= factor;
    },
    LengthSquared: function () {
      return (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z);
    },
    Length: function () {
      return Math.sqrt(this.LengthSquared());
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Vector4", false, {
    get_Zero: function () {
      return Object.create(Microsoft.Xna.Framework.Vector4.prototype);
    },
    Normalize$0: function (v) {
      v.Normalize();
      return v;
    },
    Multiply$0: function (lhs, rhs) {
      return Microsoft.Xna.Framework.Vector4.op_Multiply$0(lhs, rhs);
    },
    Multiply$2: function (lhs, rhs) {
      return Microsoft.Xna.Framework.Vector4.op_Multiply$1(lhs, rhs);
    },
    op_Addition: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector4.prototype);
      result.X = lhs.X + rhs.X;
      result.Y = lhs.Y + rhs.Y;
      result.Z = lhs.Z + rhs.Z;
      result.W = lhs.W + rhs.W;
      return result;
    },
    op_Subtraction: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector4.prototype);
      result.X = lhs.X - rhs.X;
      result.Y = lhs.Y - rhs.Y;
      result.Z = lhs.Z - rhs.Z;
      result.W = lhs.W - rhs.W;
      return result;
    },
    op_Division$0: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector4.prototype);
      result.X = lhs.X / rhs.X;
      result.Y = lhs.Y / rhs.Y;
      result.Z = lhs.Z / rhs.Z;
      result.W = lhs.W / rhs.W;
      return result;
    },
    op_Division$1: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector4.prototype);
      result.X = lhs.X / rhs;
      result.Y = lhs.Y / rhs;
      result.Z = lhs.Z / rhs;
      result.W = lhs.W / rhs;
      return result;
    },
    op_Multiply$0: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector4.prototype);
      result.X = lhs.X * rhs.X;
      result.Y = lhs.Y * rhs.Y;
      result.Z = lhs.Z * rhs.Z;
      result.W = lhs.W * rhs.W;
      return result;
    },
    op_Multiply$1: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector4.prototype);
      result.X = lhs.X * rhs;
      result.Y = lhs.Y * rhs;
      result.Z = lhs.Z * rhs;
      result.W = lhs.W * rhs;
      return result;
    },
    op_Multiply$2: function (lhs, rhs) {
      var result = Object.create(Microsoft.Xna.Framework.Vector4.prototype);
      result.X = lhs * rhs.X;
      result.Y = lhs * rhs.Y;
      result.Z = lhs * rhs.Z;
      result.W = lhs * rhs.W;
      return result;
    },
    op_Equality: function (lhs, rhs) {
      return (lhs.X === rhs.X) && (lhs.Y === rhs.Y) && 
             (lhs.Z === rhs.Z) && (lhs.W === rhs.W);
    },
    op_Inequality: function (lhs, rhs) {
      return (lhs.X !== rhs.X) || (lhs.Y !== rhs.Y) || 
             (lhs.Z !== rhs.Z) || (lhs.W !== rhs.W);
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Vector4", true, {
    _ctor$0: function (x, y, z, w) {
      this.X = x;
      this.Y = y;
      this.Z = z;
      this.W = w;
    },
    _ctor$1: function (xy, z, w) {
      this.X = xy.X;
      this.Y = xy.Y;
      this.Z = z;
      this.W = w;
    },
    _ctor$2: function (xyz, w) {
      this.X = xyz.X;
      this.Y = xyz.Y;
      this.Z = xyz.Z;
      this.W = w;
    },
    _ctor$3: function (value) {
      this.X = this.Y = this.Z = this.W = value;
    },
    MemberwiseClone: function () {
      var result = Object.create(Microsoft.Xna.Framework.Vector4.prototype);
      result.X = this.X;
      result.Y = this.Y;
      result.Z = this.Z;
      return result;
    },
    Normalize: function () {
      var factor = 1.0 / Math.sqrt(this.LengthSquared());
      this.X *= factor;
      this.Y *= factor;
      this.Z *= factor;
      this.W *= factor;
    },
    LengthSquared: function () {
      return (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z);
    },
    Length: function () {
      return Math.sqrt(this.LengthSquared());
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.GameServiceContainer", true, {
    _ctor: function () {
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Game", false, {
    ForceQuit: function () {
      Microsoft.Xna.Framework.Game._QuitForced = true;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Game", true, {
    _ctor: function () {
      this.content = JSIL.New(Microsoft.Xna.Framework.Content.ContentManager, "_ctor$0", []);
      this.gameServices = new Microsoft.Xna.Framework.GameServiceContainer();
      this.components = new Microsoft.Xna.Framework.GameComponentCollection();
      this._frameDelay = 1000 / 60;
      this._isDead = false;

      if (typeof (Date.now) === "function") {
        Object.defineProperty(this, "_GetNow", {
          configurable: true, enumerable: true, value: Date.now
        });
      }

      this._runHandle = null;
      this._gameTime = JSIL.New(Microsoft.Xna.Framework.GameTime, "_ctor$0", []);
      this._lastFrame = this._nextFrame = this._started = 0;
    },
    get_Components: function () {
      return this.components;
    },
    get_Content: function () {
      return this.content;
    },
    get_Services: function () {
      return this.gameServices;
    },
    get_IsActive: function () {
      return true && !Microsoft.Xna.Framework.Game._QuitForced && !this._isDead;
    },
    Initialize: function () {
      for (var i = 0, l = this.components._size; i < l; i++) {
        var component = this.components._items[i];
        component.Initialize();
      }

      this.LoadContent();
    },
    get_GraphicsDevice: function () {
      return this.graphicsDeviceService.GraphicsDevice;
    },
    LoadContent: function () {
    },
    UnloadContent: function () {
    },
    $ComponentsOfType: function (type) {
      var result = new Array();
      for (var i = 0, l = this.components._size; i < l; i++) {
        var item = this.components._items[i];

        if (JSIL.CheckType(item, type))
          result.push(item);
      }
      return result;
    },
    Draw: function (gameTime) {
      if (Microsoft.Xna.Framework.Game._QuitForced || this._isDead)
        return;

      var drawableComponents = this.$ComponentsOfType(Microsoft.Xna.Framework.IDrawable);
      for (var i = 0, l = drawableComponents.length; i < l; i++) {
        var drawable = drawableComponents[i];

        if (drawable.Visible)
          drawable.Draw(gameTime);
      }
    },
    Update: function (gameTime) {
      if (Microsoft.Xna.Framework.Game._QuitForced || this._isDead)
        return;

      var updateableComponents = this.$ComponentsOfType(Microsoft.Xna.Framework.IUpdateable);
      for (var i = 0, l = updateableComponents.length; i < l; i++) {
        var updateable = updateableComponents[i];

        if (updateable.Enabled)
          updateable.Update(gameTime);
      }
    },
    Run: function () {
      Microsoft.Xna.Framework.Game._QuitForced = false;
      this.Initialize();
      this._QueueStep();
    },
    _GetNow: function () {
      return (new Date()).getTime();
    },
    _DeferCall: function (callback, lng) {
      setTimeout(callback, 0);
    },
    _QueueStep: function () {
      if (Microsoft.Xna.Framework.Game._QuitForced)
        return;

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

          if (delay <= 0)
            stepCallback();
          else
            self._DeferCall(shouldStepCallback, delay >= 5);
        };

        // It's important that we use setTimeout at least once after every frame in order to let the browser pump messages
        this._DeferCall(shouldStepCallback, true);
      }
    },
    _Step: function () {
      var now = this._GetNow();
      if (this._lastFrame === 0) {
        var elapsed = 0;
        var total = 0;
        this._started = now;
      } else {
        var elapsed = now - this._lastFrame;
        var total = now - this._started;
      }

      this._lastFrame = now;
      this._nextFrame = now + this._frameDelay;

      // Some of the XNA samples seem to fall over and die if elapsed is too large. :|
      if (elapsed > this._frameDelay)
        elapsed = this._frameDelay;

      var millisecondInTicks = 10000;

      this._gameTime.elapsedRealTime._ticks = this._gameTime.elapsedGameTime._ticks = Math.floor(elapsed * millisecondInTicks);
      this._gameTime.totalRealTime._ticks = this._gameTime.totalGameTime._ticks = Math.floor(total * millisecondInTicks);

      var failed = true;
      try {
        this.Update(this._gameTime);
        this.Draw(this._gameTime);
        failed = false;
      } finally {
        if (failed || Microsoft.Xna.Framework.Game._QuitForced)
          this.Exit();
        else
          this._QueueStep();
      }
    },
    Exit: function () {
      this.Dispose();
    },
    Dispose: function () {
      if (this._runHandle !== null)
        window.clearInterval(this._runHandle);

      this._runHandle = null;
      this.UnloadContent();

      this._isDead = true;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.GameComponent", true, {
    _ctor: function (game) {
      this.enabled = true;
      this.initialized = false;
      this.game = game;
    },
    get_Enabled: function () {
      return this.enabled;
    },
    set_Enabled: function (value) {
      this.enabled = value;
    },
    get_Game: function () {
      return this.game;
    },
    Initialize: function () {
      if (this.initialized)
        return;

      this.initialized = true;
    },
    Update: function (gameTime) {
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.DrawableGameComponent", true, {
    _ctor: function (game) {
      Microsoft.Xna.Framework.GameComponent.prototype._ctor.call(this, game);

      this.visible = true;
    },
    get_Visible: function () {
      return this.visible;
    },
    set_Visible: function (value) {
      this.visible = value;
    },
    get_GraphicsDevice: function () {
      return this.game.graphicsDeviceService.GraphicsDevice;
    },
    Initialize: function () {
      if (this.initialized)
        return;

      Microsoft.Xna.Framework.GameComponent.prototype.Initialize.call(this);

      this.LoadContent();
    },
    Draw: function (gameTime) {
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Input.Keyboard", false, {
    GetState: function (playerIndex) {
      var keys = JSIL.Host.getHeldKeys();
      return new Microsoft.Xna.Framework.Input.KeyboardState(keys);
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Input.KeyboardState", true, {
    keys: [],
    _ctor: function (keys) {
      // Note that these keys should be represented as raw integral key codes, not enumeration members
      this.keys = keys;
    },
    IsKeyDown: function (key) {
      return Array.prototype.indexOf.call(this.keys, key.value) !== -1;
    },
    IsKeyUp: function (key) {
      return Array.prototype.indexOf.call(this.keys, key.value) === -1;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Input.Mouse", false, {
    GetState: function (playerIndex) {
      var buttons = JSIL.Host.getHeldButtons();
      var position = JSIL.Host.getMousePosition();
      return new Microsoft.Xna.Framework.Input.MouseState(position, buttons);
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Input.MouseState", true, {
    _ctor: function (position, buttons) {
      this.position = position;
      this.buttons = buttons;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Input.GamePad", false, {
    GetState: function (playerIndex) {
      return new Microsoft.Xna.Framework.Input.GamePadState();
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Input.GamePadState", true, {
    _ctor: function () {
      this._buttons = new Microsoft.Xna.Framework.Input.GamePadButtons();
      this._thumbs = new Microsoft.Xna.Framework.Input.GamePadThumbSticks();
      this._triggers = new Microsoft.Xna.Framework.Input.GamePadTriggers();
    },
    get_IsConnected: function () {
      return false;
    },
    get_Buttons: function () {
      return this._buttons;
    },
    IsButtonDown: function (button) {
      return false;
    },
    get_ThumbSticks: function () {
      return this._thumbs;
    },
    get_Triggers: function () {
      return this._triggers;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Input.GamePadButtons", true, {
    get_Back: function () {
      return false;
    },
    get_LeftShoulder: function () {
      return false;
    },
    get_RightShoulder: function () {
      return false;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Input.GamePadThumbSticks", true, {
    get_Left: function () {
      return this._left;
    },
    get_Right: function () {
      return this._right;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.GraphicsDeviceManager", true, {
    _ctor: function () {
      this.device = new Microsoft.Xna.Framework.Graphics.GraphicsDevice();
    },
    get_GraphicsDevice: function () {
      return this.device;
    },
    set_PreferredBackBufferWidth: function (value) {
      this._width = value;
    },
    set_PreferredBackBufferHeight: function (value) {
      this._height = value;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Graphics.Viewport", true, {
    get_X: function () {
      return this._x;
    },
    get_Y: function () {
      return this._y;
    },
    get_Width: function () {
      return this._width;
    },
    get_Height: function () {
      return this._height;
    },
    set_X: function (value) {
      this._x = value;
    },
    set_Y: function (value) {
      this._y = value;
    },
    set_Width: function (value) {
      this._width = value;
    },
    set_Height: function (value) {
      this._height = value;
    },
    get_TitleSafeArea: function () {
      return new Microsoft.Xna.Framework.Rectangle(this._x, this._y, this._width, this._height);
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.GameTime", true, {
    _ctor$0: function () {
      this.totalRealTime = new System.TimeSpan();
      this.elapsedRealTime = new System.TimeSpan();
      this.totalGameTime = new System.TimeSpan();
      this.elapsedGameTime = new System.TimeSpan();
      this.isRunningSlowly = false;
    },
    _ctor$1: function (totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime, isRunningSlowly) {
      this.totalRealTime = totalRealTime;
      this.elapsedRealTime = elapsedRealTime;
      this.totalGameTime = totalGameTime;
      this.elapsedGameTime = elapsedGameTime;
      this.isRunningSlowly = isRunningSlowly;
    },
    _ctor$2: function (totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime) {
      Microsoft.Xna.Framework.GameTime.prototype._ctor$1.call(this, totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime, false);
    },
    get_TotalRealTime: function () {
      return this.totalRealTime;
    },
    get_TotalGameTime: function () {
      return this.totalGameTime;
    },
    get_ElapsedRealTime: function () {
      return this.elapsedRealTime;
    },
    get_ElapsedGameTime: function () {
      return this.elapsedGameTime;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Rectangle", false, {
    _cctor: function () {
      Microsoft.Xna.Framework.Rectangle._empty = new Microsoft.Xna.Framework.Rectangle();
    },
    get_Empty: function () {
      return Microsoft.Xna.Framework.Rectangle._empty;
    },
    op_Equality: function (lhs, rhs) {
      return lhs.X === rhs.X && 
        lhs.Y === rhs.Y && 
        lhs.Width === rhs.Width && 
        lhs.Height === rhs.Height;
    },
    op_Inequality: function (lhs, rhs) {
      return lhs.X !== rhs.X || 
        lhs.Y !== rhs.Y || 
        lhs.Width !== rhs.Width || 
        lhs.Height !== rhs.Height;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Rectangle", true, {
    _ctor: function (x, y, width, height) {
      this.X = x;
      this.Y = y;
      this.Width = width;
      this.Height = height;
    },
    get_Left: function () {
      return this.X;
    },
    get_Top: function () {
      return this.Y;
    },
    get_Right: function () {
      return this.X + this.Width;
    },
    get_Bottom: function () {
      return this.Y + this.Height;
    },
    get_Center: function () {
      return new Microsoft.Xna.Framework.Point(
        Math.floor(this.X + (this.Width / 2)),
        Math.floor(this.Y + (this.Height / 2))
      );
    },
    Contains$1: function (value) {
      return this.X <= value.X && value.X < this.X + this.Width && this.Y <= value.Y && value.Y < this.Y + this.Height;
    },
    Intersects$0: function (value) {
      return value.X < this.X + this.Width && this.X < value.X + value.Width && value.Y < this.Y + this.Height && this.Y < value.Y + value.Height;
    },
    MemberwiseClone: function () {
      var result = Object.create(Microsoft.Xna.Framework.Rectangle.prototype);
      result.X = this.X;
      result.Y = this.Y;
      result.Width = this.Width;
      result.Height = this.Height;
      return result;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Point", false, {
    _cctor: function () {
      Microsoft.Xna.Framework.Point._zero = new Microsoft.Xna.Framework.Point();
    },
    get_Zero: function () {
      return Microsoft.Xna.Framework.Point._zero;
    },
    op_Equality: function (lhs, rhs) {
      return lhs.Equals$0(rhs);
    },
    op_Inequality: function (lhs, rhs) {
      return lhs.X !== rhs.X || lhs.Y !== rhs.Y;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Point", true, {
    _ctor: function (x, y) {
      this.X = x;
      this.Y = y;
    },
    Equals$0: function (rhs) {
      return this.X === rhs.X && this.Y === rhs.Y;
    },
    MemberwiseClone: function () {
      var result = Object.create(Microsoft.Xna.Framework.Point.prototype);
      result.X = this.X;
      result.Y = this.Y;
      return result;
    }
  }
);

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
}

$jsilxna.Color = {
  _cctor : function () {
    var self = this;
    var proto = this.prototype;
    var makeColor = $jsilxna.makeColor;
    self.black = makeColor(proto, 0, 0, 0);
    self.transparentBlack = makeColor(proto, 0, 0, 0, 0);
    self.white = makeColor(proto, 255, 255, 255);
    self.transparentWhite = makeColor(proto, 255, 255, 255, 0);
    self.red = makeColor(proto, 255, 0, 0);
    self.yellow = makeColor(proto, 255, 255, 0);
    self.orange = makeColor(proto, 255, 165, 0); 
    self.green = makeColor(proto, 0, 128, 0);
    self.cyan = makeColor(proto, 0, 255, 255);
    self.blue = makeColor(proto, 0, 0, 255);
    self.purple = makeColor(proto, 128, 0, 128); 
    self.cornflowerBlue = makeColor(proto, 100, 149, 237);
  },

  get_Black : function () {
    return this.black;
  },
  get_TransparentBlack : function () {
    return this.transparentBlack;
  },
  get_White : function () {
    return this.white;
  },
  get_TransparentWhite : function () {
    return this.transparentWhite;
  },
  get_Red : function () {
    return this.red;
  },
  get_Orange : function () {
    return this.orange;
  },
  get_Yellow : function () {
    return this.yellow;
  },
  get_Green : function () {
    return this.green;
  },
  get_Cyan : function () {
    return this.cyan;
  },
  get_Blue : function () {
    return this.blue;
  },
  get_CornflowerBlue : function () {
    return this.cornflowerBlue;
  },
  get_Purple : function () {
    return this.purple;
  },
  get_Transparent : function () {
    return this.transparentBlack;
  },

  op_Multiply : function (color, multiplier) {
    var result = Object.create(Object.getPrototypeOf(color));
    result.a = Math.floor(color.a * multiplier);
    result.r = Math.floor(color.r * multiplier);
    result.g = Math.floor(color.g * multiplier);
    result.b = Math.floor(color.b * multiplier);
    return result;    
  }
};

$jsilxna.ColorPrototype = {
  _ctor$1 : function (r, g, b) {
    this.a = 255;
    this.r = r;
    this.g = g;
    this.b = b;
  },
  _ctor$2 : function (r, g, b, a) {
    this.a = a;
    this.r = r;
    this.g = g;
    this.b = b;
  },
  _ctor$3 : function (r, g, b) {
    this.a = 255;
    this.r = Math.floor(r * 255);
    this.g = Math.floor(g * 255);
    this.b = Math.floor(b * 255);
  },
  _ctor$4 : function (r, g, b, a) {
    this.a = Math.floor(a * 255);
    this.r = Math.floor(r * 255);
    this.g = Math.floor(g * 255);
    this.b = Math.floor(b * 255);
  },
  _ctor$5 : function (v3) {
    this._ctor$3(v3.X, v3.Y, v3.Z);
  },
  _ctor$6 : function (v4) {
    this._ctor$4(v4.X, v4.Y, v4.Z, v4.W);
  },

  get_A : function () {
    return this.a;
  },
  get_R : function () {
    return this.r;
  },
  get_G : function () {
    return this.g;
  },
  get_B : function () {
    return this.b;
  },
  set_A : function (value) {
    this.a = value;
    this._cachedCss = null;
  },
  set_R : function (value) {
    this.r = value;
    this._cachedCss = null;
  },
  set_G : function (value) {
    this.g = value;
    this._cachedCss = null;
  },
  set_B : function (value) {
    this.b = value;
    this._cachedCss = null;
  },

  _cachedCss : null,
  toCss : function (alpha) {
    if ((this._cachedCss != null) && (this._cachedAlpha == alpha)) {
      return this._cachedCss;
    }

    var a = alpha || this.a;
    if (a < 255) {
      this._cachedAlpha = a;
      return this._cachedCss = "rgba(" + this.r + 
        "," + this.g +
        "," + this.b +
        "," + a +
      ")";    
    } else {
      this._cachedAlpha = a;
      return this._cachedCss = "rgb(" + this.r + 
        "," + this.g +
        "," + this.b +
      ")";
    }
  },

  MemberwiseClone : function () {
    var result = Object.create(Object.getPrototypeOf(this));
    result.a = this.a;
    result.r = this.r;
    result.g = this.g;
    result.b = this.b;
    return result;
  }
};

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.GraphicsDeviceManager", true, {
    _ctor: function (game) {
      this.game = game;
      this.device = new Microsoft.Xna.Framework.Graphics.GraphicsDevice();
      game.graphicsDeviceService = this;
      game.graphicsDeviceManager = this;
    },

    get_GraphicsDevice: function () {
      return this.device;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Graphics.SpriteBatch", true, {
    $drawDebugRects: false,

    _ctor: function (device) {
      this.device = device;
    },

    Begin: function () {
    },

    End: function () {
    },

    InternalDraw: function (texture, position, sourceRectangle, color, rotation, origin, scale, effects) {
      var needRestore = false;
      var image = texture.image;
      var positionIsRect = typeof (position.Width) === "number";
      var scaleX = 1, scaleY = 1, originX = 0, originY = 0;
      var sourceX = 0, sourceY = 0, sourceW = texture.Width, sourceH = texture.Height;
      var positionX, positionY;
      
      if (typeof (scale) === "number")
        scaleX = scaleY = scale;
      else if ((typeof (scale) === "object") && (scale !== null) && (typeof (scale.X) === "number")) {
        scaleX = scale.X;
        scaleY = scale.Y;
      }

      positionX = position.X;
      positionY = position.Y;

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

      if ((typeof (origin) === "object") && (origin !== null) && (typeof (origin.X) === "number")) {
        originX = origin.X;
        positionX -= originX;
        originY = origin.Y;
        positionY -= originY;
      }

      if (sourceRectangle !== null) {
        sourceX = sourceRectangle.X;
        sourceY = sourceRectangle.Y;
        sourceW = sourceRectangle.Width;
        sourceH = sourceRectangle.Height;
      }

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

      if ((typeof (color) === "object") && (color !== null)) {
        if ((color.r < 255) || (color.g < 255) || (color.b < 255)) {
          var newImage = $jsilxna.getImageMultiplied(image, color);
          if (newImage === image) {
            // Broken browser
          } else {
            image = newImage;
            sourceX += 1;
            sourceY += 1;
          }
        }
    
        if (color.a < 255) {
          if (!needRestore)    
            this.device.context.save();
          needRestore = true;

          this.device.context.globalAlpha = color.a / 255;
        }
      }

      var destW, destH;

      if (positionIsRect) {
        destW = position.Width * scaleX;
        destH = position.Height * scaleY;
      } else {
        destW = sourceW * scaleX;
        destH = sourceH * scaleY;
      }

      // Negative width/height cause an exception in Firefox
      if (destW < 0) {
        if (!needRestore)    
          this.device.context.save();
        needRestore = true;

        this.device.context.scale(-1, 1);
        positionX = -positionX;
        destW = -destW;
      }
      if (destH < 0) {
        if (!needRestore)    
          this.device.context.save();
        needRestore = true;

        this.device.context.scale(1, -1);
        positionY = -positionY;
        destH = -destH;
      }

      // 0x0 blits cause an exception in IE
      if (
        (destW > 0) && (destH > 0) &&
        (sourceW > 0) && (sourceH > 0)
      ) {
        if (this.$drawDebugRects) {
          this.device.context.fillStyle = "rgba(255, 0, 0, 0.5)";
          this.device.context.fillRect(
            positionX, positionY, destW, destH
          );
        }

        this.device.context.drawImage(
          image, 
          sourceX, sourceY, sourceW, sourceH,
          positionX, positionY, destW, destH
        );
      }

      if (needRestore)
        this.device.context.restore();
    },

    InternalDrawString: function (font, text, position, color, scale, effects) {
      // FIXME: Temporary work around for RPG demo expecting to have 7 lines of text but only having 5.
      if ((typeof (text) === "undefined") || (text === null))
        return;

      var needRestore = false;
      var positionX = position.X;
      var positionY = position.Y;

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

      this.device.context.textBaseline = "top";
      this.device.context.textAlign = "start";

      var fontCss = font.toCss(scale || 1.0);
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
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Storage.StorageContainer", false, {
    get_TitleLocation: function () {
      return JSIL.Host.getRootDirectory();
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Graphics.GraphicsDevice", true, {
    _ctor: function () {
      this.canvas = JSIL.Host.getCanvas();
      this.context = this.canvas.getContext("2d");
      this.viewport = new Microsoft.Xna.Framework.Graphics.Viewport();
      this.viewport.Width = this.canvas.clientWidth || this.canvas.width;
      this.viewport.Height = this.canvas.clientHeight || this.canvas.height;
    },
    get_Viewport: function () {
      return this.viewport;
    },
    set_Viewport: function (newViewport) {
      this.viewport = newViewport;
      this.canvas = JSIL.Host.getCanvas(this.viewport.Width, this.viewport.Height);
      this.context = this.canvas.getContext("2d");
    },
    InternalClear: function (color) {
      this.context.fillStyle = color.toCss();
      this.context.fillRect(0, 0, this.viewport.Width, this.viewport.Height);
    },
    DrawUserPrimitives: function (primitiveType, vertices, vertexOffset, primitiveCount) {
      switch (primitiveType) {
        case Microsoft.Xna.Framework.Graphics.PrimitiveType.LineList:
          for (var i = 0; i < primitiveCount; i++) {
            var j = i * 2;
            this.context.lineWidth = 2;
            this.context.strokeStyle = vertices[j].Color.toCss();
            this.context.beginPath();
            this.context.moveTo(vertices[j].Position.X, vertices[j].Position.Y);
            this.context.lineTo(vertices[j + 1].Position.X, vertices[j + 1].Position.Y);
            this.context.closePath();
            this.context.stroke();
          }

          break;
        default:
          JSIL.Host.error(new Error("The primitive type " + primitiveType.toString() + " is not implemented."));
          return;
      }
    }

  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Audio.SoundBank", true, {
    GetCue: function () {
      return null;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.MathHelper", false, {
    Min: function (lhs, rhs) {
      return Math.min(lhs, rhs);
    },
    Max: function (lhs, rhs) {
      return Math.max(lhs, rhs);
    },
    Clamp: function (value, min, max) {
      if (max < min)
        max = min;

      if (value < min)
        return min;
      else if (value > max)
        return max;
      else
        return value;
    }
  }
);