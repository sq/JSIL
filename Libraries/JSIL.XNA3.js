"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

var $jsilxna = JSIL.DeclareAssembly("JSIL.XNA");

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

  if (positionIsRect)
    this.device.context.drawImage(
      image, 
      sourceX, sourceY, sourceW, sourceH,
      positionX, positionY, position.Width * scaleX, position.Height * scaleY
    );
  else
    this.device.context.drawImage(
      image, 
      sourceX, sourceY, sourceW, sourceH,
      positionX, positionY, sourceW * scaleX, sourceH * scaleY
    );

  this.device.context.restore();
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype.InternalDrawString = function (font, text, position, color) {
  this.device.context.textBaseline = "top";
  this.device.context.textAlign = "start";
  this.device.context.font = font.toCss();
  this.device.context.fillStyle = color.toCss();
  this.device.context.fillText(text, position.X, position.Y);
};

Microsoft.Xna.Framework.Graphics.SpriteBatch.prototype.End = function () {
};

Microsoft.Xna.Framework.Storage.StorageContainer.get_TitleLocation = function () {
  return JSIL.Host.getRootDirectory();
};

JSIL.CopyObjectValues(
  $jsilxna.Color, Microsoft.Xna.Framework.Graphics.Color
);

JSIL.CopyObjectValues(
  $jsilxna.ColorPrototype, Microsoft.Xna.Framework.Graphics.Color.prototype
);

JSIL.SealTypes(
  $jsilxna, "Microsoft.Xna.Framework.Graphics", 
  "Color"
);