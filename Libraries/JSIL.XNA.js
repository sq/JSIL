"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

JSIL.DeclareNamespace(this, "Microsoft");
JSIL.DeclareNamespace(Microsoft, "Xna");
JSIL.DeclareNamespace(Microsoft.Xna, "Framework");
JSIL.DeclareNamespace(Microsoft.Xna.Framework, "Input");
JSIL.DeclareNamespace(Microsoft.Xna.Framework, "Graphics");

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "Game", "Microsoft.Xna.Framework.Game");

JSIL.MakeStruct(Microsoft.Xna.Framework, "Plane", "Microsoft.Xna.Framework.Plane");
JSIL.MakeStruct(Microsoft.Xna.Framework, "Point", "Microsoft.Xna.Framework.Point");
JSIL.MakeStruct(Microsoft.Xna.Framework, "Quaternion", "Microsoft.Xna.Framework.Quaternion");
JSIL.MakeStruct(Microsoft.Xna.Framework, "Ray", "Microsoft.Xna.Framework.Ray");
JSIL.MakeStruct(Microsoft.Xna.Framework, "Rectangle", "Microsoft.Xna.Framework.Rectangle");
JSIL.MakeStruct(Microsoft.Xna.Framework, "Vector2", "Microsoft.Xna.Framework.Vector2");
JSIL.MakeStruct(Microsoft.Xna.Framework, "Vector3", "Microsoft.Xna.Framework.Vector3");
JSIL.MakeStruct(Microsoft.Xna.Framework, "Vector4", "Microsoft.Xna.Framework.Vector4");
JSIL.MakeStruct(Microsoft.Xna.Framework, "Matrix", "Microsoft.Xna.Framework.Matrix");

JSIL.MakeStruct(Microsoft.Xna.Framework.Graphics, "VertexPositionColor", "Microsoft.Xna.Framework.Graphics.VertexPositionColor");
JSIL.MakeStruct(Microsoft.Xna.Framework.Graphics, "VertexPositionTexture", "Microsoft.Xna.Framework.Graphics.VertexPositionTexture");
JSIL.MakeStruct(Microsoft.Xna.Framework.Graphics, "VertexPositionColorTexture", "Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture");
JSIL.MakeStruct(Microsoft.Xna.Framework.Graphics, "VertexPositionNormalTexture", "Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture");

JSIL.MakeInterface(
	Microsoft.Xna.Framework, "IGraphicsDeviceManager", "Microsoft.Xna.Framework.IGraphicsDeviceManager", {
	  "CreateDevice": Function,
	  "BeginDraw": Function,
	  "EndDraw": Function
	});

JSIL.MakeClass(System.Object, Microsoft.Xna.Framework, "GraphicsDeviceManager", "Microsoft.Xna.Framework.GraphicsDeviceManager");

// ======================================================================================
// Vector types
// ======================================================================================

Microsoft.Xna.Framework.Vector2.prototype.X = 0;
Microsoft.Xna.Framework.Vector2.prototype.Y = 0;
Microsoft.Xna.Framework.Vector2._zero = new Microsoft.Xna.Framework.Vector2();
Microsoft.Xna.Framework.Vector2._one = new Microsoft.Xna.Framework.Vector2();
Microsoft.Xna.Framework.Vector2._unitX = new Microsoft.Xna.Framework.Vector2();
Microsoft.Xna.Framework.Vector2._unitY = new Microsoft.Xna.Framework.Vector2();
Microsoft.Xna.Framework.Vector2.get_Zero = function () {
  return Microsoft.Xna.Framework.Vector2._zero;
};

Microsoft.Xna.Framework.Vector2.get_One = function () {
  return Microsoft.Xna.Framework.Vector2._one;
};

Microsoft.Xna.Framework.Vector2.get_UnitX = function () {
  return Microsoft.Xna.Framework.Vector2._unitX;
};

Microsoft.Xna.Framework.Vector2.get_UnitY = function () {
  return Microsoft.Xna.Framework.Vector2._unitY;
};

Microsoft.Xna.Framework.Vector2.prototype._ctor$0 = function (x, y) {
  this.X = x;
  this.Y = y;
};

Microsoft.Xna.Framework.Vector2.prototype._ctor$1 = function (value) {
  this.Y = value;
  this.X = value;
};

Microsoft.Xna.Framework.Vector2.prototype.toString = function () {
  return System.String.Format("{{X:{0} Y:{1}}}", [this.X.toString(), this.Y.toString()]);
};

Microsoft.Xna.Framework.Vector2.prototype.Equals$0 = function (other) {
  return ((this.X === other.X) && (this.Y === other.Y));
};

Microsoft.Xna.Framework.Vector2.prototype.Equals$1 = function (obj) {
  var result = false;

  if (JSIL.TryCast(obj, Microsoft.Xna.Framework.Vector2.MemberwiseClone()) === new Microsoft.Xna.Framework.Vector2()) {
    result = this.Equals(JSIL.Cast(obj, Microsoft.Xna.Framework.Vector2.MemberwiseClone()));
  }
  return result;
};

Microsoft.Xna.Framework.Vector2.prototype.GetHashCode = function () {
  return (this.X.GetHashCode() + this.Y.GetHashCode());
};

Microsoft.Xna.Framework.Vector2.prototype.length = function () {
  return JSIL.Cast(System.Math.Sqrt(((this.X * this.X) + (this.Y * this.Y))), System.Single);
};

Microsoft.Xna.Framework.Vector2.prototype.LengthSquared = function () {
  return ((this.X * this.X) + (this.Y * this.Y));
};

Microsoft.Xna.Framework.Vector2.Distance$0 = function (value1, value2) {
  var num = (value1.X - value2.X);
  var num2 = (value1.Y - value2.Y);
  return JSIL.Cast(System.Math.Sqrt(((num * num) + (num2 * num2))), System.Single);
};

Microsoft.Xna.Framework.Vector2.Distance$1 = function (/* ref */value1, /* ref */value2, /* ref */result) {
  var num = (value1.X - value2.X);
  var num2 = (value1.Y - value2.Y);
  result.value = JSIL.Cast(System.Math.Sqrt(((num * num) + (num2 * num2))), System.Single);
};

Microsoft.Xna.Framework.Vector2.DistanceSquared$0 = function (value1, value2) {
  var num = (value1.X - value2.X);
  var num2 = (value1.Y - value2.Y);
  return ((num * num) + (num2 * num2));
};

Microsoft.Xna.Framework.Vector2.DistanceSquared$1 = function (/* ref */value1, /* ref */value2, /* ref */result) {
  var num = (value1.X - value2.X);
  var num2 = (value1.Y - value2.Y);
  result.value = ((num * num) + (num2 * num2));
};

