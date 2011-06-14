"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

var $jsilxna = JSIL.DeclareAssembly("JSIL.XNA");

Microsoft.Xna.Framework.GraphicsDeviceManager.prototype._ctor = function () {
  this.device = new Microsoft.Xna.Framework.Graphics.GraphicsDevice();
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

JSIL.CopyObjectValues(
  $jsilxna.Color, Microsoft.Xna.Framework.Color
);

JSIL.CopyObjectValues(
  $jsilxna.ColorPrototype, Microsoft.Xna.Framework.Color.prototype
);

JSIL.SealTypes(
  $jsilxna, "Microsoft.Xna.Framework", 
  "Color"
);
