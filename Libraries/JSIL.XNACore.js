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
    },
    Load$b1: JSIL.GenericMethod(
      ["T"],
      function (T, assetName) {
        return JSIL.Host.getAsset(assetName);
      }
    ),
    Unload: function () {
      // Unnecessary since we rely on the host to preload our assets.
    }
  }
);

JSIL.MakeClass($jsilcore.System.Object, "HTML5Asset", true);
HTML5Asset.prototype._ctor = function (assetName) {
  this.name = assetName;
};

JSIL.MakeClass("HTML5Asset", "HTML5ImageAsset", true);
HTML5ImageAsset.prototype._ctor = function (assetName, image) {
  HTML5Asset.prototype._ctor.call(this, assetName);
  this.image = image;
  this.Width = image.naturalWidth;
  this.Height = image.naturalHeight;
};

JSIL.MakeClass("HTML5Asset", "HTML5SoundAsset", true);
HTML5SoundAsset.prototype._ctor = function (assetName, sound) {
  HTML5Asset.prototype._ctor.call(this, assetName);
  this.sound = sound;
};
HTML5SoundAsset.prototype.Play$0 = function () {
  if (this.sound !== null) {
    this.sound.play();
  }
};

JSIL.MakeClass("HTML5Asset", "HTML5FontAsset", true);
HTML5FontAsset.prototype._cachedCss = null;
HTML5FontAsset.prototype._ctor = function (assetName, id, pointSize, lineHeight) {
  HTML5Asset.prototype._ctor.call(this, assetName);
  this.id = id;
  this.pointSize = pointSize;
  this.lineHeight = lineHeight;
  this.canvas = JSIL.Host.getCanvas();
  this.context = this.canvas.getContext("2d");
};
HTML5FontAsset.prototype.toCss = function (scale) {
  scale = (scale || 1.0);
  if ((this._cachedCss != null) && (this._cachedScale === scale)) {
    return this._cachedScale;
  } else {
    this._cachedScale = scale;
    return this._cachedCss = (this.pointSize * scale) + 'pt "' + this.id + '"';
  }
};
HTML5FontAsset.prototype.MeasureString$0 = function (text) {
  this.context.font = this.toCss();
  var metrics = this.context.measureText(text);
  return new Microsoft.Xna.Framework.Vector2(metrics.width, this.lineHeight);
};

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Media.MediaPlayer", false, {
    Play$0: function (song) {
      if (song !== null)
        song.Play$0();
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.MathHelper", false, {
    Clamp: function (value, min, max) {
      if (value <= min)
        return min;
      else if (value >= max)
        return max;
      else
        return value;
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Vector2", false, {
    get_Zero: function () {
      return Object.create(Microsoft.Xna.Framework.Vector2.prototype);
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
    LengthSquared: function () {
      return (this.X * this.X) + (this.Y * this.Y);
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Vector3", false, {
    get_Zero: function () {
      return Object.create(Microsoft.Xna.Framework.Vector3.prototype);
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
    LengthSquared: function () {
      return (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z);
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Vector4", false, {
    get_Zero: function () {
      return Object.create(Microsoft.Xna.Framework.Vector4.prototype);
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
    LengthSquared: function () {
      return (this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z);
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
    Initialize: function () {
      this.LoadContent();
    },
    get_GraphicsDevice: function () {
      return this.graphicsDeviceService.GraphicsDevice;
    },
    LoadContent: function () {
    },
    UnloadContent: function () {
    },
    Draw: function (gameTime) {
    },
    Update: function (gameTime) {
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
    }
  }
);

JSIL.ImplementExternals(
  "Microsoft.Xna.Framework.Graphics.Viewport", true, {
    get_Width: function () {
      return this._width;
    },
    get_Height: function () {
      return this._height;
    },
    set_Width: function (value) {
      this._width = value;
    },
    set_Height: function (value) {
      this._height = value;
    },
    get_TitleSafeArea: function () {
      return new Microsoft.Xna.Framework.Rectangle(0, 0, this._width, this._height);
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

      this.device.context.drawImage(
        image, 
        sourceX, sourceY, sourceW, sourceH,
        positionX, positionY, destW, destH
      );

      if (needRestore)
        this.device.context.restore();
    },

    InternalDrawString: function (font, text, position, color, scale, effects) {
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
      this.device.context.font = font.toCss(scale || 1.0);
      this.device.context.fillStyle = color.toCss();
      this.device.context.fillText(text, positionX, positionY);

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