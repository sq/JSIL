"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

var $jsilxna = JSIL.DeclareAssembly("JSIL.XNA");

$jsilxna.imageMultipliedCache = {};

$jsilxna.getImageMultiplied = function (image, color) {
  var key = image.src + color.toCss();
  if ($jsilxna.imageMultipliedCache.hasOwnProperty(key))
    return $jsilxna.imageMultipliedCache[key];

  var canvas = document.createElement("canvas");
  var context = canvas.getContext("2d");

  // Workaround for bug in Firefox's canvas implementation that treats the outside of a canvas as solid white
  canvas.width = image.naturalWidth + 2;
  canvas.height = image.naturalHeight + 2;

  context.save();
  context.clearRect(0, 0, image.naturalWidth + 2, image.naturalHeight + 2);
  context.globalCompositeAlpha = 1.0;
  context.drawImage(image, 1, 1);

  var imageData = context.getImageData(1, 1, image.naturalWidth, image.naturalHeight);
  var rmul = color.R / 255;
  var gmul = color.G / 255;
  var bmul = color.B / 255;
  var amul = color.A / 255;
  var bytes = imageData.data;
  var foundPixels = false;

  for (var i = 0, l = image.naturalWidth * image.naturalHeight * 4; i < l; i += 4) {
    bytes[i] *= rmul;
    bytes[i + 1] *= gmul;
    bytes[i + 2] *= bmul;
    if (bytes[i + 3] > 0)
      foundPixels = true;
    bytes[i + 3] *= amul;
  }

  if (foundPixels) {
    context.putImageData(imageData, 1, 1);
    $jsilxna.imageMultipliedCache[key] = canvas;
  } else {
    // Workaround for bug in Chrome 12+ which causes getImageData to return pure black. LAME.
    $jsilxna.imageMultipliedCache[key] = image;
    return image;
  }
  return canvas;
};

Microsoft.Xna.Framework.Content.ContentManager.prototype._ctor$0 = function (serviceProvider) {
}
Microsoft.Xna.Framework.Content.ContentManager.prototype._ctor$1 = function (serviceProvider, rootDirectory) {
}
Microsoft.Xna.Framework.Content.ContentManager.prototype.Load$b1 = JSIL.GenericMethod(
  ["T"],
  function (T, assetName) {
    return JSIL.Host.getAsset(assetName);
  }
);
Microsoft.Xna.Framework.Content.ContentManager.prototype.Unload = function () {
  // Unnecessary since we rely on the host to preload our assets.
};

JSIL.MakeClass("System.Object", "HTML5Asset", true);
HTML5Asset.prototype._ctor = function (assetName) {
  this.name = assetName;
}

JSIL.MakeClass("HTML5Asset", "HTML5ImageAsset", true);
HTML5ImageAsset.prototype._ctor = function (assetName, image) {
  HTML5Asset.prototype._ctor.call(this, assetName);
  this.image = image;
  this.Width = image.naturalWidth;
  this.Height = image.naturalHeight;
}

JSIL.MakeClass("HTML5Asset", "HTML5SoundAsset", true);
HTML5SoundAsset.prototype._ctor = function (assetName, sound) {
  HTML5Asset.prototype._ctor.call(this, assetName);
  this.sound = sound;
}
HTML5SoundAsset.prototype.Play$0 = function () {
  if (this.sound != null) {
    this.sound.play();
  }
}

JSIL.MakeClass("HTML5Asset", "HTML5FontAsset", true);
HTML5FontAsset.prototype._ctor = function (assetName, id, pointSize, lineHeight) {
  HTML5Asset.prototype._ctor.call(this, assetName);
  this.id = id;
  this.pointSize = pointSize;
  this.lineHeight = lineHeight;
  this.canvas = JSIL.Host.getCanvas();
  this.context = this.canvas.getContext("2d");
}
HTML5FontAsset.prototype.toCss = function (scale) {
  return (this.pointSize * (scale || 1)) + 'pt "' + this.id + '"';
};
HTML5FontAsset.prototype.MeasureString$0 = function (text) {
  this.context.font = this.toCss();
  var metrics = this.context.measureText(text);
  return new Microsoft.Xna.Framework.Vector2(metrics.width, this.lineHeight);
};

Microsoft.Xna.Framework.Media.MediaPlayer.Play$0 = function (song) {
  if (song !== null)
    song.Play$0();
};