Microsoft.Xna.Framework.Vector2.Dot$0 = function (value1, value2) {
  return ((value1.X * value2.X) + (value1.Y * value2.Y));
};

Microsoft.Xna.Framework.Vector2.Dot$1 = function (/* ref */value1, /* ref */value2, /* ref */result) {
  result.value = ((value1.X * value2.X) + (value1.Y * value2.Y));
};

Microsoft.Xna.Framework.Vector2.prototype.Normalize = function () {
  var num2 = (1 / JSIL.Cast(System.Math.Sqrt(((this.X * this.X) + (this.Y * this.Y))), System.Single));
  this.X *= num2;
  this.Y *= num2;
};

Microsoft.Xna.Framework.Vector2.Normalize$0 = function (value) {
  var result = new Microsoft.Xna.Framework.Vector2();
  var num2 = (1 / JSIL.Cast(System.Math.Sqrt(((value.X * value.X) + (value.Y * value.Y))), System.Single));
  result.X = (value.X * num2);
  result.Y = (value.Y * num2);
  return result;
};

Microsoft.Xna.Framework.Vector2.Normalize$1 = function (/* ref */value, /* ref */result) {
  var num2 = (1 / JSIL.Cast(System.Math.Sqrt(((value.X * value.X) + (value.Y * value.Y))), System.Single));
  result.X = (value.X * num2);
  result.Y = (value.Y * num2);
};

Microsoft.Xna.Framework.Vector2.Reflect$0 = function (vector, normal) {
  var result = new Microsoft.Xna.Framework.Vector2();
  var num = ((vector.X * normal.X) + (vector.Y * normal.Y));
  result.X = (vector.X - (2 * num * normal.X));
  result.Y = (vector.Y - (2 * num * normal.Y));
  return result;
};

Microsoft.Xna.Framework.Vector2.Reflect$1 = function (/* ref */vector, /* ref */normal, /* ref */result) {
  var num = ((vector.X * normal.X) + (vector.Y * normal.Y));
  result.X = (vector.X - (2 * num * normal.X));
  result.Y = (vector.Y - (2 * num * normal.Y));
};

Microsoft.Xna.Framework.Vector2.Min$0 = function (value1, value2) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X < value2.X) ? value1.X : value2.X;
  result.Y = (value1.Y < value2.Y) ? value1.Y : value2.Y;
  return result;
};

Microsoft.Xna.Framework.Vector2.Min$1 = function (/* ref */value1, /* ref */value2, /* ref */result) {
  result.X = (value1.X < value2.X) ? value1.X : value2.X;
  result.Y = (value1.Y < value2.Y) ? value1.Y : value2.Y;
};

Microsoft.Xna.Framework.Vector2.Max$0 = function (value1, value2) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X > value2.X) ? value1.X : value2.X;
  result.Y = (value1.Y > value2.Y) ? value1.Y : value2.Y;
  return result;
};

Microsoft.Xna.Framework.Vector2.Max$1 = function (/* ref */value1, /* ref */value2, /* ref */result) {
  result.X = (value1.X > value2.X) ? value1.X : value2.X;
  result.Y = (value1.Y > value2.Y) ? value1.Y : value2.Y;
};

Microsoft.Xna.Framework.Vector2.Clamp$0 = function (value1, min, max) {
  var result = new Microsoft.Xna.Framework.Vector2();
  var num = value1.X;
  num = (num > max.X) ? max.X : num;
  num = (num < min.X) ? min.X : num;
  var num2 = value1.Y;
  num2 = (num2 > max.Y) ? max.Y : num2;
  num2 = (num2 < min.Y) ? min.Y : num2;
  result.X = num;
  result.Y = num2;
  return result;
};

Microsoft.Xna.Framework.Vector2.Clamp$1 = function (/* ref */value1, /* ref */min, /* ref */max, /* ref */result) {
  var num = value1.X;
  num = (num > max.X) ? max.X : num;
  num = (num < min.X) ? min.X : num;
  var num2 = value1.Y;
  num2 = (num2 > max.Y) ? max.Y : num2;
  num2 = (num2 < min.Y) ? min.Y : num2;
  result.X = num;
  result.Y = num2;
};

Microsoft.Xna.Framework.Vector2.Lerp$0 = function (value1, value2, amount) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X + ((value2.X - value1.X) * amount));
  result.Y = (value1.Y + ((value2.Y - value1.Y) * amount));
  return result;
};

Microsoft.Xna.Framework.Vector2.Lerp$1 = function (/* ref */value1, /* ref */value2, amount, /* ref */result) {
  result.X = (value1.X + ((value2.X - value1.X) * amount));
  result.Y = (value1.Y + ((value2.Y - value1.Y) * amount));
};

Microsoft.Xna.Framework.Vector2.Barycentric$0 = function (value1, value2, value3, amount1, amount2) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X + (amount1 * (value2.X - value1.X)) + (amount2 * (value3.X - value1.X)));
  result.Y = (value1.Y + (amount1 * (value2.Y - value1.Y)) + (amount2 * (value3.Y - value1.Y)));
  return result;
};

Microsoft.Xna.Framework.Vector2.Barycentric$1 = function (/* ref */value1, /* ref */value2, /* ref */value3, amount1, amount2, /* ref */result) {
  result.X = (value1.X + (amount1 * (value2.X - value1.X)) + (amount2 * (value3.X - value1.X)));
  result.Y = (value1.Y + (amount1 * (value2.Y - value1.Y)) + (amount2 * (value3.Y - value1.Y)));
};

Microsoft.Xna.Framework.Vector2.SmoothStep$0 = function (value1, value2, amount) {
  var result = new Microsoft.Xna.Framework.Vector2();
  amount = (amount > 1) ? 1 : (amount < 0) ? 0 : amount;
  amount = (amount * amount * (3 - (2 * amount)));
  result.X = (value1.X + ((value2.X - value1.X) * amount));
  result.Y = (value1.Y + ((value2.Y - value1.Y) * amount));
  return result;
};

Microsoft.Xna.Framework.Vector2.SmoothStep$1 = function (/* ref */value1, /* ref */value2, amount, /* ref */result) {
  amount = (amount > 1) ? 1 : (amount < 0) ? 0 : amount;
  amount = (amount * amount * (3 - (2 * amount)));
  result.X = (value1.X + ((value2.X - value1.X) * amount));
  result.Y = (value1.Y + ((value2.Y - value1.Y) * amount));
};

