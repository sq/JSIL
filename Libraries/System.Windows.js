"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");
  
JSIL.DeclareAssembly("JSIL.Windows");

JSIL.ImplementExternals(
  "System.Windows.Forms.Control", true, {
    clientWidth: 16,
    clientHeight: 16,
    SetStyle: function (styles, newState) {
    },
    _ctor: function () {
      this._controls = new (System.Collections.Generic.List$b1.Of(System.Object)) ();
    },
    get_Controls: function () {
      return this._controls;
    },
    get_ClientSize: function () {
      return new System.Drawing.Size(this.clientWidth, this.clientHeight);
    },
    set_ClientSize: function (size) {
      this.clientWidth = size.Width;
      this.clientHeight = size.Height;
    },
    add_Paint: function (handler) {
    },
    Show: function () {
    },
    Refresh: function () {
    }
  }
);

JSIL.ImplementExternals(
  "System.Windows.Forms.Form", true, {
    _ctor: function () {
      System.Windows.Forms.Control.prototype._ctor.call(this);
    }
  }
);

JSIL.ImplementExternals(
  "System.Windows.Forms.StatusBar", true, {
    _ctor: function () {
      this._panels = new (System.Collections.Generic.List$b1.Of(System.Object)) ();
    },
    get_Panels: function () {
      return this._panels;
    }
  }
);

JSIL.ImplementExternals(
  "System.Windows.Forms.TabControl", true, {
    _ctor: function () {
      this._tabPages = new (System.Collections.Generic.List$b1.Of(System.Object)) ();
    },
    get_TabPages: function () {
      return this._tabPages;
    }
  }
);
