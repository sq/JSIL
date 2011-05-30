"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

var $jsilxna = JSIL.DeclareAssembly("JSIL.XNA3");

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype._ctor = function (game) {
  this.game = game;
  this.device = new Microsoft.Xna.Framework.Graphics.GraphicsDevice();
  game.graphicsDeviceService = this;
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

Microsoft.Xna.Framework.Graphics.GraphicsDevice.prototype.DrawUserPrimitives = function (primitiveType, vertices, vertexOffset, primitiveCount) {
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
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype._ctor = function (device) {
  this.device = device;
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype.Begin = function () {
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype.InternalDraw = function (texture, position, sourceRectangle, color) {
  if ((sourceRectangle !== null) && (sourceRectangle.value !== null)) {
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

Microsoft.Xna.Framework.Graphics.Color.prototype.MemberwiseClone = function () {
  var result = Object.create(Microsoft.Xna.Framework.Graphics.Color.prototype);
  result.a = this.a;
  result.r = this.r;
  result.g = this.g;
  result.b = this.b;
  return result;
}

Microsoft.Xna.Framework.Graphics.Color._cctor = function () {
  var self = Microsoft.Xna.Framework.Graphics.Color;
  self.black = new Microsoft.Xna.Framework.Graphics.Color(0, 0, 0);
  self.transparentBlack = new Microsoft.Xna.Framework.Graphics.Color(0, 0, 0, 0);
  self.white = new Microsoft.Xna.Framework.Graphics.Color(255, 255, 255);
  self.transparentWhite = new Microsoft.Xna.Framework.Graphics.Color(255, 255, 255, 0);
  self.yellow = new Microsoft.Xna.Framework.Graphics.Color(255, 255, 0);
  self.cornflowerBlue = new Microsoft.Xna.Framework.Graphics.Color(100, 149, 237);
};

Microsoft.Xna.Framework.Graphics.Color.get_Black = function () {
  return Microsoft.Xna.Framework.Graphics.Color.black;
};
Microsoft.Xna.Framework.Graphics.Color.get_TransparentBlack = function () {
  return Microsoft.Xna.Framework.Graphics.Color.transparentBlack;
};
Microsoft.Xna.Framework.Graphics.Color.get_White = function () {
  return Microsoft.Xna.Framework.Graphics.Color.white;
};
Microsoft.Xna.Framework.Graphics.Color.get_Yellow = function () {
  return Microsoft.Xna.Framework.Graphics.Color.yellow;
};
Microsoft.Xna.Framework.Graphics.Color.get_CornflowerBlue = function () {
  return Microsoft.Xna.Framework.Graphics.Color.cornflowerBlue;
};
Microsoft.Xna.Framework.Graphics.Color.get_TransparentWhite = function () {
  return Microsoft.Xna.Framework.Graphics.Color.transparentWhite;
};

JSIL.SealTypes(
  $jsilxna, "Microsoft.Xna.Framework.Graphics", 
  "Color"
);
