"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

JSIL.DeclareNamespace(System, "Drawing");

JSIL.MakeClass(System.Object, System.Drawing, "Bitmap", "System.Drawing.Bitmap");

if (JSIL.HostType.IsBrowser) {
  System.Drawing.Bitmap.prototype._ctor = function (width, height) {
    this.canvas = JSIL.Host.getCanvas(width, height);

    this.context = this.canvas.getContext('2d');
    this.context.globalCompositeOperation = "copy";

    this.buffer = this.context.createImageData(width, height);
    this.context.putImageData(this.buffer, 0, 0);

    this.setPixelCount = 0;
    this.flushInterval = 16;
  }
  System.Drawing.Bitmap.prototype.SetPixel = function (x, y, color) {
    var index = ((y * this.buffer.width) + x) * 4;
    var data = this.buffer.data;
    data[index] = color.R;
    data[index + 1] = color.G;
    data[index + 2] = color.B;
    data[index + 3] = 255;

    if (this.setPixelCount++ >= this.flushInterval) {
      this.setPixelCount = 0;
      this.context.putImageData(this.buffer, 0, 0);
    }
  }
  System.Drawing.Bitmap.prototype.Save = function (filename) {
    this.context.putImageData(this.buffer, 0, 0);
  } 
} else {
  System.Drawing.Bitmap.prototype._ctor = function (width, height) {
    this.Width = width;
    this.Height = height;
    this.Pixels = new Array(width * height);
  }
  System.Drawing.Bitmap.prototype.SetPixel = function (x, y, color) {
    if ((x < 0) || (y < 0) || (x >= this.Width) || (y >= this.Height))
      throw new Error("Coordinates out of bounds");

    this.Pixels[(y * this.Width) + x] = color;
  }
  System.Drawing.Bitmap.prototype.Save = function (filename) {
  }
}

JSIL.MakeStruct(System.Drawing, "Color", "System.Drawing.Color");
System.Drawing.Color.prototype.A = 0;
System.Drawing.Color.prototype.R = 0;
System.Drawing.Color.prototype.G = 0;
System.Drawing.Color.prototype.B = 0;
System.Drawing.Color.prototype.Name = null;
System.Drawing.Color.prototype._ctor = function (a, r, g, b, name) {
  this.A = a;
  this.R = r;
  this.G = g;
  this.B = b;
  this.Name = name;
}
System.Drawing.Color.prototype.toString = function () {
  if ((typeof (this.Name) != "undefined") && (this.Name != null))
    return this.Name;
  else
    return System.String.Format("Color({0}, {1}, {2}, {3})", this.A, this.R, this.G, this.B);
}
System.Drawing.Color.prototype.MemberwiseClone = function () {
  if ((typeof (this.Name) != "undefined") && (this.Name != null)) {
    return this;
  } else {
    return new System.Drawing.Color(this.A, this.R, this.G, this.B, this.Name);
  }
}
System.Drawing.Color.FromArgb = function () {
  if (arguments.length == 3) {
    return new System.Drawing.Color(255, arguments[0], arguments[1], arguments[2]);
  } else if (arguments.length == 4) {
    return new System.Drawing.Color(arguments[0], arguments[1], arguments[2], arguments[3]);
  } else {
    throw new Error("Expected (r, g, b) or (a, r, g, b)");
  }
};
System.Drawing.Color._cctor = function () {
  System.Drawing.Color.Black = new System.Drawing.Color(0xFF, 0x0, 0x0, 0x0, "Black");
  System.Drawing.Color.White = new System.Drawing.Color(0xFF, 0xFF, 0xFF, 0xFF, "White");
  System.Drawing.Color.OldLace = new System.Drawing.Color(0xFF, 0xFD, 0xF5, 0xE6, "OldLace");
  System.Drawing.Color.BlueViolet = new System.Drawing.Color(0xFF, 0x8A, 0x2B, 0xE2, "BlueViolet");
  System.Drawing.Color.Aquamarine = new System.Drawing.Color(0xFF, 0x7F, 0xFF, 0xD4, "Aquamarine");
};