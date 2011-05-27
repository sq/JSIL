"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

JSIL.DeclareAssembly("JSIL.XNA");

JSIL.MakeClass("System.Object", "HTML5ContentManager", true);
HTML5ContentManager.prototype._ctor = function () {
};
HTML5ContentManager.prototype.Load = function (assetName) {
  return new HTML5Asset(assetName);
};

JSIL.MakeClass("System.Object", "HTML5Asset", true);
HTML5Asset.prototype._ctor = function (assetName) {
  this.name = assetName;
}

Microsoft.Xna.Framework.Vector2.prototype._ctor$0 = function (x, y) {
  this.X = x;
  this.Y = y;
};

Microsoft.Xna.Framework.Vector2.prototype._ctor$1 = function (value) {
  this.X = this.Y = value;
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

Microsoft.Xna.Framework.Game._QuitForced = false;
Microsoft.Xna.Framework.Game.ForceQuit = function () {
  Microsoft.Xna.Framework.Game._QuitForced = true;
};

Microsoft.Xna.Framework.Game.prototype._runHandle = null;
Microsoft.Xna.Framework.Game.prototype._ctor = function () {
  this.content = new HTML5ContentManager();
  this._frameDelay = 1000;
};
Microsoft.Xna.Framework.Game.prototype.get_Content = function () {
  return this.content;
};
Microsoft.Xna.Framework.Game.prototype.Initialize = function () {
  this.LoadContent();
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
  var self = this;
  this.Initialize();
  setTimeout(function () {
    self._Step();
  }, this._frameDelay);
};
Microsoft.Xna.Framework.Game.prototype._Step = function () {
  var failed = true;
  try {
    var gameTime = new Microsoft.Xna.Framework.GameTime();
    this.Update(gameTime);
    this.Draw(gameTime);
    failed = false;
  } finally {
    if (failed || Microsoft.Xna.Framework.Game._QuitForced) {
      this.Exit();
    } else {
      var self = this;
      setTimeout(function () {
        self._Step();
      }, this._frameDelay);
    }
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
  this.keys = keys;
};

Microsoft.Xna.Framework.Input.KeyboardState.prototype.IsKeyDown = function (key) {
  return Array.prototype.indexOf.call(this.keys, key) !== -1;
};

Microsoft.Xna.Framework.Input.KeyboardState.prototype.IsKeyUp = function (key) {
  return Array.prototype.indexOf.call(this.keys, key) === -1;
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

Microsoft.Xna.Framework.Graphics.GraphicsDevice.prototype._ctor = function () {
  this.canvas = JSIL.Host.getCanvas();
  this.viewport = new Microsoft.Xna.Framework.Graphics.Viewport(this.canvas);
};

Microsoft.Xna.Framework.Graphics.GraphicsDevice.prototype.get_Viewport = function () {
  return this.viewport;
};

Microsoft.Xna.Framework.Graphics.GraphicsDevice.prototype.set_Viewport = function (newViewport) {
  this.viewport = newViewport;
  this.canvas = JSIL.Host.getCanvas(this.viewport.Width, this.viewport.Height);
};

Microsoft.Xna.Framework.Graphics.GraphicsDevice.prototype.InternalClear = function (color) {
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype._ctor = function (device) {
  this.device = device;
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype.Begin = function () {
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype.End = function () {
};

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