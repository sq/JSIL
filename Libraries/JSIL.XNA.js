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

Microsoft.Xna.Framework.Game.prototype._runHandle = null;
Microsoft.Xna.Framework.Game.prototype._ctor = function () {
  this.content = new HTML5ContentManager();
};
Microsoft.Xna.Framework.Game.prototype.get_Content = function () {
  return this.content;
};
Microsoft.Xna.Framework.Game.prototype.LoadContent = function () {
};
Microsoft.Xna.Framework.Game.prototype.UnloadContent = function () {
};
Microsoft.Xna.Framework.Game.prototype.Run = function () {
  this.LoadContent();
  this._runHandle = window.setInterval(this._Step, 1);
};
Microsoft.Xna.Framework.Game.prototype._Step = function () {
  var gameTime = new Microsoft.Xna.Framework.GameTime();
  try {
    this.Update(gameTime);
    this.Draw(gameTime);
  } catch (e) {
    this.Exit();
    throw e;
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