Microsoft.Xna.Framework.Vector2.CatmullRom$0 = function (value1, value2, value3, value4, amount) {
  var result = new Microsoft.Xna.Framework.Vector2();
  var num = (amount * amount);
  var num2 = (amount * num);
  result.X = (0.5 * ((2 * value2.X) + ((-value1.X + value3.X) * amount) + (((((2 * value1.X) - (5 * value2.X)) + (4 * value3.X)) - value4.X) * num) + ((((-value1.X + (3 * value2.X)) - (3 * value3.X)) + value4.X) * num2)));
  result.Y = (0.5 * ((2 * value2.Y) + ((-value1.Y + value3.Y) * amount) + (((((2 * value1.Y) - (5 * value2.Y)) + (4 * value3.Y)) - value4.Y) * num) + ((((-value1.Y + (3 * value2.Y)) - (3 * value3.Y)) + value4.Y) * num2)));
  return result;
};

Microsoft.Xna.Framework.Vector2.CatmullRom$1 = function (/* ref */value1, /* ref */value2, /* ref */value3, /* ref */value4, amount, /* ref */result) {
  var num = (amount * amount);
  var num2 = (amount * num);
  result.X = (0.5 * ((2 * value2.X) + ((-value1.X + value3.X) * amount) + (((((2 * value1.X) - (5 * value2.X)) + (4 * value3.X)) - value4.X) * num) + ((((-value1.X + (3 * value2.X)) - (3 * value3.X)) + value4.X) * num2)));
  result.Y = (0.5 * ((2 * value2.Y) + ((-value1.Y + value3.Y) * amount) + (((((2 * value1.Y) - (5 * value2.Y)) + (4 * value3.Y)) - value4.Y) * num) + ((((-value1.Y + (3 * value2.Y)) - (3 * value3.Y)) + value4.Y) * num2)));
};

Microsoft.Xna.Framework.Vector2.Hermite$0 = function (value1, tangent1, value2, tangent2, amount) {
  var result = new Microsoft.Xna.Framework.Vector2();
  var num = (amount * amount);
  var num2 = (amount * num);
  var num3 = (((2 * num2) - (3 * num)) + 1);
  var num4 = ((-2 * num2) + (3 * num));
  var num5 = ((num2 - (2 * num)) + amount);
  var num6 = (num2 - num);
  result.X = ((value1.X * num3) + (value2.X * num4) + (tangent1.X * num5) + (tangent2.X * num6));
  result.Y = ((value1.Y * num3) + (value2.Y * num4) + (tangent1.Y * num5) + (tangent2.Y * num6));
  return result;
};

Microsoft.Xna.Framework.Vector2.Hermite$1 = function (/* ref */value1, /* ref */tangent1, /* ref */value2, /* ref */tangent2, amount, /* ref */result) {
  var num = (amount * amount);
  var num2 = (amount * num);
  var num3 = (((2 * num2) - (3 * num)) + 1);
  var num4 = ((-2 * num2) + (3 * num));
  var num5 = ((num2 - (2 * num)) + amount);
  var num6 = (num2 - num);
  result.X = ((value1.X * num3) + (value2.X * num4) + (tangent1.X * num5) + (tangent2.X * num6));
  result.Y = ((value1.Y * num3) + (value2.Y * num4) + (tangent1.Y * num5) + (tangent2.Y * num6));
};

