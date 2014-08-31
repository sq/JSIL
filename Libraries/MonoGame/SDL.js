"use strict";

if (typeof (JSIL) === "undefined") 
  throw new Error("JSIL.Core required");

var $jsilsdl = JSIL.DeclareAssembly("JSIL.SDL");

var $sdlasms = new JSIL.AssemblyCollection({
  2: "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
  4: "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
  8: "System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 
  9: "Tao.Sdl, Version=1.2.13.0, Culture=neutral, PublicKeyToken=9c7a200e36c0094e"
});

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.SDL");

JSIL.DeclareNamespace("Tao");
JSIL.DeclareNamespace("Tao.Sdl");


JSIL.SDL.gamepadState = null;

JSIL.SDL.InitializeJoystick = function (resultRef) {
};


JSIL.ImplementExternals("Tao.Sdl.Sdl", function ($) {
  var SDL_INIT_TIMER = 1;
  var SDL_INIT_AUDIO = 16;
  var SDL_INIT_VIDEO = 32;
  var SDL_INIT_CDROM = 256;
  var SDL_INIT_JOYSTICK = 512;
  var SDL_INIT_NOPARACHUTE = 1048576;
  var SDL_INIT_EVENTTHREAD = 16777216;
  var SDL_INIT_EVERYTHING = 65535;

  $.Method({Static:true , Public:true }, "SDL_Init", 
    new JSIL.MethodSignature($.Int32, [$.Int32], []), 
    function SDL_Init (flags) {
      var result = new JSIL.BoxedVariable(0);

      if (flags & SDL_INIT_TIMER)
        result.set(-1);

      if (flags & SDL_INIT_AUDIO)
        result.set(-1);

      if (flags & SDL_INIT_VIDEO)
        result.set(-1);

      if (flags & SDL_INIT_CDROM)
        result.set(-1);

      if (flags & SDL_INIT_JOYSTICK)
        JSIL.SDL.InitializeJoystick(result);

      return result.get();
    }
  );

  $.Method({Static:true , Public:true }, "SDL_NumJoysticks", 
    new JSIL.MethodSignature($.Int32, [], []), 
    function SDL_NumJoysticks () {
      var svc = JSIL.Host.getService("nativeGamepad");
      
      if (!svc)
        return 0;

      // HACK: HTML5 is awful and won't enumerate joysticks we haven't used yet.
      // HACK: MonoGame SDL is awful and won't enumerate joysticks that are hotplugged.
      // Thus...
      // return svc.getCount();
      return 1;
    }
  );

  $.Method({Static:true , Public:true }, "SDL_JoystickUpdate", 
    JSIL.MethodSignature.Void, 
    function SDL_JoystickUpdate () {
      var svc = JSIL.Host.getService("nativeGamepad");

      if (!svc)
        return;

      JSIL.SDL.gamepadState = svc.getState();
    }
  );

  $.Method({Static:true , Public:true }, "SDL_JoystickName", 
    new JSIL.MethodSignature($.String, [$.Int32], []), 
    function SDL_JoystickName (device_index) {
      var svc = JSIL.Host.getService("nativeGamepad");

      if (!svc)
        return null;

      var state = svc.getState();
      if (!state)
        return null;

      if (!state[device_index])
        return null;

      return state[device_index].id;
    }
  );

  $.Method({Static:true , Public:true }, "SDL_JoystickNumAxes", 
    new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.IntPtr")], []), 
    function SDL_JoystickNumAxes (joystick) {
      // FIXME: SDL suuuuuuuuuuuuuuuuuuuuuuucks, hotplug augh etc.
      return 4;

      var index = joystick.ToInt32() - 1;

      var svc = JSIL.Host.getService("nativeGamepad", true);
      var state = svc.getState();
      if (!state)
        return 0;

      if (!state[index])
        return 0;

      return state[index].axes.length;    
    }
  );

  $.Method({Static:true , Public:true }, "SDL_JoystickNumBalls", 
    new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.IntPtr")], []), 
    function SDL_JoystickNumBalls (joystick) {
      // FIXME
      return 0;
    }
  );

  $.Method({Static:true , Public:true }, "SDL_JoystickNumButtons", 
    new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.IntPtr")], []), 
    function SDL_JoystickNumButtons (joystick) {
      // FIXME: SDL suuuuuuuuuuuuuuuuuuuuuuucks, hotplug augh etc.
      return 16;

      var index = joystick.ToInt32() - 1;

      var svc = JSIL.Host.getService("nativeGamepad", true);
      var state = svc.getState();
      if (!state)
        return 0;

      if (!state[index])
        return 0;

      return state[index].buttons.length;    
    }
  );

  $.Method({Static:true , Public:true }, "SDL_JoystickNumHats", 
    new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.IntPtr")], []), 
    function SDL_JoystickNumHats (joystick) {
      // FIXME
      // HACK: UGH MonoGame requires a hat representing dpad. WORST.
      return 1;
    }
  );

  $.Method({Static:true , Public:true }, "SDL_JoystickOpen", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.IntPtr"), [$.Int32], []), 
    function SDL_JoystickOpen (device_index) {
      // HACK: Returning IntPtr.Zero is treated as 'no joystick', which I guess makes sense, kinda.
      var result = new System.IntPtr(device_index + 1);
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "SDL_JoystickGetAxis", 
    new JSIL.MethodSignature($.Int16, [$jsilcore.TypeRef("System.IntPtr"), $.Int32], []), 
    function SDL_JoystickGetAxis (joystick, axis) {
      var index = joystick.ToInt32() - 1;

      var svc = JSIL.Host.getService("nativeGamepad", true);
      var states = svc.getState();
      if (!states)
        return 0;

      var state = states[index];
      if (!state)
        return 0;

      return (state.axes[axis] * 32767) | 0;
    }
  );

  $.Method({Static:true , Public:true }, "SDL_JoystickGetBall", 
    new JSIL.MethodSignature($.Int32, [
        $jsilcore.TypeRef("System.IntPtr"), $.Int32, 
        $jsilcore.TypeRef("JSIL.Reference", [$.Int32]), $jsilcore.TypeRef("JSIL.Reference", [$.Int32])
      ], []), 
    function SDL_JoystickGetBall (joystick, ball, /* ref */ dx, /* ref */ dy) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SDL_JoystickGetButton", 
    new JSIL.MethodSignature($.Byte, [$jsilcore.TypeRef("System.IntPtr"), $.Int32], []), 
    function SDL_JoystickGetButton (joystick, button) {
      var index = joystick.ToInt32() - 1;

      var svc = JSIL.Host.getService("nativeGamepad", true);
      var states = svc.getState();
      if (!states)
        return 0;

      var state = states[index];
      if (!state)
        return 0;

      return state.buttons[button] ? 1 : 0;
    }
  );

  $.Method({Static:true , Public:true }, "SDL_JoystickGetHat", 
    new JSIL.MethodSignature($.Byte, [$jsilcore.TypeRef("System.IntPtr"), $.Int32], []), 
    function SDL_JoystickGetHat (joystick, hat) {
      // FIXME: uuuugh monogame demands a hat representing the dpad, 
      //  and tries to query it even if you say you don't have one
      // WORST
      return 0;
    }
  );
});