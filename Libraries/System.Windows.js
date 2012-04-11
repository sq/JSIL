"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");
  
JSIL.DeclareAssembly("JSIL.Windows");

JSIL.DeclareNamespace("JSIL");

JSIL.ImplementExternals("System.Windows.Forms.Control", function ($) {
  var coreCtor = function _ctor () {
    this._controls = new $asms[15].System.Windows.Forms.Control_ControlCollection ();
  };

  $.RawMethod(false, "$coreCtor", coreCtor);

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    coreCtor
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    coreCtor
  );

  $.Method({Static:false, Public:true }, "get_Controls", 
    (new JSIL.MethodSignature($asms[15].TypeRef("System.Windows.Forms.Control/ControlCollection"), [], [])), 
    function get_Controls () {
      return this._controls;
    }
  );

  $.Method({Static:false, Public:true }, "get_ClientSize", 
    (new JSIL.MethodSignature($asms[11].TypeRef("System.Drawing.Size"), [], [])), 
    function get_ClientSize () {
      return new System.Drawing.Size(this.clientWidth, this.clientHeight);
    }
  );

  $.Method({Static:false, Public:true }, "set_ClientSize", 
    (new JSIL.MethodSignature(null, [$asms[11].TypeRef("System.Drawing.Size")], [])), 
    function set_ClientSize (value) {
      this.clientWidth = value.Width;
      this.clientHeight = value.Height;
    }
  );
});

JSIL.ImplementExternals("System.Windows.Forms.Control/ControlCollection", function ($) {
  $jsilcore.$ListExternals($, $asms[15].TypeRef("System.Windows.Forms.Control"), false);
});

JSIL.ImplementExternals("System.Windows.Forms.StatusBar/StatusBarPanelCollection", function ($) {
  $jsilcore.$ListExternals($, $asms[15].TypeRef("System.Windows.Forms.StatusBarPanel"), true);
});

JSIL.ImplementExternals("System.Windows.Forms.TabControl/TabPageCollection", function ($) {
  $jsilcore.$ListExternals($, $asms[15].TypeRef("System.Windows.Forms.TabPage"), true);
});

JSIL.ImplementExternals("System.Windows.Forms.ListBox/ObjectCollection", function ($) {
  $jsilcore.$ListExternals($, $.Object, true);
});

JSIL.ImplementExternals("System.Windows.Forms.Form", function ($) {
  $.InheritDefaultConstructor();
});

JSIL.ImplementExternals("System.Windows.Forms.ListBox", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this.$coreCtor();
      this._items = new $asms[15].System.Windows.Forms.ListBox_ObjectCollection ();
    }
  );

  $.Method({Static:false, Public:true }, "get_Items", 
    (new JSIL.MethodSignature($asms[15].TypeRef("System.Windows.Forms.ListBox/ObjectCollection"), [], [])), 
    function get_Items () {
      return this._items;
    }
  );

  $.Method({Static:false, Public:true }, "get_SelectedIndex", 
    (new JSIL.MethodSignature($.Int32, [], [])), 
    function get_SelectedIndex () {
      return 0;
    }
  );

  $.Method({Static:false, Public:true }, "get_SelectedItem", 
    (new JSIL.MethodSignature($.Object, [], [])), 
    function get_SelectedItem () {
      return null;
    }
  );
});

JSIL.ImplementExternals("System.Windows.Forms.StatusBar", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this.$coreCtor();
      this._panels = new $asms[15].System.Windows.Forms.StatusBar_StatusBarPanelCollection ();
    }
  );

  $.Method({Static:false, Public:true }, "get_Panels", 
    (new JSIL.MethodSignature($asms[15].TypeRef("System.Windows.Forms.StatusBar/StatusBarPanelCollection"), [], [])), 
    function get_Panels () {
      return this._panels;
    }
  );
});

JSIL.ImplementExternals("System.Windows.Forms.TabControl", function ($) {
  $.Method({Static:false, Public:true }, ".ctor", 
    (new JSIL.MethodSignature(null, [], [])), 
    function _ctor () {
      this.$coreCtor();
      this._tabPages = new $asms[15].System.Windows.Forms.TabControl_TabPageCollection ();
    }
  );

  $.Method({Static:false, Public:true }, "get_TabPages", 
    (new JSIL.MethodSignature($asms[15].TypeRef("System.Windows.Forms.TabControl/TabPageCollection"), [], [])), 
    function get_TabPages () {
      return this._tabPages;
    }
  );
});