Microsoft.Xna.Framework.Vector2.Transform$0 = function (position, matrix) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = ((position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41);
  result.Y = ((position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42);
  return result;
};

Microsoft.Xna.Framework.Vector2.Transform$1 = function (/* ref */position, /* ref */matrix, /* ref */result) {
  result.X = ((position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41);
  result.Y = ((position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42);
};

Microsoft.Xna.Framework.Vector2.TransformNormal$0 = function (normal, matrix) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = ((normal.X * matrix.M11) + (normal.Y * matrix.M21));
  result.Y = ((normal.X * matrix.M12) + (normal.Y * matrix.M22));
  return result;
};

Microsoft.Xna.Framework.Vector2.TransformNormal$1 = function (/* ref */normal, /* ref */matrix, /* ref */result) {
  result.X = ((normal.X * matrix.M11) + (normal.Y * matrix.M21));
  result.Y = ((normal.X * matrix.M12) + (normal.Y * matrix.M22));
};

Microsoft.Xna.Framework.Vector2.Transform$2 = function (value, rotation) {
  var result = new Microsoft.Xna.Framework.Vector2();
  var num2 = (rotation.Y + rotation.Y);
  var num3 = (rotation.Z + rotation.Z);
  var num4 = (rotation.W * num3);
  var num6 = (rotation.X * num2);
  var num8 = (rotation.Z * num3);
  result.X = ((value.X * (1 - (rotation.Y * num2) - num8)) + (value.Y * (num6 - num4)));
  result.Y = ((value.X * (num6 + num4)) + (value.Y * (1 - (rotation.X * (rotation.X + rotation.X)) - num8)));
  return result;
};

Microsoft.Xna.Framework.Vector2.Transform$3 = function (/* ref */value, /* ref */rotation, /* ref */result) {
  var num2 = (rotation.Y + rotation.Y);
  var num3 = (rotation.Z + rotation.Z);
  var num4 = (rotation.W * num3);
  var num6 = (rotation.X * num2);
  var num8 = (rotation.Z * num3);
  result.X = ((value.X * (1 - (rotation.Y * num2) - num8)) + (value.Y * (num6 - num4)));
  result.Y = ((value.X * (num6 + num4)) + (value.Y * (1 - (rotation.X * (rotation.X + rotation.X)) - num8)));
};

Microsoft.Xna.Framework.Vector2.Transform$4 = function (sourceArray, /* ref */matrix, destinationArray) {

  if (sourceArray !== null) {
    throw new System.ArgumentNullException("sourceArray");
  }

  if (destinationArray !== null) {
    throw new System.ArgumentNullException("destinationArray");
  }

  if (destinationArray.length < sourceArray.length) {
    throw new System.ArgumentException(Microsoft.Xna.Framework.FrameworkResources.NotEnoughTargetSize);
  }
  var i = 0;

  __while0__:
  while (i < sourceArray.length) {
    var x = sourceArray[i].X;
    var y = sourceArray[i].Y;
    destinationArray[i].X = ((x * matrix.M11) + (y * matrix.M21) + matrix.M41);
    destinationArray[i].Y = ((x * matrix.M12) + (y * matrix.M22) + matrix.M42);
    ++i;
  }
};

Microsoft.Xna.Framework.Vector2.Transform$5 = function (sourceArray, sourceIndex, /* ref */matrix, destinationArray, destinationIndex, length) {

  if (sourceArray !== null) {
    throw new System.ArgumentNullException("sourceArray");
  }

  if (destinationArray !== null) {
    throw new System.ArgumentNullException("destinationArray");
  }

  if (sourceArray.length < (sourceIndex + length)) {
    throw new System.ArgumentException(Microsoft.Xna.Framework.FrameworkResources.NotEnoughSourceSize);
  }

  if (destinationArray.length < (destinationIndex + length)) {
    throw new System.ArgumentException(Microsoft.Xna.Framework.FrameworkResources.NotEnoughTargetSize);
  }

  __while0__:
  while (length > 0) {
    var x = sourceArray[sourceIndex].X;
    var y = sourceArray[sourceIndex].Y;
    destinationArray[destinationIndex].X = ((x * matrix.M11) + (y * matrix.M21) + matrix.M41);
    destinationArray[destinationIndex].Y = ((x * matrix.M12) + (y * matrix.M22) + matrix.M42);
    ++sourceIndex;
    ++destinationIndex;
    --length;
  }
};

Microsoft.Xna.Framework.Vector2.TransformNormal$2 = function (sourceArray, /* ref */matrix, destinationArray) {

  if (sourceArray !== null) {
    throw new System.ArgumentNullException("sourceArray");
  }

  if (destinationArray !== null) {
    throw new System.ArgumentNullException("destinationArray");
  }

  if (destinationArray.length < sourceArray.length) {
    throw new System.ArgumentException(Microsoft.Xna.Framework.FrameworkResources.NotEnoughTargetSize);
  }
  var i = 0;

  __while0__:
  while (i < sourceArray.length) {
    var x = sourceArray[i].X;
    var y = sourceArray[i].Y;
    destinationArray[i].X = ((x * matrix.M11) + (y * matrix.M21));
    destinationArray[i].Y = ((x * matrix.M12) + (y * matrix.M22));
    ++i;
  }
};

Microsoft.Xna.Framework.Vector2.TransformNormal$3 = function (sourceArray, sourceIndex, /* ref */matrix, destinationArray, destinationIndex, length) {

  if (sourceArray !== null) {
    throw new System.ArgumentNullException("sourceArray");
  }

  if (destinationArray !== null) {
    throw new System.ArgumentNullException("destinationArray");
  }

  if (sourceArray.length < (sourceIndex + length)) {
    throw new System.ArgumentException(Microsoft.Xna.Framework.FrameworkResources.NotEnoughSourceSize);
  }

  if (destinationArray.length < (destinationIndex + length)) {
    throw new System.ArgumentException(Microsoft.Xna.Framework.FrameworkResources.NotEnoughTargetSize);
  }

  __while0__:
  while (length > 0) {
    var x = sourceArray[sourceIndex].X;
    var y = sourceArray[sourceIndex].Y;
    destinationArray[destinationIndex].X = ((x * matrix.M11) + (y * matrix.M21));
    destinationArray[destinationIndex].Y = ((x * matrix.M12) + (y * matrix.M22));
    ++sourceIndex;
    ++destinationIndex;
    --length;
  }
};

Microsoft.Xna.Framework.Vector2.Transform$6 = function (sourceArray, /* ref */rotation, destinationArray) {

  if (sourceArray !== null) {
    throw new System.ArgumentNullException("sourceArray");
  }

  if (destinationArray !== null) {
    throw new System.ArgumentNullException("destinationArray");
  }

  if (destinationArray.length < sourceArray.length) {
    throw new System.ArgumentException(Microsoft.Xna.Framework.FrameworkResources.NotEnoughTargetSize);
  }
  var num2 = (rotation.Y + rotation.Y);
  var num3 = (rotation.Z + rotation.Z);
  var num4 = (rotation.W * num3);
  var num6 = (rotation.X * num2);
  var num8 = (rotation.Z * num3);
  var i = 0;

  __while0__:
  while (i < sourceArray.length) {
    var x = sourceArray[i].X;
    var y = sourceArray[i].Y;
    destinationArray[i].X = ((x * (1 - (rotation.Y * num2) - num8)) + (y * (num6 - num4)));
    destinationArray[i].Y = ((x * (num6 + num4)) + (y * (1 - (rotation.X * (rotation.X + rotation.X)) - num8)));
    ++i;
  }
};

Microsoft.Xna.Framework.Vector2.Transform$7 = function (sourceArray, sourceIndex, /* ref */rotation, destinationArray, destinationIndex, length) {

  if (sourceArray !== null) {
    throw new System.ArgumentNullException("sourceArray");
  }

  if (destinationArray !== null) {
    throw new System.ArgumentNullException("destinationArray");
  }

  if (sourceArray.length < (sourceIndex + length)) {
    throw new System.ArgumentException(Microsoft.Xna.Framework.FrameworkResources.NotEnoughSourceSize);
  }

  if (destinationArray.length < (destinationIndex + length)) {
    throw new System.ArgumentException(Microsoft.Xna.Framework.FrameworkResources.NotEnoughTargetSize);
  }
  var num2 = (rotation.Y + rotation.Y);
  var num3 = (rotation.Z + rotation.Z);
  var num4 = (rotation.W * num3);
  var num6 = (rotation.X * num2);
  var num8 = (rotation.Z * num3);

  __while0__:
  while (length > 0) {
    var x = sourceArray[sourceIndex].X;
    var y = sourceArray[sourceIndex].Y;
    destinationArray[destinationIndex].X = ((x * (1 - (rotation.Y * num2) - num8)) + (y * (num6 - num4)));
    destinationArray[destinationIndex].Y = ((x * (num6 + num4)) + (y * (1 - (rotation.X * (rotation.X + rotation.X)) - num8)));
    ++sourceIndex;
    ++destinationIndex;
    --length;
  }
};

Microsoft.Xna.Framework.Vector2.Negate$0 = function (value) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = -value.X;
  result.Y = -value.Y;
  return result;
};

Microsoft.Xna.Framework.Vector2.Negate$1 = function (/* ref */value, /* ref */result) {
  result.X = -value.X;
  result.Y = -value.Y;
};

Microsoft.Xna.Framework.Vector2.Add$0 = function (value1, value2) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X + value2.X);
  result.Y = (value1.Y + value2.Y);
  return result;
};

Microsoft.Xna.Framework.Vector2.Add$1 = function (/* ref */value1, /* ref */value2, /* ref */result) {
  result.X = (value1.X + value2.X);
  result.Y = (value1.Y + value2.Y);
};

Microsoft.Xna.Framework.Vector2.Subtract$0 = function (value1, value2) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X - value2.X);
  result.Y = (value1.Y - value2.Y);
  return result;
};

Microsoft.Xna.Framework.Vector2.Subtract$1 = function (/* ref */value1, /* ref */value2, /* ref */result) {
  result.X = (value1.X - value2.X);
  result.Y = (value1.Y - value2.Y);
};

Microsoft.Xna.Framework.Vector2.Multiply$0 = function (value1, value2) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X * value2.X);
  result.Y = (value1.Y * value2.Y);
  return result;
};