Microsoft.Xna.Framework.MathHelper.Clamp = function (value, min, max) {
  if (value <= min)
    return min;
  else if (value >= max)
    return max;
  else
    return value;
};

Microsoft.Xna.Framework.Vector2.get_Zero = function () {
  return Object.create(Microsoft.Xna.Framework.Vector2.prototype);
};

Microsoft.Xna.Framework.Vector2.prototype._ctor$0 = function (x, y) {
  this.X = x;
  this.Y = y;
};

Microsoft.Xna.Framework.Vector2.prototype._ctor$1 = function (value) {
  this.X = this.Y = value;
};

Microsoft.Xna.Framework.Vector2.prototype.MemberwiseClone = function () {
  var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
  result.X = this.X;
  result.Y = this.Y;
  return result;
}

Microsoft.Xna.Framework.Vector2.op_Addition = function (lhs, rhs) {
  var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
  result.X = lhs.X + rhs.X;
  result.Y = lhs.Y + rhs.Y;
  return result;
}

Microsoft.Xna.Framework.Vector2.op_Subtraction = function (lhs, rhs) {
  var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
  result.X = lhs.X - rhs.X;
  result.Y = lhs.Y - rhs.Y;
  return result;
}

Microsoft.Xna.Framework.Vector2.op_Division$0 = function (lhs, rhs) {
  var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
  result.X = lhs.X / rhs.X;
  result.Y = lhs.Y / rhs.Y;
  return result;
}

Microsoft.Xna.Framework.Vector2.op_Division$1 = function (lhs, rhs) {
  var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
  result.X = lhs.X / rhs;
  result.Y = lhs.Y / rhs;
  return result;
}

Microsoft.Xna.Framework.Vector2.op_Multiply$0 = function (lhs, rhs) {
  var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
  result.X = lhs.X * rhs.X;
  result.Y = lhs.Y * rhs.Y;
  return result;
}

Microsoft.Xna.Framework.Vector2.op_Multiply$1 = function (lhs, rhs) {
  var result = Object.create(Microsoft.Xna.Framework.Vector2.prototype);
  result.X = lhs.X * rhs;
  result.Y = lhs.Y * rhs;
  return result;
}

Microsoft.Xna.Framework.Vector2.op_Equality = function (lhs, rhs) {
  return (lhs.X === rhs.X) && (lhs.Y === rhs.Y);
}

Microsoft.Xna.Framework.Vector2.op_Inequality = function (lhs, rhs) {
  return (lhs.X !== rhs.X) || (lhs.Y !== rhs.Y);
}

Microsoft.Xna.Framework.Vector2.prototype.LengthSquared = function () {
  return (this.X * this.X) + (this.Y * this.Y);
}

Microsoft.Xna.Framework.Vector3.get_Zero = function () {
  return Object.create(Microsoft.Xna.Framework.Vector3.prototype);
};

Microsoft.Xna.Framework.Vector3.prototype._ctor$0 = function (x, y, z) {
  this.X = x;
  this.Y = y;
  this.Z = z;
};

Microsoft.Xna.Framework.Vector3.prototype._ctor$1 = function (value) {
  this.X = this.Y = this.Z = value;
};

Microsoft.Xna.Framework.Vector3.prototype._ctor$2 = function (xy, z) {
  this.X = xy.X;
  this.Y = xy.Y;
  this.Z = z;
};

Microsoft.Xna.Framework.Vector3.prototype.MemberwiseClone = function () {
  var result = Object.create(Microsoft.Xna.Framework.Vector3.prototype);
  result.X = this.X;
  result.Y = this.Y;
  result.Z = this.Z;
  return result;
}

Microsoft.Xna.Framework.Vector4.get_Zero = function () {
  return Object.create(Microsoft.Xna.Framework.Vector4.prototype);
};

Microsoft.Xna.Framework.Vector4.prototype._ctor$0 = function (x, y, z, w) {
  this.X = x;
  this.Y = y;
  this.Z = z;
  this.W = w;
};

Microsoft.Xna.Framework.Vector4.prototype._ctor$1 = function (xy, z, w) {
  this.X = xy.X;
  this.Y = xy.Y;
  this.Z = z;
  this.W = w;
};

