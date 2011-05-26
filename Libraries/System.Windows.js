"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");
  
JSIL.DeclareAssembly("JSIL.Windows");

System.Windows.Forms.Control.prototype.clientWidth = 16;
System.Windows.Forms.Control.prototype.clientHeight = 16;

System.Windows.Forms.Control.prototype.SetStyle = function (styles, newState) {
};

System.Windows.Forms.Control.prototype._ctor = function () {
  this.Controls = new System.Collections.Generic.List$b1();
};

System.Windows.Forms.Control.prototype.get_Controls = function () {
  return this.Controls;
};

System.Windows.Forms.Control.prototype.get_ClientSize = function () {
  return new System.Drawing.Size(this.clientWidth, this.clientHeight);
};

System.Windows.Forms.Control.prototype.set_ClientSize = function (size) {
  this.clientWidth = size.Width;
  this.clientHeight = size.Height;
};

System.Windows.Forms.Control.prototype.add_Paint = function (handler) {
};

System.Windows.Forms.Control.prototype.Show = function () {
};

System.Windows.Forms.Control.prototype.Refresh = function () {
};

System.Windows.Forms.Form.prototype._ctor = function () {
  System.Windows.Forms.Control.prototype._ctor.call(this);
};

System.Windows.Forms.StatusBar.prototype._ctor = function () {
  this.Panels = new System.Collections.Generic.List$b1();
};

System.Windows.Forms.TabControl.prototype._ctor = function () {
  this.TabPages = new System.Collections.Generic.List$b1();
};