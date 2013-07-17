"use strict";

if (typeof (JSIL) === "undefined") 
  throw new Error("JSIL.Core required");

var $jsilopentk = JSIL.DeclareAssembly("JSIL.OpenTK");

var $mgasms = new JSIL.AssemblyCollection({
    0: "Lidgren.Network, Version=2011.3.12.0, Culture=neutral, PublicKeyToken=null", 
    1: "MonoGame.Framework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null", 
    2: "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
    3: "OpenTK, Version=1.1.0.0, Culture=neutral, PublicKeyToken=bad199fe84eb3df4", 
    4: "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
    5: "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", 
    6: "System.Runtime.Serialization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
    7: "System.ServiceModel.Internals, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", 
    8: "System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
    9: "Tao.Sdl, Version=1.2.13.0, Culture=neutral, PublicKeyToken=9c7a200e36c0094e", 
    10: "SMDiagnostics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
    11: "System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
    12: "System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
    13: "System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
  });


JSIL.DeclareNamespace("OpenTK");


JSIL.ImplementExternals("OpenTK.GameWindow", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [], []), 
    function _ctor () {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [$.Int32, $.Int32], []), 
    function _ctor (width, height) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.GraphicsMode")
      ], []), 
    function _ctor (width, height, mode) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.GraphicsMode"), $.String
      ], []), 
    function _ctor (width, height, mode, title) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.GraphicsMode"), $.String, 
        $mgasms[3].TypeRef("OpenTK.GameWindowFlags")
      ], []), 
    function _ctor (width, height, mode, title, options) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.GraphicsMode"), $.String, 
        $mgasms[3].TypeRef("OpenTK.GameWindowFlags"), $mgasms[3].TypeRef("OpenTK.DisplayDevice")
      ], []), 
    function _ctor (width, height, mode, title, options, device) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.GraphicsMode"), $.String, 
        $mgasms[3].TypeRef("OpenTK.GameWindowFlags"), $mgasms[3].TypeRef("OpenTK.DisplayDevice"), 
        $.Int32, $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.GraphicsContextFlags")
      ], []), 
    function _ctor (width, height, mode, title, options, device, major, minor, flags) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.GraphicsMode"), $.String, 
        $mgasms[3].TypeRef("OpenTK.GameWindowFlags"), $mgasms[3].TypeRef("OpenTK.DisplayDevice"), 
        $.Int32, $.Int32, 
        $mgasms[3].TypeRef("OpenTK.Graphics.GraphicsContextFlags"), $mgasms[3].TypeRef("OpenTK.Graphics.IGraphicsContext")
      ], []), 
    function _ctor (width, height, mode, title, options, device, major, minor, flags, sharedContext) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "add_Load", 
    new JSIL.MethodSignature(null, [$mgasms[2].TypeRef("System.EventHandler`1", [$mgasms[2].TypeRef("System.EventArgs")])], []), 
    function add_Load (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "add_RenderFrame", 
    new JSIL.MethodSignature(null, [$mgasms[2].TypeRef("System.EventHandler`1", [$mgasms[3].TypeRef("OpenTK.FrameEventArgs")])], []), 
    function add_RenderFrame (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "add_Unload", 
    new JSIL.MethodSignature(null, [$mgasms[2].TypeRef("System.EventHandler`1", [$mgasms[2].TypeRef("System.EventArgs")])], []), 
    function add_Unload (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "add_UpdateFrame", 
    new JSIL.MethodSignature(null, [$mgasms[2].TypeRef("System.EventHandler`1", [$mgasms[3].TypeRef("OpenTK.FrameEventArgs")])], []), 
    function add_UpdateFrame (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "Dispose", 
    new JSIL.MethodSignature(null, [], []), 
    function Dispose () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "Exit", 
    new JSIL.MethodSignature(null, [], []), 
    function Exit () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_Context", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.Graphics.IGraphicsContext"), [], []), 
    function get_Context () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_IsExiting", 
    new JSIL.MethodSignature($.Boolean, [], []), 
    function get_IsExiting () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_Joysticks", 
    new JSIL.MethodSignature($mgasms[2].TypeRef("System.Collections.Generic.IList`1", [$mgasms[3].TypeRef("OpenTK.Input.JoystickDevice")]), [], []), 
    function get_Joysticks () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_Keyboard", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.Input.KeyboardDevice"), [], []), 
    function get_Keyboard () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_Mouse", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.Input.MouseDevice"), [], []), 
    function get_Mouse () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_RenderFrequency", 
    new JSIL.MethodSignature($.Double, [], []), 
    function get_RenderFrequency () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_RenderPeriod", 
    new JSIL.MethodSignature($.Double, [], []), 
    function get_RenderPeriod () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_RenderTime", 
    new JSIL.MethodSignature($.Double, [], []), 
    function get_RenderTime () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_TargetRenderFrequency", 
    new JSIL.MethodSignature($.Double, [], []), 
    function get_TargetRenderFrequency () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_TargetRenderPeriod", 
    new JSIL.MethodSignature($.Double, [], []), 
    function get_TargetRenderPeriod () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_TargetUpdateFrequency", 
    new JSIL.MethodSignature($.Double, [], []), 
    function get_TargetUpdateFrequency () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_TargetUpdatePeriod", 
    new JSIL.MethodSignature($.Double, [], []), 
    function get_TargetUpdatePeriod () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_UpdateFrequency", 
    new JSIL.MethodSignature($.Double, [], []), 
    function get_UpdateFrequency () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_UpdatePeriod", 
    new JSIL.MethodSignature($.Double, [], []), 
    function get_UpdatePeriod () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_UpdateTime", 
    new JSIL.MethodSignature($.Double, [], []), 
    function get_UpdateTime () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_VSync", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.VSyncMode"), [], []), 
    function get_VSync () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "get_WindowState", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.WindowState"), [], []), 
    function get_WindowState () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "MakeCurrent", 
    new JSIL.MethodSignature(null, [], []), 
    function MakeCurrent () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "remove_Load", 
    new JSIL.MethodSignature(null, [$mgasms[2].TypeRef("System.EventHandler`1", [$mgasms[2].TypeRef("System.EventArgs")])], []), 
    function remove_Load (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "remove_RenderFrame", 
    new JSIL.MethodSignature(null, [$mgasms[2].TypeRef("System.EventHandler`1", [$mgasms[3].TypeRef("OpenTK.FrameEventArgs")])], []), 
    function remove_RenderFrame (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "remove_Unload", 
    new JSIL.MethodSignature(null, [$mgasms[2].TypeRef("System.EventHandler`1", [$mgasms[2].TypeRef("System.EventArgs")])], []), 
    function remove_Unload (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "remove_UpdateFrame", 
    new JSIL.MethodSignature(null, [$mgasms[2].TypeRef("System.EventHandler`1", [$mgasms[3].TypeRef("OpenTK.FrameEventArgs")])], []), 
    function remove_UpdateFrame (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "Run", 
    new JSIL.MethodSignature(null, [], []), 
    function Run () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "Run", 
    new JSIL.MethodSignature(null, [$.Double], []), 
    function Run (updateRate) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "Run", 
    new JSIL.MethodSignature(null, [$.Double, $.Double], []), 
    function Run (updates_per_second, frames_per_second) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "set_TargetRenderFrequency", 
    new JSIL.MethodSignature(null, [$.Double], []), 
    function set_TargetRenderFrequency (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "set_TargetRenderPeriod", 
    new JSIL.MethodSignature(null, [$.Double], []), 
    function set_TargetRenderPeriod (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "set_TargetUpdateFrequency", 
    new JSIL.MethodSignature(null, [$.Double], []), 
    function set_TargetUpdateFrequency (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "set_TargetUpdatePeriod", 
    new JSIL.MethodSignature(null, [$.Double], []), 
    function set_TargetUpdatePeriod (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "set_VSync", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.VSyncMode")], []), 
    function set_VSync (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "set_WindowState", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.WindowState")], []), 
    function set_WindowState (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "SwapBuffers", 
    new JSIL.MethodSignature(null, [], []), 
    function SwapBuffers () {
      throw new Error('Not implemented');
    }
  );
});