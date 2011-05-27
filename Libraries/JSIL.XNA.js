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
  this.image = JSIL.Host.getAsset(assetName);
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
  this._frameDelay = 1000 / 60;
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
  this.Initialize();
  this._QueueStep();
};
Microsoft.Xna.Framework.Game.prototype._QueueStep = function () {
  if (Microsoft.Xna.Framework.Game._QuitForced)
    return;

  var self = this;
  var stepCallback = function () {
    self._Step();
  };

  if (typeof (mozRequestAnimationFrame) !== "undefined") {
    mozRequestAnimationFrame(stepCallback);
  } else if (typeof (webkitRequestAnimationFrame) !== "undefined") {
    webkitRequestAnimationFrame(stepCallback);
  } else {
    setTimeout(stepCallback, this._frameDelay);  
  }
};
Microsoft.Xna.Framework.Game.prototype._Step = function () {
  var failed = true;
  try {
    var gameTime = new Microsoft.Xna.Framework.GameTime();
    this.Update(gameTime);
    this.Draw(gameTime);
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

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype._ctor = function (device) {
  this.device = device;
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype.Begin = function () {
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype.InternalDraw = function (texture, position, sourceRectangle, color) {
  if ((typeof (sourceRectangle) === "object") && (sourceRectangle.value !== null)) {
    var sr = sourceRectangle.value;
    this.device.context.drawImage(
      texture.image, 
      sr.X, sr.Y, sr.Width, sr.Height,
      position.X, position.Y, position.Width, position.Height
    );
  } else if (typeof (position.Width) === "number") {
    this.device.context.drawImage(
      texture.image, position.X, position.Y, position.Width, position.Height
    );
  } else {
    this.device.context.drawImage(
      texture.image, position.X, position.Y
    );
  }
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

Microsoft.Xna.Framework.Graphics.Color.prototype._ctor$1 = function (r, g, b) {
  this.a = 255;
  this.r = r;
  this.g = g;
  this.b = b;
}
Microsoft.Xna.Framework.Graphics.Color.prototype._ctor$2 = function (r, g, b, a) {
  this.a = a;
  this.r = r;
  this.g = g;
  this.b = b;
}
Microsoft.Xna.Framework.Graphics.Color.prototype._ctor$3 = function (r, g, b) {
  this.a = 255;
  this.r = Math.floor(r * 255);
  this.g = Math.floor(g * 255);
  this.b = Math.floor(b * 255);
}
Microsoft.Xna.Framework.Graphics.Color.prototype._ctor$4 = function (r, g, b, a) {
  this.a = Math.floor(a * 255);
  this.r = Math.floor(r * 255);
  this.g = Math.floor(g * 255);
  this.b = Math.floor(b * 255);
}
Microsoft.Xna.Framework.Graphics.Color.prototype._ctor$5 = function (v3) {
  Microsoft.Xna.Framework.Graphics.Color.prototype._ctor$3.call(this, v3.X, v3.Y, v3.Z);
}
Microsoft.Xna.Framework.Graphics.Color.prototype._ctor$6 = function (v4) {
  Microsoft.Xna.Framework.Graphics.Color.prototype._ctor$4.call(this, v4.X, v4.Y, v4.Z, v4.W);
}

Microsoft.Xna.Framework.Graphics.Color.prototype.get_A = function () {
  return this.a;
}
Microsoft.Xna.Framework.Graphics.Color.prototype.get_R = function () {
  return this.r;
}
Microsoft.Xna.Framework.Graphics.Color.prototype.get_G = function () {
  return this.g;
}
Microsoft.Xna.Framework.Graphics.Color.prototype.get_B = function () {
  return this.b;
}

Microsoft.Xna.Framework.Graphics.Color.prototype.toCss = function () {
  if (this.A < 255) {
    return System.String.Format(
      "rgba({0}, {1}, {2}, {3})",
      this.R, this.G, this.B, this.A / 255.0
    );
  } else {
    return System.String.Format(
      "rgb({0}, {1}, {2})",
      this.R, this.G, this.B
    );
  }
}

Microsoft.Xna.Framework.Graphics.Color.get_Black = function () {
  return new Microsoft.Xna.Framework.Graphics.Color(0, 0, 0);
};
Microsoft.Xna.Framework.Graphics.Color.get_TransparentBlack = function () {
  return new Microsoft.Xna.Framework.Graphics.Color(0, 0, 0, 0);
};
Microsoft.Xna.Framework.Graphics.Color.get_White = function () {
  return new Microsoft.Xna.Framework.Graphics.Color(255, 255, 255);
};
Microsoft.Xna.Framework.Graphics.Color.get_TransparentWhite = function () {
  return new Microsoft.Xna.Framework.Graphics.Color(255, 255, 255, 0);
};

Microsoft.Xna.Framework.Rectangle.prototype._ctor = function (x, y, width, height) {
  this.X = x;
  this.Y = y;
  this.Width = width;
  this.Height = height;
}