Microsoft.Xna.Framework.Vector4.prototype._ctor$2 = function (xyz, w) {
  this.X = xyz.X;
  this.Y = xyz.Y;
  this.Z = xyz.Z;
  this.W = w;
};

Microsoft.Xna.Framework.Vector4.prototype._ctor$3 = function (value) {
  this.X = this.Y = this.Z = this.W = value;
};

Microsoft.Xna.Framework.Vector4.prototype.MemberwiseClone = function () {
  var result = Object.create(Microsoft.Xna.Framework.Vector4.prototype);
  result.X = this.X;
  result.Y = this.Y;
  result.Z = this.Z;
  result.W = this.W;
  return result;
}

Microsoft.Xna.Framework.GameServiceContainer.prototype._ctor = function () {
};

Microsoft.Xna.Framework.Game._QuitForced = false;
Microsoft.Xna.Framework.Game.ForceQuit = function () {
  Microsoft.Xna.Framework.Game._QuitForced = true;
};

Microsoft.Xna.Framework.Game.prototype._runHandle = null;
Microsoft.Xna.Framework.Game.prototype._ctor = function () {
  this.content = JSIL.New(Microsoft.Xna.Framework.Content.ContentManager, "_ctor$0", []);
  this.gameServices = new Microsoft.Xna.Framework.GameServiceContainer();
  this._frameDelay = 1000 / 60;

  if (typeof (Date.now) === "function")
    this._GetNow = Date.now;

  this._gameTime = JSIL.New(Microsoft.Xna.Framework.GameTime, "_ctor$0", []);
  this._lastFrame = this._nextFrame = this._started = 0;
};
Microsoft.Xna.Framework.Game.prototype.get_Content = function () {
  return this.content;
};
Microsoft.Xna.Framework.Game.prototype.get_Services = function () {
  return this.gameServices;
};
Microsoft.Xna.Framework.Game.prototype.Initialize = function () {
  this.LoadContent();
};
Microsoft.Xna.Framework.Game.prototype.get_GraphicsDevice = function () {
  return this.graphicsDeviceService.GraphicsDevice;
};
Microsoft.Xna.Framework.Game.prototype.LoadContent = function () {
};
Microsoft.Xna.Framework.Game.prototype.UnloadContent = function () {
};
Microsoft.Xna.Framework.Game.prototype.Draw = function (gameTime) {
};
Microsoft.Xna.Framework.Game.prototype.Update = function (gameTime) {
};
Microsoft.Xna.Framework.Game.prototype.Run = function () {
  Microsoft.Xna.Framework.Game._QuitForced = false;
  this.Initialize();
  this._QueueStep();
};
Microsoft.Xna.Framework.Game.prototype._GetNow = function () {
  return (new Date()).getTime();
};
Microsoft.Xna.Framework.Game.prototype._DeferCall = function (callback, lng) {
  setTimeout(callback, 0);
};
Microsoft.Xna.Framework.Game.prototype._QueueStep = function () {
  if (Microsoft.Xna.Framework.Game._QuitForced)
    return;

  var self = this;
  var stepCallback = self._Step.bind(self);

  if (typeof (mozRequestAnimationFrame) !== "undefined") {
    mozRequestAnimationFrame(stepCallback);
  } else if (typeof (webkitRequestAnimationFrame) !== "undefined") {
    webkitRequestAnimationFrame(stepCallback);
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
};
Microsoft.Xna.Framework.Game.prototype._Step = function () {
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

  this._gameTime.elapsedRealTime._ticks = this._gameTime.elapsedGameTime._ticks = Math.floor(elapsed * System.TimeSpan.MillisecondInTicks);
  this._gameTime.totalRealTime._ticks = this._gameTime.totalGameTime._ticks = Math.floor(total * System.TimeSpan.MillisecondInTicks);

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
};
Microsoft.Xna.Framework.Game.prototype.Exit = function () {
  this.Dispose();
}
Microsoft.Xna.Framework.Game.prototype.Dispose = function () {
  if (this._runHandle !== null)
    window.clearInterval(this._runHandle);

  this._runHandle = null;
  this.UnloadContent();
}

Microsoft.Xna.Framework.Input.Keyboard.GetState = function (playerIndex) {
  var keys = JSIL.Host.getHeldKeys();
  return new Microsoft.Xna.Framework.Input.KeyboardState(keys);
};

Microsoft.Xna.Framework.Input.KeyboardState.prototype.keys = [];
Microsoft.Xna.Framework.Input.KeyboardState.prototype._ctor = function (keys) {
  // Note that these keys should be represented as raw integral key codes, not enumeration members
  this.keys = keys;
};

Microsoft.Xna.Framework.Input.KeyboardState.prototype.IsKeyDown = function (key) {
  return Array.prototype.indexOf.call(this.keys, key.value) !== -1;
};

Microsoft.Xna.Framework.Input.KeyboardState.prototype.IsKeyUp = function (key) {
  return Array.prototype.indexOf.call(this.keys, key.value) === -1;
};

Microsoft.Xna.Framework.Input.Mouse.GetState = function (playerIndex) {
  var buttons = JSIL.Host.getHeldButtons();
  var position = JSIL.Host.getMousePosition();
  return new Microsoft.Xna.Framework.Input.MouseState(position, buttons);
};

Microsoft.Xna.Framework.Input.GamePad.GetState = function (playerIndex) {
  return new Microsoft.Xna.Framework.Input.GamePadState();
};

Microsoft.Xna.Framework.Input.GamePadState.prototype._ctor = function () {
  this._buttons = new Microsoft.Xna.Framework.Input.GamePadButtons();
  this._thumbs = new Microsoft.Xna.Framework.Input.GamePadThumbSticks();
  this._triggers = new Microsoft.Xna.Framework.Input.GamePadTriggers();
}

Microsoft.Xna.Framework.Input.GamePadButtons.prototype.get_Back = function () {
  return false;
}

Microsoft.Xna.Framework.Input.GamePadState.prototype.get_Buttons = function () {
  return this._buttons;
}

Microsoft.Xna.Framework.Input.GamePadState.prototype.IsButtonDown = function (button) {
  return false;
}

Microsoft.Xna.Framework.Input.GamePadState.prototype.get_ThumbSticks = function () {
  return this._thumbs;
}

Microsoft.Xna.Framework.Input.GamePadState.prototype.get_Triggers = function () {
  return this._triggers;
}

Microsoft.Xna.Framework.Input.GamePadThumbSticks.prototype.get_Left = function () {
  return this._left;
}

Microsoft.Xna.Framework.Input.GamePadThumbSticks.prototype.get_Right = function () {
  return this._right;
}

Microsoft.Xna.Framework.Input.MouseState.prototype._ctor = function (position, buttons) {
  this.position = position;
  this.buttons = buttons;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype._ctor = function () {
  this.device = new Microsoft.Xna.Framework.Graphics.GraphicsDevice();
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_GraphicsDevice = function () {
  return this.device;
};

Microsoft.Xna.Framework.Graphics.Viewport.prototype.get_Width = function () {
  return this._width;
}
Microsoft.Xna.Framework.Graphics.Viewport.prototype.get_Height = function () {
  return this._height;
}
Microsoft.Xna.Framework.Graphics.Viewport.prototype.set_Width = function (value) {
  this._width = value;
}
Microsoft.Xna.Framework.Graphics.Viewport.prototype.set_Height = function (value) {
  this._height = value;
}
Microsoft.Xna.Framework.Graphics.Viewport.prototype.get_TitleSafeArea = function () {
  return new Microsoft.Xna.Framework.Rectangle(0, 0, this._width, this._height);
}

Microsoft.Xna.Framework.GameTime.prototype._ctor$0 = function () {
  this.totalRealTime = new System.TimeSpan();
  this.elapsedRealTime = new System.TimeSpan();
  this.totalGameTime = new System.TimeSpan();
  this.elapsedGameTime = new System.TimeSpan();
  this.isRunningSlowly = false;
};

Microsoft.Xna.Framework.GameTime.prototype._ctor$1 = function (totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime, isRunningSlowly) {
  this.totalRealTime = totalRealTime;
  this.elapsedRealTime = elapsedRealTime;
  this.totalGameTime = totalGameTime;
  this.elapsedGameTime = elapsedGameTime;
  this.isRunningSlowly = isRunningSlowly;
};

Microsoft.Xna.Framework.GameTime.prototype._ctor$2 = function (totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime) {
  Microsoft.Xna.Framework.GameTime.prototype._ctor$1.call(this, totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime, false);
};

Microsoft.Xna.Framework.GameTime.prototype.get_TotalRealTime = function () {
  return this.totalRealTime;
}
Microsoft.Xna.Framework.GameTime.prototype.get_TotalGameTime = function () {
  return this.totalGameTime;
}
Microsoft.Xna.Framework.GameTime.prototype.get_ElapsedRealTime = function () {
  return this.elapsedRealTime;
}
Microsoft.Xna.Framework.GameTime.prototype.get_ElapsedGameTime = function () {
  return this.elapsedGameTime;
}

Microsoft.Xna.Framework.Rectangle.prototype._ctor = function (x, y, width, height) {
  this.X = Math.floor(x);
  this.Y = Math.floor(y);
  this.Width = Math.floor(width);
  this.Height = Math.floor(height);
}

Microsoft.Xna.Framework.Rectangle.prototype.get_Left = function () {
  return this.X;
}
Microsoft.Xna.Framework.Rectangle.prototype.get_Top = function () {
  return this.Y;
}
Microsoft.Xna.Framework.Rectangle.prototype.get_Right = function () {
  return this.X + this.Width;
}
Microsoft.Xna.Framework.Rectangle.prototype.get_Bottom = function () {
  return this.Y + this.Height;
}
Microsoft.Xna.Framework.Rectangle.prototype.get_Center = function () {
  return new Microsoft.Xna.Framework.Point(
    Math.floor(this.X + (this.Width / 2)),
    Math.floor(this.Y + (this.Height / 2))
  );
}
Microsoft.Xna.Framework.Rectangle.prototype.Contains$1 = function (value) {
  return this.X <= value.X && value.X < this.X + this.Width && this.Y <= value.Y && value.Y < this.Y + this.Height;
}
Microsoft.Xna.Framework.Rectangle.prototype.Intersects$0 = function (value) {
  return value.X < this.X + this.Width && this.X < value.X + value.Width && value.Y < this.Y + this.Height && this.Y < value.Y + value.Height;
}
Microsoft.Xna.Framework.Rectangle.prototype.MemberwiseClone = function () {
  var result = Object.create(Microsoft.Xna.Framework.Rectangle.prototype);
  result.X = this.X;
  result.Y = this.Y;
  result.Width = this.Width;
  result.Height = this.Height;
  return result;
}

Microsoft.Xna.Framework.Point.prototype._ctor = function (x, y) {
  this.X = Math.floor(x);
  this.Y = Math.floor(y);
}
Microsoft.Xna.Framework.Point._cctor = function () {
  Microsoft.Xna.Framework.Point._zero = new Microsoft.Xna.Framework.Point();
}
Microsoft.Xna.Framework.Point.prototype.Equals$0 = function (rhs) {
  return this.X === rhs.X && this.Y === rhs.Y;
};
Microsoft.Xna.Framework.Point.get_Zero = function () {
  return Microsoft.Xna.Framework.Point._zero;
};
Microsoft.Xna.Framework.Point.op_Equality = function (lhs, rhs) {
  return lhs.Equals$0(rhs);
};
Microsoft.Xna.Framework.Point.op_Inequality = function (lhs, rhs) {
  return lhs.X !== rhs.X || lhs.Y !== rhs.Y;
};

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
    result.a = Math.floor(this.a * multiplier);
    result.r = Math.floor(this.r * multiplier);
    result.g = Math.floor(this.g * multiplier);
    result.b = Math.floor(this.b * multiplier);
    return result;    
  },
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

  toCss : function () {
    if (this.A < 255) {
      return "rgba(" + this.R + 
        "," + this.G +
        "," + this.B +
        "," + this.A / 255.0 +
      ")";    
    } else {
      return "rgb(" + this.R + 
        "," + this.G +
        "," + this.B +
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

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype._ctor = function (game) {
  this.game = game;
  this.device = new Microsoft.Xna.Framework.Graphics.GraphicsDevice();
  game.graphicsDeviceService = this;
  game.graphicsDeviceManager = this;
};

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype.get_GraphicsDevice = function () {
  return this.device;
};

Microsoft.Xna.Framework.Graphics.GraphicsDevice.prototype._ctor = function () {
  this.canvas = JSIL.Host.getCanvas();
  this.context = this.canvas.getContext("2d");
  this.viewport = new Microsoft.Xna.Framework.Graphics.Viewport();
  this.viewport.Width = this.canvas.clientWidth || this.canvas.width;
  this.viewport.Height = this.canvas.clientHeight || this.canvas.height;
};

Microsoft.Xna.Framework.Graphics.GraphicsDevice.prototype.get_Viewport = function () {
  return this.viewport;
};

Microsoft.Xna.Framework.Graphics.GraphicsDevice.prototype.set_Viewport = function (newViewport) {
  this.viewport = newViewport;
  this.canvas = JSIL.Host.getCanvas(this.viewport.Width, this.viewport.Height);
  this.context = this.canvas.getContext("2d");
};

Microsoft.Xna.Framework.Graphics.GraphicsDevice.prototype.InternalClear = function (color) {
  this.context.fillStyle = color.toCss();
  this.context.fillRect(0, 0, this.viewport.Width, this.viewport.Height);
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype.InternalDraw = function (texture, position, sourceRectangle, color, rotation, origin, scale, effects) {
  var image = texture.image;
  var positionIsRect = typeof (position.Width) === "number";
  var scaleX = 1, scaleY = 1, originX = 0, originY = 0;
  var sourceX = 0, sourceY = 0, sourceW = image.naturalWidth, sourceH = image.naturalHeight;
  var positionX, positionY;
  if (typeof (scale) === "number")
    scaleX = scaleY = scale;
  else if ((typeof (scale) === "object") && (scale !== null) && (typeof (scale.X) === "number")) {
    scaleX = scale.X;
    scaleY = scale.Y;
  }

  positionX = position.X;
  positionY = position.Y;

  this.device.context.save();

  effects = effects || Microsoft.Xna.Framework.Graphics.SpriteEffects.None;

  if ((effects & Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally) == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally) {
    this.device.context.scale(-1, 1);
    positionX = -positionX;
  }

  if ((effects & Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically) == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically) {
    this.device.context.scale(1, -1);
    positionY = -positionY;
  }

  if ((typeof (origin) === "object") && (origin !== null) && (typeof (origin.X) === "number")) {
    originX = origin.X;
    positionX -= originX;
    originY = origin.Y;
    positionY -= originY;
  }

  if ((sourceRectangle !== null) && (sourceRectangle.value !== null)) {
    var sr = sourceRectangle.value;
    sourceX = sr.X;
    sourceY = sr.Y;
    sourceW = sr.Width;
    sourceH = sr.Height;
  }

  if (sourceX < 0) {
    sourceW += sourceX;
    sourceX = 0;
  }
  if (sourceY < 0) {
    sourceH += sourceY;
    sourceY = 0;
  }
  if (sourceW > image.naturalWidth - sourceX)
    sourceW = image.naturalWidth - sourceX;
  if (sourceH > image.naturalHeight - sourceY)
    sourceH = image.naturalHeight - sourceY;

  if ((typeof (color) === "object") && (color !== null)) {
    if ((color.R != 255) || (color.G != 255) || (color.B != 255)) {
      var newImage = $jsilxna.getImageMultiplied(image, color);
      if (newImage === image) {
        // Broken browser
      } else {
        image = newImage;
        sourceX += 1;
        sourceY += 1;
      }
    } else if (color.A != 255) {
      this.device.context.globalAlpha = color.A / 255;
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
    this.device.context.scale(-1, 1);
    positionX = -positionX;
    destW = -destW;
  }
  if (destH < 0) {
    this.device.context.scale(1, -1);
    positionY = -positionY;
    destH = -destH;
  }

  try {
    this.device.context.drawImage(
      image, 
      sourceX, sourceY, sourceW, sourceH,
      positionX, positionY, destW, destH
    );
  } catch (e) {
    throw e;
  }

  this.device.context.restore();
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype.InternalDrawString = function (font, text, position, color, scale, effects) {
  var positionX = position.X;
  var positionY = position.Y;

  effects = effects || Microsoft.Xna.Framework.Graphics.SpriteEffects.None;

  if ((effects & Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally) == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally) {
    this.device.context.scale(-1, 1);
    positionX = -positionX;
  }

  if ((effects & Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically) == Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically) {
    this.device.context.scale(1, -1);
    positionY = -positionY;
  }

  this.device.context.textBaseline = "top";
  this.device.context.textAlign = "start";
  this.device.context.font = font.toCss(scale || 1);
  this.device.context.fillStyle = color.toCss();
  this.device.context.fillText(text, positionX, positionY);
};
