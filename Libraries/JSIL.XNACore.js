"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

var $jsilxna = JSIL.DeclareAssembly("JSIL.XNACore");

JSIL.MakeClass("System.Object", "HTML5ContentManager", true);
HTML5ContentManager.prototype._ctor = function () {
};
HTML5ContentManager.prototype.Load = function (assetName) {
  return JSIL.Host.getAsset(assetName);
};

JSIL.MakeClass("System.Object", "HTML5Asset", true);
HTML5Asset.prototype._ctor = function (assetName) {
  this.name = assetName;
}

JSIL.MakeClass("HTML5Asset", "HTML5ImageAsset", true);
HTML5ImageAsset.prototype._ctor = function (assetName, image) {
  HTML5Asset.prototype._ctor.call(this, assetName);
  this.image = image;
}

JSIL.MakeClass("HTML5Asset", "HTML5FontAsset", true);
HTML5FontAsset.prototype._ctor = function (assetName, font) {
  HTML5Asset.prototype._ctor.call(this, assetName);
  this.font = font;
}

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

Microsoft.Xna.Framework.Game._QuitForced = false;
Microsoft.Xna.Framework.Game.ForceQuit = function () {
  Microsoft.Xna.Framework.Game._QuitForced = true;
};

Microsoft.Xna.Framework.Game.prototype._runHandle = null;
Microsoft.Xna.Framework.Game.prototype._ctor = function () {
  this.content = new HTML5ContentManager();
  this._frameDelay = 1000 / 60;

  if (typeof (Date.now) === "function")
    this._GetNow = Date.now;

  if (
    (typeof (window) === "object") &&
    (typeof (window.postMessage) === "function")
  ) {
    var w = window;
    var deferredCalls = [];
    var runDeferredCalls = function () {
      while (deferredCalls.length > 0) {
        var callback = deferredCalls.shift();
        callback();
      }
    };
    var onMessage = function (evt) {
      if (evt.data === "xna_rundeferredcalls")
        runDeferredCalls();
    };
    window.addEventListener("message", onMessage, false);
    this._DeferCall = function (callback, long) {
      if (long) {
        setTimeout(callback, 0);
      } else {
        var needMessage = deferredCalls.length <= 0;
        deferredCalls.push(callback);
        if (needMessage)
          w.postMessage("xna_rundeferredcalls", "*");
      }
    };
  }

  this._gameTime = JSIL.New(Microsoft.Xna.Framework.GameTime, "_ctor$0", []);
  this._nextFrame = this._GetNow();
};
Microsoft.Xna.Framework.Game.prototype.get_Content = function () {
  return this.content;
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
Microsoft.Xna.Framework.Game.prototype._DeferCall = function (callback, long) {
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
  this._nextFrame = this._GetNow() + this._frameDelay;

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

Microsoft.Xna.Framework.GameTime.prototype._ctor$0 = function () {
};

Microsoft.Xna.Framework.GameTime.prototype._ctor$1 = function (totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime, isRunningSlowly) {
  this.totalRealTime = totalRealTime;
  this.elapsedRealTime = elapsedRealTime;
  this.totalGameTime = totalGameTime;
  this.elapsedGameTime = elapsedGameTime;
  this.isRunningSlowly = isRunningSlowly;
};

Microsoft.Xna.Framework.GameTime.prototype._ctor$2 = function (totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime) {
  Microsoft.Xna.Framework.GameTime.prototype._ctor$1.call(this, totalRealTime, elapsedRealTime, totalGameTime, elapsedGameTime);
};

Microsoft.Xna.Framework.Rectangle.prototype._ctor = function (x, y, width, height) {
  this.X = x;
  this.Y = y;
  this.Width = width;
  this.Height = height;
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

Microsoft.Xna.Framework.Rectangle.prototype.MemberwiseClone = function () {
  var result = Object.create(Microsoft.Xna.Framework.Rectangle.prototype);
  result.X = this.X;
  result.Y = this.Y;
  result.Width = this.Width;
  result.Height = this.Height;
  return result;
}

Microsoft.Xna.Framework.Storage.StorageContainer.get_TitleLocation = function () {
  return JSIL.Host.getRootDirectory();
};