Microsoft.Xna.Framework.Vector2.Multiply$1 = function (/* ref */value1, /* ref */value2, /* ref */result) {
  result.X = (value1.X * value2.X);
  result.Y = (value1.Y * value2.Y);
};

Microsoft.Xna.Framework.Vector2.Multiply$2 = function (value1, scaleFactor) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X * scaleFactor);
  result.Y = (value1.Y * scaleFactor);
  return result;
};

Microsoft.Xna.Framework.Vector2.Multiply$3 = function (/* ref */value1, scaleFactor, /* ref */result) {
  result.X = (value1.X * scaleFactor);
  result.Y = (value1.Y * scaleFactor);
};

Microsoft.Xna.Framework.Vector2.Divide$0 = function (value1, value2) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X / value2.X);
  result.Y = (value1.Y / value2.Y);
  return result;
};

Microsoft.Xna.Framework.Vector2.Divide$1 = function (/* ref */value1, /* ref */value2, /* ref */result) {
  result.X = (value1.X / value2.X);
  result.Y = (value1.Y / value2.Y);
};

Microsoft.Xna.Framework.Vector2.Divide$2 = function (value1, divider) {
  var result = new Microsoft.Xna.Framework.Vector2();
  var num = (1 / divider);
  result.X = (value1.X * num);
  result.Y = (value1.Y * num);
  return result;
};

Microsoft.Xna.Framework.Vector2.Divide$3 = function (/* ref */value1, divider, /* ref */result) {
  var num = (1 / divider);
  result.X = (value1.X * num);
  result.Y = (value1.Y * num);
};

Microsoft.Xna.Framework.Vector2.op_UnaryNegation = function (value) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = -value.X;
  result.Y = -value.Y;
  return result;
};

Microsoft.Xna.Framework.Vector2.op_Equality = function (value1, value2) {
  return ((value1.X === value2.X) && (value1.Y === value2.Y));
};

Microsoft.Xna.Framework.Vector2.op_Inequality = function (value1, value2) {
  return ((value1.X !== value2.X) || (value1.Y !== value2.Y));
};

Microsoft.Xna.Framework.Vector2.op_Addition = function (value1, value2) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X + value2.X);
  result.Y = (value1.Y + value2.Y);
  return result;
};

Microsoft.Xna.Framework.Vector2.op_Subtraction = function (value1, value2) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X - value2.X);
  result.Y = (value1.Y - value2.Y);
  return result;
};

Microsoft.Xna.Framework.Vector2.op_Multiply$0 = function (value1, value2) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X * value2.X);
  result.Y = (value1.Y * value2.Y);
  return result;
};

Microsoft.Xna.Framework.Vector2.op_Multiply$1 = function (value, scaleFactor) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value.X * scaleFactor);
  result.Y = (value.Y * scaleFactor);
  return result;
};

Microsoft.Xna.Framework.Vector2.op_Multiply$2 = function (scaleFactor, value) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value.X * scaleFactor);
  result.Y = (value.Y * scaleFactor);
  return result;
};

Microsoft.Xna.Framework.Vector2.op_Division$0 = function (value1, value2) {
  var result = new Microsoft.Xna.Framework.Vector2();
  result.X = (value1.X / value2.X);
  result.Y = (value1.Y / value2.Y);
  return result;
};

Microsoft.Xna.Framework.Vector2.op_Division$1 = function (value1, divider) {
  var result = new Microsoft.Xna.Framework.Vector2();
  var num = (1 / divider);
  result.X = (value1.X * num);
  result.Y = (value1.Y * num);
  return result;
};

Microsoft.Xna.Framework.Vector2._cctor = function () {
  Microsoft.Xna.Framework.Vector2._zero = new Microsoft.Xna.Framework.Vector2();
  Microsoft.Xna.Framework.Vector2._one = new Microsoft.Xna.Framework.Vector2(1, 1);
  Microsoft.Xna.Framework.Vector2._unitX = new Microsoft.Xna.Framework.Vector2(1, 0);
  Microsoft.Xna.Framework.Vector2._unitY = new Microsoft.Xna.Framework.Vector2(0, 1);
};

JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2.prototype, "_ctor", [
		["_ctor$0", [System.Single, System.Single]],
		["_ctor$1", [System.Single]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2.prototype, "Equals", [
		["Equals$0", [Microsoft.Xna.Framework.Vector2]],
		["Equals$1", [System.Object]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Distance", [
		["Distance$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["Distance$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(System.Single)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "DistanceSquared", [
		["DistanceSquared$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["DistanceSquared$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(System.Single)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Dot", [
		["Dot$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["Dot$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(System.Single)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Normalize", [
		["Normalize$0", [Microsoft.Xna.Framework.Vector2]],
		["Normalize$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Reflect", [
		["Reflect$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["Reflect$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Min", [
		["Min$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["Min$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Max", [
		["Max$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["Max$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Clamp", [
		["Clamp$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["Clamp$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Lerp", [
		["Lerp$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, System.Single]],
		["Lerp$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), System.Single, JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Barycentric", [
		["Barycentric$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, System.Single, System.Single]],
		["Barycentric$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), System.Single, System.Single, JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "SmoothStep", [
		["SmoothStep$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, System.Single]],
		["SmoothStep$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), System.Single, JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "CatmullRom", [
		["CatmullRom$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, System.Single]],
		["CatmullRom$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), System.Single, JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Hermite", [
		["Hermite$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, System.Single]],
		["Hermite$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), System.Single, JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Transform", [
		["Transform$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Matrix]],
		["Transform$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Matrix), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]],
		["Transform$2", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Quaternion]],
		["Transform$3", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Quaternion), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]],
		["Transform$4", [System.Array.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Matrix), System.Array.Of(Microsoft.Xna.Framework.Vector2)]],
		["Transform$5", [System.Array.Of(Microsoft.Xna.Framework.Vector2), System.Int32, JSIL.Reference.Of(Microsoft.Xna.Framework.Matrix), System.Array.Of(Microsoft.Xna.Framework.Vector2), System.Int32, System.Int32]],
		["Transform$6", [System.Array.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Quaternion), System.Array.Of(Microsoft.Xna.Framework.Vector2)]],
		["Transform$7", [System.Array.Of(Microsoft.Xna.Framework.Vector2), System.Int32, JSIL.Reference.Of(Microsoft.Xna.Framework.Quaternion), System.Array.Of(Microsoft.Xna.Framework.Vector2), System.Int32, System.Int32]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "TransformNormal", [
		["TransformNormal$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Matrix]],
		["TransformNormal$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Matrix), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]],
		["TransformNormal$2", [System.Array.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Matrix), System.Array.Of(Microsoft.Xna.Framework.Vector2)]],
		["TransformNormal$3", [System.Array.Of(Microsoft.Xna.Framework.Vector2), System.Int32, JSIL.Reference.Of(Microsoft.Xna.Framework.Matrix), System.Array.Of(Microsoft.Xna.Framework.Vector2), System.Int32, System.Int32]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Negate", [
		["Negate$0", [Microsoft.Xna.Framework.Vector2]],
		["Negate$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Add", [
		["Add$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["Add$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Subtract", [
		["Subtract$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["Subtract$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Multiply", [
		["Multiply$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["Multiply$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]],
		["Multiply$2", [Microsoft.Xna.Framework.Vector2, System.Single]],
		["Multiply$3", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), System.Single, JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "Divide", [
		["Divide$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["Divide$1", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]],
		["Divide$2", [Microsoft.Xna.Framework.Vector2, System.Single]],
		["Divide$3", [JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2), System.Single, JSIL.Reference.Of(Microsoft.Xna.Framework.Vector2)]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "op_Multiply", [
		["op_Multiply$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["op_Multiply$1", [Microsoft.Xna.Framework.Vector2, System.Single]],
		["op_Multiply$2", [System.Single, Microsoft.Xna.Framework.Vector2]]
	]
);
JSIL.OverloadedMethod(Microsoft.Xna.Framework.Vector2, "op_Division", [
		["op_Division$0", [Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2]],
		["op_Division$1", [Microsoft.Xna.Framework.Vector2, System.Single]]
	]
);
Object.defineProperty(Microsoft.Xna.Framework.Vector2, "Zero", {
  get: Microsoft.Xna.Framework.Vector2.get_Zero
});
Object.defineProperty(Microsoft.Xna.Framework.Vector2, "One", {
  get: Microsoft.Xna.Framework.Vector2.get_One
});
Object.defineProperty(Microsoft.Xna.Framework.Vector2, "UnitX", {
  get: Microsoft.Xna.Framework.Vector2.get_UnitX
});
Object.defineProperty(Microsoft.Xna.Framework.Vector2, "UnitY", {
  get: Microsoft.Xna.Framework.Vector2.get_UnitY
});
Microsoft.Xna.Framework.Vector2._cctor();
Microsoft.Xna.Framework.Vector2.prototype.__ImplementInterface__(System.IEquatable$b1.Of(Microsoft.Xna.Framework.Vector2));

Object.seal(Microsoft.Xna.Framework.Vector2.prototype);
Object.seal(Microsoft.Xna.Framework.Vector2);

// ======================================================================================
// Vertex types
// ======================================================================================

Microsoft.Xna.Framework.Graphics.VertexPositionColor.prototype.__StructFields__ = {
  Position: Microsoft.Xna.Framework.Vector3,
  Color: Microsoft.Xna.Framework.Graphics.Color
};
Microsoft.Xna.Framework.Graphics.VertexPositionColor.prototype._ctor = function (position, color) {
  this.Position = position;
  this.Color = color;
};
Microsoft.Xna.Framework.Graphics.VertexPositionColor.prototype.toString = function () {
  return System.String.Format("{{Position:{0} Color:{1}}}", [this.Position, this.Color]);
};
Microsoft.Xna.Framework.Graphics.VertexPositionColor.op_Equality = function (left, right) {
  return (Microsoft.Xna.Framework.Graphics.Color.op_Equality(left.Color.MemberwiseClone(), right.Color.MemberwiseClone()) && Microsoft.Xna.Framework.Vector3.op_Equality(left.Position.MemberwiseClone(), right.Position.MemberwiseClone()));
};
Microsoft.Xna.Framework.Graphics.VertexPositionColor.op_Inequality = function (left, right) {
  return !Microsoft.Xna.Framework.Graphics.VertexPositionColor.op_Equality(left, right);
};
Microsoft.Xna.Framework.Graphics.VertexPositionColor.prototype.Equals = function (obj) {
  return (obj &&
		(obj.GetType() === this.GetType()) && Microsoft.Xna.Framework.Graphics.VertexPositionColor.op_Equality(this, JSIL.Cast(obj, Microsoft.Xna.Framework.Graphics.VertexPositionColor.MemberwiseClone())));
};

Object.seal(Microsoft.Xna.Framework.Graphics.VertexPositionColor.prototype);
Object.seal(Microsoft.Xna.Framework.Graphics.VertexPositionColor);

Microsoft.Xna.Framework.Graphics.VertexPositionTexture.prototype.__StructFields__ = {
  Position: Microsoft.Xna.Framework.Vector3,
  TextureCoordinate: Microsoft.Xna.Framework.Vector2
};
Microsoft.Xna.Framework.Graphics.VertexPositionTexture.prototype._ctor = function (position, textureCoordinate) {
  this.Position = position;
  this.TextureCoordinate = textureCoordinate;
};
Microsoft.Xna.Framework.Graphics.VertexPositionTexture.prototype.toString = function () {
  return System.String.Format("{{Position:{0} TextureCoordinate:{1}}}", [this.Position, this.TextureCoordinate]);
};
Microsoft.Xna.Framework.Graphics.VertexPositionTexture.op_Equality = function (left, right) {
  return (Microsoft.Xna.Framework.Vector3.op_Equality(left.Position.MemberwiseClone(), right.Position.MemberwiseClone()) && Microsoft.Xna.Framework.Vector2.op_Equality(left.TextureCoordinate.MemberwiseClone(), right.TextureCoordinate.MemberwiseClone()));
};
Microsoft.Xna.Framework.Graphics.VertexPositionTexture.op_Inequality = function (left, right) {
  return !Microsoft.Xna.Framework.Graphics.VertexPositionTexture.op_Equality(left, right);
};
Microsoft.Xna.Framework.Graphics.VertexPositionTexture.prototype.Equals = function (obj) {
  return (obj &&
		(obj.GetType() === this.GetType()) && Microsoft.Xna.Framework.Graphics.VertexPositionTexture.op_Equality(this, JSIL.Cast(obj, Microsoft.Xna.Framework.Graphics.VertexPositionTexture.MemberwiseClone())));
};

Object.seal(Microsoft.Xna.Framework.Graphics.VertexPositionTexture.prototype);
Object.seal(Microsoft.Xna.Framework.Graphics.VertexPositionTexture);

Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture.VertexElements = null;
Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture.prototype.__StructFields__ = {
  Position: Microsoft.Xna.Framework.Vector3,
  Color: Microsoft.Xna.Framework.Graphics.Color,
  TextureCoordinate: Microsoft.Xna.Framework.Vector2
};
Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture.prototype._ctor = function (position, color, textureCoordinate) {
  this.Position = position;
  this.Color = color;
  this.TextureCoordinate = textureCoordinate;
};
Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture.prototype.toString = function () {
  return System.String.Format("{{Position:{0} Color:{1} TextureCoordinate:{2}}}", [this.Position, this.Color, this.TextureCoordinate]);
};
Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture.op_Equality = function (left, right) {
  return (!(!Microsoft.Xna.Framework.Vector3.op_Equality(left.Position.MemberwiseClone(), right.Position.MemberwiseClone()) ||
			!Microsoft.Xna.Framework.Graphics.Color.op_Equality(left.Color.MemberwiseClone(), right.Color.MemberwiseClone())) && Microsoft.Xna.Framework.Vector2.op_Equality(left.TextureCoordinate.MemberwiseClone(), right.TextureCoordinate.MemberwiseClone()));
};
Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture.op_Inequality = function (left, right) {
  return !Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture.op_Equality(left, right);
};
Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture.prototype.Equals = function (obj) {
  return (obj &&
		(obj.GetType() === this.GetType()) && Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture.op_Equality(this, JSIL.Cast(obj, Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture.MemberwiseClone())));
};

Object.seal(Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture.prototype);
Object.seal(Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture);

Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.prototype.__StructFields__ = {
  Position: Microsoft.Xna.Framework.Vector3,
  Normal: Microsoft.Xna.Framework.Vector3,
  TextureCoordinate: Microsoft.Xna.Framework.Vector2
};
Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.prototype._ctor = function (position, normal, textureCoordinate) {
  this.Position = position;
  this.Normal = normal;
  this.TextureCoordinate = textureCoordinate;
};
Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.prototype.toString = function () {
  return System.String.Format("{{Position:{0} Normal:{1} TextureCoordinate:{2}}}", [this.Position, this.Normal, this.TextureCoordinate]);
};
Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.op_Equality = function (left, right) {
  return (!(!Microsoft.Xna.Framework.Vector3.op_Equality(left.Position.MemberwiseClone(), right.Position.MemberwiseClone()) ||
			!Microsoft.Xna.Framework.Vector3.op_Equality(left.Normal.MemberwiseClone(), right.Normal.MemberwiseClone())) && Microsoft.Xna.Framework.Vector2.op_Equality(left.TextureCoordinate.MemberwiseClone(), right.TextureCoordinate.MemberwiseClone()));
};
Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.op_Inequality = function (left, right) {
  return !Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.op_Equality(left, right);
};
Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.prototype.Equals = function (obj) {
  return (obj &&
		(obj.GetType() === this.GetType()) && Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.op_Equality(this, JSIL.Cast(obj, Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.MemberwiseClone())));
};
Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture._cctor = function () {
  Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.VertexElements = JSIL.Array.New(Microsoft.Xna.Framework.Graphics.VertexElement.MemberwiseClone(), [new Microsoft.Xna.Framework.Graphics.VertexElement(0, 0, Microsoft.Xna.Framework.Graphics.VertexElementFormat.Vector3, Microsoft.Xna.Framework.Graphics.VertexElementMethod.Default, Microsoft.Xna.Framework.Graphics.VertexElementUsage.Position, 0), new Microsoft.Xna.Framework.Graphics.VertexElement(0, 12, Microsoft.Xna.Framework.Graphics.VertexElementFormat.Vector3, Microsoft.Xna.Framework.Graphics.VertexElementMethod.Default, Microsoft.Xna.Framework.Graphics.VertexElementUsage.Normal, 0), new Microsoft.Xna.Framework.Graphics.VertexElement(0, 24, Microsoft.Xna.Framework.Graphics.VertexElementFormat.Vector2, Microsoft.Xna.Framework.Graphics.VertexElementMethod.Default, Microsoft.Xna.Framework.Graphics.VertexElementUsage.TextureCoordinate, 0)].MemberwiseClone());
};

Object.seal(Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.prototype);
Object.seal(Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture);

// ======================================================================================
// Game
// =====================================================================================

Microsoft.Xna.Framework.Game.prototype.graphicsDeviceManager = null;
Microsoft.Xna.Framework.Game.prototype.graphicsDeviceService = null;
Microsoft.Xna.Framework.Game.prototype.host = null;
Microsoft.Xna.Framework.Game.prototype.isActive = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.exitRequested = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.isMouseVisible = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.inRun = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.gameTime = null;
Microsoft.Xna.Framework.Game.prototype.clock = null;
Microsoft.Xna.Framework.Game.prototype.isFixedTimeStep = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.drawRunningSlowly = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.updatesSinceRunningSlowly1 = 0;
Microsoft.Xna.Framework.Game.prototype.updatesSinceRunningSlowly2 = 0;
Microsoft.Xna.Framework.Game.prototype.doneFirstUpdate = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.doneFirstDraw = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.forceElapsedTimeToZero = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.suppressDraw = new System.Boolean();
Microsoft.Xna.Framework.Game.prototype.gameComponents = null;
Microsoft.Xna.Framework.Game.prototype.updateableComponents = null;
Microsoft.Xna.Framework.Game.prototype.currentlyUpdatingComponents = null;
Microsoft.Xna.Framework.Game.prototype.drawableComponents = null;
Microsoft.Xna.Framework.Game.prototype.currentlyDrawingComponents = null;
Microsoft.Xna.Framework.Game.prototype.notYetInitialized = null;
Microsoft.Xna.Framework.Game.prototype.gameServices = null;
Microsoft.Xna.Framework.Game.prototype.content = null;
Microsoft.Xna.Framework.Game.prototype.Activated = null;
Microsoft.Xna.Framework.Game.prototype.Deactivated = null;
Microsoft.Xna.Framework.Game.prototype.Exiting = null;
Microsoft.Xna.Framework.Game.prototype.Disposed = null;
Microsoft.Xna.Framework.Game.prototype.__StructFields__ = {
  maximumElapsedTime: System.TimeSpan,
  inactiveSleepTime: System.TimeSpan,
  lastFrameElapsedRealTime: System.TimeSpan,
  totalGameTime: System.TimeSpan,
  targetElapsedTime: System.TimeSpan,
  accumulatedElapsedGameTime: System.TimeSpan,
  lastFrameElapsedGameTime: System.TimeSpan
};
Microsoft.Xna.Framework.Game.prototype.get_Components = function () {
  return this.gameComponents;
};

Microsoft.Xna.Framework.Game.prototype.get_Services = function () {
  return this.gameServices;
};

Microsoft.Xna.Framework.Game.prototype.get_InactiveSleepTime = function () {
  return this.inactiveSleepTime;
};

Microsoft.Xna.Framework.Game.prototype.set_InactiveSleepTime = function (value) {

  if (System.TimeSpan.op_LessThan(value.MemberwiseClone(), System.TimeSpan.Zero.MemberwiseClone())) {
    throw new System.ArgumentOutOfRangeException("value", Microsoft.Xna.Framework.Resources.InactiveSleepTimeCannotBeZero);
  }
  this.inactiveSleepTime = value.MemberwiseClone();
};

Microsoft.Xna.Framework.Game.prototype.get_IsMouseVisible = function () {
  return this.isMouseVisible;
};

Microsoft.Xna.Framework.Game.prototype.set_IsMouseVisible = function (value) {
  this.isMouseVisible = value;

  if (this.get_Window() === null) {
    this.get_Window().IsMouseVisible = value;
  }
};

Microsoft.Xna.Framework.Game.prototype.get_TargetElapsedTime = function () {
  return this.targetElapsedTime;
};

Microsoft.Xna.Framework.Game.prototype.set_TargetElapsedTime = function (value) {

  if (System.TimeSpan.op_LessThanOrEqual(value.MemberwiseClone(), System.TimeSpan.Zero.MemberwiseClone())) {
    throw new System.ArgumentOutOfRangeException("value", Microsoft.Xna.Framework.Resources.TargetElaspedCannotBeZero);
  }
  this.targetElapsedTime = value.MemberwiseClone();
};

Microsoft.Xna.Framework.Game.prototype.get_IsFixedTimeStep = function () {
  return this.isFixedTimeStep;
};

Microsoft.Xna.Framework.Game.prototype.set_IsFixedTimeStep = function (value) {
  this.isFixedTimeStep = value;
};

Microsoft.Xna.Framework.Game.prototype.get_Window = function () {

  if (this.host === null) {
    return this.host.Window;
  }
  return null;
};

Microsoft.Xna.Framework.Game.prototype.get_IsActive = function () {
  var flag = false;

  if (Microsoft.Xna.Framework.GamerServices.GamerServicesDispatcher.IsInitialized) {
    flag = Microsoft.Xna.Framework.GamerServices.Guide.IsVisible;
  }
  return (this.isActive && !flag);
};

Microsoft.Xna.Framework.Game.prototype.get_GraphicsDevice = function () {
  var graphicsDeviceService = this.graphicsDeviceService;

  if (graphicsDeviceService !== null) {
    graphicsDeviceService = JSIL.TryCast(this.get_Services().GetService(Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService), Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService);

    if (graphicsDeviceService !== null) {
      throw new System.InvalidOperationException(Microsoft.Xna.Framework.Resources.NoGraphicsDeviceService);
    }
  }
  return graphicsDeviceService.IGraphicsDeviceService_GraphicsDevice;
};

Microsoft.Xna.Framework.Game.prototype.get_Content = function () {
  return this.content;
};

Microsoft.Xna.Framework.Game.prototype.set_Content = function (value) {

  if (value !== null) {
    throw new System.ArgumentNullException();
  }
  this.content = value;
};

Microsoft.Xna.Framework.Game.prototype.get_IsActiveIgnoringGuide = function () {
  return this.isActive;
};

Microsoft.Xna.Framework.Game.prototype.add_Activated = function (value) {
  this.Activated = System.Delegate.Combine(this.Activated, value);
};

Microsoft.Xna.Framework.Game.prototype.remove_Activated = function (value) {
  this.Activated = System.Delegate.Remove(this.Activated, value);
};

Microsoft.Xna.Framework.Game.prototype.add_Deactivated = function (value) {
  this.Deactivated = System.Delegate.Combine(this.Deactivated, value);
};

Microsoft.Xna.Framework.Game.prototype.remove_Deactivated = function (value) {
  this.Deactivated = System.Delegate.Remove(this.Deactivated, value);
};

Microsoft.Xna.Framework.Game.prototype.add_Exiting = function (value) {
  this.Exiting = System.Delegate.Combine(this.Exiting, value);
};

Microsoft.Xna.Framework.Game.prototype.remove_Exiting = function (value) {
  this.Exiting = System.Delegate.Remove(this.Exiting, value);
};

Microsoft.Xna.Framework.Game.prototype.add_Disposed = function (value) {
  this.Disposed = System.Delegate.Combine(this.Disposed, value);
};

Microsoft.Xna.Framework.Game.prototype.remove_Disposed = function (value) {
  this.Disposed = System.Delegate.Remove(this.Disposed, value);
};

Microsoft.Xna.Framework.Game.prototype._ctor = function () {
  this.maximumElapsedTime = System.TimeSpan.FromMilliseconds(500);
  this.gameTime = new Microsoft.Xna.Framework.GameTime();
  this.isFixedTimeStep = true;
  this.updatesSinceRunningSlowly1 = 2147483647;
  this.updatesSinceRunningSlowly2 = 2147483647;
  this.updateableComponents = new (System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.IUpdateable))();
  this.currentlyUpdatingComponents = new (System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.IUpdateable))();
  this.drawableComponents = new (System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.IDrawable))();
  this.currentlyDrawingComponents = new (System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.IDrawable))();
  this.notYetInitialized = new (System.Collections.Generic.List$b1.Of(Microsoft.Xna.Framework.IGameComponent))();
  this.gameServices = new Microsoft.Xna.Framework.GameServiceContainer();
  System.Object.prototype._ctor.call(this);
  this.EnsureHost();
  this.gameComponents = new Microsoft.Xna.Framework.GameComponentCollection();
  this.gameComponents.add_ComponentAdded(JSIL.Delegate.New("System.EventHandler`1[Microsoft.Xna.Framework.GameComponentCollectionEventArgs]", this, Microsoft.Xna.Framework.Game.prototype.GameComponentAdded));
  this.gameComponents.add_ComponentRemoved(JSIL.Delegate.New("System.EventHandler`1[Microsoft.Xna.Framework.GameComponentCollectionEventArgs]", this, Microsoft.Xna.Framework.Game.prototype.GameComponentRemoved));
  this.content = new Microsoft.Xna.Framework.Content.ContentManager(this.gameServices);
  this.host.Window.add_Paint(JSIL.Delegate.New("System.EventHandler", this, Microsoft.Xna.Framework.Game.prototype.Paint));
  this.clock = new Microsoft.Xna.Framework.GameClock();
  this.totalGameTime = System.TimeSpan.Zero.MemberwiseClone();
  this.accumulatedElapsedGameTime = System.TimeSpan.Zero.MemberwiseClone();
  this.lastFrameElapsedGameTime = System.TimeSpan.Zero.MemberwiseClone();
  this.targetElapsedTime = System.TimeSpan.FromTicks(166667);
  this.inactiveSleepTime = System.TimeSpan.FromMilliseconds(20);
};